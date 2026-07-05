using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Duelist
{
    public class ComLibHooks
    {
        public ComLibHooks(Map map, CommunityLib.HooksDelegateRegistry registry)
        {
            registry.RegisterHook_onMoveTaken(onMoveTaken);
            registry.RegisterHook_onAgentBattleStarts(onAgentBattleStarts);
            registry.RegisterHook_onAgentBattle_Setup(onAgentBattle_Setup);
            registry.RegisterHook_interceptAgentBattleStep(interceptAgentBattleStep);
            registry.RegisterHook_onPopupBattleAgent_Populate(onPopupBattleAgent_Populate);
            registry.RegisterHook_onAgentAI_EndOfProcess(onAgentAI_EndOfProcess);
        }

        public void onMoveTaken(Unit u, Location locA, Location locB)
        {
            foreach (T_Champion champion in ModCore.Get().champions)
            {
                if (champion.person != null && champion.person.unit != null && champion.person.unit.location == locB)
                {
                    champion.populateDuels();
                }
            }
        }

        public BattleAgents onAgentBattleStarts(UA att, UA def)
        {
            //Console.WriteLine("Duelist: Agent Battle Started");
            Tuple<UA, UA> pair = new Tuple<UA, UA>(att, def);
            if (ModCore.Get().pendingDuels.Contains(pair))
            {
                //Console.WriteLine("Duelist: Pending Duel Started");
                ModCore.Get().pendingDuels.Remove(pair);
                return new BattleAgents_Duel(att, def);
            }

            if (def.person != null)
            {
                T_HonourableDuel duel = (T_HonourableDuel)def.person.traits.FirstOrDefault(t => t is T_HonourableDuel);
                if (duel != null && duel.cooldown <= 0)
                {
                    //Console.WriteLine("Duelist: Duel Started");
                    duel.use();
                    return new BattleAgents_Duel(att, def);
                }
            }

            return null;
        }

        public void onAgentBattle_Setup(BattleAgents battle)
        {
            //Console.WriteLine("Duelist: Battle setup underway");
            if (battle is BattleAgents_Duel duel)
            {
                //Console.WriteLine("Duelist: Battle is Duel");
                duel.setup();
            }
        }

        public bool interceptAgentBattleStep(PopupBattleAgent popupBattle, BattleAgents battle, out bool battleOver)
        {
            //Console.WriteLine("Duelist: Battle intercept step hook called");
            if (battle is BattleAgents_Duel duel)
            {
                //Console.WriteLine("Duelist: Intercepting duel");
                battleOver = duel.stepAlt(popupBattle);
                return true;
            }

            battleOver = false;
            return false;
        }

        public void onPopupBattleAgent_Populate(PopupBattleAgent popupBattle, BattleAgents battle)
        {
            //Console.WriteLine("Duelist: Popup Battle populate hook called");
            if (battle is BattleAgents_Duel duel)
            {
                //Console.WriteLine("Duelist: Battle is Duel");
                duel.populatePopup(popupBattle);
            }
        }

        public void onAgentAI_EndOfProcess(UA ua, CommunityLib.AgentAI.AIData aiData, List<CommunityLib.AgentAI.ChallengeData> validChallengeData, List<CommunityLib.AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
        {
            if (ua.task is Task_PerformChallenge tChallenge && tChallenge.challenge is Rt_ChampionDuel duel)
            {
                duel.onImmediateBegin(ua);
            }
            else if (ua.task is Task_GoToPerformChallenge tGoChallenge && tGoChallenge.challenge is Rt_ChampionDuel duel2)
            {
                duel2.onImmediateBegin(ua);
            }
        }
    }
}
