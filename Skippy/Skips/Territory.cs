using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace Skippy.Skips {
    internal partial class SigHooks {
        internal static readonly ushort[] TerritoryPrae = [1044, 1045];
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
            
            bool exempt = IsExemptedTerritory((ushort)territory);
            SetEnabled(!exempt);
            
            if (!exempt) {
                RefreshHooks();
            }
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
