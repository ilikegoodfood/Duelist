using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duelist
{
    public class ComLibHooks : CommunityLib.Hooks
    {
        public ComLibHooks(Map map)
            : base(map)
        {

        }

        public override BattleAgents onPlayerStartsPendingAgentBattle(UA agent, UA att, UA def)
        {
            Tuple<UA, UA> pair = new Tuple<UA, UA>(att, def);
            if (ModCore.Get().pendingDuels.Contains(pair))
            {
                ModCore.Get().pendingDuels.Remove(pair);
                return new BattleAgents_Duel(att, def);
            }

            return null;
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
    }
}
