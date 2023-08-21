### Changelog

**v0.9.8.5 NETCore - August 20, 2023**

* Fixed a bug which caused ShortcutID generation to fail on non-English Windows systems.


**v0.9.8.4 NETCore - August 17, 2023**

* The plugin being non-functional for many months was due to a Steam update which changed the way non-Steam shortcut ShortcutID values were generated. After spending WAY too many hours/days/weeks poking and prodding the new update in IDA Pro, a solution to the problem has finally been found. I really appreciate everyone's patience while I worked to solve the issue.
* Outside of fixing the issue with the ShortcutID, very little else has changed in this update. For the next update, I'm asking users to submit feature requests to the support thread and I will prioritize the most popular/practical ones. I'm looking forward to implementing some new features instead of troubleshooting annoying Steam-related issues/changes.


**v0.9.8.3 NETCore - August 5, 2022**

* The issue mentioned in the previous changelog, concerning temporary shortcut IDs being generated based on empty values for the shortcut name and path, was apparently fixed by Valve almost immediately after the last update was posted. Accordingly, the fix implemented to work around this issue is no longer needed. That work around appears to have been causing another issue related to the desktop overlay being used as opposed to the Big Picture overlay, even when a controller was connected. As a result of all of this, this hotfix update is being pushed out to roll back that work around which should fix the issue with the overlay. Sorry for the inconvenience!


**v0.9.8.2 NETCore - August 2, 2022**

* A Steam update made massive changes to the IClientShortcuts vtable, removing nearly half of its entries (about 30). Thankfully, none of the removed entries were critical to the functionality needed for the shortcut management the plugin performs. 
* The Steam update also introduced another change which is what prevented me from fixing the plugin by simply updating the online vtables DB. This change alters the way that temporary shortcuts work, making it so that the method previously used to generate shortcut IDs no longer creates the correct value. The more technical explanation is this: Shortcut IDs for non-Steam shortcuts are generated using the shortcut's name and exe path. Now however, temporary shortcut properties no longer actually show the shortcut title, exe path, or any other properties for that matter. The data obviously still exists but it isn't show and therefore Steam calculates the shortcut ID using these blank values causing Steam to always generate the same shortcut ID for all temporary shortcuts (because it is using blank values for the title and exe path). This may just be a bug but it doesn't really matter bc I am now using Steam's API to launch the game using its app ID instead of creating a shortcut using the shortcut ID.

**v0.9.8.1 NETCore - May 24, 2022**

* Fixed a bug which crashed the plugin if the user did not already have 'config.xml' and 'vtables.xml' files in their SteamLauncher directory.
* Added a 'Status' filter type to the 'Selective Use' feature which should help filter based on where games were imported from (Xbox Game Pass, GOG, etc).

**v0.9.8.0 NETCore - May 23, 2022**

* Overhauled a large portion of the codebase, changing nearly all functionality related to Steam internals into dynamic code that can be updated from an online database. Now, regardless of whether vtable offsets are modified or the number/order/type of parameters are changed, I can fix any broken functionality by simply updating an online definitions database. This should make future updates much quicker and more seamless.
* Added a much requested feature: the ability to selectively use the plugin based on a list of filters. This new feature can be found in the settings menu under the tab labeled 'Selective Use'. I won't go into a lot of detail here as I believe it is pretty self-explanatory but I will describe one thing that might not be totally clear by the description in the settings menu. In LaunchBox, if you open any game's properties window and navigate to the 'Custom Fields' tab, you can add a new entry and set the key to 'SLEnabled' (without the quotes). If you then set the value to '0', this game will ALWAYS be ignored by SteamLauncher, regardless of any user-supplied filters. Alternatively, you can set the value to '1' and this game will ALWAYS have its launch intercepted (and run via Steam) by SteamLauncher, regardless of any user-supplied filters. If you have any additional questions, just leave a message on the support thread and I would be happy to explain things further.

**v0.9.7.7 NETCore Public - February 22, 2022**

* Sorry I haven't had a working build uploaded here for quite some time.. I was really trying to finish a complete overhaul of the plugin that I've been working on but I just haven't had enough time to finish it up yet. Instead of requiring people to msg me for a working build, I'm posting this build that I've shared with scores of people in private. Everything should be working just fine. I'm going to skip the change log this time around but I will share all the new changes whenever I have time to finish up and post the complete overhaul I've been working on. Really the only change that needs to be noted between this build and the previous one is that I fixed some breaking changes that had been caused by the way that some of the LB/BB internals operate. This version still contains the online updater feature so if there are any vtable offset changes in Steam, I should be able to provide a quick, easy definitions file update instead of having to release a new version of the plugin (the definition file is checked and updated automatically on every startup). If you have any issues, please report them in the support thread. Thanks!

**v0.9.7.6 NETCore Alpha - March 31, 2021**

* Due to major changes in 'steamclient.dll' since the last release, the previous method for locating certain needed vtable offsets was broken which caused the plugin to stop working for both the beta and non-beta version of Steam. This release adds a new automatic updater for retrieving those vtable offsets from an online DB maintained by the plugin author. This change may only be temporary until a more robust means of dynamically finding these offsets can be developed. It should however mean that the plugin will not be broken by minor vtable changes in future Steam updates.. at least not for very long as the online DB can be updated within a matter of hours after a Steam update is released. All of the new settings can be found under the 'Misc' tab in the settings UI. These include: turning on/off the online automatic updater, manually providing the vtable index for 'GetIClientShortcuts', and manually providing the vtable index for 'GetIClientShortcuts' in the beta version of Steam (sometimes it can differ between the 2 versions).

**v0.9.7.5 NETCore Alpha - February 13, 2021**

* Fixed Bug: When the plugin retrieved the path to a Windows shortcut file (.lnk), if it was enclosed within double quotes, it would be handled incorrectly.
* Removed Dependency: Removed dependency on IWshRuntimeLibrary which was only used for handling Windows shortcut files (.lnk). Added native implementation.

**v0.9.7.4 NETCore Alpha - February 7, 2021**

* Recent Steam update made significant modifications to the IClientShortcuts vtable which broke the method I was using to dynamically find the correct offset for a vtable entry. This update simply fixes that problem.
* Important Note: It has been a long while since I've posted updates to the plugin download page. I've been posting regular updates to the plugin support thread but due to being extremely busy/overwhelmed, I haven't kept up with keeping the plugin download page updated (mostly because I felt I needed to write up proper support docs and change logs when posting the updates in an official capacity). Since I last updated the plugin on the plugin download page, I have made a lot of very significant changes to the plugin (probably one of the biggest being the newly added Settings UI; access via "Tools -> SteamLauncher Settings" in LB). I'm going to resume posting updates to the plugin download page regardless of whether or not I have enough time to fully document each release as I want new users to be able to easily find a working copy of the plugin. Just be warned that the current documentation is out of date. Whenever I finally release v1.0 of the plugin, I will also post an entire rewrite of the plugin documentation along with new logos, images, and I will also update the source code on my Github.

**v0.9.3.0 Beta - June 26, 2019**

* Finally added a dynamic method for finding vtable offsets which should ensure that Steam updates will no longer break compatibility with the plugin

**v0.9.2.0 Beta - June 18, 2019**

* Added full 'Universal Steam Launching' support for LaunchBox's native DOSBox and ScummVM implementation
* Fixed a bug that could cause an emulator path inside of LaunchBox to be overwritten with the path of the Steam executable
* Revised the way that game/emulator window focus is managed in order to fix some problems with games/emulators that use a hybrid borderless full screen mode
* Added support for empty custom platform names; using the example below, your Steam status will now show up as "Doom" instead of "Doom ()"
  * Example: \<Platform Name="Windows" Custom="" /\>
* Added support for games/emulators whose path points to a .lnk Windows shortcut file (which is common for PC games that are added through LaunchBox's "Import Windows Games" wizard)
* Many other small bug fixes, improvements, and optimizations

**v0.9.1.0 Beta - March 25, 2019**

* Beta version of the new plugin including a major overhaul and/or replacement of existing features
* There is now a universal toggle to enable/disable the plugin, and when enabled, any game/emulator launched through LaunchBox/BigBox will now be launched via Steam, without the need to select the “Launch via Steam” context menu item
* Nearly all of the limitations of the old plugin are now eliminated:
  * Compressed roms now supported
  * Non-default emulator configs now supported (“Launch with [...]”)
  * Startup/Shutdown screens now supported


**v0.9.0.5 - Sep 19, 2018**

* The last Steam client update added 3 new entries to the IClientShortcuts vftable. Instead of adding them to the end of the table, as one might expect, they inserted them all into the middle of the table in different positions. This altered the static offsets relied upon by the plugin, causing it to stop working. This update fixes those offsets. I am working on a more reliable solution, so that I do not have to rely on easily-broken static offsets. Until I implement these changes however, I will continue to release these small fixes if needed.

**v0.9.0.4 - Sep 19, 2018**

* Another quick bug-fix release to fix a problem reported by The_Keeper86. Relative 'Start In' paths for LaunchBox games/roms were not being correctly resolved to absolute paths, so this release simply fixes this issue and this issue alone

**v0.9.0.3 - Sep 14, 2018**

* Some changes to the Steam client API broke the plugin, so this was a quick release to fix those problems

* This release relies mostly on static vftable offsets which makes it less resilient against future Steam updates. I'm looking into implementing new smarter ways to locate vftable offsets, so this is just a temporary fix until I can implement those changes

**v0.9.0.2 - May 26, 2018**

* Added support for PC/Windows games, DOSBox games, and ScummVM games

* Fixed a bug preventing any games with a non-ASCII character in their file name from being launched

* Added a workaround for an annoying issue that causes the Steam UI to steal focus and/or move into the foreground in front of LaunchBox/BigBox after exiting certain games. This workaround can be enabled/disabled by changing the value of 'PreventSteamFocusStealing' in config.xml

**v0.9.0.1 - Feb 24, 2018**

* Fixed compatibility with the Steam beta (the Feb 14th Steam beta update made the plugin and/or LaunchBox crash)

**v0.9.0.0 - Feb 11, 2018**

* Initial public release
