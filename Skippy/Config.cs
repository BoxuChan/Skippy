using System;
using Dalamud.Configuration;

namespace Skippy {
    [Serializable]
    public class Config : IPluginConfiguration {
        public int Version { get; set; } = 2;

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

        public bool SkipCustomTalk { get; set; } = false;
        
        public bool SkipNormalCutscenes { get; set; } = false;

        public bool SkipFeedBuddy { get; set; } = false;

        public bool SkipOceanFishing { get; set; } = false;
        public bool SkipCrystallineConflict { get; set; } = false;
        public bool SkipInn { get; set; } = false;

        public bool AutoEnable4Man { get; set; } = false;

        public bool HideWarning { get; set; } = false;
    }
}
