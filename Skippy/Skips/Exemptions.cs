using FFXIVClientStructs.FFXIV.Client.Game.UI;
using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace Skippy.Skips {
    internal partial class SigHooks {
        private static readonly TerritoryIntendedUse[] GoldSaucerIntendedUses = [
            TerritoryIntendedUse.ChocoboSquareOld,
            TerritoryIntendedUse.ChocoboRacing,
            TerritoryIntendedUse.GoldSaucer,
            TerritoryIntendedUse.OriginalStepsOfFaith,
            TerritoryIntendedUse.LordOfVerminion,
            TerritoryIntendedUse.TripleTriadBattlehall,
            TerritoryIntendedUse.LeapOfFaith,
            TerritoryIntendedUse.TripleTriadOpenTournament,
            TerritoryIntendedUse.TripleTriadInvitationalParlor,
            TerritoryIntendedUse.Blunderville,
            TerritoryIntendedUse.AirForceOne,
            TerritoryIntendedUse.KeyboundBrawler,
        ];

        private enum Hook { MSQ, GoldSaucer, MassivePC, CustomTalk, NormalCutscenes, Inn, FeedBuddy }

        private bool ResolveManaged(Hook hook, ushort territory, TerritoryIntendedUse use, out bool skip) {
            skip = false;

            if (IsRiskyTerritory(use, out var risky)) {
                skip = risky;
                return true;
            }

            if (IsPvPVanilla(use)) {
                return true;
            }

            if (IsMSQTerritory(territory)) {
                skip = hook == Hook.MSQ && MSQSkipsIn(territory);
                return true;
            }

            if (use == TerritoryIntendedUse.CosmicExploration) {
                skip = hook is Hook.MassivePC or Hook.NormalCutscenes && _config.SkipCosmicExploration;
                return true;
            }

            if (IsGoldSaucerTerritory(territory, use)) {
                skip = hook == Hook.GoldSaucer && GoldSaucerSkipsIn(territory, use);
                return true;
            }

            if (IsWorkshopTerritory()) {
                skip = hook is Hook.CustomTalk or Hook.NormalCutscenes && SubmarinesSkip;
                return true;
            }

            return false;
        }

        private bool ShouldSkip(Hook hook, bool unmanagedSkip) {
            var territory = (ushort) _clientState.TerritoryType;
            var use = GetIntendedUse(territory);

            return ResolveManaged(hook, territory, use, out var skip) ? skip : unmanagedSkip;
        }

        private string Tag(string category) {
            var use = GetIntendedUse((ushort) _clientState.TerritoryType);
            return IsRiskyTerritory(use, out _) ? "RiskySkips" : category;
        }

        private bool DevSkip(bool research, string name) {
            if (!research) {
                return false;
            }

            var use = GetIntendedUse((ushort) _clientState.TerritoryType);
            if (IsRiskyTerritory(use, out _) || IsPvPVanilla(use)) {
                return false;
            }

            LogExemption(name);
            return true;
        }

        private unsafe bool CutsceneSeenDetour(UIState* pointer, uint cutsceneId) {
            var territory = (ushort) _clientState.TerritoryType;
            var use = GetIntendedUse(territory);

            if (_config.ResearchExtraLogs) {
                _pluginLog.Information("CutsceneSeenDetour: cutsceneId={0} territory={1} use={2} ({3}) active={4} AllowCutsceneSeenGlobally={5}", cutsceneId, territory, use, (byte) use, TerritoryActive(territory), _config.AllowCutsceneSeenGlobally);
            }

            if (IsRiskyTerritory(use, out var risky)) {
                return risky || _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (IsPvPVanilla(use)) {
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (IsMSQTerritory(territory)) {
                return MSQSkipsIn(territory) || _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (use == TerritoryIntendedUse.CosmicExploration) {
                return _config.SkipCosmicExploration || _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (IsGoldSaucerTerritory(territory, use)) {
                return GoldSaucerSkipsIn(territory, use) || _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (IsWorkshopTerritory()) {
                return SubmarinesSkip || _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (use == TerritoryIntendedUse.HousingOutdoor && _config.SkipFeedBuddy) {
                return true;
            }

            if (_config.AllowCutsceneSeenGlobally) {
                LogExemption("CutsceneSeen_Behaviour", true, cutsceneId);
                return true;
            }

            return _cutsceneSeenHook!.Original(pointer, cutsceneId);
        }

        private long ExemptionMSQRoulette(nint luaState) {
            if (DevSkip(_config.ResearchMSQHook, "Dev_MSQHook")) {
                return 1L;
            }

            if (!ShouldSkip(Hook.MSQ, false)) {
                return _msqHook!.Original(luaState);
            }

            LogExemption(Tag("MSQRoulette"));
            return 1L;
        }

        private long ExemptionGoldSaucer(nint luaState) {
            if (DevSkip(_config.ResearchGoldSaucerHook, "Dev_GoldSaucerHook")) {
                return 1L;
            }

            if (!ShouldSkip(Hook.GoldSaucer, false)) {
                return _goldSaucerHook!.Original(luaState);
            }

            LogExemption(Tag("GoldSaucer"));
            return 1L;
        }

        private long ExemptionMassivePC(nint luaState) {
            if (DevSkip(_config.ResearchMassivePCHook, "Dev_MassivePCHook")) {
                return 1L;
            }

            if (!ShouldSkip(Hook.MassivePC, _config.SkipMassivePC)) {
                return _massivePCHook!.Original(luaState);
            }

            LogExemption(Tag("MassivePC"));
            return 1L;
        }

        private long ExemptionCustomTalk(nint luaState) {
            if (DevSkip(_config.ResearchCustomTalkHook, "Dev_CustomTalkHook")) {
                return 1L;
            }

            if (!ShouldSkip(Hook.CustomTalk, _config.SkipCustomTalk)) {
                return _customTalkHook!.Original(luaState);
            }

            LogExemption(Tag("CustomTalk"));
            return 1L;
        }

        private long ExemptionNormalCutscenes(nint luaState1, nint luaState2) {
            if (DevSkip(_config.ResearchNormalCutscenesHook, "Dev_NormalCutscenesHook")) {
                return 1L;
            }

            if (!ShouldSkip(Hook.NormalCutscenes, _config.SkipNormalCutscenes)) {
                return _normalCutscenesHook!.Original(luaState1, luaState2);
            }

            var use = GetIntendedUse((ushort) _clientState.TerritoryType);
            LogExemption(use == TerritoryIntendedUse.CosmicExploration ? "MassivePC" : IsWorkshopTerritory() ? "CustomTalk" : Tag("NormalCutscenes"));
            return 1L;
        }

        private long ExemptionInn(nint luaState) {
            if (DevSkip(_config.ResearchInnHook, "Dev_InnHook")) {
                return 1L;
            }

            if (!ShouldSkip(Hook.Inn, false)) {
                return _innHook!.Original(luaState);
            }

            LogExemption("RiskySkips");
            return 1L;
        }

        private long ExemptionFeedBuddy(nint luaState) {
            if (DevSkip(_config.ResearchFeedBuddyHook, "Dev_FeedBuddyHook")) {
                return 1L;
            }

            if (!ShouldSkip(Hook.FeedBuddy, _config.SkipFeedBuddy)) {
                return _feedBuddyHook!.Original(luaState);
            }

            LogExemption("FeedBuddy");
            return 1L;
        }
    }
}