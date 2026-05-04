using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIAutoParty() {
            SectionHeader("Auto-Party Mode");
            
            ImGui.TextWrapped("When you queue for an MSQ Roulette duty in a premade party of 4 and the queue pops, the MSQ Roulette Skip will be enabled automatically. Once you leave the instance, it will be disabled again ; that way, you don't have to take any risk of skipping cutscenes without people you trust, or in a group that may not have Skippy and get you reported!");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool changed = false;
            bool autoEnable = _config.AutoEnable4Man;
            
            FeatureCheckbox("##AutoEnable4Man", "Auto-Enable MSQ Roulette Skip in 4-man Party", "When your queue pops for Castrum Meridianum, The Praetorium, Porta Decumana, or MSQ Roulette while in a premade party of 4, the skip will be enabled automatically. It will disable itself again once you leave the instance.", ref autoEnable, out changed);
            
            if (changed) {
                _config.AutoEnable4Man = autoEnable;

                if (autoEnable) {
                    _config.SkipMSQRoulette = false;
                    _config.ExemptPrae = false;
                    _config.ExemptCastrum = false;
                    _config.ExemptPorta = false;
                    
                    Skippy.Instance.Hooks.RefreshHooks();
                    Skippy.Instance.PrintChat("[Skippy] Auto-Party Mode Enabled - The skip will now only activate when queuing into MSQ Roulette duties.");
                }

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
