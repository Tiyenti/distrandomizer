using System;
using System.Collections.Generic;
using System.Linq;
using Spectrum.API;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Interfaces.Plugins;
using UnityEngine;
using Spectrum.Interop.Game;
using System.IO;
using Spectrum.Interop.Game.Vehicle;
using System.Reflection;
using System.Diagnostics;

namespace DistanceRando
{

    public class Entry : IPlugin
    {
        Dictionary<string, RandoMap> maps = new Dictionary<string, RandoMap>();

        bool started = false;
        bool startGame = false;
        bool singleRaceStarted = false;

        bool jumpShouldBeEnabled = false;
        bool wingsShouldBeEnabled = false;
        bool jetsShouldBeEnabled = false;

        int seed = 0;

        List<string> availableMaps = new List<string>(){ "Broken Symmetry", "Lost Society", "Negative Space", "Departure", "Ground Zero",
                                                "Aftermath", "Friction", "The Thing About Machines", "Corruption", "Monolith" };
        Ability[] abilities = new Ability[] { Ability.Jump, Ability.Wings, Ability.Jets };

        FileVersionInfo version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        public void Initialize(IManager manager, string ipcIdentifier)
        {
            MainMenu.Loaded += (sender, args) =>
            {
                if (G.Sys.GameManager_.FirstMainMenuLoad_)
                {
                    manager.Hotkeys.Bind("R", () => {
                        if (Game.SceneName == "MainMenu" && G.Sys.GameManager_.SoloAndNotOnline_)
                        {
                            if (!G.Sys.MenuPanelManager_.TrackmogrifyMenuLogic_.trackmogrifyInput_.isSelected)
                            {
                                G.Sys.MenuPanelManager_.TrackmogrifyMenuLogic_.Display((inputSeed, isRandom) =>
                                {
                                    if (isRandom)
                                    {
                                        seed = new System.Random().Next(0, int.MaxValue);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            seed = int.Parse(inputSeed);
                                        }
                                        catch (FormatException)
                                        {
                                            seed = inputSeed.GetHashCode();
                                        }
                                    }
                                    G.Sys.MenuPanelManager_.Pop();
                                    G.Sys.MenuPanelManager_.ShowTimedOk(10, $"Rando seed has been set to:\n[FF0000]{inputSeed}[-]\n" +
                                        "Start the [FF0000]Intro[-] map in Adventure mode to begin.", "Rando enabled");
                                    startGame = true;
                                    Game.WatermarkText =
                                        $"RANDOMIZER ENABLED - SEED: {inputSeed}\n" +
                                        $"DISTANCE {SystemVersion.DistanceBuild} - RANDOMIZER {version.FileVersion}";
                                });
                            }
                        }
                    });
                }
                else
                {
                    maps = new Dictionary<string, RandoMap>();
                    started = false;
                    startGame = false;
                    jumpShouldBeEnabled = false;
                    wingsShouldBeEnabled = false;
                    jetsShouldBeEnabled = false;
                    availableMaps = new List<string>(){ "Broken Symmetry", "Lost Society", "Negative Space", "Departure", "Ground Zero",
                                                "Aftermath", "Friction", "The Thing About Machines", "Corruption", "Monolith" };
                }
            };

            Events.Scene.BeginSceneSwitchFadeOut.Subscribe((data) => {
                // Yes. We are intercepting a map load. I know it's bad, but this is the only way to get this to work I could find :P
                if (startGame)
                {
                    Console.WriteLine(G.Sys.GameManager_.NextLevelPathRelative_);
                    if (G.Sys.GameManager_.NextLevelPathRelative_ == "OfficialLevels/adventureintro.bytes")
                    {
                        StartRandoGame();
                    }
                    else
                    {
                        startGame = false;
                    }
                }
            });

            Race.Started += (sender, args) =>
            {
                if (started)
                {
                    ApplyRandoChanges();
                    singleRaceStarted = true;
                    CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
                    Console.WriteLine("Start event fired");
                    Console.WriteLine($"Jump {car.Jump_.AbilityEnabled_} - Wings {car.Wings_.AbilityEnabled_} - Jets {car.Jets_.AbilityEnabled_}");
                }
            };

            Race.Finished += (sender, args) =>
            {
                if (started) singleRaceStarted = false;
            };

            LocalVehicle.Exploded += (sender, args) =>
            {
                if (started)
                {
                    RandoMap map = maps[Game.LevelName];
                    CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
                    if (car.Jump_.AbilityEnabled_ && !map.jumpEnabled)
                    {
                        jumpShouldBeEnabled = true;
                    }
                    if (car.Wings_.AbilityEnabled_ && !map.wingsEnabled)
                    {
                        wingsShouldBeEnabled = true;
                    }
                    if (car.Jets_.AbilityEnabled_ && !map.jetsEnabled)
                    {
                        jetsShouldBeEnabled = true;
                    }
                }
            };

            LocalVehicle.Respawned += (sender, args) =>
            {
                if (started)
                {
                    Console.WriteLine("Respawn event fired");
                    CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
                    try
                    {
                        RandoMap map = maps[Game.LevelName];
                        if (!singleRaceStarted)
                        {
                            jumpShouldBeEnabled = map.jumpEnabled;
                            wingsShouldBeEnabled = map.wingsEnabled;
                            jetsShouldBeEnabled = map.jetsEnabled;
                        }
                        car.Boost_.AbilityEnabled_ = map.boostEnabled;
                        car.Jump_.AbilityEnabled_ = jumpShouldBeEnabled;
                        car.Wings_.AbilityEnabled_ = wingsShouldBeEnabled;
                        car.Jets_.AbilityEnabled_ = jetsShouldBeEnabled;
                        Console.WriteLine($"Jump {car.Jump_.AbilityEnabled_} - Wings {car.Wings_.AbilityEnabled_} - Jets {car.Jets_.AbilityEnabled_}");

                    }
                    catch (KeyNotFoundException)
                    {
                        // this should only ever happen on Credits or Destination Unknown, so just enable everything
                        car.Boost_.AbilityEnabled_ = true;
                        car.Jump_.AbilityEnabled_ = true;
                        car.Wings_.AbilityEnabled_ = true;
                        car.Jets_.AbilityEnabled_ = true;
                    }

                    car.GetComponent<HornGadget>().enabled = true;
                    car.GetComponent<LocalPlayerControlledCar>().showBackToResetWarning_ = false;
                }
            };

            
        }

        public void Shutdown()
        {

        }

        void DisableAbilities()
        {
            RandoMap map = maps[Game.LevelName];

            G.Sys.GameManager_.Level_.Settings_.disableBoosting_ = !map.boostEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableJumping_ = !map.jumpEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableFlying_ = !map.wingsEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableJetRotating_ = !map.jetsEnabled;
        }

        void ApplyRandoChanges()
        {
            if (Game.LevelName == "Destination Unknown" || Game.LevelName == "Credits")
            {
                return;
            }

            RandoMap map = maps[Game.LevelName];

            G.Sys.GameManager_.Level_.Settings_.disableBoosting_ = !map.boostEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableJumping_ = !map.jumpEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableFlying_ = !map.wingsEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableJetRotating_ = !map.jetsEnabled;

            CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
            /*car.Boost_.AbilityEnabled_ = map.boostEnabled;
            car.Jump_.AbilityEnabled_ = map.jumpEnabled;
            car.Wings_.AbilityEnabled_ = map.wingsEnabled;
            car.Jets_.AbilityEnabled_ = map.jetsEnabled;*/

            jumpShouldBeEnabled = map.jumpEnabled;
            wingsShouldBeEnabled = map.wingsEnabled;
            jetsShouldBeEnabled = map.jetsEnabled;
            car.GetComponent<HornGadget>().enabled = true;

            foreach (var obj in UnityEngine.Object.FindObjectsOfType<InfoDisplayLogic>())
            {
                if (obj.gameObject.name == "InfoDisplayBox" || obj.gameObject.name == "InfoAndIndicatorDisplayBox")
                {
                    GameObject.Destroy(obj.gameObject);
                }
                else
                {
                    obj.gameObject.RemoveComponent<InfoDisplayLogic>();
                }
            }

            foreach (var obj in UnityEngine.Object.FindObjectsOfType<WingCorruptionZone>())
            {
                GameObject.Destroy(obj.gameObject);
            }

            foreach (var obj in UnityEngine.Object.FindObjectsOfType<AdventureAbilitySettings>())
            {
                GameObject.Destroy(obj.gameObject);
            }

            if (map.abilityEnabled != Ability.None)
            {
                Console.WriteLine($"enables {map.abilityEnabled.ToString()}");
                EnableAbilitiesTrigger trigger = GameObject.Find("EnableAbilitiesBox").GetComponent<EnableAbilitiesTrigger>();

                if (map.abilityEnabled == Ability.Jump)
                {
                    trigger.enableJumping_ = true;
                    trigger.enableFlying_ = false;
                    trigger.enableBoosting_ = false;
                    trigger.enableJetRotating_ = false;
                }
                else if (map.abilityEnabled == Ability.Wings)
                {
                    trigger.enableJumping_ = false;
                    trigger.enableFlying_ = true;
                    trigger.enableBoosting_ = false;
                    trigger.enableJetRotating_ = false;
                }
                else if (map.abilityEnabled == Ability.Jets)
                {
                    trigger.enableJumping_ = false;
                    trigger.enableFlying_ = false;
                    trigger.enableBoosting_ = false;
                    trigger.enableJetRotating_ = true;
                }
                else if (map.abilityEnabled == Ability.Boost)
                {
                    trigger.enableJumping_ = false;
                    trigger.enableFlying_ = false;
                    trigger.enableBoosting_ = true;
                    trigger.enableJetRotating_ = false;
                }
            }
        }

        void StartRandoGame()
        {
            Randomize();

            Console.WriteLine(maps.Count);

            var firstMap = maps.First();
            G.Sys.GameManager_.NextLevelPath_ = GetLevelPathFromName(firstMap.Key);
            G.Sys.GameManager_.NextGameModeName_ = "Adventure";

            G.Sys.GameManager_.LevelPlaylist_.Clear();
            foreach (var map in maps)
            {
                G.Sys.GameManager_.LevelPlaylist_.Add(new LevelPlaylist.ModeAndLevelInfo(GameModeID.Adventure, map.Key, GetLevelPathFromName(map.Key)));
            }

            G.Sys.GameManager_.LevelPlaylist_.Add(new LevelPlaylist.ModeAndLevelInfo(GameModeID.Adventure, "Destination Unknown", GetLevelPathFromName("Destination Unknown")));
            G.Sys.GameManager_.LevelPlaylist_.Add(new LevelPlaylist.ModeAndLevelInfo(GameModeID.Adventure, "Credits", GetLevelPathFromName("Credits")));

            started = true;
            startGame = false;
        }

        string GetLevelPathFromName(string name)
        {
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return Path.Combine(basePath, $"Distance_Data/Resources/{name.ToLowerInvariant()}.bytes");
        }

        void Randomize()
        {
            System.Random random = new System.Random(seed);

            bool isJumpEnabled = false;
            bool isWingsEnabled = false;
            bool isJetsEnabled = false;

            bool canFly = false;

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

            while (maps.Count != 10)
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
                                    else if  (logicInfo.abilityCompleteRequirement == AbilityRequirement.WingsJets && !canFly)
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
                            Console.WriteLine($"no jump");
                            // can't complete the map, no jump
                            continue;
                        }
                        else if (!canFly && logicInfo.abilityCompleteRequirement == AbilityRequirement.WingsJets)
                        {
                            Console.WriteLine($"no flight");
                            // can't complete the map, no flight
                            continue;
                        }
                        else if ((logicInfo.abilityCompleteRequirement == AbilityRequirement.JumpWingsJets && (!canFly || !isJumpEnabled)))
                        {
                            // nope move along
                            Console.WriteLine($"no jump or flight");
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

        bool MapEnablesAbilities(string mapName)
        {
            if (mapName == "Broken Symmetry" || mapName == "Lost Society" || mapName == "Departure" ||
                mapName == "Aftermath" || mapName == "Monolith")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void LoadSeed()
        {

        }
    }

    public class RandoGame
    {

    }

    public enum Ability { None, Boost, Jump, Wings, Jets }

    public class RandoMap
    {
        public Ability abilityEnabled = Ability.None;
        public bool boostEnabled = false;
        public bool jumpEnabled = false;
        public bool wingsEnabled = false;
        public bool jetsEnabled = false;

        public RandoMap(Ability abilityEnabled, bool boostEnabled, bool jumpEnabled, bool wingsEnabled, bool jetsEnabled)
        {
            this.abilityEnabled = abilityEnabled;
            this.boostEnabled = boostEnabled;
            this.jumpEnabled = jumpEnabled;
            this.wingsEnabled = wingsEnabled;
            this.jetsEnabled = jetsEnabled;
        }
    }

    public enum AbilityRequirement { None, Jump, WingsJets, JumpWingsJets }

    public class MapLogicInfo
    {
        public bool unlocksAbility = false;
        public AbilityRequirement abilityUnlockRequirement = AbilityRequirement.None;
        public AbilityRequirement abilityCompleteRequirement = AbilityRequirement.None;

        public MapLogicInfo(bool unlocksAbilitiy, AbilityRequirement unlock, AbilityRequirement completion)
        {
            this.unlocksAbility = unlocksAbilitiy;
            abilityUnlockRequirement = unlock;
            abilityCompleteRequirement = completion;
        }

        public static MapLogicInfo GetMapLogicInfo(string name)
        {
            if (name == "Broken Symmetry")
            {
                return new MapLogicInfo(true, AbilityRequirement.None, AbilityRequirement.None);
            }
            else if (name == "Lost Society")
            {
                return new MapLogicInfo(true, AbilityRequirement.None, AbilityRequirement.Jump);
            }
            else if (name == "Negative Space")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.Jump);
            }
            else if (name == "Departure")
            {
                return new MapLogicInfo(true, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "Ground Zero")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "Aftermath")
            {
                return new MapLogicInfo(true, AbilityRequirement.JumpWingsJets, AbilityRequirement.JumpWingsJets);
            }
            else if (name == "Friction")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.WingsJets);
            }
            else if (name == "The Thing About Machines")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.JumpWingsJets);
            }
            else if (name == "Corruption")
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.None);
            }
            else if (name == "Monolith")
            {
                return new MapLogicInfo(true, AbilityRequirement.JumpWingsJets, AbilityRequirement.JumpWingsJets);
            }
            else
            {
                return new MapLogicInfo(false, AbilityRequirement.None, AbilityRequirement.None);
            }
        }
    }
}
