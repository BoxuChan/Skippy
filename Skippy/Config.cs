using System;
using Dalamud.Configuration;

namespace Skippy {
    public enum SkippedCategory {
        IsEnabled,
        AutoEnable4Man,
        SkipMSQRoulette,
        ExemptPrae,
        ExemptCastrum,
        ExemptPorta,
        SkipMassivePC,
        SkipGoldSaucer,
        ExemptChocoboRacing,
        ExemptVerminion,
        ExemptTripleTriad,
        ExemptFallGuys,
        ExemptAirForceOne,
        ExemptMahjong,
        SkipCustomTalk,
        SkipNormalCutscenes,
        SkipFeedBuddy,
        SkipOceanFishing,
        SkipCrystallineConflict,
        SkipInn,
    }

    [Serializable]
    public class Config : IPluginConfiguration {
        public int Version { get; set; } = 4;

        public bool IsEnabled { get; set; } = true;

        public bool SkipMSQRoulette { get; set; } = true;
        public bool ExemptPrae{ get; set; } = false;
        public bool ExemptCastrum { get; set; } = false;
        public bool ExemptPorta { get; set; } = false;

        public bool SkipMassivePC  { get; set; } = false;

        public bool SkipGoldSaucer { get; set; } = false;
        public bool ExemptChocoboRace { get; set; } = false;
        public bool ExemptVerminion { get; set; } = false;
        public bool ExemptTripleTriad { get; set; } = false;
        public bool FallGuysEventActive { get; set; } = false;
        public bool ExemptFallGuys { get; set; } = false;
        public bool ExemptAirForceOne { get; set; } = false;
        public bool ExemptMahjong { get; set; } = false;

        public bool SkipCustomTalk { get; set; } = false;
        public bool ExemptSubmarines { get; set; } = false;
        
        public bool SkipNormalCutscenes { get; set; } = false;

        public bool DevMode { get; set; } = false;
        public string DevPassword { get; set; } = string.Empty;
        public bool ResearchMSQHook { get; set; } = false;
        public bool ResearchMassivePCHook { get; set; } = false;
        public bool ResearchGoldSaucerHook { get; set; } = false;
        public bool ResearchCustomTalkHook { get; set; } = false;
        public bool ResearchNormalCutscenesHook { get; set; } = false;
        public bool ResearchInnHook { get; set; } = false;
        public bool ResearchFeedBuddyHook { get; set; } = false;

        public bool SkipFeedBuddy { get; set; } = false;

        public bool SkipOceanFishing { get; set; } = false;
        public bool SkipCrystallineConflict { get; set; } = false;
        public bool SkipInn { get; set; } = false;

        public bool AutoEnable4Man { get; set; } = false;

        public bool HideWarning { get; set; } = false;
    }
}
