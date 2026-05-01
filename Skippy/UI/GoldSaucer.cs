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
            bool master  = _config.SkipGoldSaucer;
            
            FeatureCheckbox("##SkipGoldSaucer", "Skip all of the Gold Saucer Cutscenes", "This will skip every cutscene in every game mode at once, to specify the game modes you want to keep the cutscenes, check below in Exemptions.", ref master, out changed);

            if (changed)
            {
                _config.SkipGoldSaucer = master; 
                Skippy.Instance.Hooks.RefreshHooks(); 
                _saveConfig();
            }

            SectionHeader("[WIP] Exemptions");
            
            ImGui.TextWrapped("There shouldn't be any extra to this category, but feel free to let me know if you notice one that isn't listed. Any ticked entries in this category will be exempt from the global skip above — cutscenes will so on play normally in those modes.");
            
            ImGui.Spacing(); 
            ImGui.Spacing();

            void Exempt(string id, string label, string tip, ref bool val) {
                bool c = false;
                
                FeatureCheckbox(id, label, tip, ref val, out c);

                if (c) {
                    Skippy.Instance.Hooks.RefreshHooks(); 
                    _saveConfig();
                }
            }

            bool chocobo = _config.ExemptChocoboRace;
            Exempt("##ExemptChocobo", "Exempt: Chocobo Racing", "Cutscenes will play normally during Chocobo Racing.", ref chocobo);
            _config.ExemptChocoboRace = chocobo;

            bool verminion = _config.ExemptVerminion;
            Exempt("##ExemptVerminion", "Exempt: Lord of Verminion", "Cutscenes will play normally during Lord of Verminion.", ref verminion);
            _config.ExemptVerminion = verminion;

            bool tripleTriad = _config.ExemptTripleTriad;
            Exempt("##ExemptTripleTriad", "Exempt: Triple Triad (Battlehall / Tournaments)", "Cutscenes will play normally in Triple Triad Battlehall, Open Tournaments, and Invitational Parlor.", ref tripleTriad);
            _config.ExemptTripleTriad = tripleTriad;

            if (!_config.FallGuysEventActive) {
                ImGui.BeginDisabled();
                
                bool dummy = _config.ExemptFallGuys;
                bool tickable = false;
                
                FeatureCheckbox("##ExemptFallGuys", "Exempt: Fall Guys Event (Blunderville)", "Blunderville is the seasonal Fall Guys crossover event. This option will only be available when the event is active.", ref dummy, out tickable);
                ImGui.EndDisabled();
                
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.TextUnformatted("The Fall Guys event isn't currently active. This option will be available once more when it comes back!");
                }
            } else {
                bool fallguys = _config.ExemptFallGuys;
                FeatureCheckbox("##ExemptFallGuys", "Exempt: Fall Guys Event (Blunderville)", "Blunderville is the seasonal Fall Guys crossover event. This option will only be available when the event is active.", ref fallguys, out changed);
                _config.ExemptFallGuys = fallguys;
            }
        }
    }
}
