using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIHookResearch() {
            SectionHeader("Hook Research");

            if (!Skippy.Instance._devModeValid) {
                ImGui.Spacing();
                ImGui.Spacing();
                
                CenteredText("Please wait while we confirm that you have access to this panel...");
                return;
            }

            ImGui.TextWrapped("This section allows you to test the various game hooks used by Skippy. Each toggle enables a signature hook in isolation, bypassing all exemption logic otherwise present within the plugin. If a hook fires, it will be logged to your Dalamud logs and sent to the research sheet, with the hook name, territory ID, zone name, and intended use.\n\n" + "Please report any found data back to Boxu so we can improve this plugin by adding more to it that may come of use to players.");

            ImGui.Spacing();
            ImGui.Spacing();

            bool locked = !_config.DevMode;
            if (locked) {
                ImGui.BeginDisabled();
            }

            void ResearchToggle(string label, string tip, string sig, ref bool val) {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(8, ImGui.GetStyle().ItemSpacing.Y));
                ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new System.Numerics.Vector2(8, ImGui.GetStyle().ItemInnerSpacing.Y));

                if (ImGui.Checkbox(label, ref val)) {
                    Skippy.Instance.Hooks.RefreshHooks();
                    _saveConfig();
                }

                ImGui.PopStyleVar(2);
                ImGui.SameLine();
                ImGuiComponents.HelpMarker(tip);
                ImGui.SameLine(0, 10f);

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.55f, 0.55f, 0.55f, 1f));
                ImGui.TextUnformatted(FontAwesomeIcon.Code.ToIconString());
                ImGui.PopStyleColor();
                ImGui.PopFont();

                if (ImGui.IsItemHovered()) {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.60f, 0.85f, 1.00f, 1f));
                    ImGui.TextUnformatted($"[SigHook(\"{sig}\")]");
                    ImGui.PopStyleColor();
                }

                if (ImGui.IsItemClicked()) {
                    ImGui.SetClipboardText($"[SigHook(\"{sig}\")]");
                }

                ImGui.Spacing();
                ImGui.Spacing();
            }
            
            void ResearchOptionToggle(string label, string tip, ref bool val) {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(8, ImGui.GetStyle().ItemSpacing.Y));
                ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new System.Numerics.Vector2(8, ImGui.GetStyle().ItemInnerSpacing.Y));

                if (ImGui.Checkbox(label, ref val)) {
                    _saveConfig();
                }

                ImGui.PopStyleVar(2);
                ImGui.SameLine();
                ImGuiComponents.HelpMarker(tip);
                
                ImGui.Spacing();
                ImGui.Spacing();
            }

            bool researchMSQ = _config.ResearchMSQHook;
            ResearchToggle("MSQ Roulette Hook", "Enables the ContentDirector_IsPlayCutscene hook globally, without any exemptions.", "48 89 5C 24 ?? 57 48 83 EC 50 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? BA ?? ?? ?? ?? B3 01 E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? 48 8B F8 E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 08 84 99 ?? ?? ?? ??", ref researchMSQ);
            _config.ResearchMSQHook = researchMSQ;

            bool researchMassivePC = _config.ResearchMassivePCHook;
            ResearchToggle("Large-Scale Content Hook", "Enables the MassivePcContentDirector_IsPlayCutscene hook globally, without any exemptions.", "48 89 5C 24 ?? 57 48 83 EC 50 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? BA ?? ?? ?? ?? B3 01 E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? 48 8B F8 E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 08 48 8B 11", ref researchMassivePC);
            _config.ResearchMassivePCHook = researchMassivePC;

            bool researchGoldSaucer = _config.ResearchGoldSaucerHook;
            ResearchToggle("Gold Saucer Hook", "Enables the GoldSaucerDirector_IsPlayCutscene hook globally, without any exemptions.", "48 89 5C 24 ?? 57 48 83 EC 50 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? BA ?? ?? ?? ?? B3 01 E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? 48 8B F8 E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 08 84 99 ?? ?? ?? ??", ref researchGoldSaucer);
            _config.ResearchGoldSaucerHook = researchGoldSaucer;

            bool researchCustomTalk = _config.ResearchCustomTalkHook;
            ResearchToggle("NPC Dialogue Hook", "Enables the CustomTalk_IsPlayCutsceneContent hook globally, without any exemptions.", "48 83 EC 58 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 08 48 85 C9 74 06", ref researchCustomTalk);
            _config.ResearchCustomTalkHook = researchCustomTalk;

            bool researchNormalCutscenes = _config.ResearchNormalCutscenesHook;
            ResearchToggle("World Cutscenes Hook", "Enables the EventSceneModuleUsualImpl_PlayCutScene hook globally, without any exemptions.", "40 53 55 57 41 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B 59 08", ref researchNormalCutscenes);
            _config.ResearchNormalCutscenesHook = researchNormalCutscenes;

            bool researchInn = _config.ResearchInnHook;
            ResearchToggle("Inn Hook", "Enables the IsEnterTerritoryEventLogin hook globally, without any exemptions.", "48 83 EC 58 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ??", ref researchInn);
            _config.ResearchInnHook = researchInn;

            bool researchFeedBuddy = _config.ResearchFeedBuddyHook;
            ResearchToggle("Feed Buddy Hook", "Enables the PlayFeedBuddyScene hook globally, without any exemptions.", "48 8B 49 08 48 85 C9 74 0C 48 81 C1 ?? ?? ?? ?? E9 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC CC CC 48 8B 41 08", ref researchFeedBuddy);
            _config.ResearchFeedBuddyHook = researchFeedBuddy;

            ImGui.Spacing();
            ImGui.Spacing();

            bool extraLogs = _config.ResearchExtraLogs;
            ResearchOptionToggle("Extra Logs", "Enables verbose research logging to your Dalamud Logs.\n\n" + "Information:\n" + "Hook, territory ID, zone, intended use, cutsceneId, isCutsceneSeen, whether the territory is exempted, ShouldPatchMemory, IsWorkshop, DevMode, DevModeValid, and the current value of every skip toggle, exemption flag, and research toggle in the config.", ref extraLogs);
            _config.ResearchExtraLogs = extraLogs;

            SectionHeader("New Hooks Exploration");

            ImGui.TextWrapped("This section will contain experimental and unconfirmed hooks that may not work as intended. If a hook fires, it will be logged to your Dalamud logs and sent to the research sheet.");

            ImGui.Spacing();
            ImGui.Spacing();

            bool researchGCRankUp = _config.ResearchGrandCompanyRankUpHook;
            ResearchToggle("Grand Company Rank-Up Hook", "Enables the EventSceneModuleUsualImpl_GrandCompanyRankUp hook globally, without any exemptions.", "40 53 48 83 EC 50 48 8B D9 48 8D 4C 24 ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 83 38 00 0F 84 ?? ?? ?? ?? 48 8B 5B 08", ref researchGCRankUp);
            _config.ResearchGrandCompanyRankUpHook = researchGCRankUp;

            bool researchHairMake = _config.ResearchHairMakeHook;
            ResearchToggle("Aesthetician Hook", "Enables the EventSceneModuleUsualImpl_HairMake hook globally, without any exemptions.", "48 89 5C 24 ?? 57 48 83 EC 50 48 8B D9 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 5B 08 33 D2 45 33 C0 8D 4A 28 E8 ?? ?? ?? ?? 48 85 C0", ref researchHairMake);
            _config.ResearchHairMakeHook = researchHairMake;

            if (locked) {
                ImGui.EndDisabled();
            }
        }
    }
}
