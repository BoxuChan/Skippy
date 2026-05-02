using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIAutoParty() {
            SectionHeader("Auto-Party Mode");
            
            ImGui.TextWrapped("When your party reaches 4 players, the MSQ Roulette Skip will be automatically enabled, if going below that player count, it will be disabled automatically so you don't take the risk of skipping cutscenes in a party without your friends, or a group that may not have Skippy and get you reported!");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool changed = false;
            bool autoEnable = _config.AutoEnable4Man;
            
            FeatureCheckbox("##AutoEnable4Man", "Auto-Enable MSQ Roulette Skip in 4-man Party", "You must be in a party of 4 for this to work, if not, the MSQ Roulette Skip will always be disabled.", ref autoEnable, out changed);
            
            if (changed) {
                _config.AutoEnable4Man = autoEnable;
                
                Skippy.Instance.SaveConfig();
            }

            SectionHeader("[WIP] Exemptions");

            ImGui.TextWrapped("In theory, I would like to make it so that you're able to \"sync\" with other players in your party.\n\n" + "If all of them have Skippy and their MSQ Roulette Skip enabled, then it would turn your Skippy on, automatically, or, would turn everyone's Skippy & MSQ Roulette Skip on through an additional mode perhaps.\n" + "However, this would require communicating across game clients, which is not possible just with an IPC check and would require a server-side relay.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();
            
            WipToggle("Check Party Members for Skippy");
        }
    }
}
