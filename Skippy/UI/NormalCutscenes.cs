using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UINormalCutscenes() {
            SectionHeader("World & Quest Cutscenes");
            
            ImGui.TextWrapped("This section covers cutscenes that may usually be triggered during overworld story scenes, side quest cutscenes, and various scripted events.\n" + "This list remains uncertain as I still need to explore what kind of cutscenes are triggered through this signature hook.");
            
            ImGui.Spacing();
            ImGui.Spacing();

            bool changed = false;
            bool master = _config.SkipNormalCutscenes;
            
            FeatureCheckbox("##SkipNormalCutscenes", "Skip all of the World & Quest Cutscenes", "If you find any sort of cutscene that is skipped by that group, feel free to inform me about it so I can figure out more about how to split the group into exemptions and more.", ref master, out changed);

            if (changed) {
                _config.SkipNormalCutscenes = master;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            bool skipAll = _config.SkipNormalCutscenes;
            SectionHeader(skipAll ? "[WIP] Exemptions" : "[WIP] Skip Specific");
            
            ImGui.TextWrapped("As I have yet to figure out the details and contents of this signature hook, no " + (skipAll ? "exemptions" : "specific skips") + " are planned just yet.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();
            
            WipToggle(skipAll ? "Exempt: (Unknown)" : "Skip: (Unknown)");
        }
    }
}
