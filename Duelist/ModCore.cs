using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duelist
{
    public class ModCore : ModKernel
    {
        private static ModCore modCore;

        private static CommunityLib.ModCore comLib;

        private Dictionary<UA, Ch_ChallengeToDuel> duelChallenges;

        public List<Tuple<UA, UA>> pendingDuels;

        public List<T_Champion> champions;

        public static ModCore Get() => modCore;

        public static CommunityLib.ModCore GetComLib() => comLib;

        public override void beforeMapGen(Map map)
        {
            modCore = this;

            Get().getModKernels(map);

            Get().duelChallenges = new Dictionary<UA, Ch_ChallengeToDuel>();
            Get().pendingDuels = new List<Tuple<UA, UA>>();
            Get().champions = new List<T_Champion>();
        }

        public override void afterLoading(Map map)
        {
            modCore = this;

            Get().getModKernels(map);

            Get().duelChallenges = new Dictionary<UA, Ch_ChallengeToDuel>();

            if (Get().pendingDuels == null)
            {
                Get().pendingDuels = new List<Tuple<UA, UA>>();
            }

            if (Get().champions == null)
            {
                Get().champions = new List<T_Champion>();
            }

            foreach (Location location in map.locations)
            {
                if (location.settlement is  SettlementHuman settlementHuman)
                {
                    foreach (Challenge challenge in settlementHuman.customChallenges)
                    {
                        if (challenge is Ch_ChallengeToDuel duel)
                        {
                            if (!duelChallenges.ContainsKey(duel.target))
                            {
                                duelChallenges.Add(duel.target, duel);
                            }
                        }
                    }
                }
            }
        }

        private void getModKernels(Map map)
        {
            foreach (ModKernel kernel in map.mods)
            {
                if (kernel is CommunityLib.ModCore core)
                {
                    comLib = core;

                    comLib.RegisterHooks(new ComLibHooks(map));
                    break;
                }
            }
        }

        public override void afterMapGenAfterHistorical(Map map)
        {
            map.overmind.agentsGeneric.Add(new UAE_Abstraction_Duelist(map));
        }

        public override void onTurnEnd(Map map)
        {
            Get().pendingDuels.Clear();
        }

        public override void onTurnStart(Map map)
        {
            if (map.burnInComplete)
            {
                foreach (Unit unit in map.units)
                {
                    if (unit is UA ua && ua.homeLocation != -1 && !ua.isCommandable() && !(ua is UAEN) && ua.person != null && ua != map.awarenessManager.chosenOne)
                    {
                        Location home = map.locations[unit.homeLocation];
                        if (home.settlement is SettlementHuman settlementHuman)
                        {
                            Ch_ChallengeToDuel duel = (Ch_ChallengeToDuel)settlementHuman.customChallenges.FirstOrDefault(ch => ch is Ch_ChallengeToDuel d && d.target == ua);
                            if (duel == null)
                            {
                                duel = new Ch_ChallengeToDuel(home, ua);
                                settlementHuman.customChallenges.Add(duel);
                            }

                            if (!duelChallenges.ContainsKey(ua))
                            {
                                duelChallenges.Add(ua, duel);
                            }
                        }
                    }
                }

                List<UA> keysToRemove = new List<UA>();
                foreach (KeyValuePair<UA, Ch_ChallengeToDuel> kvp in duelChallenges)
                {
                    if (kvp.Key.isDead || !kvp.Key.location.units.Contains(kvp.Key) || kvp.Key.isCommandable() || kvp.Key == map.awarenessManager.chosenOne)
                    {
                        keysToRemove.Add(kvp.Key);

                        if (kvp.Value.location.settlement is SettlementHuman settlementHuman)
                        {
                            Ch_ChallengeToDuel duel = (Ch_ChallengeToDuel)settlementHuman.customChallenges.FirstOrDefault(ch => ch is Ch_ChallengeToDuel d && d.target == kvp.Key);
                            if (duel != null)
                            {
                                settlementHuman.customChallenges.Remove(duel);
                            }
                        }
                    }
                }

                foreach (UA key in keysToRemove)
                {
                    duelChallenges.Remove(key);
                }
            }

            foreach (T_Champion champion in champions.ToList())
            {
                if (champion.person == null || !champion.person.traits.Contains(champion))
                {
                    champions.Remove(champion);
                }
            }
        }

        public override void onPersonRecycled(Person person)
        {
            foreach (T_Champion champion in champions.ToList())
            {
                if (champion.person == person)
                {
                    champions.Remove(champion);
                    break;
                }
            }
        }

        public override void onAgentAIDecision(UA uA)
        {
            if (uA.task is Task_PerformChallenge tChallenge && tChallenge.challenge is Rt_ChampionDuel duel)
            {
                duel.onImmediateBegin(uA);
            }
        }

        public override string interceptCombatOutcomeEvent(string currentlyChosenEvent, UA victor, UA defeated, BattleAgents battleAgents)
        {
            if (battleAgents is BattleAgents_Duel duel)
            {
                if (defeated.isDead)
                {
                    if (victor.isCommandable())
                    {
                        if (victor == duel.att)
                        {
                            return "duelist.duelVictoryAttackingLethal";
                        }
                        else
                        {
                            return "duelist.duelVictoryDefendingLethal";
                        }
                    }
                    else if (defeated.isCommandable())
                    {
                        if (defeated == duel.att)
                        {
                            return "duelist.duelDefeatAttackingLethal";
                        }
                        else
                        {
                            return "duelist.duelDefeatDefendingLethal";
                        }
                    }
                }
                else
                {
                    if (victor.isCommandable())
                    {
                        if (victor == duel.att)
                        {
                            return "duelist.duelVictoryAttackingRetreat";
                        }
                        else
                        {
                            return "duelist.duelVictoryDefendingRetreat";
                        }
                    }
                    else if (defeated.isCommandable())
                    {
                        if (defeated == duel.att)
                        {
                            return "duelist.duelDefeatAttackingRetreat";
                        }
                        else
                        {
                            return "duelist.duelDefeatDefendingRetreat";
                        }
                    }

                    defeated.map.adjacentMoveTo(defeated, victor.location);
                    World.log(defeated.getName() + " does not retreat when surrendering a duel. Returning them to " + victor.location.getName(true));
                }
            }

            return currentlyChosenEvent;
        }

        public override void onAgentBattleTerminate(BattleAgents battleAgents)
        {
            if (battleAgents is BattleAgents_Duel duel)
            {
                if (duel.outcome == BattleAgents.OUTCOME_RETREAT_ATT && !duel.att.isDead)
                {
                    duel.att.map.adjacentMoveTo(duel.att, duel.def.location);
                    World.log(duel.att.getName() + " does not retreat when surrendering a duel. Returning them to " + duel.def.location.getName(true));
                }
                else if (duel.outcome == BattleAgents.OUTCOME_RETREAT_DEF && !duel.def.isDead)
                {
                    duel.att.map.adjacentMoveTo(duel.def, duel.att.location);
                    World.log(duel.def.getName() + " does not retreat when surrendering a duel. Returning them to " + duel.att.location.getName(true));
                }
            }
        }
    }
}
