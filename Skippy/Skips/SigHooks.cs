using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;

using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace Skippy.Skips {
    internal partial class SigHooks {
        private readonly Config _config;
        private readonly IPluginLog _pluginLog;
        private readonly IGameInteropProvider _gameInteropProvider;
        private readonly IClientState _clientState;
        private readonly IDataManager _dataManager;
        private readonly CutsceneAddressResolver _address;

        private static readonly HttpClient Http = new();

        private const string DocURL = "https://script.google.com/macros/s/AKfycbwHPFG5hb8wQUKOSl60pnASLwrqhACP2nshiGh6iAbg6ftei3PZs4YTi1DhcSreV4tQBA/exec";

        private Hook<UIState.Delegates.IsCutsceneSeen>? _cutsceneSeenHook;
        private Hook<ContentDirectorDelegate>? _msqHook;
        private Hook<ContentDirectorDelegate>? _massivePCHook;
        private Hook<ContentDirectorDelegate>? _goldSaucerHook;
        private Hook<ContentDirectorDelegate>? _customTalkHook;
        private Hook<NormalCutscenesDelegate>? _normalCutscenesHook;
        private Hook<SinglePtrDelegate>? _innHook;
        private Hook<SinglePtrDelegate>? _feedBuddyHook;
        private Hook<SinglePtrDelegate>? _grandCompanyRankUpHook;
        private Hook<SinglePtrDelegate>? _hairMakeHook;

        private delegate long ContentDirectorDelegate(nint luaState);
        private delegate long NormalCutscenesDelegate(nint luaState1, nint luaState2);
        private delegate long SinglePtrDelegate(nint luaState);

        internal SigHooks(
            Config config,
            IPluginLog pluginLog,
            IGameInteropProvider gameInteropProvider,
            IClientState clientState,
            IDataManager dataManager,
            CutsceneAddressResolver address) {
            _config = config;
            _pluginLog = pluginLog;
            _gameInteropProvider = gameInteropProvider;
            _clientState = clientState;
            _dataManager = dataManager;
            _address = address;
        }

        internal void RefreshHooks() {
            bool exempt = IsExemptedTerritory((ushort)_clientState.TerritoryType);
            SetEnabled(_config.IsEnabled && !exempt && ShouldPatchMemory());

            bool msq = _config.IsEnabled && (_config.SkipMSQRoulette || _config.SkipOceanFishing || _config.SkipCrystallineConflict || _config.ExemptPrae || _config.ExemptCastrum || _config.ExemptPorta || _config.ResearchMSQHook);
            const string msqSig = "48 89 5C 24 ?? 57 48 83 EC 50 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? BA ?? ?? ?? ?? B3 01 E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? 48 8B F8 E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 08 84 99 ?? ?? ?? ??";

            if (msq) {
                if (_msqHook == null) {
                    try {
                        _msqHook = _gameInteropProvider.HookFromSignature<ContentDirectorDelegate>(msqSig, ExemptionMSQRoulette);
                        _msqHook.Enable();
                    } catch (Exception e) {
                        _pluginLog.Warning(e, "Couldn't find the MSQ hook signature. The Skips for MSQ Roulette/Ocean Fishing/Crystalline Conflict won't work.");
                    }
                } else if (!_msqHook.IsEnabled) {
                    _msqHook.Enable();
                }
            } else {
                _msqHook?.Dispose();
                _msqHook = null;
            }

            RefreshContentDirectorHook(ref _massivePCHook, "48 89 5C 24 ?? 57 48 83 EC 50 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? BA ?? ?? ?? ?? B3 01 E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? 48 8B F8 E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 08 48 8B 11", _config.IsEnabled && (_config.SkipMassivePC || _config.ResearchMassivePCHook), ExemptionMassivePC);

            bool goldSaucer = _config.IsEnabled && (_config.SkipGoldSaucer
                || _config.ExemptChocoboRace || _config.ExemptVerminion || _config.ExemptTripleTriad || _config.ExemptFallGuys
                || _config.ExemptAirForceOne || _config.ExemptMahjong || _config.ResearchGoldSaucerHook);
            RefreshContentDirectorHook(ref _goldSaucerHook, "48 89 5C 24 ?? 57 48 83 EC 50 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? BA ?? ?? ?? ?? B3 01 E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? 48 8B F8 E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 08 84 99 ?? ?? ?? ??", goldSaucer, ExemptionGoldSaucer);

            RefreshContentDirectorHook(ref _customTalkHook, "48 83 EC 58 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 08 48 85 C9 74 06", _config.IsEnabled && (_config.SkipCustomTalk || _config.ResearchCustomTalkHook), ExemptionCustomTalk);

            RefreshNormalCutscenesHook(_config.IsEnabled && (_config.SkipNormalCutscenes || _config.ExemptSubmarines || _config.ResearchNormalCutscenesHook));

            RefreshInnHook(_config.IsEnabled && (_config.SkipInn || _config.ResearchInnHook));

            RefreshFeedBuddyHook(_config.IsEnabled && (_config.SkipFeedBuddy || _config.ResearchFeedBuddyHook));

            RefreshSinglePtrHook(ref _grandCompanyRankUpHook, "40 53 48 83 EC 50 48 8B D9 48 8D 4C 24 ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 83 38 00 0F 84 ?? ?? ?? ?? 48 8B 5B 08 E8 ?? ?? ?? ?? 33 D2 48 8B C8 E8 ?? ?? ?? ?? 48 83 7B ?? ?? 75 04 33 DB EB 2D 48 8B 4B 60 48 8B 53 58 48 FF C9 48 03 D1 48 8B 4B 50 48 8B C2 48 FF C9 48 D1 E8 48 23 C8 48 8B 43 48 83 E2 01 48 8B 04 C8 48 8B 1C D0 48 8B 0D ?? ?? ?? ?? 48 89 1D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B C8 48 8B 10 FF 92 ?? ?? ?? ?? 48 8B C8 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? 48 8B C8 E8 ?? ?? ?? ?? 33 D2 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 8B D8 E8 ?? ?? ?? ?? 8B C3 48 83 C4 50 5B C3 33 DB 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 8B C3 48 83 C4 50 5B C3 CC CC CC CC CC CC CC CC 40 55 53 48 8D 6C 24 ??", _config.IsEnabled && _config.ResearchGrandCompanyRankUpHook, _ => { LogExemption("Dev_GrandCompanyRankUp"); return 1L; });

            RefreshSinglePtrHook(ref _hairMakeHook, "48 89 5C 24 ?? 57 48 83 EC 50 48 8B D9 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 5B 08 33 D2 45 33 C0 8D 4A 28 E8 ?? ?? ?? ?? 48 85 C0", _config.IsEnabled && _config.ResearchHairMakeHook, _ => { LogExemption("Dev_HairMake"); return 1L; });
        }

        private void RefreshContentDirectorHook(ref Hook<ContentDirectorDelegate>? hook, string sig, bool enable, ContentDirectorDelegate? exempt = null) {
            if (enable) {
                if (hook == null) {
                    try {
                        hook = _gameInteropProvider.HookFromSignature<ContentDirectorDelegate>(sig, exempt ?? (_ => 1L));
                        hook.Enable();
                    } catch (Exception e) {
                        _pluginLog.Warning(e, "Couldn't find this hook signature : {0}", sig[..Math.Min(40, sig.Length)]);
                    }
                } else if (!hook.IsEnabled) {
                    hook.Enable();
                }
            } else {
                hook?.Dispose();
                hook = null;
            }
        }

        private void RefreshSinglePtrHook(ref Hook<SinglePtrDelegate>? hook, string sig, bool enable, SinglePtrDelegate? detour = null) {
            if (enable) {
                if (hook == null) {
                    try {
                        hook = _gameInteropProvider.HookFromSignature<SinglePtrDelegate>(sig, detour ?? (_ => 1L));
                        hook.Enable();
                    } catch (Exception e) {
                        _pluginLog.Warning(e, "Couldn't find hook signature: {0}", sig[..Math.Min(40, sig.Length)]);
                    }
                } else if (!hook.IsEnabled) {
                    hook.Enable();
                }
            } else {
                hook?.Disable();
            }
        }

        private void RefreshNormalCutscenesHook(bool enable) {
            const string sig = "40 53 55 57 41 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B 59 08";
            
            if (enable) {
                if (_normalCutscenesHook == null) {
                    try {
                        _normalCutscenesHook = _gameInteropProvider.HookFromSignature<NormalCutscenesDelegate>(sig, ExemptionNormalCutscenes);
                        _normalCutscenesHook.Enable();
                    } catch (Exception e) {
                        _pluginLog.Warning(e, "Couldn't find the Normal Cutscenes hook signature. The Skips for Normal Cutscenes won't work.");
                    }
                } else if (!_normalCutscenesHook.IsEnabled) {
                    _normalCutscenesHook.Enable();
                }
            } else {
                _normalCutscenesHook?.Dispose();
                _normalCutscenesHook = null;
            }
        }

        private void RefreshInnHook(bool enable) {
            const string sig = "48 83 EC 58 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ??";
            
            if (enable) {
                if (_innHook == null) {
                    try {
                        _innHook = _gameInteropProvider.HookFromSignature<SinglePtrDelegate>(sig, ExemptionInn);
                        _innHook.Enable();
                    } catch (Exception e) {
                        _pluginLog.Warning(e, "Couldn't find the Inn hook signature. The Skip for the Inn won't work.");
                    }
                } else if (!_innHook.IsEnabled) {
                    _innHook.Enable();
                }
            } else {
                _innHook?.Dispose();
                _innHook = null;
            }
        }

        private void RefreshFeedBuddyHook(bool enable) {
            const string sig = "48 8B 49 08 48 85 C9 74 0C 48 81 C1 ?? ?? ?? ?? E9 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC CC CC 48 8B 41 08";
            
            if (enable) {
                if (_feedBuddyHook == null) {
                    try {
                        _feedBuddyHook = _gameInteropProvider.HookFromSignature<SinglePtrDelegate>(sig, ExemptionFeedBuddy);
                        _feedBuddyHook.Enable();
                    } catch (Exception e) {
                        _pluginLog.Warning(e, "Couldn't find the Feed Buddy hook signature. The Skip for the Feed Buddy Animation won't work.");
                    }
                } else if (!_feedBuddyHook.IsEnabled) {
                    _feedBuddyHook.Enable();
                }
            } else {
                _feedBuddyHook?.Dispose();
                _feedBuddyHook = null;
            }
        }

        internal void TearDownHooks() {
            SetEnabled(false);
            
            _msqHook?.Dispose();
            _massivePCHook?.Dispose();
            _goldSaucerHook?.Dispose();
            _customTalkHook?.Dispose();
            _normalCutscenesHook?.Dispose();
            _innHook?.Dispose();
            _feedBuddyHook?.Dispose();
            _grandCompanyRankUpHook?.Dispose();
            _hairMakeHook?.Dispose();
        }

        internal unsafe void SetEnabled(bool isEnabled) {
            if (!_address.Valid) {
                return;
            }
            
            if (isEnabled) {
                SafeMemory.Write<short>(_address.Offset1, -28528);
                SafeMemory.Write<short>(_address.Offset2, -28528);
                
                if (_cutsceneSeenHook == null) {
                    _cutsceneSeenHook = _gameInteropProvider.HookFromAddress<UIState.Delegates.IsCutsceneSeen>(UIState.MemberFunctionPointers.IsCutsceneSeen, CutsceneSeenDetour);
                    _cutsceneSeenHook.Enable();
                }
            } else {
                SafeMemory.Write<short>(_address.Offset1, 14709);
                SafeMemory.Write<short>(_address.Offset2, 6260);
                
                _cutsceneSeenHook?.Dispose();
                _cutsceneSeenHook = null;
            }
        }

        private const string Version = "2.2.3.0";

        internal static async Task<string> FetchPassword() {
            try {
                var url = DocURL + "?action=getPassword";
                return (await Http.GetStringAsync(url).ConfigureAwait(false)).Trim();
            } catch {
                return string.Empty;
            }
        }

        private void LogToSheet(string hook, ushort territory, string placeName, TerritoryIntendedUse intendedUse, bool isCutsceneSeen = false, uint cutsceneId = 0) {
            bool isDev = hook.StartsWith("Dev_");
            
            _ = Task.Run(async () => {
                try {
                    var json = $"{{\"hook\":\"{hook}\",\"territory\":{territory},\"place\":\"{placeName}\",\"intendedUse\":\"{intendedUse}\",\"time\":\"{DateTime.UtcNow:u}\",\"version\":\"{Version}\",\"devMode\":{(isDev ? "true" : "false")},\"isCutsceneSeen\":{(isCutsceneSeen ? "true" : "false")},\"cutsceneId\":{cutsceneId}}}";
                    await Http.PostAsync(DocURL, new StringContent(json, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                } catch { }
            });
        }

        private void LogExemption(string hook, bool isCutsceneSeen = false, uint cutsceneId = 0) {
            var territory = (ushort)_clientState.TerritoryType;
            var use = GetIntendedUse(territory);
            var name = _dataManager.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(territory)?.PlaceName.Value.Name.ToString() ?? "Unknown";

            if (_config.ResearchExtraLogs) {
                _pluginLog.Information("[Skippy] {0} — territory={1} ({2}) intendedUse={3} isCutsceneSeen={4} cutsceneId={5}", hook, territory, name, use, isCutsceneSeen, cutsceneId);
            }

            LogToSheet(hook, territory, name, use, isCutsceneSeen, cutsceneId);
        }
    }
}
