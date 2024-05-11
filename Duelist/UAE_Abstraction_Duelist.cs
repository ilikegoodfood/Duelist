using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Duelist
{
    public class UAE_Abstraction_Duelist : UAE_Abstraction
    {
        public UAE_Abstraction_Duelist(Map map)
            : base(map, -1)
        {
            this.map = map;
        }

        public override string getName()
        {
            return "A Duelist";
        }

        public override string getDesc()
        {
            return "The Duelist is one of many cocky young nobles with a penchant for theatrics and violence, easily swayed with the promise of power and wealth. A Duelist has high stats in might and intrigue. A Duelist can also challenge heroes to duels from their home locations, which triggers combat that excludes items and minions.";
        }

        public override string getFlavour()
        {
            return "Beauty, education, and wealth. A fine combination of traits for a young noble to possess. Ambition, arrogance, and pride. Ones that make them so easy to manipulate. All it takes is a promise. Keeping that promise barely matters at all.";
        }

        public override string getRestrictions()
        {
            return "Requires a city";
        }

        public override Sprite getBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getForeground()
        {
            if (ModCore.opt_oldBrightPortraits)
            {
                return EventManager.getImg("Duelist.duelist_oldbright_human.png");
            }

            if (ModCore.opt_oldPortraits)
            {
                return EventManager.getImg("Duelist.duelist_old_human.png");
            }

            return EventManager.getImg("Duelist.duelist_human.png");
        }

        public override int getStatMight()
        {
            return 4;
        }

        public override int getStatLore()
        {
            return 1;
        }

        public override int getStatIntrigue()
        {
            return 3;
        }

        public override int getStatCommand()
        {
            return 1;
        }

        public override bool validTarget(Location loc)
        {
            if (World.allowAllAgents)
            {
                return true;
            }

            if (map.overmind.nEnthralled >= map.overmind.getAgentCap())
            {
                return false;
            }

            return loc.settlement is Set_City || loc.settlement is Set_ElvenCity;
        }

        public override void createAgent(Location target)
        {
            if (target.map.overmind.god is God_Eternity brokenMaker)
            {
                brokenMaker.agentBuffer.Add(this);
            }

            UAE_Duelist duelist = null;

            if (target.soc is Society society)
            {
                duelist = new UAE_Duelist(target, society);
            }
            else
            {
                duelist = new UAE_Duelist(target, map.soc_dark);
            }

            if (target.settlement is Set_ElvenCity)
            {
                duelist.person.species = map.species_elf;
            }
            else
            {
                duelist.person.species = map.species_human;
            }

            duelist.person.stat_might = getStatMight();
            duelist.person.stat_lore = getStatLore();
            duelist.person.stat_intrigue = getStatIntrigue();
            duelist.person.stat_command = getStatCommand();

            map.units.Add(duelist);
            target.units.Add(duelist);

            map.overmind.agents.Add(duelist);
            map.overmind.availableEnthrallments--;

            duelist.person.shadow = 1.0;
            duelist.person.skillPoints++;
            duelist.person.state = Person.personState.enthralled;

            duelist.person.hates.Clear();
            duelist.person.extremeHates.Clear();
            duelist.person.likes.Clear();
            duelist.person.extremeLikes.Clear();

            if (target.settlement is SettlementHuman settlementHuman && target.index == duelist.homeLocation)
            {
                if (!settlementHuman.fundingActions.Any(fA => fA.heroIndex == duelist.person.index))
                {
                    settlementHuman.fundingActions.Add(new Act_FundHero(target, duelist.person));
                }
            }

            if (!map.automatic)
            {
                GraphicalMap.selectedUnit = duelist;
                map.world.prefabStore.popAgentLevelUp(duelist);
                GraphicalMap.panTo(target.hex);
            }

            if (map.overmind.god is God_Tutorial2)
            {
                map.tutorialManager.state2 = ManagerTutorial.STATE_B2_INFILTRATE_VILLAGE;
            }
        }
    }
}
