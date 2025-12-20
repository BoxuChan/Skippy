using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Plugins.a08381.Skippy
{
    public class Skippy : IDalamudPlugin
    {
        public string Name => "Skippy";

        private readonly Config _config;
        private readonly RandomNumberGenerator _csp;
        private readonly decimal _base = uint.MaxValue;
        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly ISigScanner _sigScanner;
        private readonly ICommandManager _commandManager;
        private readonly IChatGui _chatGui;
        private readonly IPluginLog _pluginLog;

        public CutsceneAddressResolver Address { get; }

        public Skippy(
            IDalamudPluginInterface pluginInterface,
            ISigScanner sigScanner,
            ICommandManager commandManager,
            IChatGui chatGui,
            IPluginLog pluginLog)
        {
            _pluginInterface = pluginInterface;
            _sigScanner = sigScanner;
            _commandManager = commandManager;
            _chatGui = chatGui;
            _pluginLog = pluginLog;

            _csp = RandomNumberGenerator.Create();

            if (_pluginInterface.GetPluginConfig() is not Config configuration || configuration.Version == 0)
                configuration = new Config { IsEnabled = true, Version = 1 };

            _config = configuration;

            Address = new CutsceneAddressResolver(_pluginLog); 
            Address.Setup(_sigScanner);

            if (Address.Valid)
            {
                _pluginLog.Information("Cutscene Offset Found.");
                if (_config.IsEnabled)
                    SetEnabled(true);
            }
            else
            {
                _pluginLog.Error("Cutscene Offset Not Found.");
                _pluginLog.Warning("Plugin Disabling...");
                return;
            }

            _commandManager.AddHandler("/sc", new CommandInfo(OnCommand)
            {
                HelpMessage = "/sc: Roll your sanity check dice."
            });
        }

        public void Dispose()
        {
            SetEnabled(false);
            _commandManager.RemoveHandler("/sc");
            GC.SuppressFinalize(this);
        }

        public void SetEnabled(bool isEnable)
        {
            if (!Address.Valid) return;
            
            if (isEnable)
            {
                SafeMemory.Write<short>(Address.Offset1, -28528);
                SafeMemory.Write<short>(Address.Offset2, -28528);
            }
            else
            {
                SafeMemory.Write<short>(Address.Offset1, 14709);
                SafeMemory.Write<short>(Address.Offset2, 6260);
            }
        }

        private void OnCommand(string command, string arguments)
        {
            if (command.ToLower() != "/sc") return;
            byte[] rndSeries = new byte[4];
            _csp.GetBytes(rndSeries);
            int rnd = (int)Math.Abs(BitConverter.ToUInt32(rndSeries, 0) / _base * 50 + 1);
            
            _chatGui.Print(_config.IsEnabled
                ? $"sancheck: 1d100={rnd + 50}, Failed"
                : $"sancheck: 1d100={rnd}, Passed");
            
            _config.IsEnabled = !_config.IsEnabled;
            SetEnabled(_config.IsEnabled);
            _pluginInterface.SavePluginConfig(_config);
        }
    }

    public class CutsceneAddressResolver : BaseAddressResolver
    {
        private readonly IPluginLog _log;

        public CutsceneAddressResolver(IPluginLog log)
        {
            _log = log;
        }

        public bool Valid => Offset1 != IntPtr.Zero && Offset2 != IntPtr.Zero;

        public IntPtr Offset1 { get; private set; }
        public IntPtr Offset2 { get; private set; }

        protected override void Setup64Bit(ISigScanner sig)
        {
            Offset1 = sig.ScanText("75 ?? 48 8b 0d ?? ?? ?? ?? ba ?? 00 00 00 48 83 c1 10 e8 ?? ?? ?? ?? 83 78 ?? ?? 74");
            Offset2 = sig.ScanText("74 18 8B D7 48 8D 0D");

            var baseAddr = Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64();

            if (Offset1 != IntPtr.Zero)
                _log.Information("Offset1: [\"ffxiv_dx11.exe\"+{0:X}]", Offset1.ToInt64() - baseAddr);
            
            if (Offset2 != IntPtr.Zero)
                _log.Information("Offset2: [\"ffxiv_dx11.exe\"+{0:X}]", Offset2.ToInt64() - baseAddr);
        }
    }
}