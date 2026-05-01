using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UICustomTalk() {
            SectionHeader("NPC Dialogue Cutscenes");
            
            ImGui.TextWrapped("This section covers cutscenes that may be triggered through NPC interactions. This might include certain quest NPCs, housing, and more.\n" + "This list remains uncertain as I still need to explore what kind of cutscenes are triggered through this signature hook.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool changed = false;
            bool master  = _config.SkipCustomTalk;
            
            FeatureCheckbox("##SkipCustomTalk", "Skip all of the NPC Dialogue Cutscenes", "If you find any sort of cutscene that is skipped by that group, feel free to inform me about it so I can figure out more about how to split the group into exemptions and more.", ref master, out changed);
            
            if (changed) {
                _config.SkipCustomTalk = master; 
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
