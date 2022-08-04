# SteamLauncher

<p align="center">
  <img src="/assets/images/logo2_window_256.png">
</p>

A LaunchBox plugin designed to allow roms/emulators to easily be launched with Steam, directly from the LaunchBox/BigBox interface, without any complicated setup or technical know-how. Simply enable SteamLauncher and all LaunchBox/BigBox game/rom launches will be intercepted and launched via Steam. This will provide access to the Steam overlay, custom controller configurations, and will display the game/rom your are currently playing to any of your Steam friends.

## Features

Here are a few of the features provided by SteamLauncher:

* Allow LaunchBox to do what its good at - be the sole organizer and maintainer of your rom/game collection, without having to duplicate its functionality in Steam (something it was not designed for).

* Keep the Steam library clean and uncluttered by hundreds of emulator/rom shortcuts.

* Get all of the benefits of Steam's excellent overlay system while playing any rom or emulator.

* Take advantage of Steam's infinitely customizable controller configuration (bindings) for both the Steam Controller and nearly all other DirectInput/XInput controllers.

* Use per-rom/per-emulator/per-game controller configurations that are automatically remembered between gaming sessions (without having to create convoluted Steam shortcuts for every rom).

* Bypass or improve upon many emulators' extremely limited controller bindings and hotkey options.

* Share the game/rom currently being played with Steam friends (ex: JohnDoe is currently playing 'Super Metroid (SNES)').

### Requirements

* .NET Core 3.1
* Steam for Windows
* LaunchBox/BigBox

## Getting Started

### Installation

* Download the latest plugin release from the LaunchBox forum plugin download page. This GitHub repository is meant as more of a resource for developers. While test builds and other pre-release packages may occassionally be posted on GitHub, the latest public release builds will always be posted on the LaunchBox forum plugin download page first:

    **LaunchBox Plugin Download**: https://forums.launchbox-app.com/files/file/972-steamlauncher/

* Extract the 'SteamLauncher' directory from the compressed package into your LaunchBox plugins directory.

    **Example**: If LaunchBox is located at 'C:\\LaunchBox', you would copy the 'SteamLauncher' directory to 'C:\\LaunchBox\\Plugins'. This would result in 'SteamLauncher.dll' and any additional files being inside the directory 'C:\\LaunchBox\\Plugins\\SteamLauncher'.

### How to Use

**Note**: Obviously, Steam must be running to launch games via Steam so one can either manually start Steam before opening LaunchBox/BigBox or SteamLauncher will automatically launch Steam when needed.

To launch a game/rom/emulator in LaunchBox/BigBox via Steam:

* Ensure SteamLauncher is enabled by using 1 of the 2 following methods:

1. Selecting the 'Tools' menu item and clicking 'Use SteamLauncher'. When SteamLauncher is enabled the menu item will be labeled '(ON)'. This menu item can also be found in BigBox.

!['Tools' Menu Items](/assets/images/SteamLauncher-ToolsMenuItems.png)


2. Selecting the 'Tools' menu item and clicking 'SteamLauncher Settings', resulting in the SteamLauncher settings dialog being displayed. By then checking the checkbox labeled 'Enable SteamLauncher' and clicking the 'Save' button, SteamLauncher will be enabled. The SteamLauncher settings dialog cannot be accessed in BigBox.

![Settings Dialog](/assets/images/SteamLauncher-SettingsWindow-PlatformNames.png)

* Once SteamLauncher is enabled, all games/roms/emulators will be intercepted by SteamLauncher and launched through Steam.

## Customize Platform Names

When games/roms are launched with SteamLauncher enabled, the title and platform of the game/rom will be displayed in one's Steam status in the format of: 'Game Title (Platform Name)'.

**Example**: Super Mario Bros. (Nintendo Entertainment System)

Friends on Steam will see this title in one's status whenever a particular game/rom is being played. If desired, the platform name displayed can be customized. For example, the platform 'Nintendo Entertainment System' can be changed to 'NES'. This customization can be performed by navigating to the 'Platform Names' tab in the SteamLauncher settings dialog.

**Note**: To entirely omit a platform name from one's Steam status, leave the 'Custom Platform Name' entry blank (as seen in the 'Windows' entry in the screenshot below).

![Platform Names](/assets/images/SteamLauncher-SettingsWindow-PlatformNames.png)


## Selective Use

The 'Selective Use' feature allows one to precisely customize the circumstances under which SteamLauncher will be enabled/disabled. The settings for this feature can be found in the 'Selective Use' tab in the SteamLauncher settings dialog.

![Selective Use](/assets/images/SteamLauncher-SettingsWindow-SelectiveUse.png)

### Filter Mode

'Filter Mode' defines how the entries in the 'Filter List' will be applied.
* **Blacklist**: SteamLauncher will always be enabled except when a matching filter is found.
* **Whitelist**: SteamLauncher will always be disabled except when a matching filter is found.
* **Off**: Disable the 'Selective Use' feature entirely.

### Filters List

The 'Filters List' is a list of user defined filters that control when SteamLauncher will be enabled/disabled.
* **Enable**: This checkbox must be checked for a particular filter list entry to be enabled. If not checked, it will be completely ignored.
* **Description**: Used purely as a means of helping the user describe what the filter list entry does. It is entirely optional and has no impact on the actual functionality of the entry.
* **Filter String**: This is the string which is matched against the target 'Filter Type', dictating whether or not this entry matches a particular game title, platform name, etc. The filter string is not case sensitive and it supports 2 types of wildcards:
  * **'\*' Wilcard**: matches **one *or more*** of *any* character.
  * **'?' Wildcard**: matches *any* **single** character.
* **Filter Type**: Dicates what field the 'Filter String' is compared against. The 'Filter Type' options are:
  * **Game Title**: Title of the game or rom (ex: Celeste, Super Mario World, Donkey Kong Country, etc).
  * **Platform Name**: Name of the platform (ex: Windows, Nintendo 64, Sony Playstation, etc).
  * **Emulator Title**: Title of the emulator (ex: Retroarch, Dolphin, Cemu, etc).
  * **Exe Path**: The path of the game or emulator EXE file (ex: 'C:\PC_Games\GOG\\\*', 'D:\\\*', '\*\\DOSBox\\\*', etc).
  * **Additional Application Name**: Name of an 'Additional App' (a LaunchBox feature which allows you to specify additional commands to run for a game).
  * **Status**: A field in LaunchBox's metadata for games (right click game, 'Edit' -> 'Edit Metadata') which specifies what platform/launcher/store a game was imported from (ex: '\*Xbox\*', '\*Microsoft Store\*', '\*GOG\*', etc).
* **Ignore Custom Fields**: Enabling this feature causes SteamLauncher to ignore all 'SLEnabled' custom field entries. What are 'SLEnabled' custom field entries? Right click on a game in LaunchBox, click 'Edit' -> 'Edit Metadata', and then select the 'Custom Fields' menu item. By adding a new entry named 'SLEnabled' and setting its value to '1' or '0', SteamLauncher can be made to always be enabled for this game (1) or always be disabled for this game (0). This custom field value will always take precedent over all other filtering features (filter strings, filter modes, etc). The only exception to this is when 'Ignore Custom Fields' is enabled (which will cause all 'SLEnabled' custom field entries to be ignored).

!['SLEnabled' Custom Field Entry](/assets/images/SteamLauncher-SLEnabled_Custom_Field_Entry.png)


## VTables 'Automatic Online Updates'

'Automatic Online Updates' of vtables definitions can be enabled/disabled in the 'Miscellaneous' tab of the SteamLauncher settings dialog. For SteamLauncher to properly work it has to access internal Steam features that are not meant to be accessed by developers outside of Valve. To access these undocumented, unexported functions, SteamLauncher uses a set of definitions which define what these functions are, their function signatures (their parameters, return values, and how to call them), and where to find them within the Steam client DLL. These definitions can change whenever Valve releases a new Steam update and if they change significantly, they can cause SteamLauncher to stop working correctly. In order for SteamLauncher to continue working properly, without the author manually patching the plugin and releasing a new SteamLauncher update every time this happens, an online database was created that can be updated when one of these changes happens, allowing SteamLauncher to continue functioning normally. If 'Automatic Online Updates' is enabled, SteamLauncher will automatically check for new updates every time LaunchBox/BigBox is launched. If new definitions are available, it will silently update and continue working normally. If automatic updates are not enabled, one can manually check for new updates by clicking the 'Force Update' button.

![Miscellaneous Tab](/assets/images/SteamLauncher-SettingsWindow-Misc.png)


## Support

If you have ANY problems with the plugin, I want to know about them. If you need help with any technical issue (see 'Logs'), want to suggest a new feature (I'm very open to any ideas), just want to chat, or anything else related to the plugin, visit the support thread at the link below. I am also available to chat on Steam for any urgent issues or if more direct help is needed.

Plugin Support Thread: https://forums.launchbox-app.com/topic/43142-steamlauncher/

### Logs

To provide help with any technical problems or to investigate a bug, I will need a copy of the debug log that is generated whenever 'Log Level' is set to 'Verbose'. To enable and collect 'Verbose' debug logs, do the following:

* Open the SteamLauncher settings dialog.

* Click the dropdown box under 'Log Level' and select 'Verbose'.

* Click the 'Save' button.

* Restart LaunchBox/BigBox.

* Now simply repeat whatever behavior was causing the issue and/or bug, and after you're sure you've replicated the behavior, close LaunchBox/BigBox.

* Look inside of the SteamLauncher directory for a file named 'debug.log' (there may be more than one sequentially named log file if there was an enormous amount of output but this is unlikely if verbose logging was turned on just briefly).

* Open this 'debug.log' file in a text editor, copy its entire contents, and paste/upload it to [pastebin.com](https://pastebin.com/).

* In the [SteamLauncher support thread](https://forums.launchbox-app.com/topic/43142-steamlauncher/), provide the PasteBin URL, the details of the problem you encountered, and the particulars of your setup (Windows version, 32-bit or 64-bit, LaunchBox version, Steam version, SteamLauncher plugin version, etc). The more information the better.

### Contact Information

**Alias**:
Lahma

**Email**:
lahma0@gmail.com

**Steam Alias**:
[lahma0](https://steamcommunity.com/id/lahma0/)

**Friend me on Steam**:
You can click on my alias above or you can copy/paste the following URL into your address bar:
[steam://friends/add/76561198237461630](steam://friends/add/76561198237461630)

**GitHub Project URL**:
https://github.com/lahma0/SteamLauncher

**LaunchBox Community Forums Profile**:
https://forums.launchbox-app.com/profile/89710-lahma/

**Plugin Support Thread**:
https://forums.launchbox-app.com/topic/43142-steamlauncher/

## Credits

**ChippiHeppu**: Logo

**m4dengi**: [steamclient_tracker](https://bitbucket.org/m4dengi/steamclient_tracker/src/master/) (unfortunately, no longer updated)

**cammelspit**: Help with developing the idea and direction for the project

**Nielk1**: Initial inspiration for the project and just a talented programmer to bounce ideas off of

**Helpful members of the LaunchBox community**: The_Keeper86, JedExodus, FromLostDays, HTPCei, Corgana, Benuno, Neil9000 (sorry for anyone I'm forgetting)

## Known Issues

### MAME Compatibility Fix

It has been observed that MAME crashes when launched through a non-Steam shortcut. This problem is not directly related to SteamLauncher, but since SteamLauncher relies on non-Steam shortcuts, launching MAME via SteamLauncher was no longer working. After a lot of investigation and tinkering by some dedicated users on the LaunchBox forums (cammelspit and JedExodus), a fix was eventually found for the issue. Whether launching MAME directly via a non-Steam shortcut or via SteamLauncher using LaunchBox/BigBox, the fix is simply to modify the launch parameters to include the following:

```
-joystickprovider xinput -keyboardprovider win32
```

If one of these parameters already exists when modifying MAME within LaunchBox, you will obviously need to replace it (don't just add a 2nd instance of '-joystickprovider' or '-keyboardprovider'). You can safely ignore any popups/warnings that LaunchBox displays about pause screen compatibility.

## Legal Stuff

©2022 Valve Corporation. Steam and the Steam logo are trademarks and/or registered trademarks of Valve Corporation in the U.S. and/or other countries.
