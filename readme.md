# Distance Randomizer
A Centrifuge plugin that randomizes Distance's Adventure mode. Almost all maps and abilities are
shuffled into a random order, requiring you to use some interesting strategies to complete some maps due to being less equipped than the developers intended.

There's now a website for the randomizer plugin at https://tiyenti.github.io/distrandomizer with more information!

## Installation
The latest version of the mod can be found and downloaded from the [releases page](https://github.com/Tiyenti/distrandomizer/releases).

See https://tiyenti.github.io/distrandomizer/install for instructions on how to install
the mod if you are unsure how to do this.

## Build instructions
Obviously, you will first need to clone the repo:

    $ git clone https://github.com/Tiyenti/distrandomizer.git

You will need to provide four dependant assemblies in order to build the project:

* `UnityEngine.dll` from Distance
* `Assembly-CSharp.dll` from Distance
* `Reactor.API.dll` from [Centrifuge](https://github.com/Ciastex/Centrifuge)
* `Centrifuge.Distance.dll` from the [Distance GSL for Centrifuge](https://github.com/REHERC/Centrifuge.Distance)

These four assemblies must be provided in a directory named `Dependencies` within the
repository root. The batch file `setup_symlinks.bat` will automatically create the `Dependencies`
directory and four symlinks to the expected locations of these files on Windows, assuming Distance is installed at `C:\Program Files (x86)\Steam\steamapps\common\Distance\Distance_Data`.

To run this file, open an elevated* terminal window in the project root directory and run:

     # .\setup_symlinks.bat

> _(* Note: If you have Developer Mode enabled in Windows 10, you shouldn't require elevated permissions to create symlinks and can run the bat file in a regular-permissions terminal.)_

This should automatically create the expected directory structure, which should look something
like the following:

    /
    ┕ Dependencies
      ┕ Assembly-CSharp.dll
      ┕ Centrifuge.Distance.dll
      ┕ Reactor.API.dll
      ┕ UnityEngine.dll
    ┕ DistanceRando-Spectrum
      ┕ ...
    ┕ mod.json
    ┕ ...

If you are not on Windows or your Distance install is located on a different directory, you can either manually create the symlinks, or instead just copy the requisite .dll files into the Dependencies directory (which is, of course, an available option on Windows as well).

distrandomizer is currently being developed with Visual Studio 2019 Community, so this is the reccomended IDE for developing/building the mod. Other IDE solutions may work as well
although they are not officially supported.

After compiling the plugin, the resulting `DistanceRando.Plugin.dll` assembly can
then be copied from the build directory to a new directory in the `[distance-path]/Centrifuge/Mods` directory alongside the `mod.json` manifest from the project root in the following structure:

    Distance_Data/
    ┕ Centrifuge
      ┕ Mods
        ┕ DistanceRando
          ┕ DistanceRando.Plugin.dll
          ┕ mod.json
        ┕ ...
      ┕ ...
    ┕ ...

From here, providing that everything is properly set up (with Centrifuge and the Distance GSL
installed), you should be able to launch the game with the compiled plugin.
