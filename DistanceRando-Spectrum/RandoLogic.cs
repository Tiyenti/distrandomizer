using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistanceRando
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

                    Console.WriteLine($"{map} - {logicInfo.abilityCompleteRequirement.ToString()} - {logicInfo.unlocksAbility} - {logicInfo.abilityUnlockRequirement}");
                    Console.WriteLine($"{isJumpEnabled} - {isJetsEnabled} - {isWingsEnabled} - {canFly}");
                    if (logicInfo.unlocksAbility)
                    {
                        if (!isJumpEnabled && logicInfo.abilityUnlockRequirement == AbilityRequirement.Jump)
                        {
                            Console.WriteLine($"map not in logic; needs jump for the triggerf, which is disabled");
                            // can't complete the map, no jump
                            continue;
                        }
                        else if (!canFly && logicInfo.abilityUnlockRequirement == AbilityRequirement.WingsJets)
                        {
                            Console.WriteLine($"map not in logic; needs flight for the trigger, which is disabled");
                            // can't complete the map, no flight
                            continue;
                        }
                        else if (logicInfo.abilityCompleteRequirement == AbilityRequirement.JumpWingsJets && (!canFly || !isJumpEnabled))
                        {
                            Console.WriteLine($"map not in logic; needs jump and flight for the trigger, and one or both is disabled");
                            // can't complete the map, no nothing
                            continue;
                        }
                        else if (logicInfo.abilityCompleteRequirement == AbilityRequirement.JumpOrFlight && !(canFly || isJumpEnabled))
                        {
                            Console.WriteLine($"map not in logic; needs jump or flight for the trigger, of which neither are enabled");
                            // can't complete the map, no nothing
                            continue;
                        }
                        else
                        {
                            // map possibly in logic
                            // one last check
                            if (trackedAbilities.Count < 3)
                            {
                                Console.WriteLine(trackedAbilities.Count);
                                var ability = abilityOrder[trackedAbilities.Count];
                                Console.WriteLine($"{ability.ToString()} - {isJumpEnabled} - {isWingsEnabled} - {isJetsEnabled} - {canFly}");
                                if (ability == Ability.Jump)
                                {
                                    if (logicInfo.abilityCompleteRequirement == AbilityRequirement.JumpWingsJets && (!canFly || !isJumpEnabled))
                                    {
                                        // this can be improved, right now even if you could beat the map by reaching the ability trigger
                                        // and getting the ability you're missing at it, the logic will still ditch it anyway.
                                        // doesn't really matter for the current officials as most of them that would benefit
                                        // from this require jumpflight to get the trigger anyway, but still.

                                        // nope move along
                                        Console.WriteLine($"did not add. requries jump+flight to beat, canFly = {canFly}, " +
                                            $"canJump = {isJumpEnabled} and the enabled ability is {ability.ToString()}");
                                        continue;
                                    }
                                    else if (logicInfo.abilityCompleteRequirement == AbilityRequirement.WingsJets && !canFly)
                                    {
                                        // nope move along
                                        Console.WriteLine($"did not add. requries flight to beat, canFly = {canFly}, and the enabled ability is {ability.ToString()}");
                                        continue;
                                    }
                                    else
                                    {
                                        // yep
                                        trackedAbilities.Add(ability);
                                        mapsToReturn.Add(map, new RandoMap(Ability.Jump, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
                                        isJumpEnabled = true;
                                        availableMaps.Remove(map);
                                        Console.WriteLine($"added {map}");
                                    }
                                }
                                else if ((ability == Ability.Wings || ability == Ability.Jets))
                                {
                                    if (logicInfo.abilityCompleteRequirement == AbilityRequirement.Jump && !isJumpEnabled)
                                    {
                                        // nope move along
                                        Console.WriteLine($"did not add. requries jump to beat, canJump = {isJumpEnabled}, and the enabled ability is {ability.ToString()}");
                                        continue;

                                    }
                                    else
                                    {
                                        // yep
                                        trackedAbilities.Add(ability);
                                        mapsToReturn.Add(map, new RandoMap(ability, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
                                        if (ability == Ability.Wings) isWingsEnabled = true; else isJetsEnabled = true;
                                        availableMaps.Remove(map);
                                        Console.WriteLine($"added {map}");
                                    }
                                }
                            }
                            else
                            {
                                // yep
                                Console.WriteLine($"added {map}");
                                mapsToReturn.Add(map, new RandoMap(Ability.None, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
                                availableMaps.Remove(map);
                            }
                        }
                    }
                    else
                    {
                        if (!isJumpEnabled && logicInfo.abilityCompleteRequirement == AbilityRequirement.Jump)
                        {
                            Console.WriteLine($"didn't add! map needs jump, and it is not enabled");
                            // can't complete the map, no jump
                            continue;
                        }
                        else if (!canFly && logicInfo.abilityCompleteRequirement == AbilityRequirement.WingsJets)
                        {
                            Console.WriteLine($"didn't add! map needs flight, and wings/jets are not enabled");
                            // can't complete the map, no flight
                            continue;
                        }
                        else if ((logicInfo.abilityCompleteRequirement == AbilityRequirement.JumpWingsJets && (!canFly || !isJumpEnabled)))
                        {
                            // nope move along
                            Console.WriteLine($"didn't add! map needs jump AND flight, and one or both are not enabled");
                            continue;
                        }
                        else if ((logicInfo.abilityCompleteRequirement == AbilityRequirement.JumpOrFlight && !(canFly || isJumpEnabled)))
                        {
                            // nope move along
                            Console.WriteLine($"didn't add! map needs jump OR flight, and neither are enabled");
                            continue;
                        }
                        else
                        {
                            // map probably in logic, so let's add it
                            mapsToReturn.Add(map, new RandoMap(Ability.None, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
                            availableMaps.Remove(map);
                            Console.WriteLine($"added {map}");
                        }
                    }
                }
            }

            return mapsToReturn;
        }
    }
}
