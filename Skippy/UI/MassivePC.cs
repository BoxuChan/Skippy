using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIMassivePC() {
            SectionHeader("Large-Scale Content");
            
            ImGui.TextWrapped("This section covers cutscenes that may be triggered inside large-scale content such as Variant Dungeons or Deep Dungeons, or so I believe.\n" + "This list remains uncertain as I still need to explore what kind of cutscenes are triggered through this signature hook.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool changed = false;
            bool master  = _config.SkipMassivePC;
            
            FeatureCheckbox("##SkipMassivePC", "Skip all of the Large-Scale Content Cutscenes", "If you find any sort of cutscene that is skipped by that group, feel free to inform me about it so I can figure out more about how to split the group into exemptions and more.", ref master, out changed);

            if (changed) {
                _config.SkipMassivePC = master;
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            SectionHeader("[WIP] Exemptions");
            
            ImGui.TextWrapped("As I have yet to figure out the details and contents of this signature hook, no exemptions or further toggles are planned just yet.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();
            
            WipToggle("Exempt: (Unknown)");
        }
    }
}
