using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIFeedBuddy() {
            SectionHeader("Feed Buddy Animation");
            
            ImGui.TextWrapped("This section covers the Companion Feeding Animation cutscene that plays when you feed your Chocobo in Housing.");
            
            ImGui.Spacing();
            ImGui.Spacing();

            bool changed = false;
            bool master  = _config.SkipFeedBuddy;
            
            FeatureCheckbox("##SkipFeedBuddy", "Skip Feed Buddy Cutscene", "Not much to say there, a bit sad you don't want to see that cute animation :(", ref master, out changed);
            
            if (changed)
            {
                _config.SkipFeedBuddy = master; 
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }
        }
    }
}
