using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Duelist
{
    public class UAE_Duelist : UAE
    {
        public UAE_Duelist(Location loc, Society sg)
            : base(loc, sg)
        {
            if (sg is Soc_Elven)
            {
                person.species = map.species_elf;
            }

            person.stat_might = 4;
            person.stat_lore = 2;
            person.stat_intrigue = 3;
            person.stat_command = 1;
            person.isMale = true;
            person.age = 21;
            person.hasSoul = true;
            person.embedIntoSociety();

            corrupted = true;
        }

        public override bool definesName()
        {
            return true;
        }

        public override string getName()
        {
            if (person.overrideName != null && person.overrideName.Length != 0)
            {
                return person.overrideName;
            }

            return "Duelist " + person.firstName;
        }

        public override bool isCommandable()
        {
            return corrupted;
        }

        public override bool hasStartingTraits()
        {
            return true;
        }

        public override List<Trait> getStartingTraits()
        {
            return new List<Trait>
            { 
                new T_Famous(),
                new T_HonourableDuel()
            };
        }

        public override Sprite getPortraitBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getPortraitForeground()
        {
            if (person.species == map.species_elf)
            {
                return EventManager.getImg("Duelist.duelist_elf.png");
            }

            return EventManager.getImg("Duelist.duelist_human.png"); ;
        }
    }
}
