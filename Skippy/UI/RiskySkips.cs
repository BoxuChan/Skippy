using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIRiskySkips() {
            var bannerHeight = ImGui.GetStyle().ItemSpacing.Y * 5f + ImGui.GetTextLineHeight() * 3f + 14f;
            
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.35f, 0.08f, 0.04f, 0.90f));
            ImGui.PushStyleColor(ImGuiCol.Border,  new Vector4(0.80f, 0.20f, 0.10f, 0.90f));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1.5f);

            if (ImGui.BeginChild("##Banner", new Vector2(-1, bannerHeight), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)) {
                ImGui.Spacing();
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.40f, 0.30f, 1f));
                
                var icon = FontAwesomeIcon.ExclamationTriangle.ToIconString();
                float iconWidth;

                using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                    iconWidth = ImGui.CalcTextSize(icon).X;
                }

                var labelWidth = ImGui.CalcTextSize(" Warning").X;
                var avail  = ImGui.GetContentRegionAvail().X;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - iconWidth - labelWidth) * 0.5f);
                
                using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                    ImGui.TextUnformatted(icon);
                }

                ImGui.SameLine(0, 4f);
                ImGui.TextUnformatted(" Warning");
                ImGui.PopStyleColor();
                
                ImGui.Spacing();
                
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.90f, 0.75f, 0.70f, 1f));
                CenteredText("All of the skips underneath are experimental and may carry a ban risk.");
                CenteredText("Please only enable them if you accept full responsibility.");
                ImGui.PopStyleColor();
                
                ImGui.Spacing();
            }
            
            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(2);
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            SectionHeader("Ocean Fishing");
            
            ImGui.TextWrapped("The very first cutscene in Ocean Fishing allows you to start directly when skipped, however the next two will only let you move around the boat, though the timer won't start (so on not letting you fish), until the moment everyone is ready.");
            
            ImGui.Spacing();
            ImGui.Spacing();

            bool cutscene1 = false; 
            bool ocean = _config.SkipOceanFishing;
            
            FeatureCheckbox("##SkipOcean", "Enable Ocean Fishing Skip", "Skips cutscenes during Ocean Fishing. Enable at your own discretion and be aware of the risks.", ref ocean, out cutscene1);
            
            if (cutscene1) {
                _config.SkipOceanFishing = ocean; 
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            SectionHeader("Crystalline Conflict");
            
            ImGui.TextWrapped("When loading into Crystalline Conflict, the camera pans around the entire map then displays each team one by one and shows the player cards of everyone in the lobby, this skip allows you to skip that entirely.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool cutscene2 = false; 
            bool cc = _config.SkipCrystallineConflict;
            
            FeatureCheckbox("##SkipCC", "Enable Crystalline Conflict Skip", "Skips the intro cutscene in Crystalline Conflict. Enable at your own discretion and be aware of the risks.", ref cc, out cutscene2);

            if (cutscene2) {
                _config.SkipCrystallineConflict = cc; 
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            SectionHeader("Inn Skip");
            
            ImGui.TextWrapped("This skip is by far the riskiest the game \"tolerates\", as it completely bypasses the Inn Login Sequence.");
            
            ImGui.Spacing();
            ImGui.Spacing();

            bool cutscene3 = false; 
            bool inn = _config.SkipInn;
            
            FeatureCheckbox("##SkipInn", "Enable Inn Skip", "Bypasses the entire Inn Login Sequence. Enable at your own discretion and be aware of the risks.", ref inn, out cutscene3);

            if (cutscene3) {
                _config.SkipInn = inn; 
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            SectionHeader("[WIP] Aesthetician Skip");
            
            ImGui.TextWrapped("This will someday allow you to skip the cutscene that plays when visiting the Aesthetician for a hair or appearance change.\n" + "It has however not yet been identified, so this feature remains a work in progress.");
            
            ImGui.Spacing();
            ImGui.Spacing();
            
            WipToggle("Enable Aesthetician Skip");
        }
    }
}
