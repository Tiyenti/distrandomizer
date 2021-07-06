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
                return new MapLogicInfo(unlocksAbility: true,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) });
            }
            else if (name == "Diversion")
            {
                return new MapLogicInfo(unlocksAbility: true,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) });
            }
            else if (name == "Euphoria")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] {
                                            new OrAbilityRequirement(Ability.Jump, new OrAbilityRequirement(Ability.Wings, Ability.Jets)
                                        ) });
            }
            else if (name == "Entanglement")
            {
                return new MapLogicInfo(unlocksAbility: true,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new OrAbilityRequirement(Ability.Wings, Ability.Jets) });
            }
            else if (name == "Automation")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new OrAbilityRequirement(Ability.Wings, Ability.Jets) });
            }
            else if (name == "Abyss")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] {
                                            new OrAbilityRequirement(Ability.Jump, new OrAbilityRequirement(Ability.Wings, Ability.Jets)
                                        ) });
            }
            else if (name == "Embers")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] {
                                            new OrAbilityRequirement(Ability.Jump, new OrAbilityRequirement(Ability.Wings, Ability.Jets))
                                        },
                                        completion: new LogicRequirement[] { new OrAbilityRequirement(Ability.Wings, Ability.Jets) });
            }
            else if (name == "Isolation")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new OrAbilityRequirement(Ability.Wings, Ability.Jets) });
            }
            else if (name == "Repulsion")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new OrAbilityRequirement(Ability.Wings, Ability.Jets) });
            }
            else if (name == "Compression")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new OrAbilityRequirement(Ability.Wings, Ability.Jets) });
            }
            else if (name == "Research")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[]
                                            { new OrAbilityRequirement(Ability.Wings, Ability.Jets), 
                                              new SingleAbilityRequirement(Ability.Jump),
                                            });
            }
            else if (name == "Contagion")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[]
                                            { new OrAbilityRequirement(Ability.Wings, Ability.Jets),
                                              new SingleAbilityRequirement(Ability.Jump),
                                            });
            }
            else if (name == "Overload")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new OrAbilityRequirement(Ability.Wings, Ability.Jets) });
            }
            else if (name == "Ascension")
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new OrAbilityRequirement(Ability.Wings, Ability.Jets) });
            }
            else
            {
                return new MapLogicInfo(unlocksAbility: false,
                                        unlock: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) },
                                        completion: new LogicRequirement[] { new SingleAbilityRequirement(Ability.None) });
            }
        }
    }
}
