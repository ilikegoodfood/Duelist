using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duelist
{
    public class T_HonourableDuel : Trait
    {
        public int cooldown;

        public T_HonourableDuel()
        {
            cooldown = 0;
        }

        public override string getName()
        {
            return "Honourable Duelist";
        }

        public override string getDesc()
        {
            return "Through a call to honour and a silver tongue, the duelist si able to convince would be asailants to settle their differences over a duel, rather than all out combat." + (cooldown > 0? " (disabled for " + cooldown + " turns)" : "");
        }

        public override void turnTick(Person p)
        {
            if (cooldown > 0)
            {
                cooldown--;
            }
        }

        public void use()
        {
            cooldown = 25;
        }
    }
}
