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
                                G.Sys.MenuPanelManager_.ShowError($"Adventure Randomizer {Metadata.RandomizerVersion}\n\n" +
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
                                    randoGame = new RandoGame(usedSeed, Metadata.LogicVersion);

                                    G.Sys.MenuPanelManager_.ShowError($"Rando seed has been set to:\n[FF0000]{inputSeed.Trim()}[-]\n\n" +
                                        $"Hash: [FF0000]{randoGame.friendlyHash}[-]\n({randoGame.truncSeedHash})\n\n" +
                                        "Start the [FF0000]Instantiation[-] map in Adventure mode to begin, or any other map to cancel.", "Rando enabled");

                                    startGame = true;
                                    Game.WatermarkText =
                                        $"ADVENTURE RANDOMIZER {Metadata.RandomizerVersion}\n{randoGame.friendlyHash}\n({randoGame.truncSeedHash})\n";

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
                    ApplyRandoChanges.OnModeStarted(randoGame);
                }
            });

            Events.Level.PostLoad.Subscribe((data) =>
            {
                if (started)
                {
                    ApplyRandoChanges.OnPostLoad(randoGame);
                }
            });

            Events.GameMode.Go.Subscribe((data) =>
            {
                Console.WriteLine("Start/Load event fired");
                Console.WriteLine($"Rando game started? {started}");
                if (started)
                {
                    if (Game.LevelName == "Instantiation")
                    {
                        return;
                    }

                    Console.WriteLine($"Rando changes applied? {randoChangesApplied}");
                    if (!randoChangesApplied)
                    {
                        Console.WriteLine(randoChangesApplied);
                        Console.WriteLine("should only be called once");

                        ApplyRandoChanges.OnGo(randoGame);

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
                        randoGame.abilityState.UpdateAbilityState();
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
                        randoGame.abilityState.SetCarAbilities(singleRaceStarted);
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
                if (playerDataLocal)
                {
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

                            int curMap = G.Sys.GameManager_.GetCurrentPlaylistIndex();

                            playerDataLocal.CarScreenLogic_.EnableAbilityBattery(abilityBatteryChanges, $"map {curMap}/16");
                            AudioManager.PostEvent("Play_OpenMap", playerDataLocal.Car_);
                        }
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

        void StartRandoGame()
        {
            print($"[RANDOMIZER] Started randomizer game! - Seed: {randoGame.seed} - Friendly hash: {randoGame.friendlyHash} - SHA256: {randoGame.truncSeedHash}");

            Console.WriteLine(randoGame.maps.Count);

            var firstMap = "Instantiation";
            G.Sys.GameManager_.NextLevelPath_ = GetLevelPathFromName(firstMap);
            G.Sys.GameManager_.NextGameModeName_ = "Adventure";

            G.Sys.GameManager_.LevelPlaylist_.Clear();

            G.Sys.GameManager_.LevelPlaylist_.Add(new LevelPlaylist.ModeAndLevelInfo(GameModeID.Adventure, firstMap, GetLevelPathFromName(firstMap)));

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
