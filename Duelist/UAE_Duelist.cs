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
            person.stat_lore = 1;
            person.stat_intrigue = 3;
            person.stat_command = 1;
            person.isMale = true;
            person.age = 21;
            person.hasSoul = true;
            person.gold = 0;
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
                new T_HonourableDuel(),
                new T_Champion()
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
                if (ModCore.opt_brightPortraits)
                {
                    return EventManager.getImg("Duelist.duelist_elf.png");
                }

                return EventManager.getImg("Duelist.duelist_dimmed_elf.png");
            }

            if (person.species == map.species_dwarf)
            {
                if (ModCore.opt_brightPortraits)
                {
                    return EventManager.getImg("Duelist.duelist_dwarf.png");
                }

                return EventManager.getImg("Duelist.duelist_dimmed_dwarf.png");
            }

            if (ModCore.opt_brightPortraits)
            {
                return EventManager.getImg("Duelist.duelist_human.png");
            }

            return EventManager.getImg("Duelist.duelist_dimmed_human.png"); ;
        }
    }
}
