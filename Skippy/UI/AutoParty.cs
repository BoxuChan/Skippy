using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIAutoParty() {
            SectionHeader("Auto-Party Mode");
            
            ImGui.TextWrapped("When you queue for an MSQ Roulette duty in a premade party of 4 and the queue pops, the MSQ Roulette Skip will be enabled automatically. Once you leave the instance, it will be disabled again — that way, you don't have to take any risk of skipping cutscenes without people you trust!");
            
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
                } else {
                    _config.CheckPartySkippy = false;
                    Skippy.Instance.PrintChat("[Skippy] Auto-Party Mode Disabled - The skip will no longer activate when queueing, check MSQ Roulette instead.");
                }

                Skippy.Instance.SaveConfig();
            }

            SectionHeader("Check Party Members for Skippy");

            ImGui.TextWrapped("When enabled, Skippy will verify that ALL members of your party also have Skippy installed before enabling the skip.\n\n" + "How it works: when the queue pops, each player with Skippy posts an anonymous request. " + "After a short wait, Skippy checks whether every member of the party posted one or not. " + "If they all did, it means everyone has Skippy and it is safe to skip the cutscenes in the instance they are queueing for. " + "If even one player is missing, the skip will NOT be enabled for this run.");

            ImGui.Spacing();
            ImGui.Spacing();

            bool checkChanged = false;
            bool checkParty = _config.CheckPartySkippy;

            if (!_config.AutoEnable4Man) ImGui.BeginDisabled();
            
            FeatureCheckbox("##CheckPartySkippy", "Check Party Members for Skippy", "Only enable the skip when every party member has Skippy installed. Requires Auto-Party Mode to function.", ref checkParty, out checkChanged);
            
            if (!_config.AutoEnable4Man) ImGui.EndDisabled();

            if (!_config.AutoEnable4Man) {
                ImGui.Spacing();

                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.65f, 0.65f, 0.40f, 1f));
                ImGui.TextWrapped("This feature requires Auto-Party Mode to be enabled in order to function. Enable it in the section above first.");
                ImGui.PopStyleColor();

                ImGui.Spacing();
            }

            if (checkChanged && _config.AutoEnable4Man) {
                _config.CheckPartySkippy = checkParty;
                Skippy.Instance.SaveConfig();

                if (checkParty) {
                    Skippy.Instance.PrintChat("[Skippy] Party Plugin Check Enabled — The skip will only activate when all party members have Skippy.");
                } else {
                    Skippy.Instance.PrintChat("[Skippy] Party Plugin Check Disabled — The skip will activate for any premade party of 4.");
                }
            }
        }
    }
}