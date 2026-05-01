using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private static void UILogger() {
            SectionHeader("Log Export");
            
            ImGui.TextWrapped("This section quickly allows you to export your Dalamud Log File to your Desktop for troubleshooting. You may then send this file to developers when reporting issues, so that they can look into any kind of log message that may help them help you out. (this goes for Skippy too!)");
            
            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing();

            var button1 = ImGuiComponents.GetIconButtonWithTextWidth(FontAwesomeIcon.FileExport, "Export Dalamud Log");
            var button2 = ImGuiComponents.GetIconButtonWithTextWidth(FontAwesomeIcon.CommentDots, "Contact @boxu");
            var gap = 32f;
            var buttons = button1 + gap + button2;
            
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X - buttons) * 0.5f);

            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.FileExport, "Export Dalamud Log")) {
                Skippy.Instance.ExportLog();
            }

            if (ImGui.IsItemHovered()) {
                using var tooltip = ImRaii.Tooltip(); 
                ImGui.TextUnformatted("Directly copies your dalamud.log to your Desktop for easy access.");
            }

            ImGui.SameLine(0, gap);

            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.CommentDots, "Contact @boxu")) {
                OpenUrl("https://discord.gg/punishxiv");
            }

            if (ImGui.IsItemHovered()) {
                using var tooltip = ImRaii.Tooltip(); 
                ImGui.TextUnformatted("Invite Link to the Puni.sh Discord Server — Contact @boxu for any kind of inquiries related to Skippy.");
            }

            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing();

            SectionHeader("Cutscene Research");
            
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.75f, 0.75f, 0.75f, 1f));
            ImGui.TextWrapped("Whenever you enable a global skip in any of the categories Skippy offers, and you encounter a cutscene, Skippy will automatically log the territory ID, zone name, and content type to a Google Sheet used for research and future exemption mapping.\n\n" + "No personal data is collected and none will ever be, only game zone information so that I can figure out what may be a good idea for future updates.");
            ImGui.PopStyleColor();
        }
    }
}
