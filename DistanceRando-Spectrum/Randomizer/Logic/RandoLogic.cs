using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistanceRando.Randomizer.Logic
{
    class RandoLogic
    {

        public Dictionary<string, RandoMap> GenerateGame(int seed)
        {
            System.Random random = new System.Random(seed);

            bool isJumpEnabled = false;
            bool isWingsEnabled = false;
            bool isJetsEnabled = false;

            bool canFly = false;

            Dictionary<string, RandoMap> mapsToReturn = new Dictionary<string, RandoMap>();

            List<string> availableMaps = new List<string>(){ "Cataclysm", "Diversion", "Euphoria", "Entanglement", "Automation",
                                                "Abyss", "Embers", "Isolation", "Repulsion", "Compression", "Research", "Contagion",
                                                "Overload", "Ascension"};

            List<Ability> availAbilities = new List<Ability> { Ability.Jump, Ability.Wings, Ability.Jets };
            List<Ability> abilityOrder = new List<Ability>();
            List<Ability> trackedAbilities = new List<Ability>();
            while (abilityOrder.Count < 3)
            {
                if (availAbilities.Count == 3)
                {
                    var index = random.Next(0, 3);
                    abilityOrder.Add(availAbilities[index]);
                    Console.WriteLine(index);
                    availAbilities.RemoveAt(index);
                }
                else if (availAbilities.Count == 2)
                {
                    var index = random.Next(0, 2);
                    abilityOrder.Add(availAbilities[index]);
                    Console.WriteLine(index);
                    availAbilities.RemoveAt(index);
                }
                else if (availAbilities.Count == 1)
                {
                    abilityOrder.Add(availAbilities[0]);
                    Console.WriteLine(0);
                    availAbilities.RemoveAt(0);
                }
            }
            foreach (var a in abilityOrder)
            {
                Console.WriteLine(a);
            }

            while (mapsToReturn.Count != 14)
            {
                int index = random.Next(0, availableMaps.Count);
                Console.WriteLine($"{availableMaps.Count} - {index}");
                var map = availableMaps[index];

                if (isWingsEnabled || isJetsEnabled)
                {
                    canFly = true;
                }

                Console.WriteLine($"{map} - {mapsToReturn.Count}");
                if (!mapsToReturn.ContainsKey(map))
                {
                    var logicInfo = MapLogicInfo.GetMapLogicInfo(map);

                    //Console.WriteLine($"{map} - {logicInfo.abilityCompleteRequirement.ToString()} - {logicInfo.unlocksAbility} - {logicInfo.abilityUnlockRequirement}");
                    Console.WriteLine($"{isJumpEnabled} - {isJetsEnabled} - {isWingsEnabled} - {canFly}");

                    if (logicInfo.unlocksAbility && trackedAbilities.Count < 3)
                    {
                        Console.WriteLine(trackedAbilities.Count);
                        var ability = abilityOrder[trackedAbilities.Count];
                        Console.WriteLine($"{ability.ToString()} - {isJumpEnabled} - {isWingsEnabled} - {isJetsEnabled} - {canFly}");

                        if (IsMapBeatable(isJumpEnabled, canFly, canFly, true, ability, logicInfo))
                        {
                            trackedAbilities.Add(ability);
                            mapsToReturn.Add(map, new RandoMap(ability, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
                            
                            switch (ability)
                            {
                                case Ability.Jump:
                                    isJumpEnabled = true;
                                    break;
                                case Ability.Wings:
                                    isWingsEnabled = true;
                                    break;
                                case Ability.Jets:
                                    isJetsEnabled = true;
                                    break;
                            }

                            availableMaps.Remove(map);
                            Console.WriteLine($"added {map}");
                        }
                    }
                    else
                    {
                        if (IsMapBeatable(isJumpEnabled, isWingsEnabled, isJetsEnabled, false, Ability.None, logicInfo))
                        {
                            // map should be in logic
                            mapsToReturn.Add(map, new RandoMap(Ability.None, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
                            availableMaps.Remove(map);
                            Console.WriteLine($"added {map}");
                        }
                    }
                }
            }

            return mapsToReturn;
        }
        
        bool CheckLogicRequirementsAreMet(bool jump, bool wings, bool jets, LogicRequirement[] logicRequirements)
        {
            // Check if all individual logic requirements have been met, allowing for the map to be completed.
            int count = 0;

            foreach (var logicReq in logicRequirements)
            {
                if (logicReq.IsRequirementMet(jump, wings, jets))
                {
                    count++;
                }
            }

            if (count == logicRequirements.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IsMapBeatable(bool jump, bool wings, bool jets, bool mapUnlocksAbility, Ability abilityUnlocked, MapLogicInfo logicInfo)
        {
            if (mapUnlocksAbility)
            {
                // First check if ability trigger can be reached

                var triggerReachable = CheckLogicRequirementsAreMet(jump, wings, jets, logicInfo.AbilityUnlockRequirements);
                
                if (triggerReachable)
                {
                    // Now check if the map is beatable with the unlocked ability.
                    bool result;
                    switch (abilityUnlocked)
                    {
                        case Ability.Jump:
                            result = CheckLogicRequirementsAreMet(true, wings, jets, logicInfo.MapCompletionRequirements);
                            break;
                        case Ability.Wings:
                            result = CheckLogicRequirementsAreMet(jump, true, jets, logicInfo.MapCompletionRequirements);
                            break;
                        case Ability.Jets:
                            result = CheckLogicRequirementsAreMet(jump, wings, true, logicInfo.MapCompletionRequirements);
                            break;
                        default:
                            result = CheckLogicRequirementsAreMet(jump, wings, jets, logicInfo.MapCompletionRequirements);
                            break;
                    }

                    // Return the result.
                    return result;
                }
                else
                {
                    // Cannot reach trigger, so the map is not completable.
                    return false;
                }
            }
            else
            {
                // There's no ability unlock on this map, so we can just check to see if the map itself is beatable.
                return CheckLogicRequirementsAreMet(jump, wings, jets, logicInfo.MapCompletionRequirements);
            }
        }
    }
}
