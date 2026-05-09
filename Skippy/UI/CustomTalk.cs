using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UICustomTalk() {
            SectionHeader("NPC Dialogue Cutscenes");
            
            ImGui.TextWrapped("This section covers cutscenes that are typically triggered via NPC interactions. This includes certain quest NPCs, housing and more.\n" + "This list is still quite uncertain as I still need to figure out all the cutscenes that are skipped via this signature hook.\n");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool changed = false;
            bool master = _config.SkipCustomTalk;
            
            FeatureCheckbox("##SkipCustomTalk", "Skip all of the NPC Dialogue Cutscenes", "If you find any cutscene that is skipped by this group, feel free to let me know about it so I can add better ways to split the group into exemptions and more.", ref master, out changed);
            
            if (changed) {
                _config.SkipCustomTalk = master;
                _config.ExemptSubmarines = false;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            bool skipAll = _config.SkipCustomTalk;
            SectionHeader(skipAll ? "[WIP] Exemptions" : "[WIP] Skip Specific");

            ImGui.TextWrapped(skipAll ? "Any enabled entries in this category will be skipped individually in case you want to only skip specific elements, rather than all of them by using the toggle in the above section. \n\n" + "As I have yet to figure out the details and contents of this hook, no exemptions or further toggles have been finalised just yet." : "Any ticked entries in this category will be skipped individually, in case you want to only skip specific parts of this category and not all of them. Should you decide to skip everything, click the checkbox above.\n\n" + "As I have yet to figure out the details and contents of this signature hook, no exemptions or further toggles are planned just yet.");
            
            ImGui.Spacing();
            ImGui.Spacing();

            bool exemptSubs = _config.ExemptSubmarines;
            string subsLabel = skipAll ? "Exempt: Submarine & Workshop" : "Skip: Submarine & Workshop";
            string subsTip = skipAll ? "Cutscenes will play normally for Submarine & Workshop related content." : "Cutscenes will be skipped for Submarine & Workshop related content.";
            FeatureCheckbox("##ExemptSubmarines", subsLabel, subsTip, ref exemptSubs, out bool subsChanged);
            
            if (subsChanged) {
                _config.ExemptSubmarines = exemptSubs;
                
                Skippy.Instance.Hooks.RefreshHooks();
                _saveConfig();
            }

            WipToggle(skipAll ? "Exempt: Grand Company Rankup" : "Skip: Grand Company Rankup");
        }
    }
}
