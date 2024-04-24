using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duelist
{
    public class T_Champion : Trait
    {
        public Person person = null;

        public List<Rt_ChampionDuel> duels;

        public override string getName()
        {
            return "Grand Champion";
        }

        public override string getDesc()
        {
            return "As a renowned champion, you may stand in for others in combat. When in the same location as a heroe who is on their way to attack another agent, you may choose to act as the champion for their target. This lowers their target's profile and menace (not below minimum). Whether the target appreciates your actions, is another matter entirely.";
        }

        public override void onAcquire(Person person)
        {
            this.person = person;
            duels = new List<Rt_ChampionDuel>();

            ModCore.Get().champions.Add(this);

            populateDuels();
        }

        public override void onMove(Location current, Location dest)
        {
            populateDuels();
        }

        public override void turnTick(Person p)
        {
            populateDuels();
        }

        public void populateDuels()
        {
            if (person.unit != null)
            {
                foreach (Rt_ChampionDuel duel in duels)
                {
                    person.unit.rituals.Remove(duel);
                }
                duels.Clear();

                foreach (Unit u in person.unit.location.units)
                {
                    if (!(u is UA ua) || u is UAE || ua.isDead || u == person.unit || u.isCommandable())
                    {
                        continue;
                    }

                    if ((ua.task is Task_AttackUnit att && att.target is UA) || (ua.task is Task_AttackUnitWithEscort attEscort && attEscort.target is UA))
                    {
                        Rt_ChampionDuel duel = (Rt_ChampionDuel)person.unit.rituals.FirstOrDefault(rt => rt is Rt_ChampionDuel cd && cd.target == u);
                        if (duel == null)
                        {
                            duel = new Rt_ChampionDuel(person.unit.location, ua);
                            person.unit.rituals.Add(duel);
                            duels.Add(duel);
                        }
                    }
                }
            }
        }
    }
}
