using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIMSQRoulette() {
            SectionHeader("MSQ Roulette");
            
            ImGui.TextWrapped("This section covers cutscenes that are triggered within the Main Scenario Roulette dungeons & trials. Whether they are played via the roulette or by queuing directly into the instance, the cutscenes will still be skipped if this section is enabled.\n" + "If any party members do not have Skippy installed, or they don't have this section toggled, you will have to wait in the loading area until they finish viewing the cutscene(s).\n\n" + "Side Note: You can enable this option mid-run if you forgot beforehand and it will start skippy cutscenes after you enable it (though it will not skip a cutscene you're currently viewing).");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool autoParty = _config.AutoEnable4Man;
            
            if (autoParty) {
                ImGui.BeginDisabled();
            }

            bool changed = false;
            bool master = _config.SkipMSQRoulette;
            
            FeatureCheckbox("##SkipMSQRoulette", "Skip all of the MSQ Instance Cutscenes", "Skips all cutscenes in Castrum Meridianum, The Praetorium, and Porta Decumana.", ref master, out changed);

            if (changed && !autoParty) {
                _config.SkipMSQRoulette = master;
                _config.ExemptCastrum = false;
                _config.ExemptPrae = false;
                _config.ExemptPorta = false;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }
            
            if (autoParty) {
                ImGui.EndDisabled();
                ImGui.Spacing();
                
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.65f, 0.65f, 0.40f, 1f));
                ImGui.TextWrapped("Auto-Party Mode is currently active - This section so on managed automatically and cannot be edited manually.\n\n" + "Disable Auto-Party Mode in the Auto-Party Mode category to regain control and be able to pick options here.");
                ImGui.PopStyleColor();
                
                ImGui.Spacing();
                ImGui.Spacing();
            }

            bool skipAll = _config.SkipMSQRoulette;
            SectionHeader(skipAll ? "Exemptions" : "Skip Specific");

            ImGui.TextWrapped(skipAll ? "Any enabled entries in this category will be skipped individually in case you want to only skip specific instances, rather than all of them by using the toggle in the above section." : "Any ticked entries in this category will be exempt from the global skip above and cutscenes will play as normal in those instances.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            if (autoParty) {
                ImGui.BeginDisabled();
            }

            bool cutscene1 = false; 
            bool exemptCast = _config.ExemptCastrum;
            string castLabel = skipAll ? "Exempt: Castrum Meridianum" : "Skip: Castrum Meridianum";
            string castTip = skipAll ? "Cutscenes will play normally inside Castrum Meridianum." : "Cutscenes will be skipped while inside Castrum Meridianum.";
            FeatureCheckbox("##ExemptCast", castLabel, castTip, ref exemptCast, out cutscene1);
            
            if (cutscene1 && !autoParty) {
                _config.ExemptCastrum = exemptCast;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }
            
            bool cutscene2 = false; 
            bool exemptPrae = _config.ExemptPrae;
            string praeLabel = skipAll ? "Exempt: The Praetorium" : "Skip: The Praetorium";
            string praeTip = skipAll ? "Cutscenes will play normally inside The Praetorium." : "Cutscenes will be skipped while inside The Praetorium.";
            FeatureCheckbox("##ExemptPrae", praeLabel, praeTip, ref exemptPrae, out cutscene2);
            
            if (cutscene2 && !autoParty) {
                _config.ExemptPrae = exemptPrae;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            bool cutscene3 = false; 
            bool exemptPorta = _config.ExemptPorta;
            string portaLabel = skipAll ? "Exempt: Porta Decumana" : "Skip: Porta Decumana";
            string portaTip = skipAll ? "Cutscenes will play normally inside Porta Decumana." : "Cutscenes will be skipped while inside Porta Decumana.";
            FeatureCheckbox("##ExemptPorta", portaLabel, portaTip, ref exemptPorta, out cutscene3);
            
            if (cutscene3 && !autoParty) {
                _config.ExemptPorta = exemptPorta;
                
                Skippy.Instance.Hooks.RefreshHooks();
                _saveConfig();
            }

            if (autoParty) {
                ImGui.EndDisabled();
            }
        }
    }
}
