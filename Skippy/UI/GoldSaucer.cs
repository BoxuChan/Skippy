using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private void UIGoldSaucer() {
            SectionHeader("Gold Saucer");
            
            ImGui.TextWrapped("This section covers the cutscenes that play inside Gold Saucer game modes. I still have to test out if anything more is contained in that group, but I do believe that should be all of them listed below.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            bool changed = false;
            bool master = _config.SkipGoldSaucer;
            
            FeatureCheckbox("##SkipGoldSaucer", "Skip all of the Gold Saucer Cutscenes", "This will skip every cutscene in every game mode at once, to specify the game modes you want to keep the cutscenes of, check below in Exemptions, or you can directly pick to skip only specific game modes below in Skip Specific if you keep this global toggle off.", ref master, out changed);

            if (changed) {
                _config.SkipGoldSaucer = master;
                _config.ExemptChocoboRace = false;
                _config.ExemptVerminion = false;
                _config.ExemptTripleTriad = false;
                _config.ExemptFallGuys = false;
                _config.ExemptAirForceOne = false;
                _config.ExemptMahjong = false;
                
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            bool skipAll = _config.SkipGoldSaucer;
            SectionHeader(skipAll ? "[WIP] Exemptions" : "[WIP] Skip Specific");

            ImGui.TextWrapped(skipAll ? "There shouldn't be any extra to this category, but feel free to let me know if you notice one that isn't listed. Any ticked entries in this category will be exempt from the global skip above — cutscenes will so on play normally in those modes." : "There shouldn't be any extra to this category, but feel free to let me know if you notice one that isn't listed. Any ticked entries in this category will be skipped individually, in case you want to only skip specific parts of this category and not all of them. Should you decide to skip everything, click the checkbox above.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            void Toggle(string id, string label, string tip, ref bool val) {
                bool c = false;
                FeatureCheckbox(id, label, tip, ref val, out c);
                
                if (c) {
                    Skippy.Instance.Hooks.RefreshHooks(); 
                    _saveConfig();
                }
            }

            bool mahjong = _config.ExemptMahjong;
            Toggle("##ExemptMahjong", skipAll ? "Exempt: Mahjong" : "Skip: Mahjong", skipAll ? "Cutscenes will play normally during Mahjong." : "Cutscenes will be skipped during Mahjong.", ref mahjong);
            _config.ExemptMahjong = mahjong;

            bool airForceOne = _config.ExemptAirForceOne;
            Toggle("##ExemptAirForceOne", skipAll ? "Exempt: Air Force One" : "Skip: Air Force One", skipAll ? "Cutscenes will play normally during Air Force One." : "Cutscenes will be skipped during Air Force One.", ref airForceOne);
            _config.ExemptAirForceOne = airForceOne;

            bool chocobo = _config.ExemptChocoboRace;
            Toggle("##ExemptChocobo", skipAll ? "Exempt: Chocobo Racing" : "Skip: Chocobo Racing", skipAll ? "Cutscenes will play normally during Chocobo Racing." : "Cutscenes will be skipped during Chocobo Racing.", ref chocobo);
            _config.ExemptChocoboRace = chocobo;

            bool verminion = _config.ExemptVerminion;
            Toggle("##ExemptVerminion", skipAll ? "Exempt: Lord of Verminion" : "Skip: Lord of Verminion", skipAll ? "Cutscenes will play normally during Lord of Verminion." : "Cutscenes will be skipped during Lord of Verminion.", ref verminion);
            _config.ExemptVerminion = verminion;

            bool tripleTriad = _config.ExemptTripleTriad;
            Toggle("##ExemptTripleTriad", skipAll ? "Exempt: Triple Triad (Battlehall / Tournaments)" : "Skip: Triple Triad (Battlehall / Tournaments)", skipAll ? "Cutscenes will play normally in Triple Triad Battlehall, Open Tournaments, and Invitational Parlor." : "Cutscenes will be skipped in Triple Triad Battlehall, Open Tournaments, and Invitational Parlor.", ref tripleTriad);
            _config.ExemptTripleTriad = tripleTriad;

            if (!_config.FallGuysEventActive) {
                ImGui.BeginDisabled();
                bool dummy = _config.ExemptFallGuys;
                bool tickable = false;
                FeatureCheckbox("##ExemptFallGuys", skipAll ? "Exempt: Fall Guys Event (Blunderville)" : "Skip: Fall Guys Event (Blunderville)", "Blunderville is the seasonal Fall Guys crossover event. This option will only be available when the event is active.", ref dummy, out tickable);
                ImGui.EndDisabled();
                
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.TextUnformatted("The Fall Guys event isn't currently active. This option will be available once more when it comes back!");
                }
            } else {
                bool fallguys = _config.ExemptFallGuys;
                Toggle("##ExemptFallGuys", skipAll ? "Exempt: Fall Guys Event (Blunderville)" : "Skip: Fall Guys Event (Blunderville)", "Blunderville is the seasonal Fall Guys crossover event. This option will only be available when the event is active.", ref fallguys);
                _config.ExemptFallGuys = fallguys;
            }
        }
    }
}
