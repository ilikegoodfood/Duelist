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
            else if (sg is Soc_Dwarves)
            {
                person.species = map.species_dwarf;
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

            return "Duelist";
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

            };
        }

        public override Sprite getPortraitBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getPortraitForeground()
        {
            return map.world.textureStore.agent_courtier;
        }
    }
}
