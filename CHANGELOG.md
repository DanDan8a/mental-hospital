## 1.3 (I'm back!)
- Reduced map size across all moons and added new config to control its size, if you were using LLL to control the map size it is recommended to disable the custom content on LLL to check the new base value unless you know what you are doing
- Added the first version of the Apparatus Room
- Added ambience lights to all main entrances for better clarity inside the map
- Reworked some scrap spawn points (I will iterate more on this)
- Reduced the amount of max steam valves in a map
- Fixed an issue that would crash the game on landing if there were no steam valves spawned on the map
- Fixed an issue that would spawn steam valves behind some walls

## 1.2.2
- Fixed the scrap generation bug. It should spawn the correct amount of items now!

## 1.2.1
- Tweaked Fire Exit chances to prevent them not spawning sometimes (hopefully)
- Removed a fix that could be causing issues when other mods modify the scrap amount generated (needs more testing)
- Reduced max amount of steam valves present at the map
- Fixed a log error related to the steam valves when the interior is not the Hospital
- Solved an issue with CullFactory's mod culling everything at some entrances
- Adjusted some decorations
- Updated README with new pictures and a google form link to report issues

## 1.2.0
- Introduced 2 new variations to the main entrance (there is now a total of 3!)
- Added the breaker box from the vanilla facility!
- Implemented the steam valves with a special twist: The gas is green, and it also deals some damage if you stay in the gas for too long 😈
- Added new options at the Mental Hospital config to personalize the dungeon size and the steam valve's fog
- Renamed the dungeon from "HospitalFlow" to "Mental Hospital" at the terminal and LLL config. It is recommended to erase the old entry at LLL's config
- Removed turrets from the jump room
- Tweaked some prop spawns
- Spray now works at the reception door and some corners (oops)
- Fixed an issue with a few gaps at the main entrance

## 1.1.1
- Added the hospital as the default interior for PsySanctum (moon) and a link to said moon at the description
- Doorways can now appear with or without a door
- Sighly reduced the spawn chances of some rooms
- Scrap should not appear at the middle of the parkour room anymore
- There is now a limit on how many lamps can appear
- Fixed the spray paint not working on certain walls

## 1.1.0
- Added the big steel doors (those that needs to be open at the terminal)
- Added 2 new variations to the Parkour room (there are now 3 different parkour versions, watch out!)
- Added states to lights, they can be turned off by things like mods or, breaker box or the Apparatus (once it's implemented)
- Reworked halls to make them similar to vanilla's factory. They are no longer pre-made shapes
- Reworked tiles' doorways
- Fixed missing textures at the bathroom
- Made overall map slighly darker
- Final pass on props LODs
- Scrap should not appear inside/below any prop anymore

## 1.0.4
**Performance:** 
- Reduced most textures from 1024 to 512 
- Removed double sided walls/floors/ceilings to avoid rendering unnecessary object 
- Removed some high poly count LODs on props 
- Drastically reduced whole assetbundle size

**Gameplay**
- Reduced map size even further, and made it sightly dynamic to each moon. Average size is now close to x1 max It should be way easier to find scrap even on easy moons 
- Fire Exits should appear further from the main entrance more often now 

**Other**
- Updated README to include a Donation button

## 1.0.3
- **Drastically reduced dungeon sized and locked it between x1 and x1.25 (Rend's size).** This change will help making loot less sparced and should feel better specially on Rend, Dine and Titan. Smaller moons may need more tweaking. 
- Potential fix to an issue with the Inverse Teleporter and LethalThing's traps which teleported played outside the interior bounds 
- Removed collision on doors while they are being open 
- Improved doors animation 
- Added a potential fix to cases on which scrap wasn't generating as it should 
- Removed deprecated config option. Consider deleting the config file so a new one gets generated on next game startup 

**Note: The dev (me) is going to be traveling until 21th so no more updates will come until he is back, please everyone behave well and don't break anything while I'm away**
## 1.0.2
- Spiders and Jesters should not get stuck on doors (maybe it still needs some tweaks)
- Several collider optimizations (beware of turrets tho)
- Office's room door doesnt have an invisible wall anymore
- Turrets no longer can aim through columns
- Prevented a cheesy exploit on the pit room
- Small optimization to some textures and the whole assetbundle
- Updated README
## 1.0.0
- First release!!
