using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private static void UICommands() {
            SectionHeader("Commands");

            if (ImGui.BeginTable("##Commands", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {
                ImGui.TableSetupColumn("Command");
                ImGui.TableSetupColumn("Parameters");
                ImGui.TableSetupColumn("Description");
                ImGui.TableHeadersRow();

                void Row(string cmd, string param, string desc) {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn(); 
                    ImGui.TextUnformatted(cmd);
                    ImGui.TableNextColumn(); 
                    ImGui.TextUnformatted(param);
                    ImGui.TableNextColumn(); 
                    ImGui.TextWrapped(desc);
                }

                Row("/skippy", "(none)", "Opens the settings window.");
                Row("/skippy", "on / start / enable", "Enables Skippy.");
                Row("/skippy", "off / stop / disable", "Disables Skippy.");
                Row("/skippy", "log / export", "Exports your Dalamud log to your Desktop.");
                Row("/skippy", "territory / zone", "Prints the current territory ID and IntendedUse to chat.");
                Row("/skippy", "user / userid / id", "Prints your anonymous UserID to chat so you can copy it and send it when reporting a bug.");

                ImGui.EndTable();
            }
        }
    }
}