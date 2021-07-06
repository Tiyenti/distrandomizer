---
title: Distrandomizer Changelog
---
# Changelog
## vA.2.1 - 2021-07-06
* Fixed: Corrected a mistake in the logic code that allowed for Embers to appear before its ability requirements had been met, resulting in an unbeatable game.

> Side note: Yeah, I'm changing the versioning scheme for these alpha releases to something that's admittedly a little idiosyncratic. Main reason for this is that I decided that "v1.0-alphaX" was not a good naming system since I don't believe the design and feature set is currently set in stone enough to consider these v1.0 release candidates - but I also can't exactly go back to version 0 now that I've already released versions under the "v1.0" name, so I've simply opted to truncate "v1.0-alphaX" into "vA.X". ~~Though I will also admit I just like how this is a more compact way to convey mostly the same information.~~

## v1.0-alpha2 - 2021-06-12
* **Logic changes:**
  * Contagion now requires jump to complete. (Complete requirement: `WingsJets` → `JumpWingsJets`)
    * While Contagion *is* possible to complete without jump, it was annoyingly easy to get stuck, annoyingly difficult to get un-stuck, and not particularly enjoyable or interesting, at least in my opinion. So it's being changed to require jump for the time being. (This may be reverted later, or might be re-added in a psuedo-"hard mode" once options are implemented.)
  * The ability trigger in Embers can now alternatively be reached with only jump. (Trigger requirement: `WingsJets` → `JumpOrFlight`)
* Backend: Slightly refactored the code by splitting certain things into seperate classes/files.
* Fixed: Corrected an issue where randomly-generated seed strings would create a different randomized game compared to the one you would get if you re-entered the same seed manually.
* Added: Pressing the Show Scores button now displays which abilities you have and what map out of 16 you are currently playing, replacing the default Adventure minimap.
* Changed: Intro title cards now display "MAP *n*/16" instead of the predefined sector number.
* Changed: The Instantiation intro cutscene, which was previously removed during randomizer runs, has been re-added. This is primarily to fix some cosmetic issues that occured due to the game using current playlist index for some visuals. This has no major changes on gameplay but will cosmetically affect:
  * Sector posters
  * Loading screens
  * Fixes the first map not having a title card
  * The 4-hour timer
* Changed: The system for the friendly/5-word hash has changed.
  * Previously, the friendly hash was 5 words that directly corrosponded to the first 5 characters of a SHA256 checksum converted to an ASCII string.
  * In this verison, the friendly hash has been reduced to 4 words, but is now based on a Base64 encoding of the hash instead of the ASCII representation. In short, instead of being 5 instances of 16 possible words, it is now 4 instances of 64 possible words.
  * This helps make the friendly hashes less unwieldly by reducing length and repeat words, while hopefully retaining a similar (or greater) level of uniqueness to the previous iteration. 
  * The 7-character SHA256 string will still be displayed as in the previous version.
* Changed: The ending sequence in Enemy has been shortened - the warp after the teleporter now takes you directly to the ending area, removing the waterfall and ocean warp cutscene areas, and the trigger at the end that disables all abilities has been removed like in Campaign+. 

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