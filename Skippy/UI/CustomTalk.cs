using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UICustomTalk() {
            SectionHeader("NPC Dialogue Cutscenes");
            
            ImGui.TextWrapped("This section covers cutscenes that may be triggered through NPC interactions. This might include certain quest NPCs, housing, and more.\n" + "This list remains uncertain as I still need to explore what kind of cutscenes are triggered through this signature hook.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool changed = false;
            bool master = _config.SkipCustomTalk;
            
            FeatureCheckbox("##SkipCustomTalk", "Skip all of the NPC Dialogue Cutscenes", "If you find any sort of cutscene that is skipped by that group, feel free to inform me about it so I can figure out more about how to split the group into exemptions and more.", ref master, out changed);
            
            if (changed) {
                _config.SkipCustomTalk = master;
                _config.ExemptSubmarines = false;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            bool skipAll = _config.SkipCustomTalk;
            SectionHeader(skipAll ? "[WIP] Exemptions" : "[WIP] Skip Specific");

            ImGui.TextWrapped(skipAll ? "Any ticked entries in this category will be exempt from the global skip above — cutscenes will so on play normally in those situations.\n\n" + "As I have yet to figure out the details and contents of this signature hook, no exemptions or further toggles are planned just yet." : "Any ticked entries in this category will be skipped individually, in case you want to only skip specific parts of this category and not all of them. Should you decide to skip everything, click the checkbox above.\n\n" + "As I have yet to figure out the details and contents of this signature hook, no exemptions or further toggles are planned just yet.");
            
            ImGui.Spacing();
            ImGui.Spacing();

            bool exemptSubs = _config.ExemptSubmarines;
            string subsLabel = skipAll ? "Exempt: Submarine & Workshop" : "Skip: Submarine & Workshop";
            string subsTip = skipAll ? "Cutscenes will play normally for Submarine & Workshop related contents." : "Cutscenes will be skipped for Submarine & Workshop related contents.";
            FeatureCheckbox("##ExemptSubmarines", subsLabel, subsTip, ref exemptSubs, out bool subsChanged);
            
            if (subsChanged) {
                _config.ExemptSubmarines = exemptSubs;
                
                Skippy.Instance.Hooks.RefreshHooks();
                _saveConfig();
            }
        }
    }
}
