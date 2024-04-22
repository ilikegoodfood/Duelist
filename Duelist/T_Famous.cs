using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duelist
{
    public class T_Famous : Trait
    {
        public T_Famous()
        {

        }

        public override string getName()
        {
            return "Rich and Famous";
        }

        public override string getDesc()
        {
            return "This person gains 200 gold, 80 profile, 20 minimum profile, and a random person of their society gains a liking and a disliking for them.";
        }

        public override void onAcquire(Person person)
        {
            person.addGold(200);

            if (person.unit != null)
            {
                person.unit.inner_profile = 80;
                person.unit.inner_profileMin = 20;
            }

            if (person.society != null && person.society != person.map.soc_dark && person.society.people.Count > 1)
            {
                int id1 = -1;
                int personID;
                Person person2;
                for (int i = 0; i < 10; i++)
                {
                    personID = person.society.people[Eleven.random.Next(person.society.people.Count)];
                    person2 = person.map.persons[personID];

                    if (personID != person.index && !person2.isDead)
                    {
                        person2.decreasePreference(person.index + 10000);
                        id1 = personID;
                        break;
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    personID = person.society.people[Eleven.random.Next(person.society.people.Count)];
                    person2 = person.map.persons[personID];

                    if (personID != person.index && personID != id1 && !person2.isDead)
                    {
                        person2.increasePreference(person.index + 10000);
                        break;
                    }
                }
            }
        }
    }
}
