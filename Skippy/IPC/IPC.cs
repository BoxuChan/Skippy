using System.Collections.Generic;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Skippy.IPC {
	internal sealed class IPC {
		private readonly ICallGateProvider<bool> _isEnabled;
		private readonly ICallGateProvider<SkippedCategory[]> _getCategories;
		private readonly ICallGateProvider<Dictionary<string,bool>> _getConfig;

		private readonly Config _config;

		internal IPC(IDalamudPluginInterface pluginInterface, Config config) {
			_config = config;

			_isEnabled = pluginInterface.GetIpcProvider<bool>("Skippy.IsEnabled");
			_isEnabled.RegisterFunc(IsEnabled);

			_getCategories = pluginInterface.GetIpcProvider<SkippedCategory[]>("Skippy.GetSkippedCategories");
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

		private SkippedCategory[] GetSkippedCategories() {
			var list = new List<SkippedCategory>();

			if (_config.IsEnabled) {
				list.Add(SkippedCategory.IsEnabled);
			}

			if (_config.AutoEnable4Man) {
				list.Add(SkippedCategory.AutoEnable4Man);
			}

			if (_config.SkipMSQRoulette) {
				list.Add(SkippedCategory.SkipMSQRoulette);
			}

			if (_config.ExemptPrae) {
				list.Add(SkippedCategory.ExemptPrae);
			}

			if (_config.ExemptCastrum) {
				list.Add(SkippedCategory.ExemptCastrum);
			}

			if (_config.ExemptPorta) {
				list.Add(SkippedCategory.ExemptPorta);
			}

			if (_config.SkipMassivePC) {
				list.Add(SkippedCategory.SkipMassivePC);
			}

			if (_config.SkipCosmicExploration) {
				list.Add(SkippedCategory.SkipCosmicExploration);
			}

			if (_config.SkipGoldSaucer) {
				list.Add(SkippedCategory.SkipGoldSaucer);
			}

			if (_config.ExemptChocoboRace) {
				list.Add(SkippedCategory.ExemptChocoboRacing);
			}

			if (_config.ExemptVerminion) {
				list.Add(SkippedCategory.ExemptVerminion);
			}

			if (_config.ExemptTripleTriad) {
				list.Add(SkippedCategory.ExemptTripleTriad);
			}

			if (_config.ExemptFallGuys) {
				list.Add(SkippedCategory.ExemptFallGuys);
			}

			if (_config.ExemptAirForceOne) {
				list.Add(SkippedCategory.ExemptAirForceOne);
			}

			if (_config.ExemptMahjong) {
				list.Add(SkippedCategory.ExemptMahjong);
			}

			if (_config.SkipCustomTalk) {
				list.Add(SkippedCategory.SkipCustomTalk);
			}

			if (_config.SkipNormalCutscenes) {
				list.Add(SkippedCategory.SkipNormalCutscenes);
			}

			if (_config.SkipFeedBuddy) {
				list.Add(SkippedCategory.SkipFeedBuddy);
			}

			if (_config.SkipOceanFishing) {
				list.Add(SkippedCategory.SkipOceanFishing);
			}

			if (_config.SkipCrystallineConflict) {
				list.Add(SkippedCategory.SkipCrystallineConflict);
			}

			if (_config.SkipInn) {
				list.Add(SkippedCategory.SkipInn);
			}

			if (_config.AllowCutsceneSeenGlobally) {
				list.Add(SkippedCategory.AllowCutsceneSeenGlobally);
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
			["SkipCosmicExploration"] = _config.SkipCosmicExploration,
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
			["AllowCutsceneSeenGlobally"] = _config.AllowCutsceneSeenGlobally
		};
	}
}