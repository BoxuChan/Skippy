using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UINormalCutscenes() {
            SectionHeader("World & Quest Cutscenes");
            
            ImGui.TextWrapped("This section covers cutscenes that are typically triggered during overworld story scenes, side quest cutscenes, and various scripted events.\n" + "This list is still quite uncertain as I still need to figure out all the cutscenes that are skipped via this signature hook.");
            
            ImGui.Spacing();
            ImGui.Spacing();

            bool changed = false;
            bool master = _config.SkipNormalCutscenes;
            
            FeatureCheckbox("##SkipNormalCutscenes", "Skip all of the World & Quest Cutscenes", "If you find any cutscene that is skipped by this group, feel free to let me know about it so I can add better ways to split the group into exemptions and more.", ref master, out changed);

            if (changed) {
                _config.SkipNormalCutscenes = master;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            bool skipAll = _config.SkipNormalCutscenes;
            SectionHeader(skipAll ? "[WIP] Exemptions" : "[WIP] Skip Specific");
            
            ImGui.TextWrapped("As I have yet to figure out the details and contents of this hook, no  " + (skipAll ? "exemptions" : "specific skips") + " or further toggles have been finalised just yet");
            
            ImGui.Spacing(); 
            ImGui.Spacing();
            
            WipToggle(skipAll ? "Exempt: (Unknown)" : "Skip: (Unknown)");
        }
    }
}
