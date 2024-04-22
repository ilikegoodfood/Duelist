using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duelist
{
    public class Task_AttendDuel : Task_AttackUnit
    {
        public bool flipRoles;

        public Task_AttendDuel(Unit self, Unit target, bool flipRoles = false)
            : base (self, target)
        {
            turnsRemaining += 5;
            this.flipRoles = flipRoles;
        }

        public override string getShort()
        {
            return "Attending Duel";
        }

        public override string getLong()
        {
            return "This agent is off to answer a challenge to a duel by " + target.getName() + ", they have " + turnsRemaining + " turns until they abandon the effort.";
        }

        public override void turnTick(Unit unit)
        {
            if (target == null || target.isDead || !target.location.units.Contains(target) || unit.isCommandable() || unit == unit.map.awarenessManager.chosenOne)
            {
                unit.task = null;
                return;
            }

            turnsRemaining--;
            if (turnsRemaining <= 0)
            {
                if (unit is UA ua)
                {
                    ua.challengesSinceRest += 2;

                    if (Eleven.random.Next(4) == 0)
                    {
                        ua.person.decreasePreference(target.personID + 10000);
                        unit.map.addUnifiedMessage(unit, target, target.getName() + " fled Duel", unit.getName() + " has made all reasonable haste to answer " + target.getName() + "'s call to a duel, but " + target.getName() + " has abandonned honour, and fled the challenge they themselves made. " + unit.getName() + " has lost respect for them.", "Fled Duel");
                    }
                }

                unit.task = null;
                return;
            }

            if (unit.location != target.location)
            {
                while (unit.movesTaken < unit.getMaxMoves())
                {
                    if (!unit.map.moveTowards(unit, target.location))
                    {
                        World.log("Move unsuccessful. Cancelling go to challenge");
                        unit.task = null;
                        return;
                    }
                    unit.movesTaken++;

                    if (unit.location == target.location)
                    {
                        break;
                    }
                }
            }

            if (unit.location == target.location)
            {
                if (target.engagedBy == null && target.turnLastEngaged != unit.map.turn)
                {
                    if (unit is UA self && target is UA other)
                    {
                        if (self.isCommandable() || other.isCommandable())
                        {
                            if (unit.map.automatic)
                            {
                                if (flipRoles)
                                {
                                    BattleAgents_Duel duel = new BattleAgents_Duel(self, other);
                                    duel.automatic();
                                }
                                else
                                {
                                    BattleAgents_Duel duel = new BattleAgents_Duel(other, self);
                                    duel.automatic();
                                }
                            }
                            else
                            {
                                if (flipRoles)
                                {
                                    unit.engaging = target;
                                    target.engagedBy = unit;
                                    unit.turnLastEngaged = unit.map.turn;
                                    target.turnLastEngaged = unit.map.turn;
                                    ModCore.Get().pendingDuels.Add(new Tuple<UA, UA>(self, other));
                                }
                                else
                                {
                                    target.engaging = unit;
                                    unit.engagedBy = target;
                                    unit.turnLastEngaged = unit.map.turn;
                                    target.turnLastEngaged = unit.map.turn;
                                    ModCore.Get().pendingDuels.Add(new Tuple<UA, UA>(other, self));
                                }
                            }
                        }
                        else
                        {
                            if (flipRoles)
                            {
                                new BattleAgents_Duel(self, other)
                                {
                                    attackReason = "They were challenged to a duel by " + other.getName()
                                }.automatic();
                                self.addProfile(unit.map.param.ua_profileFromAttacking * 2 / 3);
                                self.addMenace(unit.map.param.ua_menaceFromAttacking / 3);
                            }
                            else
                            {
                                new BattleAgents_Duel(other, self)
                                {
                                    attackReason = "They were challenged to a duel by " + other.getName()
                                }.automatic();
                                other.addProfile((unit.map.param.ua_profileFromAttacking * 2) / 3);
                            }
                        }
                    }
                    unit.task = null;
                }
            }
        }
    }
}
