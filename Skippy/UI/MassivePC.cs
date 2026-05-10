using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIMassivePC() {
            SectionHeader("Large-Scale Content");
            
            ImGui.TextWrapped("This section covers cutscenes that may be triggered inside large-scale content such as Variant Dungeons or Deep Dungeons, or so I believe.\n" + "This list remains uncertain as I still need to explore what kind of cutscenes are triggered through this signature hook.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool changed = false;
            bool master = _config.SkipMassivePC;
            
            FeatureCheckbox("##SkipMassivePC", "Skip all of the Large-Scale Content Cutscenes", "If you find any sort of cutscene that is skipped by that group, feel free to inform me about it so I can figure out more about how to split the group into exemptions and more.", ref master, out changed);

            if (changed) {
                _config.SkipMassivePC = master;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            bool skipAll = _config.SkipMassivePC;
            SectionHeader(skipAll ? "[WIP] Exemptions" : "[WIP] Skip Specific");

            ImGui.TextWrapped(skipAll ? "Any enabled entries in this category will be exempt from the global skip above and cutscenes will play as normal in those instances." : "Any ticked entries in this category will be skipped individually, even if the global skip above is not enabled.");

            ImGui.Spacing(); 
            ImGui.Spacing();

            bool cosmicChanged = false;
            bool cosmicSkip = _config.SkipCosmicExploration;
            string cosmicLabel = skipAll ? "Exempt: Cosmic Exploration" : "Skip: Cosmic Exploration";
            FeatureCheckbox("##SkipCosmicExploration", cosmicLabel, "Skips cutscenes that trigger within Cosmic Exploration maps. (Sinus Ardorum, Phaenna, Oizys)", ref cosmicSkip, out cosmicChanged);

            if (cosmicChanged) {
                _config.SkipCosmicExploration = skipAll ? !cosmicSkip : cosmicSkip;

                Skippy.Instance.Hooks.RefreshHooks();
                _saveConfig();
            }
        }
    }
}