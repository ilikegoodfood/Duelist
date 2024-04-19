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

        public override void onAgentBattle_Setup(BattleAgents battle)
        {
            if (battle is BattleAgents_Duel duel)
            {
                duel.setup();
            }
        }

        public override bool interceptAgentBattleStep(PopupBattleAgent popupBattle, BattleAgents battle, out bool battleOver)
        {
            if (battle is BattleAgents_Duel duel)
            {
                battleOver = duel.stepAlt(popupBattle);
                return true;
            }

            battleOver = false;
            return false;
        }

        public override void onPopupBattleAgent_Step(PopupBattleAgent popupBattle, BattleAgents battle)
        {
            if (battle is BattleAgents_Duel duel)
            {
                duel.bStep();
            }
        }
    }
}
