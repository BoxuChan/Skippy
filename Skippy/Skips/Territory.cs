using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace Skippy.Skips {
    internal partial class SigHooks {
        internal static readonly ushort[] TerritoryPrae = [1044];
        internal static readonly ushort[] TerritoryCastrum = [1043];
        internal static readonly ushort[] TerritoryPorta = [1046];
        private static readonly ushort[] TerritoryMahjong = [831];

        internal unsafe bool IsWorkshopTerritory() {
            var manager = FFXIVClientStructs.FFXIV.Client.Game.HousingManager.Instance();
            return manager != null && manager->IsInWorkshop();
        }

        internal TerritoryIntendedUse GetIntendedUse(ushort territoryId) {
            if (_dataManager.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(territoryId) is { } row) {
                return (TerritoryIntendedUse)row.TerritoryIntendedUse.RowId;
            }
            
            return 0;
        }

        internal void OnTerritoryChanged(uint territory) {
            if (!_config.IsEnabled) {
                return;
            }
            
            if (_config.ResearchExtraLogs) {
                var use = GetIntendedUse((ushort)territory);
                var name = _dataManager.GetExcelSheet<TerritoryType>()?.GetRowOrDefault((ushort)territory)?.PlaceName.Value.Name.ToString() ?? "Unknown";
                _pluginLog.Information("TerritoryChanged: id={0} name={1} intendedUse={2} ({3})", territory, name, use, (byte)use);
            }
            
            bool exempt = IsExemptedTerritory((ushort)territory);
            SetEnabled(!exempt && ShouldPatchMemory());
            
            if (!exempt) {
                RefreshHooks();
            }
        }

        internal bool ShouldPatchMemory() {
            var territory = (ushort)_clientState.TerritoryType;
            var use = GetIntendedUse(territory);
            bool inMSQ = System.Array.IndexOf(TerritoryPrae, territory) >= 0 || System.Array.IndexOf(TerritoryCastrum, territory) >= 0 || System.Array.IndexOf(TerritoryPorta, territory) >= 0;

            if (_config.SkipMSQRoulette && inMSQ) {
                return true;
            }

            if (_config.SkipOceanFishing && use == TerritoryIntendedUse.OceanFishing) {
                return true;
            }

            if (_config.SkipCrystallineConflict && (use == TerritoryIntendedUse.CrystallineConflict || use == TerritoryIntendedUse.CrystallineConflictCustomMatch)) {
                return true;
            }

            if (_config.SkipMassivePC) {
                return true;
            }

            if (_config.SkipGoldSaucer && System.Array.IndexOf(GoldSaucerIntendedUses, use) >= 0) {
                return true;
            }

            if (_config.SkipCustomTalk) {
                return true;
            }

            if (_config.ExemptSubmarines && IsWorkshopTerritory()) {
                return true;
            }

            if (_config.SkipNormalCutscenes) {
                return true;
            }

            if (_config.SkipFeedBuddy) {
                return true;
            }

            if (_config.SkipInn) {
                return true;
            }

            if (_config.AllowCutsceneSeenGlobally) {
                return true;
            }

            if (_config.ResearchMSQHook || _config.ResearchMassivePCHook || _config.ResearchGoldSaucerHook || _config.ResearchCustomTalkHook || _config.ResearchNormalCutscenesHook || _config.ResearchInnHook || _config.ResearchFeedBuddyHook) {
                return true;
            }

            return false;
        }

        internal bool IsExemptedTerritory(ushort territory) {
            var use = GetIntendedUse(territory);

            if (!_config.SkipOceanFishing && use == TerritoryIntendedUse.OceanFishing) {
                return true;
            }

            switch (_config.SkipCrystallineConflict) {
                case false when use == TerritoryIntendedUse.CrystallineConflict:
                case false when use == TerritoryIntendedUse.CrystallineConflictCustomMatch:
                    return true;
            }

            switch (use) {
                case TerritoryIntendedUse.Frontline:
                case TerritoryIntendedUse.RivalWings:
                    return true;
            }

            if (_config.ExemptPrae && System.Array.IndexOf(TerritoryPrae, territory) >= 0) {
                return true;
            }

            if (_config.ExemptCastrum && System.Array.IndexOf(TerritoryCastrum, territory) >= 0) {
                return true;
            }

            if (_config.ExemptPorta && System.Array.IndexOf(TerritoryPorta, territory) >= 0) {
                return true;
            }

            if (_config.ExemptChocoboRace && use == TerritoryIntendedUse.ChocoboRacing) {
                return true;
            }

            if (_config.ExemptVerminion && use == TerritoryIntendedUse.LordOfVerminion) {
                return true;
            }

            if (_config.ExemptTripleTriad && (use == TerritoryIntendedUse.TripleTriadBattlehall || use == TerritoryIntendedUse.TripleTriadOpenTournament || use == TerritoryIntendedUse.TripleTriadInvitationalParlor)) {
                return true;
            }

            if (_config.ExemptFallGuys && use == TerritoryIntendedUse.Blunderville) {
                return true;
            }

            if (_config.ExemptAirForceOne && use == TerritoryIntendedUse.AirForceOne) {
                return true;
            }

            if (_config.ExemptMahjong && System.Array.IndexOf(TerritoryMahjong, territory) >= 0) {
                return true;
            }

            return false;
        }

        internal void PrintTerritory(IChatGui chat) {
            var territory = (ushort)_clientState.TerritoryType;
            var use = GetIntendedUse(territory);
            var name = _dataManager.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(territory)?.PlaceName.Value.Name.ToString() ?? "Unknown";
            
            chat.Print($"[Skippy] Territory: {territory} ({name}) -- IntendedUse: {use} ({(byte)use})");
        }
    }
}