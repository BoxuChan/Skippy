using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Plugins.a08381.Skippy
{
    public class Skippy : IDalamudPlugin
    {

        private readonly Config _config;
        private readonly RandomNumberGenerator _csp;

        private readonly decimal _base = uint.MaxValue;

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public Skippy()
        {
            if (Interface.GetPluginConfig() is not Config configuration || configuration.Version == 0)
                configuration = new Config { IsEnabled = true, Version = 1 };

            _config = configuration;

            Address = new CutsceneAddressResolver();

            Address.Setup(SigScanner);

            if (Address.Valid)
            {
                PluginLog.Information("Cutscene Offset Found.");
                if (_config.IsEnabled)
                    SetEnabled(true);
            }
            else
            {
                PluginLog.Error("Cutscene Offset Not Found.");
                PluginLog.Warning("Plugin Disabling...");
                Dispose();
                return;
            }
            _csp = RandomNumberGenerator.Create();

            CommandManager.AddHandler("/sc", new CommandInfo(OnCommand)
            {
                HelpMessage = "/sc: Roll your sanity check dice."
            });
        }

        public void Dispose()
        {
            SetEnabled(false);
            GC.SuppressFinalize(this);
        }

        public string Name => "Skippy";
        
        [PluginService] public static IDalamudPluginInterface Interface { get; private set; }
        
        [PluginService] public static ISigScanner SigScanner { get; private set; }

        [PluginService] public static ICommandManager CommandManager { get; private set; }
        
        [PluginService] public static IChatGui ChatGui { get; private set; }

        [PluginService] public static IPluginLog PluginLog { get; private set; }

        public CutsceneAddressResolver Address { get; }

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
            ChatGui.Print(_config.IsEnabled
                ? $"sancheck: 1d100={rnd + 50}, Failed"
                : $"sancheck: 1d100={rnd}, Passed");
            _config.IsEnabled = !_config.IsEnabled;
            SetEnabled(_config.IsEnabled);
            Interface.SavePluginConfig(_config);
        }
    }

    public class CutsceneAddressResolver : BaseAddressResolver
    {

        public bool Valid => Offset1 != IntPtr.Zero && Offset2 != IntPtr.Zero;

        public IntPtr Offset1 { get; private set; }
        public IntPtr Offset2 { get; private set; }

        protected override void Setup64Bit(ISigScanner sig)
        {
            Offset1 = sig.ScanText("75 ?? 48 8b 0d ?? ?? ?? ?? ba ?? 00 00 00 48 83 c1 10 e8 ?? ?? ?? ?? 83 78 ?? ?? 74");
            Offset2 = sig.ScanText("74 18 8B D7 48 8D 0D");
            Skippy.PluginLog.Information(
                "Offset1: [\"ffxiv_dx11.exe\"+{0}]",
                (Offset1.ToInt64() - Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64()).ToString("X")
                );
            Skippy.PluginLog.Information(
                "Offset2: [\"ffxiv_dx11.exe\"+{0}]",
                (Offset2.ToInt64() - Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64()).ToString("X")
                );
        }

    }
}
