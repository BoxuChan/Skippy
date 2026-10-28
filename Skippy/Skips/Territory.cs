using System;

using Dalamud.Plugin.Services;

using Lumina.Excel.Sheets;
using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace Skippy.Skips {
    internal partial class SigHooks {
        internal static readonly ushort[] TerritoryPrae = [1044];
        internal static readonly ushort[] TerritoryCastrum = [1043];
        internal static readonly ushort[] TerritoryPorta = [1048];
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

        internal static bool IsMSQTerritory(ushort t) => Array.IndexOf(TerritoryPrae, t) >= 0 || Array.IndexOf(TerritoryCastrum, t) >= 0 || Array.IndexOf(TerritoryPorta, t) >= 0;

        internal static bool IsPvPVanilla(TerritoryIntendedUse use) => use is TerritoryIntendedUse.Frontline or TerritoryIntendedUse.RivalWings;

        internal static bool IsGoldSaucerTerritory(ushort t, TerritoryIntendedUse use) => Array.IndexOf(GoldSaucerIntendedUses, use) >= 0 || Array.IndexOf(TerritoryMahjong, t) >= 0;

        internal bool IsRiskyTerritory(TerritoryIntendedUse use, out bool skip) {
            switch (use) {
                case TerritoryIntendedUse.OceanFishing:
                    skip = _config.SkipOceanFishing;
                    return true;

                case TerritoryIntendedUse.CrystallineConflict:
                case TerritoryIntendedUse.CrystallineConflictCustomMatch:
                    skip = _config.SkipCrystallineConflict;
                    return true;

                case TerritoryIntendedUse.Inn:
                    skip = _config.SkipInn;
                    return true;
            }

            skip = false;
            return false;
        }

        internal bool MSQSkipsIn(ushort t) {
            bool tick = Array.IndexOf(TerritoryPrae, t) >= 0 ? _config.ExemptPrae : Array.IndexOf(TerritoryCastrum, t) >= 0 ? _config.ExemptCastrum : Array.IndexOf(TerritoryPorta, t) >= 0 && _config.ExemptPorta;
            return _config.SkipMSQRoulette ? !tick : tick;
        }

        internal bool GoldSaucerSkipsIn(ushort t, TerritoryIntendedUse use) {
            bool tick;

            switch (use) {
                case TerritoryIntendedUse.ChocoboRacing:
                    tick = _config.ExemptChocoboRace;
                    break;
                
                case TerritoryIntendedUse.LordOfVerminion:
                    tick = _config.ExemptVerminion;
                    break;
                
                case TerritoryIntendedUse.TripleTriadBattlehall:
                case TerritoryIntendedUse.TripleTriadOpenTournament:
                case TerritoryIntendedUse.TripleTriadInvitationalParlor:
                    tick = _config.ExemptTripleTriad;
                    break;
                
                case TerritoryIntendedUse.Blunderville:
                    tick = _config.ExemptFallGuys;
                    break;
                
                case TerritoryIntendedUse.AirForceOne:
                    tick = _config.ExemptAirForceOne;
                    break;
                
                case TerritoryIntendedUse.KeyboundBrawler:
                    tick = _config.ExemptKeyboundBrawler;
                    break;
                
                default:
                    tick = Array.IndexOf(TerritoryMahjong, t) >= 0 && _config.ExemptMahjong;
                    break;
            }

            return _config.SkipGoldSaucer ? !tick : tick;
        }

        internal bool SubmarinesSkip => _config.SkipCustomTalk ? !_config.ExemptSubmarines : _config.ExemptSubmarines;

        internal bool TerritoryActive(ushort t) {
            var use = GetIntendedUse(t);

            if (IsRiskyTerritory(use, out var risky)) {
                return risky;
            }

            if (IsPvPVanilla(use)) {
                return false;
            }

            if (IsMSQTerritory(t)) {
                return MSQSkipsIn(t);
            }

            if (use == TerritoryIntendedUse.CosmicExploration) {
                return _config.SkipCosmicExploration;
            }

            if (IsGoldSaucerTerritory(t, use)) {
                return GoldSaucerSkipsIn(t, use);
            }

            if (IsWorkshopTerritory()) {
                return SubmarinesSkip;
            }

            return _config.SkipMassivePC || _config.SkipCustomTalk || _config.SkipNormalCutscenes || _config.SkipFeedBuddy || _config.AllowCutsceneSeenGlobally;
        }

        internal bool ShouldPatchMemory() => TerritoryActive((ushort) _clientState.TerritoryType);

        internal bool IsExemptedTerritory(ushort territory) => !TerritoryActive(territory);

        internal void OnTerritoryChanged(uint territory) {
            if (!_config.IsEnabled) {
                return;
            }

            if (_config.ResearchExtraLogs) {
                var use = GetIntendedUse((ushort) territory);
                var name = _dataManager.GetExcelSheet<TerritoryType>()?.GetRowOrDefault((ushort) territory)?.PlaceName.Value.Name.ToString() ?? "Unknown";
                bool active = TerritoryActive((ushort) territory);

                _pluginLog.Information("TerritoryChanged: id={0} name={1} intendedUse={2} ({3}) active={4} IsWorkshop={5}", territory, name, use, (byte) use, active, IsWorkshopTerritory());
            }

            RefreshHooks();
        }

        internal void PrintTerritory(IChatGui chat) {
            var territory = (ushort) _clientState.TerritoryType;
            var use = GetIntendedUse(territory);
            var name = _dataManager.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(territory)?.PlaceName.Value.Name.ToString() ?? "Unknown";

            chat.Print($"[Skippy] Territory: {territory} ({name}) -- IntendedUse: {use} ({(byte)use})");
        }
    }
}