# Distance Randomizer (2.0)
A Spectrum plugin that randomizes Distance's Adventure mode.

## Changes from the normal game
- All maps and abilities have been rearranged into a random order.
- Boost is enabled from the start, you don't need to pass through an ability trigger for it.
- Wing corruption zones are no longer present to allow for more variety.
- The majority of the tutorial text in the game has been removed, since you don't need to learn
  how to play the game in rando.
- You can honk.

## How to play
Randomizer is now a Spectrum plugin rather than an external script, so the installation and
use is now simpler than the Python version I wrote last year.

First, you need to install Spectrum and install the plugin. [Here's a tutorial for how to do
both of those things.](https://gist.github.com/TntMatthew/54ab92d326cbdeee35fc91acf092e283) Just
grab the latest version of the randomizer from the [releases page](https://github.com/TntMatthew/distrandomizer/releases).

Once you have installed Spectrum and the randomizer plugin, you're ready to go. Press `R` on the main menu
to open up the seed entry prompt. You can either use a specific seed, or leave the
field blank to generate a random seed. If you're doing a race, I'd recommend making everybody
use the same seed to keep things fair. Once you've entered the seed you want, press Enter - this
will set the seed and prime randomizer. Once you're ready to start the game, go into the
Adventure menu and start the Intro map. The rando game will then be generated, allowing you
to actually play it. If you decide you don't want to play rando after all, loading any map except the
intro will cancel the rando game.

Obviously, since the abilities are given to you in a random order, you may not be able to
complete things in the original way, so you'll have to use unusual strats to complete some
maps. Remember that you can slam your car against walls to shove it off of roads without jump,
and note that it's possible to drive up the arrows on some jump barriers. Sometimes you may get
stuck behind a checkpoint; you may need to go backwards to give yourself enough room to pass it.
It should be impossible to get totally stuck, but if that ever does happen, make sure you toss
an issue my way with that seed so I can see what went wrong.

Skipping ability triggers will still give you the ability you would have gotten in the previous
map like the normal game, so don't be afriad to skip ability triggers in some maps (like Departure) in the interest of saving time.
If you want to keep track of the maps and abilities you've already completed/unlocked, [I've made a simple tracker](https://tntmatthew.github.io/disttracker)
that will allow you to do that.