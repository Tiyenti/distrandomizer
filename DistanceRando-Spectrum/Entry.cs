using Centrifuge.Distance.Game;
using DistanceRando.CustomBehaviours;
using DistanceRando.Randomizer;
using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using Reactor.API.Runtime.Patching;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace DistanceRando
{
	[ModEntryPoint("com.github.tiyenti/DistanceRando")]
	public class Entry : MonoBehaviour
	{
		internal static Entry Instance;

		RandoGame randoGame = null;

		bool started = false;
		bool startGame = false;
		bool singleRaceStarted = false;

		bool randoChangesApplied = false;

		AbilityInventoryScreen carInventoryScreen = new AbilityInventoryScreen();

		public void Initialize(IManager manager)
		{
			Instance = this;

			DontDestroyOnLoad(this);

			RuntimePatcher.AutoPatch();
		}

		public void LateInitialize(IManager manager)
		{
			// Randomizer plugin control events

			Events.MainMenu.Initialized.Subscribe((data) =>
			{
				if (started)
				{
					print($"[RANDOMIZER] End randomizer game! - Seed: {randoGame.seed} - Friendly hash: {randoGame.friendlyHash} - SHA256: {randoGame.truncSeedHash}");
				}

				ResetValues();
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

					// If the speedrun timer is not enabled, then hide the text manually
					if (G.Sys.OptionsManager_.General_.SpeedrunTimer_ == false)
					{
						var watermarkText = GameObject.Find("AlphaVersion");
						watermarkText.GetComponent<UILabel>().enabled = false;
					}
				}
			});

			// Events to handle the map changes required for the rando

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

			// Events to update the abiliyState object, and set the car's abiities to what they should be

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

		public void ShowRandomizerMenu()
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
						$"ADVENTURE RANDOMIZER {Metadata.RandomizerVersion}\n{randoGame.friendlyHash} ({randoGame.truncSeedHash})\n";

					// If the speedrun timer is not enabled, show the text manually here
					if (G.Sys.OptionsManager_.General_.SpeedrunTimer_ == false)
                    {
						var watermarkText = GameObject.Find("AlphaVersion");
						watermarkText.GetComponent<UILabel>().enabled = true;
						// Also set the width to be wider, so it can display the information more cleanly.
						watermarkText.GetComponent<UILabel>().width = 500;
                    }
				});
			}
		}

		// Show ability inventory on show scores press
		void Update()
		{
			if (started)
			{
				carInventoryScreen.Update();
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
