using Dalamud.Bindings.ImGui;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIBehaviour() {
            SectionHeader("First-Time Viewing");

            ImGui.TextWrapped("The game possesses a function called IsCutsceneSeen which determines whether a cutscene has already been viewed by the player. When it returns true, the game treats the cutscene as already seen and skips it — this is the same mechanism the game itself uses to skip cutscenes you have already watched.\n\n" + "Skippy hooks into that function so that, when a relevant skip category is active, it skips the cutscene as if you had already seen it. This is what allows Skippy to skip cutscenes that are otherwise marked as unskippable.\n\n" + "This used to be the problem that led to Skippy skipping all of your first-time views on any cutscene in the game, however this issue has since then been fixed! This means first-time cutscene views are fully protected now unless they are one you wish to skip!\n\n" + "The option below is an opt-in that removes that restriction. With it enabled, every cutscene in the game, including ones you have never seen before — may be skipped by Skippy at any time. Only enable this if you are certain you have seen every cutscene and do not mind losing the first-time viewing experience.");

            ImGui.Spacing();
            ImGui.Spacing();

            bool changed = false;
            bool global = _config.AllowCutsceneSeenGlobally;

            FeatureCheckbox("##AllowCutsceneSeenGlobally", "Disable your first-time viewing experience everywhere", "Opts in to allowing any cutscene in the game to be skipped. This may so on cause cutscenes you've never seen to be skipped.", ref global, out changed);

            if (changed) {
                _config.AllowCutsceneSeenGlobally = global;
                Skippy.Instance.Hooks.RefreshHooks();
                _saveConfig();
            }
        }
    }
}
