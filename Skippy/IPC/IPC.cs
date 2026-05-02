using System.Collections.Generic;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Skippy.IPC {
    internal sealed class IPC {
        private readonly ICallGateProvider<bool> _isEnabled;
        private readonly ICallGateProvider<string[]> _getCategories;
        private readonly ICallGateProvider<Dictionary<string,bool>> _getConfig;

        private readonly Config _config;

        internal IPC(IDalamudPluginInterface pluginInterface, Config config) {
            _config = config;

            _isEnabled = pluginInterface.GetIpcProvider<bool>("Skippy.IsEnabled");
            _isEnabled.RegisterFunc(IsEnabled);

            _getCategories = pluginInterface.GetIpcProvider<string[]>("Skippy.GetSkippedCategories");
            _getCategories.RegisterFunc(GetSkippedCategories);

            _getConfig = pluginInterface.GetIpcProvider<Dictionary<string,bool>>("Skippy.GetConfig");
            _getConfig.RegisterFunc(GetConfig);
        }

        internal void Dispose() {
            _isEnabled.UnregisterFunc();
            _getCategories.UnregisterFunc();
            _getConfig.UnregisterFunc();
        }

        private bool IsEnabled() => _config.IsEnabled;

        private string[] GetSkippedCategories() {
            var list = new List<string>();
            
            if (_config.IsEnabled && _config.SkipMSQRoulette) {
                list.Add("MSQ Roulette");
            }

            if (_config.IsEnabled && _config.SkipOceanFishing) {
                list.Add("Ocean Fishing");
            }

            if (_config.IsEnabled && _config.SkipCrystallineConflict) {
                list.Add("Crystalline Conflict");
            }

            if (_config.IsEnabled && _config.SkipMassivePC)
            {
                list.Add("Large-Scale Content");
            }

            if (_config.IsEnabled && _config.SkipGoldSaucer)
            {
                list.Add("Gold Saucer");
            }

            if (_config.IsEnabled && _config.SkipCustomTalk)
            {
                list.Add("NPC Dialogue Cutscenes");
            }

            if (_config.IsEnabled && _config.SkipNormalCutscenes)
            {
                list.Add("World & Quest Cutscenes");
            }

            if (_config.IsEnabled && _config.SkipFeedBuddy)
            {
                list.Add("Feed Buddy Scene");
            }

            if (_config.IsEnabled && _config.SkipInn)
            {
                list.Add("Inn Skip");
            }
            return list.ToArray();
        }

        private Dictionary<string, bool> GetConfig() => new() {
            ["AutoEnable4Man"] = _config.AutoEnable4Man,
            ["IsEnabled"] = _config.IsEnabled,
            ["SkipMSQRoulette"] = _config.SkipMSQRoulette,
            ["ExemptPrae"] = _config.ExemptPrae,
            ["ExemptCastrum"] = _config.ExemptCastrum,
            ["ExemptPorta"] = _config.ExemptPorta,
            ["SkipMassivePC"] = _config.SkipMassivePC,
            ["SkipGoldSaucer"] = _config.SkipGoldSaucer,
            ["ExemptChocoboRacing"] = _config.ExemptChocoboRace,
            ["ExemptVerminion"] = _config.ExemptVerminion,
            ["ExemptTripleTriad"] = _config.ExemptTripleTriad,
            ["ExemptFallGuys"] = _config.ExemptFallGuys,
            ["ExemptAirForceOne"] = _config.ExemptAirForceOne,
            ["ExemptMahjong"] = _config.ExemptMahjong,
            ["SkipCustomTalk"] = _config.SkipCustomTalk,
            ["SkipNormalCutscenes"] = _config.SkipNormalCutscenes,
            ["SkipFeedBuddy"] = _config.SkipFeedBuddy,
            ["SkipOceanFishing"] = _config.SkipOceanFishing,
            ["SkipCrystallineConflict"] = _config.SkipCrystallineConflict,
            ["SkipInn"] = _config.SkipInn,
        };
    }
}
