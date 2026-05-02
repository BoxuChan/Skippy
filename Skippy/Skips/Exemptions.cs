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

            bool inMSQTerritory = Array.IndexOf(TerritoryPrae, territory) >= 0 || Array.IndexOf(TerritoryCastrum, territory) >= 0 || Array.IndexOf(TerritoryPorta, territory) >= 0;

            if (_config.SkipMSQRoulette) {
                if (!inMSQTerritory) {
                    return _msqHook!.Original(luaState);
                }

                if ((_config.ExemptPrae && Array.IndexOf(TerritoryPrae, territory) >= 0) || (_config.ExemptCastrum && Array.IndexOf(TerritoryCastrum, territory) >= 0) || (_config.ExemptPorta && Array.IndexOf(TerritoryPorta, territory) >= 0)) {
                    return _msqHook!.Original(luaState);
                }
            } else {
                bool skip = (Array.IndexOf(TerritoryPrae, territory) >= 0 && _config.ExemptPrae) || (Array.IndexOf(TerritoryCastrum, territory) >= 0 && _config.ExemptCastrum) || (Array.IndexOf(TerritoryPorta, territory) >= 0 && _config.ExemptPorta);
                
                if (!skip) {
                    return _msqHook!.Original(luaState);
                }
            }

            LogExemption("MSQRoulette");
            return 1L;
        }

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
        ];

        private long ExemptionGoldSaucer(nint luaState) {
            var territory = (ushort)_clientState.TerritoryType;
            var use = GetIntendedUse(territory);

            if (_config.SkipGoldSaucer) {
                if (System.Array.IndexOf(GoldSaucerIntendedUses, use) < 0) {
                    return _goldSaucerHook!.Original(luaState);
                }

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
                    
                    case TerritoryIntendedUse.AirForceOne:
                        exempt = _config.ExemptAirForceOne;
                        break;
                }

                if (!exempt && System.Array.IndexOf(TerritoryMahjong, territory) >= 0) {
                    exempt = _config.ExemptMahjong;
                }

                if (exempt) {
                    return _goldSaucerHook!.Original(luaState);
                }
            } else {
                bool skip = false;
                
                switch (use) {
                    case TerritoryIntendedUse.ChocoboRacing:
                        skip = _config.ExemptChocoboRace;
                        break;
                    
                    case TerritoryIntendedUse.LordOfVerminion:
                        skip = _config.ExemptVerminion;
                        break;
                    
                    case TerritoryIntendedUse.TripleTriadBattlehall:
                    case TerritoryIntendedUse.TripleTriadOpenTournament:
                    case TerritoryIntendedUse.TripleTriadInvitationalParlor:
                        skip = _config.ExemptTripleTriad;
                        break;
                    
                    case TerritoryIntendedUse.Blunderville:
                        skip = _config.ExemptFallGuys;
                        break;
                    
                    case TerritoryIntendedUse.AirForceOne:
                        skip = _config.ExemptAirForceOne;
                        break;
                }

                if (!skip && System.Array.IndexOf(TerritoryMahjong, territory) >= 0) {
                    skip = _config.ExemptMahjong;
                }

                if (!skip) {
                    return _goldSaucerHook!.Original(luaState);
                }
            }

            LogExemption("GoldSaucer");
            return 1L;
        }

        private long ExemptionMassivePC(nint luaState) {
            LogExemption("MassivePC"); 
            return 1L;
        }

        private long ExemptionCustomTalk(nint luaState) {
            bool isWorkshop = IsWorkshopTerritory();

            if (_config.SkipCustomTalk) {
                if (_config.ExemptSubmarines && isWorkshop) {
                    return _customTalkHook!.Original(luaState);
                }
            } else {
                if (!(_config.ExemptSubmarines && isWorkshop)) {
                    return _customTalkHook!.Original(luaState);
                }
            }

            LogExemption("CustomTalk");
            return 1L;
        }

        private long ExemptionNormalCutscenes(nint luaState1, nint luaState2) {
            LogExemption("NormalCutscenes"); 
            return 1L;
        }
    }
}
