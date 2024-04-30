using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace Duelist
{
    public class ComLibHooks : CommunityLib.Hooks
    {
        public ComLibHooks(Map map)
            : base(map)
        {

        }

        public override void onMoveTaken(Unit u, Location locA, Location locB)
        {
            foreach (T_Champion champion in ModCore.Get().champions)
            {
                if (champion.person != null && champion.person.unit != null && champion.person.unit.location == locB)
                {
                    champion.populateDuels();
                }
            }
        }

        public override BattleAgents onAgentBattleStarts(UA att, UA def)
        {
            Tuple<UA, UA> pair = new Tuple<UA, UA>(att, def);
            if (ModCore.Get().pendingDuels.Contains(pair))
            {
                ModCore.Get().pendingDuels.Remove(pair);
                return new BattleAgents_Duel(att, def);
            }

            if (def.person != null)
            {
                T_HonourableDuel duel = (T_HonourableDuel)def.person.traits.FirstOrDefault(t => t is T_HonourableDuel);
                if (duel != null && duel.cooldown <= 0)
                {
                    duel.use();
                    return new BattleAgents_Duel(att, def);
                }
            }

            return null;
        }

        public override bool interceptAgentBattleAutomatic(BattleAgents battle)
        {
            if (battle is BattleAgents_Duel duel)
            {
                duel.automatic();
                return true;
            }

            return false;
        }

        public override void onAgentBattle_Setup(BattleAgents battle)
        {
            //Console.WriteLine("Duelist: Battle setup hook called");
            if (battle is BattleAgents_Duel duel)
            {
                //Console.WriteLine("Duelist: Battle is Duel");
                duel.setup();
            }
        }

        public override bool interceptAgentBattleStep(PopupBattleAgent popupBattle, BattleAgents battle, out bool battleOver)
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

        public override void onPopupBattleAgent_Populate(PopupBattleAgent popupBattle, BattleAgents battle)
        {
            //Console.WriteLine("Duelist: Popup Battle populate hook called");
            if (battle is BattleAgents_Duel duel)
            {
                //Console.WriteLine("Duelist: Battle is Duel");
                duel.populatePopup(popupBattle);
            }
        }

        public override void onAgentAI_EndOfProcess(UA ua, CommunityLib.AgentAI.AIData aiData, List<CommunityLib.AgentAI.ChallengeData> validChallengeData, List<CommunityLib.AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
        {
            if (ua.task is Task_PerformChallenge tChallenge && tChallenge.challenge is Rt_ChampionDuel duel)
            {
                duel.onImmediateBegin(ua);
            }
        }
    }
}
