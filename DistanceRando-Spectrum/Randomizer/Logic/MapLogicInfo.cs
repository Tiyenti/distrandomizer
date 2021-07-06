using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistanceRando.Randomizer.Logic
{

    public enum AbilityRequirement { None, Jump, WingsJets, JumpWingsJets, JumpOrFlight }

    public class MapLogicInfo
    {
        public bool unlocksAbility = false;
        public LogicRequirement[] AbilityUnlockRequirements;
        public LogicRequirement[] MapCompletionRequirements;

        [Obsolete("Use the newer LogicRequirement[] constructor instead.", true)]
        public MapLogicInfo(bool unlocksAbilitiy, AbilityRequirement unlock, AbilityRequirement completion)
        {
            this.unlocksAbility = unlocksAbilitiy;

            //abilityUnlockRequirement = unlock;
            //abilityCompleteRequirement = completion;
        }

        public MapLogicInfo(bool unlocksAbility, LogicRequirement[] unlock, LogicRequirement[] completion)
        {
            this.unlocksAbility = unlocksAbility;

            AbilityUnlockRequirements = unlock;
            MapCompletionRequirements = completion;
        }

        public static MapLogicInfo GetMapLogicInfo(string name)
        {
            if (name == "Cataclysm")
            {
                return new MapLogicInfo(true, AbilityRequirement.None, AbilityRequirement.None);
            }
            else if (name == "Diversion")
            {
                return new MapLogicInfo(true, AbilityRequirement.None, AbilityRequirement.None);
            }
            else if (name == "Euphoria")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.JumpOrFlight);
            }
            else if (name == "Entanglement")
            {
                return new MapLogicInfo(true, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "Automation")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "Abyss")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.JumpOrFlight);
            }
            else if (name == "Embers")
            {
                return new MapLogicInfo(true, AbilityRequirement.JumpOrFlight, AbilityRequirement.WingsJets);
            }
            else if (name == "Isolation")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "Repulsion")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "Compression")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "Research")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.JumpWingsJets);
            }
            else if (name == "Contagion")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.JumpWingsJets);
            }
            else if (name == "Overload")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "Ascension")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.None);
            }
        }
    }
}
