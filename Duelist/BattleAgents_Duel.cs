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

            att.defence = getDefenceLimited(att);
            def.defence = getDefenceLimited(def);

            attStartingHP = att.hp + att.defence;
            defStartingHP = def.hp + def.defence;
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
                outcome = OUTCOME_DEATH_ATT;
                return true;
            }

            if (def.isDead)
            {
                World.log("Agent Comat " + def.getName() + " is dead. Combat ends");
                outcome = OUTCOME_DEATH_DEF;
                return true;
            }

            if (state == 0 && round > 0)
            {
                if (checkAIRetreat(popup))
                {
                    return true;
                }
            }

            return skirmish && round == 2;
        }

        public int getAttackStatLimited(UA ua)
        {
            int result = ua.person.stat_might + (ua.person.level / 2);
            foreach (Trait trait in ua.person.traits)
            {
                result += trait.getAttackChange();
            }

            return Math.Max(0, result);
        }

        public int getDefenceLimited(UA ua)
        {
            int result = ua.innerDefence;
            foreach (Trait trait in ua.person.traits)
            {
                result += trait.getDefenceChange();
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

        public new void automatic()
        {
            for (int i = 0; i < 128; i++)
            {
                if (stepAlt(null))
                {
                    break;
                }
            }

            if (skirmish && round == 2)
            {
                skirmishEnd();
                return;
            }

            if (att.isDead)
            {
                victory(def, att);
            }
            else if (def.isDead)
            {
                victory(att, def);
            }
            else
            {
                terminate();
            }
        }

        public void populatePopup(PopupBattleAgent popupBattle)
        {
            for (int i = 0; i < att.minions.Count(); i++)
            {
                popupBattle.attMinionNames[i].text = "";
                popupBattle.attMinionStats[i].text = "";
                popupBattle.minionsAttFore[i].sprite = att.map.world.textureStore.clear;
                popupBattle.minionsAttBack[i].sprite = att.map.world.textureStore.clear;

                popupBattle.defMinionNames[i].text = "";
                popupBattle.defMinionStats[i].text = "";
                popupBattle.minionsDefFore[i].sprite = def.map.world.textureStore.clear;
                popupBattle.minionsDefBack[i].sprite = def.map.world.textureStore.clear;
            }
        }
    }
}
