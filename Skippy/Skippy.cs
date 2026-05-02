using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Skippy.Skips;
using Skippy.UI;

namespace Skippy {
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class Skippy : IDalamudPlugin {
        internal static Skippy Instance { get; private set; } = null!;

        private readonly Config _config;
        private readonly RandomNumberGenerator _csp;
        private readonly decimal _base = uint.MaxValue;
        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly ICommandManager _commandManager;
        private readonly IChatGui _chatGui;
        private readonly IPluginLog _pluginLog;
        private readonly IPartyList _partyList;
        private readonly IFramework _framework;
        
        private int _lastPartySize = -1;

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
            IPartyList partyList,
            IFramework framework) {
            Instance = this;

            _pluginInterface = pluginInterface;
            _commandManager = commandManager;
            _chatGui = chatGui;
            _pluginLog = pluginLog;
            _partyList = partyList;
            _framework = framework;

            _csp = RandomNumberGenerator.Create();

            if (_pluginInterface.GetPluginConfig() is not Config configuration || configuration.Version < 3) {
                configuration = new Config { Version = 3 };
            }

            _config = configuration;
            Address = new CutsceneAddressResolver(_pluginLog, sigScanner);
            Hooks = new SigHooks(_config, _pluginLog, gameInteropProvider, clientState, dataManager, Address);

            _IPC = new IPC.IPC(_pluginInterface, _config);
            _mainUI = new Menu(_config, Hooks.SetEnabled, SaveConfig, _pluginInterface, textureProvider);
            _windowSystem.AddWindow(_mainUI);

            _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;
            _pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
            _pluginInterface.UiBuilder.OpenMainUi += OpenConfigUi;
            
            clientState.TerritoryChanged += Hooks.OnTerritoryChanged;
            
            _framework.Update += OnFrameworkUpdate;

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
            _commandManager.AddHandler("/sc", new CommandInfo(OnCommand) {
                HelpMessage = "/sc: Roll your sanity check dice."
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
            _framework.Update -= OnFrameworkUpdate;

            _commandManager.RemoveHandler("/skippy");
            _commandManager.RemoveHandler("/sc");
            
            _csp?.Dispose();
            GC.SuppressFinalize(this);
        }

        internal void SaveConfig() => _pluginInterface.SavePluginConfig(_config);

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

        private void OnFrameworkUpdate(IFramework framework) {
            if (!_config.AutoEnable4Man) {
                return;
            }

            var size = _partyList.Length;
            
            if (size == _lastPartySize) {
                return;
            }
            
            _lastPartySize = size;

            if (size == 4 && !_config.SkipMSQRoulette) {
                _config.SkipMSQRoulette = true;
                Hooks.RefreshHooks();
                _pluginInterface.SavePluginConfig(_config);
                _chatGui.Print("[Skippy] Auto-Party: 4-man Stack Detected — MSQ Roulette Skip has been enabled.");
            } else if (size != 4 && _config.SkipMSQRoulette) {
                _config.SkipMSQRoulette = false;
                Hooks.RefreshHooks();
                _pluginInterface.SavePluginConfig(_config);
                _chatGui.Print("[Skippy] Auto-Party: Party no longer has 4 players — MSQ Roulette Skip has been disabled.");
            }
        }

        private void OpenConfigUi() => _mainUI.IsOpen = true;

        private void OnCommand(string command, string arguments) {
            if (command.ToLower() == "/sc") {
                SanityCheck(); 
                return;
            }

            if (command.ToLower() == "/skippy") {
                TogglePlugin(arguments.Trim().ToLower()); 
                return;
            }
        }

        private void SanityCheck() {
            byte[] rndSeries = new byte[4];
            _csp.GetBytes(rndSeries);
            int rnd = (int)Math.Abs(BitConverter.ToUInt32(rndSeries, 0) / _base * 50 + 1);
            
            _chatGui.Print(_config.IsEnabled ? $"sancheck: 1d100={rnd + 50}, Failed" : $"sancheck: 1d100={rnd}, Passed");
            TogglePluginState();
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
