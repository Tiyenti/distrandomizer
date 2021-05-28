---
title: Distrandomizer Changelog
---
## v1.0-alpha1 - 2021-05-28
* **Major:** Fully updated the randomizer mod to work with Distance 1.0!
* Updated the mod backend to use the newer Centrifuge modding system instead of the old
Spectrum framework.
* Added: New `JumpOrFlight` value to map requirement logic for maps that can be done with either jump or flight, but don't requrie both
  (used on Abyss and Euphoria currently)
* Added: Seeds now show a hash value to more easily determine if both players will get the same game, displayed in two forms - 5 human-readable words that directly corrospond to the first 5 characters of the hash (this may be subject to change), and also the actual first 7 characters of the computed SHA256 checksum.
* Changed: As a result, the entered seed on the speedrun timer text has now been replaced with the two hash values.
* Added: Can now press `R` on the main menu again after completing a seed to diplay the hash values for easier reference.
* Changed: The dialog box displayed after entering a seed now displays the seed hash, no longer closes automatically, and clarifies that
  starting any map other than Instantiation will cancel the game.
* Improved: Now trims whitespace characters on entered seeds.
* Improved: Now outputs randomzier game start/end messages in the Unity log so seeds used are recoverable (important for bugtesting).
* Fixed: Fixed a weird bug where the triggers wouldn't work properly under specific circumstances by implementing a custom ability trigger object.
* Added: The car ability display upon unlocking an ability is now progressive and displays all abilities you currently have instead only the one you just unlocked.

## v0.2.1 - 2018-06-10
* Fixed freezes on Destination Unknown and Credits (for real this time)

## v0.2 - 2018-06-01
* Updated plugin to Spectrum Gamma
* Fixed some oversights in the randomization code that allowed for impossible seeds
* Fixed being able to open the seed entry prompt while online
* Fixed ability icons sometimes appearing at the start of the map
* Fixed abilities not getting enabled at the start of the map if you didn't die in the previous map
* Fixed Destination Unknown and Credits failing to load

## v0.1 - 2018-05-19
* Initial release