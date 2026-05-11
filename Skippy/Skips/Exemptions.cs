using System;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace Skippy.Skips {
    internal partial class SigHooks {
        private unsafe bool CutsceneSeenDetour(UIState* pointer, uint cutsceneId) {
            var territory = (ushort)_clientState.TerritoryType;
            var use = GetIntendedUse(territory);

            if (_config.ResearchExtraLogs) {
                _pluginLog.Information("CutsceneSeenDetour: cutsceneId={0} territory={1} use={2} ({3})\n" + "--> IsExemptedTerritory={4}  IsWorkshop={5}\n" + "--> SkipMSQRoulette={6}  SkipOceanFishing={7}  SkipCrystallineConflict={8}  SkipCosmicExploration={9}\n" + "--> SkipGoldSaucer={10}  SkipInn={11}  SkipCustomTalk={12}  SkipFeedBuddy={13}  SkipMassivePC={14}  SkipNormalCutscenes={15}\n" + "--> AllowCutsceneSeenGlobally={16}", cutsceneId, territory, use, (byte)use, IsExemptedTerritory(territory), IsWorkshopTerritory(), _config.SkipMSQRoulette, _config.SkipOceanFishing, _config.SkipCrystallineConflict, _config.SkipCosmicExploration, _config.SkipGoldSaucer, _config.SkipInn, _config.SkipCustomTalk, _config.SkipFeedBuddy, _config.SkipMassivePC, _config.SkipNormalCutscenes, _config.AllowCutsceneSeenGlobally);
            }

            if (IsExemptedTerritory(territory)) {
                if (_config.ResearchExtraLogs) {
                    _pluginLog.Information("CutsceneSeenDetour: territory {0} is exempted -> calling original. (cutsceneId={1})", territory, cutsceneId);
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            bool inMSQ = Array.IndexOf(TerritoryPrae, territory) >= 0 || Array.IndexOf(TerritoryCastrum, territory) >= 0 || Array.IndexOf(TerritoryPorta, territory) >= 0;
            
            if (inMSQ) {
                if (_config.SkipMSQRoulette) {
                    if (_config.ResearchExtraLogs) {
                        _pluginLog.Information("CutsceneSeenDetour: inMSQ + SkipMSQRoulette=true -> returning true (skip). (territory={0} cutsceneId={1})", territory, cutsceneId);
                    }
                    
                    return true;
                }
                
                if (_config.ResearchExtraLogs) {
                    _pluginLog.Information("CutsceneSeenDetour: inMSQ but SkipMSQRoulette=false -> calling original. (territory={0} cutsceneId={1})", territory, cutsceneId);
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (use == TerritoryIntendedUse.OceanFishing) {
                if (_config.SkipOceanFishing) {
                    return true;
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (use == TerritoryIntendedUse.CrystallineConflict || use == TerritoryIntendedUse.CrystallineConflictCustomMatch) {
                if (_config.SkipCrystallineConflict) {
                    return true;
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (use == TerritoryIntendedUse.CosmicExploration) {
                if (_config.SkipCosmicExploration) {
                    return true;
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (Array.IndexOf(GoldSaucerIntendedUses, use) >= 0) {
                if (_config.SkipGoldSaucer) {
                    return true;
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (use == TerritoryIntendedUse.Inn) {
                if (_config.SkipInn) {
                    return true;
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (use == TerritoryIntendedUse.HousingIndoor) {
                if (IsWorkshopTerritory() && _config.SkipCustomTalk) {
                    return true;
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (use == TerritoryIntendedUse.HousingOutdoor) {
                if (_config.SkipFeedBuddy) {
                    return true;
                }
                
                return _cutsceneSeenHook!.Original(pointer, cutsceneId);
            }

            if (_config.SkipMassivePC) {
                return true;
            }

            if (_config.SkipNormalCutscenes) {
                return true;
            }

            if (_config.AllowCutsceneSeenGlobally) {
                if (Array.IndexOf(GoldSaucerIntendedUses, use) >= 0) {
                    LogExemption("CutsceneSeen_GoldSaucer", true, cutsceneId);
                } else if (use == TerritoryIntendedUse.Inn) {
                    LogExemption("CutsceneSeen_RiskySkips", true, cutsceneId);
                } else if (IsWorkshopTerritory()) {
                    LogExemption("CutsceneSeen_CustomTalk", true, cutsceneId);
                } else {
                    LogExemption("CutsceneSeen_NormalCutscenes", true, cutsceneId);
                }
                return true;
            }

            if (_config.ResearchExtraLogs) {
                _pluginLog.Information("CutsceneSeenDetour: no skip matched -> calling original. (territory={0} use={1} ({2}) inMSQ={3} cutsceneId={4})", territory, use, (byte)use, inMSQ, cutsceneId);
            }

            return _cutsceneSeenHook!.Original(pointer, cutsceneId);
        }

        private long ExemptionMSQRoulette(nint luaState) {
            var territory = (ushort)_clientState.TerritoryType;
            var use = GetIntendedUse(territory);

            switch (use) {
                case TerritoryIntendedUse.OceanFishing:
                    if (_config.SkipOceanFishing) {
                        LogExemption("RiskySkips");
                        return 1L;
                    }
                    return _msqHook!.Original(luaState);

                case TerritoryIntendedUse.CrystallineConflict:
                case TerritoryIntendedUse.CrystallineConflictCustomMatch:
                    if (_config.SkipCrystallineConflict) {
                        LogExemption("RiskySkips");
                        return 1L;
                    }
                    return _msqHook!.Original(luaState);

                case TerritoryIntendedUse.Frontline:
                case TerritoryIntendedUse.RivalWings:
                    return _msqHook!.Original(luaState);
            }

            bool inMSQTerritory = Array.IndexOf(TerritoryPrae, territory) >= 0 || Array.IndexOf(TerritoryCastrum, territory) >= 0 || Array.IndexOf(TerritoryPorta, territory) >= 0;

            if (_config.ResearchMSQHook) {
                LogExemption("Dev_MSQHook");
                return 1L;
            }

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

            if (_config.ResearchGoldSaucerHook) {
                LogExemption("Dev_GoldSaucerHook");
                return 1L;
            }

            if (_config.SkipGoldSaucer) {
                if (Array.IndexOf(GoldSaucerIntendedUses, use) < 0) {
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

                if (!exempt && Array.IndexOf(TerritoryMahjong, territory) >= 0) {
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

                if (!skip && Array.IndexOf(TerritoryMahjong, territory) >= 0) {
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
            if (_config.ResearchMassivePCHook) {
                LogExemption("Dev_MassivePCHook");
                return 1L;
            }

            var use = GetIntendedUse((ushort)_clientState.TerritoryType);

            if (use == TerritoryIntendedUse.CosmicExploration) {
                if (!_config.SkipCosmicExploration)
                    return _massivePCHook!.Original(luaState);

                LogExemption("MassivePC");
                return 1L;
            }

            if (!_config.SkipMassivePC)
                return _massivePCHook!.Original(luaState);

            LogExemption("MassivePC");
            return 1L;
        }

        private long ExemptionCustomTalk(nint luaState) {
            if (_config.ResearchCustomTalkHook) {
                LogExemption("Dev_CustomTalkHook");
                return 1L;
            }

            LogExemption("CustomTalk");
            return 1L;
        }

        private long ExemptionNormalCutscenes(nint luaState1, nint luaState2) {
            if (_config.ResearchNormalCutscenesHook) {
                LogExemption("Dev_NormalCutscenesHook");
                return 1L;
            }

            bool isWorkshop = IsWorkshopTerritory();
            var use = GetIntendedUse((ushort)_clientState.TerritoryType);

            if (use == TerritoryIntendedUse.OceanFishing && !_config.SkipOceanFishing)
                return _normalCutscenesHook!.Original(luaState1, luaState2);

            // Cosmic Exploration triggers via MassivePC & NormalCutscenes but are part of MassivePC
            if (use == TerritoryIntendedUse.CosmicExploration) {
                if (!_config.SkipCosmicExploration)
                    return _normalCutscenesHook!.Original(luaState1, luaState2);

                LogExemption("MassivePC");
                return 1L;
            }

            if (_config.SkipNormalCutscenes) {
                if (_config.ExemptSubmarines && isWorkshop) {
                    return _normalCutscenesHook!.Original(luaState1, luaState2);
                }
            } else {
                if (!(_config.ExemptSubmarines && isWorkshop)) {
                    return _normalCutscenesHook!.Original(luaState1, luaState2);
                }
            }

            // Submarines trigger via NormalCutscenes but are part of CustomTalk
            LogExemption(isWorkshop ? "CustomTalk" : "NormalCutscenes");
            return 1L;
        }

        private long ExemptionInn(nint luaState) {
            if (_config.ResearchInnHook) {
                LogExemption("Dev_InnHook");
                return 1L;
            }

            if (!_config.SkipInn)
                return _innHook!.Original(luaState);

            LogExemption("RiskySkips");
            return 1L;
        }

        private long ExemptionFeedBuddy(nint luaState) {
            if (_config.ResearchFeedBuddyHook) {
                LogExemption("Dev_FeedBuddyHook");
                return 1L;
            }

            if (!_config.SkipFeedBuddy)
                return _feedBuddyHook!.Original(luaState);

            LogExemption("FeedBuddy");
            return 1L;
        }
    }
}