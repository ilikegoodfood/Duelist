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

            //Console.WriteLine("Duelist: Grand Champion Aquired");
            populateDuels();
        }

        public override void onMove(Location current, Location dest)
        {
            //Console.WriteLine("Duelist: Grand Champion moved");
            populateDuels(dest);
        }

        public override void turnTick(Person p)
        {
            //Console.WriteLine("Duelist: Grand Champion turn ticked");
            populateDuels();
        }

        public void populateDuels(Location loc = null)
        {
            //Console.WriteLine("Duelist: Populating duels");
            if (person.unit != null)
            {
                if (loc == null)
                {
                    loc = person.unit.location;
                }

                //Console.WriteLine("Duelist: Grand Champion has unit");
                foreach (Rt_ChampionDuel duel in duels)
                {
                    person.unit.rituals.Remove(duel);
                }
                duels.Clear();
                //Console.WriteLine("Duelist: Cleaned old duels from Grand Champion");

                foreach (Unit u in loc.units)
                {
                    if (!(u is UA ua) || u is UAE || ua.isDead || u == person.unit || u.isCommandable())
                    {
                        //Console.WriteLine("Duelist: Found inval;id unit");
                        continue;
                    }

                    if ((ua.task is Task_AttackUnit att && att.target is UA) || (ua.task is Task_AttackUnitWithEscort attEscort && attEscort.target is UA))
                    {
                        //Console.WriteLine("Duelist: Found valid Unit");
                        Rt_ChampionDuel duel = (Rt_ChampionDuel)person.unit.rituals.FirstOrDefault(rt => rt is Rt_ChampionDuel cd && cd.target == u);
                        if (duel == null)
                        {
                            //Console.WriteLine("Duelist: Created new duel");
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
