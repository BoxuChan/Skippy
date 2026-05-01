using System;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace Skippy.Skips {
    internal partial class SigHooks {
        private unsafe bool CutsceneSeenDetour(UIState* pointer, uint cutsceneId) {
            return !IsExemptedTerritory((ushort)_clientState.TerritoryType) || _cutsceneSeenHook!.Original(pointer, cutsceneId);
        }

        private long ExemptionMSQRoulette(nint luaState) {
            var territory = (ushort)_clientState.TerritoryType;
            var use = GetIntendedUse(territory);

            switch (use) {
                case TerritoryIntendedUse.OceanFishing:
                    return _config.SkipOceanFishing ? 1L : _msqHook!.Original(luaState);
                
                case TerritoryIntendedUse.CrystallineConflict:
                case TerritoryIntendedUse.CrystallineConflictCustomMatch:
                    return _config.SkipCrystallineConflict ? 1L : _msqHook!.Original(luaState);
                
                case TerritoryIntendedUse.Frontline:
                case TerritoryIntendedUse.RivalWings:
                    return _msqHook!.Original(luaState);
            }

            if (!_config.SkipMSQRoulette || _config.ExemptPrae  
                && System.Array.IndexOf(TerritoryPrae, territory) >= 0 || _config.ExemptCastrum
                && System.Array.IndexOf(TerritoryCastrum, territory) >= 0 || _config.ExemptPorta
                && System.Array.IndexOf(TerritoryPorta, territory) >= 0) {
                return _msqHook!.Original(luaState);
            }

            LogExemption("MSQRoulette");
            return 1L;
        }

        private long ExemptionGoldSaucer(nint luaState) {
            var use = GetIntendedUse((ushort)_clientState.TerritoryType);
            bool exempt = false;

            switch (use) {
                case TerritoryIntendedUse.ChocoboRacing:
                    exempt = _config.ExemptChocoboRace;
                    break;

                case TerritoryIntendedUse.LordOfVerminion:
                    exempt = _config.ExemptVerminion;
                    break;

                case TerritoryIntendedUse.TripleTriadBattlehall:
                case TerritoryIntendedUse.TripleTriadOpenTournament:
                case TerritoryIntendedUse.TripleTriadInvitationalParlor:
                    exempt = _config.ExemptTripleTriad;
                    break;

                case TerritoryIntendedUse.Blunderville:
                    exempt = _config.ExemptFallGuys;
                    break;
            }

            if (exempt) {
                return _goldSaucerHook!.Original(luaState);
            }
            
            LogExemption("GoldSaucer");
            return 1L;
        }

        private long ExemptionMassivePC(nint luaState) {
            LogExemption("MassivePC"); 
            return 1L;
        }

        private long ExemptionCustomTalk(nint luaState) {
            LogExemption("CustomTalk"); 
            return 1L;
        }

        private long ExemptionNormalCutscenes(nint luaState1, nint luaState2) {
            LogExemption("NormalCutscenes"); 
            return 1L;
        }
    }
}
