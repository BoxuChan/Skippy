using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Dalamud;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Skippy
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class Skippy : IDalamudPlugin
    {
        private readonly Config _config;
        private readonly RandomNumberGenerator _csp;
        private readonly decimal _base = uint.MaxValue;
        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly ISigScanner _sigScanner;
        private readonly ICommandManager _commandManager;
        private readonly IChatGui _chatGui;
        private readonly IPluginLog _pluginLog;

        private readonly IGameInteropProvider _gameInteropProvider;
        private Hook<UIState.Delegates.IsCutsceneSeen>? _cutsceneSeenHook;
        
        public CutsceneAddressResolver Address { get; }

        public Skippy(
            IDalamudPluginInterface pluginInterface,
            ISigScanner sigScanner,
            ICommandManager commandManager,
            IChatGui chatGui,
            IPluginLog pluginLog,
            IGameInteropProvider gameInteropProvider)
        {
            _pluginInterface = pluginInterface;
            _sigScanner = sigScanner;
            _commandManager = commandManager;
            _chatGui = chatGui;
            _pluginLog = pluginLog;
            _gameInteropProvider = gameInteropProvider;

            _csp = RandomNumberGenerator.Create();

            if (_pluginInterface.GetPluginConfig() is not Config configuration || configuration.Version == 0)
                configuration = new Config { IsEnabled = true, Version = 1 };

            _config = configuration;
            Address = new CutsceneAddressResolver(_pluginLog, _sigScanner);

            if (Address.Valid)
            {
                _pluginLog.Information("Cutscene Offset Found.");
                if (_config.IsEnabled) {
                    SetEnabled(true);
                    _chatGui.Print("[Skippy] Plugin has been properly loaded in!");
                    _chatGui.Print("[Skippy] Plugin has been enabled.");
                } else {
                    _chatGui.Print("[Skippy] Plugin has been properly loaded in!");
                    _chatGui.Print("[Skippy] Plugin is currently disabled. (Tip: Use '/skippy on')");
                }
            }
            else
            {
                _pluginLog.Error("Cutscene Offset Not Found.");
                _pluginLog.Warning("Plugin Disabling...");
                _chatGui.Print("[Skippy] Plugin has not loaded in properly!");
                _chatGui.PrintError("[Skippy] Cutscene offsets have not been found! The plugin will not work. Please use '/skippy log' and send the file to the developer.");
                _chatGui.Print("[Skippy] Plugin has been disabled.");
                return;
            }

            _commandManager.AddHandler("/skippy", new CommandInfo(OnCommand)
            {
                HelpMessage = "/skippy [on/off/log]: Toggle the plugin state or export debug logs to send to the developer." 
            });

            _commandManager.AddHandler("/sc", new CommandInfo(OnCommand)
            {
                HelpMessage = "/sc: Roll your sanity check dice."
            });
        }

        public void Dispose()
        {
            SetEnabled(false);
            _commandManager.RemoveHandler("/skippy");
            _commandManager.RemoveHandler("/sc");
            _csp?.Dispose();
            GC.SuppressFinalize(this);
        }

        public unsafe void SetEnabled(bool isEnable)
        {
            if (!Address.Valid) return;

            if (isEnable)
            {
                SafeMemory.Write<short>(Address.Offset1, -28528);
                SafeMemory.Write<short>(Address.Offset2, -28528);
                
                if (_cutsceneSeenHook == null)
                {
                    _cutsceneSeenHook = _gameInteropProvider.HookFromAddress<UIState.Delegates.IsCutsceneSeen>(
                        UIState.MemberFunctionPointers.IsCutsceneSeen, 
                        CutsceneSeenDetour);
                    _cutsceneSeenHook.Enable();
                }
            }
            else
            {
                SafeMemory.Write<short>(Address.Offset1, 14709);
                SafeMemory.Write<short>(Address.Offset2, 6260);
                
                _cutsceneSeenHook?.Dispose();
                _cutsceneSeenHook = null;
            }
        }
        
        private unsafe bool CutsceneSeenDetour(UIState* thisPtr, uint cutsceneId)
        {
            bool result = _cutsceneSeenHook!.Original(thisPtr, cutsceneId);
            _pluginLog.Verbose($"Checking cutscene {cutsceneId}, seen: {result}");
            
            return true;
        }

        private void OnCommand(string command, string arguments)
        {
            if (command.ToLower() == "/sc")
            {
                SanityCheck();
                return;
            }

            if (command.ToLower() == "/skippy")
            {
                TogglePlugin(arguments.Trim().ToLower());
                return;
            }
        }

        private void SanityCheck()
        {
            byte[] rndSeries = new byte[4];
            _csp.GetBytes(rndSeries);
            int rnd = (int)Math.Abs(BitConverter.ToUInt32(rndSeries, 0) / _base * 50 + 1);

            _chatGui.Print(_config.IsEnabled
                ? $"sancheck: 1d100={rnd + 50}, Failed"
                : $"sancheck: 1d100={rnd}, Passed");

            TogglePluginState();
        }

        private void TogglePlugin(string args)
        {
            switch (args)
            {
                case "on":
                case "start":
                case "enable":
                    SetPluginState(true);

                    _chatGui.Print("[Skippy] Plugin has been enabled.");
                    _chatGui.Print("[Skippy] Cutscenes in MSQ Roulette Dungeons/Trial will now be skipped!");
                    break;

                case "off":
                case "stop":
                case "disable":
                    SetPluginState(false);

                    _chatGui.Print("[Skippy] Plugin has been disabled.");
                    _chatGui.Print("[Skippy] Cutscenes in MSQ Roulette Dungeons/Trial will no longer be skipped!");
                    break;

                case "log":
                case "export":
                case "exportlog":
                    ExportLog();
                    break;
                
                default:
                    TogglePluginState();

                    string status = _config.IsEnabled ? "enabled" : "disabled";
                    _chatGui.Print($"[Skippy] Plugin has been {status}. (Tip: Use '/skippy on' or '/skippy off')");
                    break;
            }
        }

        private void TogglePluginState()
        {
            SetPluginState(!_config.IsEnabled);
        }

        private void SetPluginState(bool isEnabled)
        {
            if (_config.IsEnabled == isEnabled) return;

            _config.IsEnabled = isEnabled;
            SetEnabled(_config.IsEnabled);

            _pluginInterface.SavePluginConfig(_config);
        }
        
        private void ExportLog()
        {
            try
            {
                var pluginConfig = _pluginInterface.ConfigDirectory;
                var xivLauncher = pluginConfig.Parent?.Parent;
                
                if (xivLauncher == null)
                {
                    _chatGui.PrintError("[Skippy] Error: Could not locate XIVLauncher directory.");
                    return;
                }

                var log = Path.Combine(xivLauncher.FullName, "dalamud.log");
                
                if (!File.Exists(log))
                {
                    _chatGui.PrintError("[Skippy] Error: dalamud.log not found.");
                    return;
                }
                
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var file = $"Skippy_TroubleshootingLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var path = Path.Combine(desktop, file);
                
                File.Copy(log, path, true);
                
                _chatGui.Print($"[Skippy] Log successfully exported to your Desktop: {file}");
            }
            catch (Exception e)
            {
                _pluginLog.Error(e, "Failed to export dalamud.log");
                _chatGui.PrintError("[Skippy] An error occurred while exporting the log. Please check PluginLog for details (Tip: Use '/xldev').");
            }
        }
    }

    public class CutsceneAddressResolver
    {
        public bool Valid => Offset1 != IntPtr.Zero && Offset2 != IntPtr.Zero;
        public IntPtr Offset1 { get; private set; }
        public IntPtr Offset2 { get; private set; }

        public CutsceneAddressResolver(IPluginLog log, ISigScanner sig)
        {
            Setup(log, sig);
        }

        private void Setup(IPluginLog log, ISigScanner sig)
        {
            try 
            {
                Offset1 = sig.ScanText("75 ?? 48 8b 0d ?? ?? ?? ?? ba ?? 00 00 00 48 83 c1 10 e8 ?? ?? ?? ?? 83 78 ?? ?? 74");
                Offset2 = sig.ScanText("74 18 8B D7 48 8D 0D");

                var baseAddr = Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64();

                if (Offset1 != IntPtr.Zero)
                    log.Information("Offset1: [\"ffxiv_dx11.exe\"+{0:X}]", Offset1.ToInt64() - baseAddr);
                
                if (Offset2 != IntPtr.Zero)
                    log.Information("Offset2: [\"ffxiv_dx11.exe\"+{0:X}]", Offset2.ToInt64() - baseAddr);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to scan for Cutscene signatures");
            }
        }
    }
}