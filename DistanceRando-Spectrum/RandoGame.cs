using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DistanceRando
{
    class RandoGame
    {
        internal Dictionary<string, RandoMap> maps = new Dictionary<string, RandoMap>();

        internal bool jumpShouldBeEnabled = false;
        internal bool wingsShouldBeEnabled = false;
        internal bool jetsShouldBeEnabled = false;

        internal int seed = 0;

        byte[] rawHash;
        string seedHash = "";
        internal string truncSeedHash = "";
        internal string friendlyHash = "";

        public RandoGame(string inputSeed, string randoVersion)
        {
            int integerSeed;
            try
            {
                integerSeed = int.Parse(inputSeed.Trim());
            }
            catch (FormatException)
            {
                integerSeed = inputSeed.Trim().GetHashCode();
            }

            this.seed = integerSeed;

            Randomize();

            rawHash = GenerateSeedHash(randoVersion, maps);
            seedHash = ConvertHashToString(rawHash);
            truncSeedHash = seedHash.Truncate(7);
            friendlyHash = FriendlyHash(rawHash);
        }

        byte[] GenerateSeedHash(string version, Dictionary<string, RandoMap> mapList)
        {
            string mapsString = "";
            foreach (var map in mapList)
            {
                mapsString += map.Key +
                            map.Value.abilityEnabled +
                            map.Value.boostEnabled +
                            map.Value.jumpEnabled +
                            map.Value.wingsEnabled +
                            map.Value.jetsEnabled;
            }

            byte[] hashBytes = Encoding.UTF8.GetBytes(version + mapsString);

            //string hash = "";
            byte[] hash;
            using (SHA256 sha = SHA256.Create())
            {
                hash = sha.ComputeHash(hashBytes);
            }

            return hash;
        }

        string ConvertHashToString(byte[] hash)
        {
            StringBuilder strBuilder = new StringBuilder();

            foreach (var b in hash)
            {
                strBuilder.Append(b.ToString("x2"));
            }

            return strBuilder.ToString();
        }

        string FriendlyHash(byte[] hash)
        {
            string truncHash = Convert.ToBase64String(hash).Truncate(4);

            string friendlyHash = "";

            foreach (char l in truncHash)
            {
                try
                {
                    friendlyHash += $"{Metadata.FriendlyHashWords[l]} ";
                }
                catch (KeyNotFoundException)
                {
                    friendlyHash += $"{l} ";
                }
            }

            return friendlyHash;
        }

        void Randomize()
        {
            System.Random random = new System.Random(seed);

            bool isJumpEnabled = false;
            bool isWingsEnabled = false;
            bool isJetsEnabled = false;

            bool canFly = false;

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

            while (maps.Count != 14)
            {
                int index = random.Next(0, availableMaps.Count);
                Console.WriteLine($"{availableMaps.Count} - {index}");
                var map = availableMaps[index];

                if (isWingsEnabled || isJetsEnabled)
                {
                    canFly = true;
                }

                Console.WriteLine($"{map} - {maps.Count}");
                if (!maps.ContainsKey(map))
                {
                    var logicInfo = MapLogicInfo.GetMapLogicInfo(map);

                    Console.WriteLine($"{map} - {logicInfo.abilityCompleteRequirement.ToString()} - {logicInfo.unlocksAbility} - {logicInfo.abilityUnlockRequirement}");
                    Console.WriteLine($"{isJumpEnabled} - {isJetsEnabled} - {isWingsEnabled} - {canFly}");
                    if (logicInfo.unlocksAbility)
                    {
                        if (!isJumpEnabled && logicInfo.abilityUnlockRequirement == AbilityRequirement.Jump)
                        {
                            Console.WriteLine($"no jump");
                            // can't complete the map, no jump
                            continue;
                        }
                        else if (!canFly && logicInfo.abilityUnlockRequirement == AbilityRequirement.WingsJets)
                        {
                            Console.WriteLine($"no flight");
                            // can't complete the map, no flight
                            continue;
                        }
                        else if (logicInfo.abilityCompleteRequirement == AbilityRequirement.JumpWingsJets && (!canFly || !isJumpEnabled))
                        {
                            Console.WriteLine($"no jump or flight");
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
                                        maps.Add(map, new RandoMap(Ability.Jump, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
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
                                        maps.Add(map, new RandoMap(ability, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
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
                                maps.Add(map, new RandoMap(Ability.None, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
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
                            maps.Add(map, new RandoMap(Ability.None, true, isJumpEnabled, isWingsEnabled, isJetsEnabled));
                            availableMaps.Remove(map);
                            Console.WriteLine($"added {map}");
                        }
                    }
                }
            }
        }
    }
}
