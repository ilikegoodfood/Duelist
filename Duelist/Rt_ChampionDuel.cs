using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace Duelist
{
    public class Rt_ChampionDuel : Ritual
    {
        public UA target;

        public Rt_ChampionDuel(Location loc, UA target)
            : base(loc)
        {
            this.target = target;
        }

        public override string getName()
        {
            string name = "Champion for ";

            if (target.task is Task_AttackUnit attack && attack.target != null)
            {
                name += attack.target.getName();
            }
            else if (target.task is Task_AttackUnitWithEscort attackWithEscort && attackWithEscort.target != null)
            {
                name += attackWithEscort.target.getName();
            }

            if (name == "Champion for ")
            {
                return "Serve as Champion";
            }

            return name;
        }

        public override string getDesc()
        {
            UA victim = null;
            if (target.task is Task_AttackUnit attack && attack.target is UA ua)
            {
                victim = ua;
            }
            else if (target.task is Task_AttackUnitWithEscort attackWithEscort && attackWithEscort.target is UA ua2)
            {
                victim = ua2;
            }

            if (victim != null)
            {
                return "Acting as the champion for " + victim.getName() + ", challenge " + target.getName() + " to a duel. Immediately start a duel as the defender, against " + target.getName() + ". " + victim.getName() + " will loose some menace and profile (not below minimum).";
            }

            return "When in the same location as a hero that is on their way to attack another agent, you may stand in as the champion of that agent. When you do, you immediately start a duel, with you as the defender, with the hero who is travelling to attack the agent, and the agent looses some profile and menace.";
        }

        public override string getRestriction()
        {
            return "Must be in the same location as at least one agent who is on their way to attack another.";
        }

        public override string getCastFlavour()
        {
            return "With the target many weeks away, who is to say if you truly are their champion, or if they will ever discover that you stood in for them?";
        }

        public override Sprite getSprite()
        {
            UA victim = null;
            if (target.task is Task_AttackUnit attack && attack.target is UA ua)
            {
                victim = ua;
            }
            else if (target.task is Task_AttackUnitWithEscort attackWithEscort && attackWithEscort.target is UA ua2)
            {
                victim = ua2;
            }

            if (victim != null)
            {
                return victim.getPortraitForeground();
            }

            return target.getPortraitForeground();
        }

        public override double getProfile()
        {
            return 0;
        }

        public override double getMenace()
        {
            return target.menace;
        }

        public override Challenge.challengeStat getChallengeType()
        {
            return Challenge.challengeStat.OTHER;
        }

        public override bool valid()
        {
            return !target.isDead && !target.isCommandable() && ((target.task is Task_AttackUnit att && att.target is UA) || (target.task is Task_AttackUnitWithEscort attEscort && attEscort.target is UA)) && target.engagedBy == null && target.engaging == null;
        }

        public override bool validFor(UM ua)
        {
            return false;
        }

        public override bool validFor(UA ua)
        {
            return ua.person != null && ua.person.traits.Any(t => t is T_Champion);
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Base", 1.0));
            return 1.0;
        }

        public override void onImmediateBegin(UA ua)
        {
            ua.addProfile(map.param.ua_profileFromAttacking * 2 / 3);

            UA victim = null;
            if (target.task is Task_AttackUnit att && att.target is UA victim1)
            {
                victim = victim1;
            }
            else if (target.task is Task_AttackUnitWithEscort attEscort && attEscort.target is UA victim2)
            {
                victim = victim2;
            }

            string reason = ua.getName() + " is standing in for ";
            if (victim != null)
            {
                reason += victim.getName();

                victim.inner_profile = Math.Max(victim.inner_profileMin, victim.inner_profile - 10);
                victim.inner_menace = Math.Max(victim.inner_menaceMin, victim.inner_menace - 10);

                if (victim.person != null)
                {
                    int combatLikeTernary = 0;
                    int championLikeTernary = 0;

                    if (victim.person.likes.Contains(Tags.COMBAT) || victim.person.extremeLikes.Contains(Tags.COMBAT))
                    {
                        combatLikeTernary = 1;
                    }
                    else if (victim.person.hates.Contains(Tags.COMBAT) || victim.person.extremeHates.Contains(Tags.COMBAT))
                    {
                        combatLikeTernary = -1;
                    }

                    if (ua.person != null)
                    {
                        if (victim.person.likes.Contains(ua.personID + 10000) || victim.person.extremeLikes.Contains(ua.personID + 10000))
                        {
                            championLikeTernary = 1;
                        }
                        else if (victim.person.hates.Contains(ua.personID + 10000) || victim.person.extremeHates.Contains(ua.personID + 10000))
                        {
                            championLikeTernary = -1;
                        }
                    }

                    if (Eleven.random.Next(4) == 0)
                    {
                        string msg = victim.getName() + " has heard of your actions on their behalf and ";
                        if (championLikeTernary > 0)
                        {
                            victim.person.increasePreference(ua.personID + 10000);
                            msg += "appreciates them. Their liking for you has increased.";
                        }
                        else if (championLikeTernary < 0)
                        {
                            victim.person.decreasePreference(ua.personID + 10000);
                            msg += "does not appreciate them at all. Their disliking for you has increased.";
                        }
                        else if (combatLikeTernary > 0)
                        {
                            victim.person.decreasePreference(ua.personID + 10000);
                            msg += "does not appreciate them at all. They have come to like you less.";
                        }
                        else if (combatLikeTernary < 0)
                        {
                            victim.person.increasePreference(ua.personID + 10000);
                            msg += "appreciates them. They have come to like you more.";
                        }
                    }
                }
                else
                {
                    reason += "an unknown third party (ERROR)";
                }
            }

            if (ua.isCommandable() || target.isCommandable())
            {
                if (ua.map.automatic)
                {
                    BattleAgents_Duel duel = new BattleAgents_Duel(target, ua);
                    duel.automatic();
                }
                else
                {
                    target.engaging = ua;
                    ua.engagedBy = target;
                    ua.turnLastEngaged = ua.map.turn;
                    target.turnLastEngaged = ua.map.turn;

                    if (ModCore.GetComLib().data.isPlayerTurn)
                    {
                        ua.map.world.prefabStore.popBattle(new BattleAgents_Duel(target, ua));
                    }
                    else
                    {
                        ModCore.Get().pendingDuels.Add(new Tuple<UA, UA>(target, ua));
                    }
                }
            }
            else
            {
                new BattleAgents_Duel(target, ua)
                {
                    attackReason = reason
                }.automatic();
            }

            ua.task = null;
        }

        public override double getComplexity()
        {
            return 0;
        }

        public override int getCompletionProfile()
        {
            return map.param.ua_profileFromAttacking * 2 / 3;
        }

        public override int getCompletionMenace()
        {
            return 0;
        }

        public override int[] buildPositiveTags()
        {
            UA victim = null;
            if (target != null)
            {
                if (target.task is Task_AttackUnit att && att.target is UA victim1)
                {
                    victim = victim1;
                }
                else if (target.task is Task_AttackUnitWithEscort attEscort && attEscort.target is UA victim2)
                {
                    victim = victim2;
                }
            }

            if (victim != null && victim.person != null)
            {
                return new int[]
                {
                    Tags.COMBAT,
                    Tags.CRUEL,
                    Tags.MIGHT,
                    victim.person.index + 10000
                };
            }

            return new int[]
                {
                    Tags.COMBAT,
                    Tags.CRUEL,
                    Tags.MIGHT
                };
        }

        public override int[] buildNegativeTags()
        {
            if (target != null && target.person != null)
            {
                return new int[]
                {
                    target.person.index + 10000
                };
            }

            return new int[0];
        }
    }
}
