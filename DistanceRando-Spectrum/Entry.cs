using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.API;
using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Reactor.API.Logging;
using Reactor.API.Storage;
using Centrifuge.Distance.Game;
using System.Security.Cryptography;

namespace DistanceRando
{
    [ModEntryPoint("com.github.tiyenti/DistanceRando")]
    public class Entry : MonoBehaviour
    {
        RandoGame randoGame = null;

        readonly string randomizerVersion = "1.0-alpha2-dev";
        readonly string logicVersion = "1.0-alpha1";

        bool started = false;
        bool startGame = false;
        bool singleRaceStarted = false;

        bool firstMainMenuLoad = true;

        bool randoChangesApplied = false;

        public void Initialize(IManager manager)
        {
            DontDestroyOnLoad(this);
        }

        public void LateInitialize(IManager manager)
        {
            Console.WriteLine("Late Initialize!");

            Events.MainMenu.Initialized.Subscribe((data) =>
            {
                if (firstMainMenuLoad)
                {
                    firstMainMenuLoad = false;
                    manager.Hotkeys.Bind("R", () => {
                        if (Game.SceneName == "MainMenu" && G.Sys.GameManager_.SoloAndNotOnline_)
                        {
                            // if prepped to start, show randomizer settings
                            if (startGame)
                            {
                                G.Sys.MenuPanelManager_.ShowError($"Adventure Randomizer {randomizerVersion}\n\n" +
                                                                $"Seed hash: [FF0000]{randoGame.friendlyHash}[-]\n" +
                                                                $"({randoGame.truncSeedHash})\n\nStart the [FF0000]Instantiation[-] map in Adventure mode to begin, or any other map to cancel.",
                                                                "Randomizer Config");
                                return;
                            }

                            if (!G.Sys.MenuPanelManager_.TrackmogrifyMenuLogic_.trackmogrifyInput_.isSelected)
                            {
                                G.Sys.MenuPanelManager_.TrackmogrifyMenuLogic_.Display((inputSeed, isRandom) =>
                                {
                                    var usedSeed = inputSeed;

                                    G.Sys.MenuPanelManager_.Pop();

                                    // Generate randomizer settings
                                    randoGame = new RandoGame(usedSeed, logicVersion);

                                    G.Sys.MenuPanelManager_.ShowError($"Rando seed has been set to:\n[FF0000]{inputSeed.Trim()}[-]\n\n" +
                                        $"Hash: [FF0000]{randoGame.friendlyHash}[-]\n({randoGame.truncSeedHash})\n\n" +
                                        "Start the [FF0000]Instantiation[-] map in Adventure mode to begin, or any other map to cancel.", "Rando enabled");

                                    startGame = true;
                                    Game.WatermarkText =
                                        $"ADVENTURE RANDOMIZER {randomizerVersion}\n{randoGame.friendlyHash}\n({randoGame.truncSeedHash})\n";

                                });
                            }
                        }
                    });
                }
                else
                {
                    if (started)
                    {
                        print($"[RANDOMIZER] End randomizer game! - Seed: {randoGame.seed} - Friendly hash: {randoGame.friendlyHash} - SHA256: {randoGame.truncSeedHash}");
                    }

                    ResetValues();
                }
            });

            Events.Scene.BeginSceneSwitchFadeOut.Subscribe((data) => {
                // Yes. We are intercepting a map load. I know it's bad, but this is the only way to get this to work I could find :P
                if (startGame)
                {
                    Console.WriteLine(G.Sys.GameManager_.NextLevelPathRelative_);
                    if (G.Sys.GameManager_.NextLevelPathRelative_ == "OfficialLevels/Instantiation.bytes")
                    {
                        StartRandoGame();
                    }
                    else
                    {
                        Console.WriteLine("rando game cancelled");
                        startGame = false;
                        ResetValues();
                    }
                }
            });

            Events.GameMode.ModeStarted.Subscribe((data) =>
            {
                // pre start stuff
                if (started)
                {
                    foreach (var obj in FindObjectsOfType<WingCorruptionZone>())
                    {
                        Destroy(obj.gameObject);
                    }

                    // Set map subtitle
                    var titleObj = FindObjectOfType<LevelIntroTitleLogic>();

                    int curMap = G.Sys.GameManager_.GetCurrentPlaylistIndex() + 1;

                    if (titleObj)
                    {
                        titleObj.subtitleText_.text = $"-  MAP {curMap}/16  -";
                    }
                    else
                    {
                        print("[RANDOMIZER] title obj null");
                    }
                }
            });

            Events.Level.PostLoad.Subscribe((data) =>
            {
                if (started)
                {

                    foreach (var obj in FindObjectsOfType<AdventureSpecialIntro>())
                    {
                        Destroy(obj.gameObject);
                    }

                    if (!(Game.LevelName == "Enemy" || Game.LevelName == "Credits"))
                    {
                        // remove warpanchor cutscenes but keep all warpanchors present in arcade mode
                        // (this could allow for abyss to unlock an ability)
                        foreach (var obj in FindObjectsOfType<WarpAnchor>())
                        {
                            if (obj.ignoreInArcade_)
                            {
                                obj.ignoreInAdventure_ = true;
                            }
                            else if (obj.ignoreInAdventure_)
                            {
                                obj.ignoreInAdventure_ = false;
                            }
                        }

                        foreach (var obj in FindObjectsOfType<GlitchFieldLogic>())
                        {
                            if (obj.ignoreInArcade_)
                            {
                                Destroy(obj.gameObject);
                            }
                        }
                    }
                }
            });

            Events.GameMode.Go.Subscribe((data) =>
            {
                Console.WriteLine("Start/Load event fired");
                Console.WriteLine($"Rando game started? {started}");
                if (started)
                {
                    Console.WriteLine($"Rando changes applied? {randoChangesApplied}");
                    if (!randoChangesApplied)
                    {
                        Console.WriteLine(randoChangesApplied);
                        Console.WriteLine("should only be called once");
                        ApplyRandoChanges();
                        randoChangesApplied = true;
                    }
                    singleRaceStarted = true;
                    CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
                    Console.WriteLine("Start event fired 2");
                    Console.WriteLine($"Jump {car.Jump_.AbilityEnabled_} - Wings {car.Wings_.AbilityEnabled_} - Jets {car.Jets_.AbilityEnabled_}");
                }
            });

            Events.ServerToClient.ModeFinished.Subscribe((data) =>
            {
                Console.WriteLine("Finish event fired");
                if (started)
                {
                    singleRaceStarted = false;
                    randoChangesApplied = false;
                }
            });

            Events.Car.Explode.SubscribeAll((sender, data) =>
            {
                if (sender.GetComponent<PlayerDataLocal>())
                {
                    if (started)
                    {
                        RandoMap map = randoGame.maps[Game.LevelName];
                        CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
                        if (car.Jump_.AbilityEnabled_ && !map.jumpEnabled)
                        {
                            randoGame.jumpShouldBeEnabled = true;
                        }
                        if (car.Wings_.AbilityEnabled_ && !map.wingsEnabled)
                        {
                            randoGame.wingsShouldBeEnabled = true;
                        }
                        if (car.Jets_.AbilityEnabled_ && !map.jetsEnabled)
                        {
                            randoGame.jetsShouldBeEnabled = true;
                        }
                    }
                }
            }
            );

            Events.Player.CarRespawn.SubscribeAll((sender, data) =>
            {
                if (sender.GetComponent<PlayerDataLocal>())
                {
                    if (started)
                    {
                        Console.WriteLine("Respawn event fired");
                        CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
                        try
                        {
                            RandoMap map = randoGame.maps[Game.LevelName];
                            if (!singleRaceStarted)
                            {
                                randoGame.jumpShouldBeEnabled = map.jumpEnabled;
                                randoGame.wingsShouldBeEnabled = map.wingsEnabled;
                                randoGame.jetsShouldBeEnabled = map.jetsEnabled;
                            }
                            car.Boost_.AbilityEnabled_ = map.boostEnabled;
                            car.Jump_.AbilityEnabled_ = randoGame.jumpShouldBeEnabled;
                            car.Wings_.AbilityEnabled_ = randoGame.wingsShouldBeEnabled;
                            car.Jets_.AbilityEnabled_ = randoGame.jetsShouldBeEnabled;

                            // Disable stock show scores so we can display our own thing.
                            car.CarLogicLocal_.PlayerDataLocal_.EnableOrDisableShowScores(false);

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

                        car.GetComponent<LocalPlayerControlledCar>().showBackToResetWarning_ = false;
                    }
                }
            });
        }

        // Show ability inventory on show scores press
        void Update()
        {
            if (started)
            {
                var playerDataLocal = G.Sys.PlayerManager_.Current_.playerData_;
                if (!playerDataLocal.EnableShowScores_ && playerDataLocal.InputStates_.GetTriggered(InputAction.ShowScore))
                {
                    AbilityBatteryChange[] abilityBatteryChanges = new AbilityBatteryChange[4];

                    if (playerDataLocal.CarLogic_.Boost_.AbilityEnabled_) abilityBatteryChanges[0] = AbilityBatteryChange.Enable;
                    if (playerDataLocal.CarLogic_.Jump_.AbilityEnabled_) abilityBatteryChanges[1] = AbilityBatteryChange.Enable;
                    if (playerDataLocal.CarLogic_.Wings_.AbilityEnabled_) abilityBatteryChanges[2] = AbilityBatteryChange.Enable;
                    if (playerDataLocal.CarLogic_.Jets_.AbilityEnabled_) abilityBatteryChanges[3] = AbilityBatteryChange.Enable;

                    if (playerDataLocal.CarScreenLogic_ != null)
                    {
                        playerDataLocal.CarScreenLogic_.StopScrensaver();

                        int curMap = G.Sys.GameManager_.GetCurrentPlaylistIndex() + 1;

                        playerDataLocal.CarScreenLogic_.EnableAbilityBattery(abilityBatteryChanges, $"map {curMap}/16");
                        AudioManager.PostEvent("Play_OpenMap", playerDataLocal.Car_);
                    }
                }
            }
        }

        void ResetValues()
        {
            started = false;
            startGame = false;
            randoChangesApplied = false;
            singleRaceStarted = false;

            randoGame = null;
        }

        void ApplyRandoChanges()
        {
            if (Game.LevelName == "Enemy" || Game.LevelName == "Credits")
            {
                return;
            }

            RandoMap map = randoGame.maps[Game.LevelName];

            G.Sys.GameManager_.Level_.Settings_.disableBoosting_ = !map.boostEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableJumping_ = !map.jumpEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableFlying_ = !map.wingsEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableJetRotating_ = !map.jetsEnabled;

            CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
            /*car.Boost_.AbilityEnabled_ = map.boostEnabled;
            car.Jump_.AbilityEnabled_ = map.jumpEnabled;
            car.Wings_.AbilityEnabled_ = map.wingsEnabled;
            car.Jets_.AbilityEnabled_ = map.jetsEnabled;*/

            randoGame.jumpShouldBeEnabled = map.jumpEnabled;
            randoGame.wingsShouldBeEnabled = map.wingsEnabled;
            randoGame.jetsShouldBeEnabled = map.jetsEnabled;
            //car.GetComponent<HornGadget>().enabled = true;

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
                SetAbilitiesTrigger[] triggers = GameObject.FindObjectsOfType<SetAbilitiesTrigger>();

                foreach (var trigger in triggers)
                {
                    if (!(trigger.visualsOnly_ || !trigger.showAbilityAlert_))
                    {
                        // replace default triggers with a custom solution
                        // (this is an attempt to fix shockingly inconsistent behaviour with the default triggers)
                        // (also it lets us have progressive ability icons!! cool!!)
                        var newTrigger = trigger.gameObject.AddComponent<RandomizerAbilityTrigger>();

                        if (map.abilityEnabled == Ability.Jump)
                        {
                            newTrigger.enableJumping = true;
                            newTrigger.enableFlying = randoGame.wingsShouldBeEnabled;
                            newTrigger.enableBoosting = true;
                            newTrigger.enableJetRotating = randoGame.jetsShouldBeEnabled;
                        }
                        else if (map.abilityEnabled == Ability.Wings)
                        {
                            newTrigger.enableJumping = randoGame.jumpShouldBeEnabled;
                            newTrigger.enableFlying = true;
                            newTrigger.enableBoosting = true;
                            newTrigger.enableJetRotating = randoGame.jetsShouldBeEnabled;
                        }
                        else if (map.abilityEnabled == Ability.Jets)
                        {
                            newTrigger.enableJumping = randoGame.jumpShouldBeEnabled;
                            newTrigger.enableFlying = randoGame.wingsShouldBeEnabled;
                            newTrigger.enableBoosting = true;
                            newTrigger.enableJetRotating = true;
                        }
                        else if (map.abilityEnabled == Ability.Boost)
                        {
                            newTrigger.enableJumping = randoGame.jumpShouldBeEnabled;
                            newTrigger.enableFlying = randoGame.wingsShouldBeEnabled;
                            newTrigger.enableBoosting = true;
                            newTrigger.enableJetRotating = randoGame.jetsShouldBeEnabled;
                        }

                        trigger.gameObject.RemoveComponent<SetAbilitiesTrigger>();
                    }
                    else
                    {
                        Destroy(trigger.gameObject);
                    }
                }
            }
            else
            {
                SetAbilitiesTrigger[] triggers = GameObject.FindObjectsOfType<SetAbilitiesTrigger>();

                foreach (var trigger in triggers)
                {
                    Destroy(trigger.gameObject);
                }
            }
        }

        void StartRandoGame()
        {
            print($"[RANDOMIZER] Started randomizer game! - Seed: {randoGame.seed} - Friendly hash: {randoGame.friendlyHash} - SHA256: {randoGame.truncSeedHash}");

            Console.WriteLine(randoGame.maps.Count);

            var firstMap = randoGame.maps.First();
            G.Sys.GameManager_.NextLevelPath_ = GetLevelPathFromName(firstMap.Key);
            G.Sys.GameManager_.NextGameModeName_ = "Adventure";

            G.Sys.GameManager_.LevelPlaylist_.Clear();
            foreach (var map in randoGame.maps)
            {
                G.Sys.GameManager_.LevelPlaylist_.Add(new LevelPlaylist.ModeAndLevelInfo(GameModeID.Adventure, map.Key, GetLevelPathFromName(map.Key)));
            }

            G.Sys.GameManager_.LevelPlaylist_.Add(new LevelPlaylist.ModeAndLevelInfo(GameModeID.Adventure, "Enemy", GetLevelPathFromName("Enemy")));
            G.Sys.GameManager_.LevelPlaylist_.Add(new LevelPlaylist.ModeAndLevelInfo(GameModeID.Adventure, "Credits", GetLevelPathFromName("Credits")));

            started = true;
            startGame = false;
        }

        string GetLevelPathFromName(string name)
        {
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return Path.Combine(basePath, $"Distance_Data/Resources/{name}.bytes");
        }
    }
}
