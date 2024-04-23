using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Duelist
{
    public class Ch_ChallengeToDuel : Challenge
    {
        public UA target;

        public Ch_ChallengeToDuel(Location loc, UA target)
            : base (loc)
        {
            this.target = target;
        }

        public override string getName()
        {
            return "Duel " + target.getName();
        }

        public override string getDesc()
        {
            return target.getName() + " will immediately drop whatever they are doing and attempt to meet up with their challenger. Upon reaching their challenger, they will start a Duel, with the challenger going first. A Duel is a unique form of agent combat where minions, items, and bodyguards are not allowed.";
        }

        public override string getRestriction()
        {
            return "Requires > 0 % Infiltration.Hero must not currently be in, or on their way to, battle.";
        }

        public override string getCastFlavour()
        {
            return "A noble house approached. A fine white glove thrown to the ground. An insult exchanged. A challenge decreed.";
        }

        public override Sprite getSprite()
        {
            return target.getPortraitForeground();
        }

        public override double getProfile()
        {
            return map.param.ch_poisonhero_aiProfile;
        }

        public override double getMenace()
        {
            return map.param.ch_poisonhero_aiMenace;
        }

        public override Challenge.challengeStat getChallengeType()
        {
            return Challenge.challengeStat.INTRIGUE;
        }

        public override bool valid()
        {
            return !target.isDead && !target.isCommandable() && !(target.task is Task_AttackUnit) && !(target.task is Task_AttackUnitWithEscort) && target != target.map.awarenessManager.chosenOne && location.settlement is SettlementHuman && location.settlement.infiltration > 0.0 && target.engagedBy == null && target.engaging == null;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAE_Duelist;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Stat: Intrigue", (double)Math.Max(1, unit.getStatIntrigue())));
            return Math.Max(1, unit.getStatIntrigue());
        }

        public override void complete(UA u)
        {
            Task_AttendDuel duel = new Task_AttendDuel(target, u);
            target.task = duel;
            target.task.turnTick(target);
            msgString = target.getName() + " is on their way to duel " + u.getName() + ". If they can't reach them within " + duel.turnsRemaining + " turns, they will consider " + u.getName() + " to have fled the duel, and may gain a disliking towards them.";
        }

        public override double getComplexity()
        {
            return 15;
        }

        public override int getCompletionProfile()
        {
            return (map.param.ua_profileFromAttacking * 2) / 3;
        }

        public override int getCompletionMenace()
        {
            return map.param.ua_menaceFromAttacking / 3;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
                {
                    Tags.CRUEL,
                    Tags.MIGHT
                };
        }

        public override int[] buildNegativeTags()
        {
            if (target?.person != null)
            {
                return new int[]
            {
                Tags.COOPERATION,
                target.personID + 10000
            };
            }

            return new int[]
            {
                Tags.COOPERATION
            };
        }
    }
}
