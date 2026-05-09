using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Skippy.Skips;
using Skippy.UI;

namespace Skippy {
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class Skippy : IDalamudPlugin {
        internal static Skippy Instance { get; private set; } = null!;

        private readonly Config _config;
        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly ICommandManager _commandManager;
        private readonly IChatGui _chatGui;
        private readonly IPluginLog _pluginLog;
        private readonly IPartyList _partyList;
        private readonly IClientState _clientState;
        
        internal bool _devModeValid { get; private set; } = false;
        private bool _autoPartyActive;

        internal readonly SigHooks Hooks;
        private readonly IPC.IPC _IPC;
        private readonly Menu _mainUI;
        private readonly WindowSystem _windowSystem = new("Skippy");

        public CutsceneAddressResolver Address { get; }

        public Skippy(
            IDalamudPluginInterface pluginInterface,
            ISigScanner sigScanner,
            ICommandManager commandManager,
            IChatGui chatGui,
            IPluginLog pluginLog,
            IGameInteropProvider gameInteropProvider,
            ITextureProvider textureProvider,
            IClientState clientState,
            IDataManager dataManager,
            IPartyList partyList) {
            Instance = this;

            _pluginInterface = pluginInterface;
            _commandManager = commandManager;
            _chatGui = chatGui;
            _pluginLog = pluginLog;
            _partyList = partyList;
            _clientState = clientState;

            if (_pluginInterface.GetPluginConfig() is not Config configuration || configuration.Version < 4) {
                configuration = new Config { Version = 5 };
                _pluginInterface.SavePluginConfig(configuration);
                
                _chatGui.Print("[Skippy] Your configuration was from an older version and has been reset to defaults. Please re-apply your settings.");
            }

            _config = configuration;

            if (_config.AutoEnable4Man) {
                if (_config.SkipMSQRoulette) {
                    _config.SkipMSQRoulette = false;
                    _pluginInterface.SavePluginConfig(_config);
                }
                
                _autoPartyActive = false;
            }
            Address = new CutsceneAddressResolver(_pluginLog, sigScanner);
            Hooks = new SigHooks(_config, _pluginLog, gameInteropProvider, clientState, dataManager, Address);

            if (_config.DevMode) {
                _ = DevValidation();
            } else {
                _devModeValid = true;
            }

            _IPC = new IPC.IPC(_pluginInterface, _config);
            _mainUI = new Menu(_config, Hooks.SetEnabled, SaveConfig, _pluginInterface, textureProvider);
            _windowSystem.AddWindow(_mainUI);

            _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;
            _pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
            _pluginInterface.UiBuilder.OpenMainUi += OpenConfigUi;
            
            clientState.TerritoryChanged += OnTerritoryChanged;
            clientState.CfPop += OnCfPop;

            if (Address.Valid) {
                _pluginLog.Information("Cutscene Offset Found.");
                
                if (_config.IsEnabled) {
                    Hooks.RefreshHooks();
                }
            } else {
                _pluginLog.Error("Cutscene Offset Not Found.");
                _pluginLog.Warning("Plugin Disabling...");
                _chatGui.PrintError("[Skippy] Cutscene offsets have not been found! The plugin will not work. Please use '/skippy log' and send the file to the developer.");
            }

            _commandManager.AddHandler("/skippy", new CommandInfo(OnCommand) {
                HelpMessage = "/skippy [on/off/log]: Toggle the plugin state, export debug logs, or open the settings window."
            });
        }

        public void Dispose() {
            Hooks.TearDownHooks();
            _IPC.Dispose();
            
            _pluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
            _pluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
            _pluginInterface.UiBuilder.OpenMainUi -= OpenConfigUi;
            
            _windowSystem.RemoveAllWindows();
            _mainUI.Dispose();
            
            _clientState.TerritoryChanged -= OnTerritoryChanged;
            _clientState.CfPop -= OnCfPop;

            _commandManager.RemoveHandler("/skippy");
            
            GC.SuppressFinalize(this);
        }
        
        internal void SaveConfig() => _pluginInterface.SavePluginConfig(_config);
        
        internal void PrintChat(string message) => _chatGui.Print(message);
        
        internal void PrintError(string message) => _chatGui.PrintError(message);
        
        internal void PrintSuccess(string message) {
            var msg = new Dalamud.Game.Text.SeStringHandling.SeStringBuilder().AddUiForeground(message, 72).Build();
            _chatGui.Print(msg);
        }
        
        internal async Task DevValidation() {
            bool copyMSQ = _config.ResearchMSQHook;
            bool copyMassivePC = _config.ResearchMassivePCHook;
            bool copyGoldSaucer = _config.ResearchGoldSaucerHook;
            bool copyCustomTalk = _config.ResearchCustomTalkHook;
            bool copyNormalCutscenes = _config.ResearchNormalCutscenesHook;
            bool copyInn = _config.ResearchInnHook;
            bool copyFeedBuddy = _config.ResearchFeedBuddyHook;
            bool copyGCRankUp = _config.ResearchGrandCompanyRankUpHook;
            bool copyHairMake = _config.ResearchHairMakeHook;

            _config.ResearchMSQHook = false;
            _config.ResearchMassivePCHook = false;
            _config.ResearchGoldSaucerHook = false;
            _config.ResearchCustomTalkHook = false;
            _config.ResearchNormalCutscenesHook = false;
            _config.ResearchInnHook = false;
            _config.ResearchFeedBuddyHook = false;
            _config.ResearchGrandCompanyRankUpHook = false;
            _config.ResearchHairMakeHook = false;
            
            Hooks.RefreshHooks();

            var password = await SigHooks.FetchPassword().ConfigureAwait(false);
            
            if (!string.IsNullOrEmpty(password) && _config.DevPassword == password) {
                _config.ResearchMSQHook = copyMSQ;
                _config.ResearchMassivePCHook = copyMassivePC;
                _config.ResearchGoldSaucerHook = copyGoldSaucer;
                _config.ResearchCustomTalkHook = copyCustomTalk;
                _config.ResearchNormalCutscenesHook = copyNormalCutscenes;
                _config.ResearchInnHook = copyInn;
                _config.ResearchFeedBuddyHook = copyFeedBuddy;
                _config.ResearchGrandCompanyRankUpHook = copyGCRankUp;
                _config.ResearchHairMakeHook = copyHairMake;
                
                Hooks.RefreshHooks();
            } else {
                _config.DevMode = false;
                _config.DevPassword = string.Empty;
                _pluginInterface.SavePluginConfig(_config);
                
                _chatGui.PrintError("[Skippy] Nice try, but you cannot simply access Developer Tools by just trying to put in a password in the config file. If you don't have the right password, it's because you're not supposed to have it!");
                _mainUI.ResetCategory();
            }
            
            _devModeValid = true;
        }

        internal async Task TryEnableDevMode(string input) {
            var password = await SigHooks.FetchPassword().ConfigureAwait(false);
            
            if (!string.IsNullOrEmpty(password) && input.Trim() == password) {
                _config.DevMode = true;
                _config.DevPassword = password;
                _pluginInterface.SavePluginConfig(_config);
                
                PrintSuccess("[Skippy] Developer Mode has been enabled. Please remember to only use the research hooks during active testing and to disable them afterwards - leaving them on permanently may be dangerous for your account.");
            } else {
                _config.DevMode = false;
                _config.DevPassword = string.Empty;
                _pluginInterface.SavePluginConfig(_config);
                
                _chatGui.PrintError("[Skippy] The entered Developer Mode password is incorrect. Developer Mode has not been enabled.");
            }
        }

        internal void ExportLog() {
            try {
                var pluginConfig = _pluginInterface.ConfigDirectory;
                var xivLauncher = pluginConfig.Parent?.Parent;

                if (xivLauncher == null) {
                    _chatGui.PrintError("[Skippy] Error: Could not locate XIVLauncher directory."); 
                    return;
                }

                var log = Path.Combine(xivLauncher.FullName, "dalamud.log");
                if (!File.Exists(log)) {
                    _chatGui.PrintError("[Skippy] Error: dalamud.log not found."); 
                    return;
                }

                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var file = $"Skippy_TroubleshootingLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var path = Path.Combine(desktop, file);
                
                File.Copy(log, path, true);
                _chatGui.Print($"[Skippy] Log successfully exported to your Desktop: {file}");
            } catch (Exception e) {
                _pluginLog.Error(e, "Failed to export dalamud.log");
                _chatGui.PrintError("[Skippy] An error occurred while exporting the log. Please check PluginLog for details (Tip: Use '/xldev').");
            }
        }

        private void OnCfPop(ContentFinderCondition condition) {
            if (!_config.AutoEnable4Man) {
                return;
            }

            bool isDirectMSQ = condition.RowId == 15 || condition.RowId == 16 || condition.RowId == 830;
            bool isMSQRoulette = condition.RowId == 0;

            if (!isDirectMSQ && !isMSQRoulette) {
                return;
            }

            if (isDirectMSQ && condition.AllowUndersized) {
                // Unrestricted Mode - always enable regardless of party size
            } else if (_partyList.Length == 4) {
                // Premade party of 4
            } else {
                _chatGui.Print("[Skippy] Auto-Party: Queue popped without a premade party of 4 — cutscenes will not be skipped on this run.");
                return;
            }

            if (!_config.SkipMSQRoulette) {
                _config.SkipMSQRoulette = true;
                _autoPartyActive = true;
                Hooks.RefreshHooks();
                _pluginInterface.SavePluginConfig(_config);
            }
        }

        private void OnTerritoryChanged(uint territory) {
            Hooks.OnTerritoryChanged(territory);

            if (!_autoPartyActive) {
                return;
            }

            bool inMSQ = System.Array.IndexOf(SigHooks.TerritoryPrae, (ushort)territory) >= 0 || System.Array.IndexOf(SigHooks.TerritoryCastrum, (ushort)territory) >= 0 || System.Array.IndexOf(SigHooks.TerritoryPorta, (ushort)territory) >= 0;

            if (inMSQ) {
                _chatGui.Print("[Skippy] Auto-Party: Entered MSQ Instance with a Premade Light Party - MSQ Roulette Skip will be active.");
            } else {
                _autoPartyActive = false;
                _config.SkipMSQRoulette = false;
                Hooks.RefreshHooks();
                _pluginInterface.SavePluginConfig(_config);
                _chatGui.Print("[Skippy] Auto-Party: Left MSQ Instance - MSQ Roulette Skip is back to being disabled.");
            }
        }

        private void OpenConfigUi() => _mainUI.IsOpen = true;

        private void OnCommand(string command, string arguments) {
            if (command.ToLower() == "/skippy") {
                TogglePlugin(arguments.Trim().ToLower()); 
                return;
            }
        }

        private void TogglePlugin(string args) {
            switch (args) {
                case "on": case "start": case "enable":
                    SetPluginState(true);
                    _chatGui.Print("[Skippy] Plugin has been enabled.");
                    break;
                
                case "off": case "stop": case "disable":
                    SetPluginState(false);
                    _chatGui.Print("[Skippy] Plugin has been disabled.");
                    break;
                
                case "log": case "export": case "exportlog":
                    ExportLog();
                    break;
                
                case "territory": case "zone":
                    Hooks.PrintTerritory(_chatGui);
                    break;
                
                default:
                    if (string.IsNullOrEmpty(args)) {
                        _mainUI.IsOpen = true;
                    }
                    break;
            }
        }

        private void TogglePluginState() => SetPluginState(!_config.IsEnabled);

        private void SetPluginState(bool isEnabled) {
            if (_config.IsEnabled == isEnabled) {
                return;
            }
            
            _config.IsEnabled = isEnabled;
            Hooks.RefreshHooks();
            _pluginInterface.SavePluginConfig(_config);
        }
    }

    public class CutsceneAddressResolver {
        public bool Valid => Offset1 != IntPtr.Zero && Offset2 != IntPtr.Zero;
        public IntPtr Offset1 { get; private set; }
        public IntPtr Offset2 { get; private set; }

        public CutsceneAddressResolver(IPluginLog log, ISigScanner sig) => Setup(log, sig);

        private void Setup(IPluginLog log, ISigScanner sig) {
            try {
                Offset1 = sig.ScanText("75 ?? 48 8b 0d ?? ?? ?? ?? ba ?? 00 00 00 48 83 c1 10 e8 ?? ?? ?? ?? 83 78 ?? ?? 74");
                Offset2 = sig.ScanText("74 18 8B D7 48 8D 0D");
                
                var baseAddr = Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64();

                if (Offset1 != IntPtr.Zero) {
                    log.Information("Offset1: [\"ffxiv_dx11.exe\"+{0:X}]", Offset1.ToInt64() - baseAddr);
                }

                if (Offset2 != IntPtr.Zero) {
                    log.Information("Offset2: [\"ffxiv_dx11.exe\"+{0:X}]", Offset2.ToInt64() - baseAddr);
                }
            } catch (Exception e) {
                log.Error(e, "Failed to scan for Cutscene signatures");
            }
        }
    }
}
