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
            var button2 = ImGuiComponents.GetIconButtonWithTextWidth(FontAwesomeIcon.CommentDots, "Contact @Boxu");
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

            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.CommentDots, "Contact @Boxu")) {
                OpenUrl("https://discord.gg/punishxiv");
            }

            if (ImGui.IsItemHovered()) {
                using var tooltip = ImRaii.Tooltip(); 
                ImGui.TextUnformatted("Invite Link to the Puni.sh Discord Server — Contact @Boxu for any kind of inquiries related to Skippy.");
            }

            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing();

            SectionHeader("Your User ID");

            ImGui.TextWrapped("Your User ID is an anonymous identifier derived from your account. It is never transmitted in a way that could identify you, it only goes one way, like a fingerprint without a face.\n\n" + "If you experience a bug and want to report it, please copy your User ID and send it to @Boxu. This lets me find your entries in the research log without knowing who you are in-game.");

            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing();

            var buttonUserId = ImGuiComponents.GetIconButtonWithTextWidth(FontAwesomeIcon.IdCard, "Copy User ID to Chat");
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X - buttonUserId) * 0.5f);

            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.IdCard, "Copy User ID to Chat")) {
                Skippy.Instance.PrintUserID();
            }

            if (ImGui.IsItemHovered()) {
                using var tooltip = ImRaii.Tooltip(); 
                ImGui.TextUnformatted("Prints your anonymous User ID to the chat window so you can copy it.\nYou can also use: /skippy user");
            }

            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing(); 
            ImGui.Spacing();

            SectionHeader("Cutscene Research");
            
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.75f, 0.75f, 0.75f, 1f));
            ImGui.TextWrapped("Whenever you enable a global skip in any of the categories Skippy offers, and you encounter a cutscene, Skippy will automatically log the territory ID, zone name, and content type to a Google Sheet used for research and future exemption mapping.\n\n" + "The only personal data included is your anonymous User ID above — no character name, no server, no identifying information. This helps me as the developer so that I can match reports to the Logger from the same player.");
            ImGui.PopStyleColor();
        }
    }
}