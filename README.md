# SteamLauncher Plugin for LaunchBox

A LaunchBox plugin designed to allow roms/emulators to easily be launched with Steam, directly from the LaunchBox/BigBox interface, without any complicated setup or technical know-how. Simply open the LaunchBox/BigBox context menu for any title and select the option labeled, "Launch via Steam".

## Features

Here are just a few of the features or benefits provided by using this plugin:

```
* Allow LaunchBox to do what its good at - be the sole organizer and maintainer of your rom/game collection, without having to duplicate its functionality in Steam (something it was not designed for)
* Keep your Steam library clean and uncluttered by hundreds of emulator/rom shortcuts
* Get all of the benefits of Steam's excellent overlay system while playing any rom or emulator
* Take advantage of Steam's infinitely customizable controller configuration (bindings) for both the Steam Controller and nearly all other DirectInput/XInput controllers
* Use per-rom/per-emulator/per-game controller configurations that are automatically remembered between gaming sessions (without having to create convoluted Steam shortcuts for every rom)
* Bypass or improve upon many emulators' extremely limited controller bindings and hotkey options
* Share with your Steam friends list your "currently playing" status for whatever rom you launched from LaunchBox/BigBox (Example: YourSteamAlias is currently playing 'Super Metroid (SNES)')
```

### Requirements

```
* .NET Framework 4.6.1+
* Steam for Windows
* LaunchBox
```

## Getting Started

### Installation

To get the plugin up and running with LaunchBox:

```
* Download the latest plugin release from GitHub or the LaunchBox forum:

[GitHub Releases](https://github.com/lahma69/SteamLauncher/releases):
https://github.com/lahma69/SteamLauncher/releases

[LaunchBox Plugin Download](https://forums.launchbox-app.com/files/file/972-steamlauncher/):
https://forums.launchbox-app.com/files/file/972-steamlauncher/

* Extract the 'SteamLauncher' directory from the compressed package into your LaunchBox plugins directory.

Detailed Example: If LaunchBox is located at 'C:\LaunchBox', you would copy the 'SteamLauncher' directory to 'C:\LaunchBox\Plugins'. This would result in 'SteamLauncher.dll' and any additional files being inside the directory 'C:\LaunchBox\Plugins\SteamLauncher'.
```

### How to Use

To use the plugin to launch your first game through Steam:

```
* Right click any title in LaunchBox or BigBox to bring up its context menu
* Click the option at the bottom of the menu labeled "Launch via Steam"

Note: Currently SteamLauncher will only launch the rom using its default emulator. If you wish to use a different emulator with that particular rom, you will have to go into that rom's settings and change its default emulator, or you can change the default emulator for the entire platform associated with that rom.
```

### Customize Platform Names

When roms are launched with the plugin, the title of the game, as Steam sees it, will be in the format of: "Game Title (Platform Name)"

Example:
```
Super Mario Bros. (Nintendo Entertainment System)
```

For whatever reason, you may want to keep the long "official" name of the platform in LaunchBox, but use a shortened, abbreviated, or different platform name inside of Steam. The plugin has this functionality available, and it can be enabled by following these instructions:

```
* Open the SteamLauncher directory and look for a file named 'config.xml'

* If you do not see the file, you need to launch at least 1 game with the plugin, and then manually close LaunchBox, and the file should then appear in the directory

* Open the file with a text editor, and look for the line labeled, '<CustomPlatformNames>'

* Anywhere under this line, and before the line labeled, '</CustomPlatformNames>', you can add as many new lines as desired in the format of:

<Platform Name="Original LaunchBox Platform Name" Custom="Custom Platform Name" />

* You may already see a couple of example lines in place with fake platform names which you can copy/paste and modify to add your own custom platform names

* After making your changes, simply save the file, close it, and then reopen LaunchBox

* The next time you launch a game, you should see your new custom platform name substituting the real platform name in Steam (an easy place to see the title of the game you're playing is in the Steam overlay)
```
Just to reiterate, here is an example of what the aforementioned lines look like in my config file:

```
<CustomPlatformNames>
    <Platform Name="Super Nintendo Entertainment System" Custom="SNES" />
    <Platform Name="Nintendo Entertainment System" Custom="NES" />
    <Platform Name="Nintendo Game Boy Advance" Custom="Nintendo GBA" />
    <Platform Name="Nintendo Game Boy Color" Custom="Nintendo GBC" />
</CustomPlatformNames>
```

## Support

Unfortunately I don't run a dedicated call center to provide technical support... However, if you need help with a technical issue or bug (see 'Debug Logs'), want to suggest a new feature (I'm very open to any ideas), just chat, or anything else related to the plugin, visit the support thread at the link below, or feel free to send me a PM or shoot me an email at the address listed below.

Note: I will see and respond to messages most quickly by posting a message to the plugin support thread listed below.

[Plugin Support Thread](https://forums.launchbox-app.com/topic/43142-steamlauncher/):
https://forums.launchbox-app.com/topic/43142-steamlauncher/

### Contact Information

Alias:
Lahma

[Email](mailto:lahma0@gmail.com):
lahma0@gmail.com

[GitHub Project URL](https://github.com/lahma69/SteamLauncher):
https://github.com/lahma69/SteamLauncher

[LaunchBox Community Forums Profile](https://forums.launchbox-app.com/profile/89710-lahma/):
https://forums.launchbox-app.com/profile/89710-lahma/

[Plugin Support Thread](https://forums.launchbox-app.com/topic/43142-steamlauncher/):
https://forums.launchbox-app.com/topic/43142-steamlauncher/

### Known Issues

There appears to be a significant bug in the Steam Client right now that can cause problems with controller bindings under a variety of situations for certain users. If you launch a rom/emulator with LaunchBox and your controller bindings don't seem to be working, simply exit the game, be sure your controller is powered on and plugged in PRIOR to starting the game, and then launch it again. This may happen the very first time you try to launch a game using the SteamLauncher plugin (one time total after freshly installing the plugin.. not on every new session in LaunchBox). If it happens frequently, please report the problem so further workarounds can be investigated.

### Debug Logs

To provide help with any technical problems or to investigate a bug, I will need a copy of the debug log that is generated whenever debug logs are enabled in the configuration file. To enable debug logs follow these instructions:

```
* Locate the file named 'config.xml' in the SteamLauncher directory and open it with a text editor

* Find the line that looks like this: <DebugLogEnabled>false</DebugLogEnabled>

* Change the word 'false' to 'true' (do not include the quotation marks)

* Save the file and close it

* Now simply repeat whatever behavior was causing the issue and/or bug, and after you're sure you've replicated the behavior, manually close LaunchBox

* Again, look inside the SteamLauncher directory, but this time looks for a file named 'debug.log' (there may be more than one sequentially named log files if there was an enormous amount of output, but this is unlikely if debugging was turned on just briefly)

* Zip up the one or more log files and attach them to your msg, along with the details of your particular setup (Windows version, 32-bit or 64-bit, LaunchBox version, Steam version, SteamLauncher plugin version, etc). The more information the better.
```

### Legal Stuff

©2017 Valve Corporation. Steam and the Steam logo are trademarks and/or registered trademarks of Valve Corporation in the U.S. and/or other countries.

### Changelog

v0.9.0.2 - May 26, 2018

```
* Added support for PC/Windows games, DOSBox games, and ScummVM games

* Fixed a bug preventing any games with a non-ASCII character in their file name from being launched

* Added a workaround for an annoying issue that causes the Steam UI to steal focus and/or move into the foreground in front of LaunchBox/BigBox after exiting certain games. This workaround can be enabled/disabled by changing the value of 'PreventSteamFocusStealing' in config.xml
```

v0.9.0.1 - Feb 24, 2018

```
* Fixed compatibility with the Steam beta (the Feb 14th Steam beta update made the plugin and/or LaunchBox crash)
```

v0.9.0.0 - Feb 11, 2018

```
* Initial public release
```
