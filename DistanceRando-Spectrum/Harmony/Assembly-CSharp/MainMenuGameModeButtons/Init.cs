using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DistanceRando.Harmony
{
	// Add the "Arcade" menu button for Distandomizer
	[HarmonyPatch(typeof(MainMenuGameModeButtons), "Init", new Type[] { typeof(IEnumerable<string>), typeof(Action<string>) })]
	internal static class MainMenuGameModeButtons__Init
	{
		private const string MENU_PANEL_NAME = "SoloGameModesButtonsPanel";

		[HarmonyPostfix]
		internal static void Postfix(MainMenuGameModeButtons __instance)
		{
			if (__instance.gameObject.name != MENU_PANEL_NAME)
			{
				return;
			}

			UITable layout = __instance.buttonsTable_;
			Transform container = layout.transform;

			GameObject createButton(string name, string description, Action onClick)
			{
				//GameObject copy = GameObject.Instantiate(blueprint, container);

				GameObject copy = UIExBlueprint.Duplicate(__instance.buttonBlueprint_);
				copy.SetActive(true);

				copy.name = name;

				copy.transform.Find("UILabel").GetComponent<UILabel>().text = name;
				copy.GetComponent<SetMenuDescriptionOnHover>().text_ = description;

				// For some reason, the button blueprint has multiple UIExButton scripts on it
				// Setting the onClick property on each of them would result in
				// the action being triggered as many times as there are button scripts on the prefab
				bool clickEventHandlerSet = false;

				foreach (UIExButton button in copy.GetComponents<UIExButton>())
				{
					button.onClick.Clear();

					if (!clickEventHandlerSet)
					{
						button.onClick.Add(new EventDelegate(() => onClick()));

						clickEventHandlerSet = true;
					}
				}

				return copy;
			}

			createButton(Metadata.MenuButtonText, Metadata.MenuButtonDesc, () => Entry.Instance.ShowRandomizerMenu());

			layout.Sort(container.GetChildren().ToList());
			layout.Reposition();
		}
	}
}