using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIMSQRoulette() {
            SectionHeader("MSQ Roulette");
            
            ImGui.TextWrapped("This section covers cutscenes that are triggered within the Main Scenario Roulette Dungeons & Trials. Whether they are played through the Roulette or by queuing into the instance directly, they will remain skippable.\n" + "If a party member lacks this option or Skippy entirely, you will have to wait in the loading area until they finish viewing the cutscene(s).\n\n" + "Side Note: You can enable this option during the instance if you forgot to prior to, it will start skipping as soon as you turn it on (although after whichever cutscene you might be already in).");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool autoParty = _config.AutoEnable4Man;
            
            if (autoParty) {
                ImGui.BeginDisabled();
            }

            bool changed = false;
            bool master = _config.SkipMSQRoulette;
            
            FeatureCheckbox("##SkipMSQRoulette", "Skip all of the MSQ Roulette Cutscenes", "Skips all cutscenes in MSQ Roulette Dungeons & Trials, whether you queue into the official MSQ Roulette in the Duty Finder, or directly queue the Dungeon/Trial yourself.", ref master, out changed);

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

            ImGui.TextWrapped(skipAll ? "Any ticked entries in this category will be exempt from the global skip above — cutscenes will so on play normally in those instances." : "Any ticked entries in this category will be skipped individually, in case you want to only skip specific parts of this category and not all of them. Should you decide to skip everything, click the checkbox above.");
            
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
