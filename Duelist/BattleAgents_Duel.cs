using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Duelist
{
    public class BattleAgents_Duel : BattleAgents
    {
        public BattleAgents_Duel(UA att, UA def)
            : base(att, def)
        {

        }

        public void setup()
        {
            escortL = null;
            escortR = null;

            for (int i = 0; i < att.minions.Count(); i++)
            {
                if (att.minions[i] != null)
                {
                    attStartingHP -= att.minions[i].hp;
                    attStartingHP -= att.minions[i].defence;
                }

                if (def.minions[i] != null)
                {
                    defStartingHP -= def.minions[i].hp;
                    defStartingHP -= att.minions[i].defence;
                }

                getGraphical().minionsAttFore[i].sprite = att.map.world.textureStore.clear;
                getGraphical().minionsAttBack[i].sprite = att.map.world.textureStore.clear;
                getGraphical().minionsDefFore[i].sprite = def.map.world.textureStore.clear;
                getGraphical().minionsDefBack[i].sprite = def.map.world.textureStore.clear;
            }

            if (att.minions.Any(m => m != null) || def.minions.Any(m => m != null))
            {
                getGraphical().addMsg("Minions may not participate in a duel", Color.red);

                getGraphical().attMoveUp.enabled = false;
                getGraphical().attMoveDown.enabled = false;
                getGraphical().defMoveUp.enabled = false;
                getGraphical().defMoveDown.enabled = false;
            }
        }

        public bool stepAlt(PopupBattleAgent popup)
        {
            switch(state)
            {
                case 0:
                    attack(getAttackStatLimited(att), att, def, popup);
                    state++;
                    break;
                case 1:
                    attack(getAttackStatLimited(def), def, att, popup);
                    state++;
                    break;
                default:
                    state = 0;
                    round++;
                    popup?.addMsg("Round " + round, new Color(0f, 0.7f, 1f));
                    break;
            }

            if (att.isDead)
            {
                World.log("Agent Comat " + att.getName() + " is dead. Combat ends");
                outcome = BattleAgents.OUTCOME_DEATH_ATT;
                return true;
            }
        }

        public int getAttackStatLimited(UA ua)
        {
            int result = ua.person.stat_might + (ua.person.level / 2);
            foreach (Trait trait in ua.person.traits)
            {
                trait.getAttackChange();
            }

            return Math.Max(0, result);
        }

        public void attack(int dmg, UA me, UA them, PopupBattleAgent popup)
        {
            foreach (ModKernel modKernel in me.map.mods)
            {
                dmg += modKernel.onAgentAttackAboutToBePerformed(me, me, them, popup, dmg, 0);
            }

            int rolloverDmg = Math.Max(0, dmg - them.defence);
            int initDefence = them.defence;
            them.defence -= dmg;
            them.hp -= rolloverDmg;

            me.launchAttack(this, me, them, rolloverDmg, initDefence);

            popup?.addMsg(me.getName() + " deals " + dmg + " (" + rolloverDmg + ") to " + them.getName(), Color.white);

            if (them.hp <= 0)
            {
                popup?.addMsg(them.getName() + " is killed by " + me.getName(), Color.red);
                them.isDead = true;
                me.person.statistic_kills++;
                me.onBattleKill(this, me, them, me, them);
            }
        }

        public void bStep()
        {
            getGraphical().minionsAttFore[i].sprite = att.map.world.textureStore.clear;
            getGraphical().minionsAttBack[i].sprite = att.map.world.textureStore.clear;
            getGraphical().minionsDefFore[i].sprite = def.map.world.textureStore.clear;
            getGraphical().minionsDefBack[i].sprite = def.map.world.textureStore.clear;
        }
    }
}
