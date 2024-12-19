# Fair Multiplayer Cutscene Experience

A **Stardew Valley** mod that adds multiplayer notifications to inform players when one of them starts a cutscene. It
also blocks the movement of all other players by opening a pause menu where they can choose a mini-game and see helpful
tips while waiting for the cutscene to finish. Once the cutscene ends, the menu will automatically close, and the game
will continue.

This mod ensures that players can enjoy cutscenes in multiplayer without worrying about other players' progress, as time
continues to pass during the cutscene, which could be considered unfair.

Optionally, you can install the [Pause Time in Multiplayer Revived](https://www.nexusmods.com/stardewvalley/mods/21327)
mod to pause the in-game time while the cutscene is playing. The Pause Time in Multiplayer Revived mod can independently
stop the game during cutscenes, but it doesn't restrict other players' actions, allowing them to continue playing while
time is frozen, which could give them an unfair advantage. This mod solves that by preventing movement and offering
mini-games to keep waiting players engaged.

**Note**: If you're planning to play only in local co-op, the Pause Time in Multiplayer Revived mod is not required, as
the game will automatically pause the time when the pause menu is opened during cutscenes.

When the /pause command is used, a pause menu will appear displaying a random tip. This feature is customizable and can
be disabled by adjusting the configuration file or using
the [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).

## Install

1. Install the latest version of [SMAPI](https://smapi.io).
2. Download the mod from the [releases page](https://github.com/HarkushaVlad/MultiplayerCutsceneNotification/releases)
   or [NexusMods](https://www.nexusmods.com/stardewvalley/mods/29913) and unzip it into your `Stardew Valley/Mods`
   folder.
3. **Optional**: If you want to pause the in-game time during cutscenes:
   - Download and unzip the [Pause Time in Multiplayer Revived](https://www.nexusmods.com/stardewvalley/mods/21327)
     mod.
   - The game host must enable the **"Any cutscene pauses"** option in the mod settings. This can be done through
     the [Generic Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) or by editing the mod's config file (
     see the [Pause Time in Multiplayer Revived mod description](https://www.nexusmods.com/stardewvalley/mods/21327)
     for configuration instructions).
4. Run the game using SMAPI.

## How to Use

1. **All players must have the mod installed**  
   If a player does not have the mod, they will not be restricted during cutscenes and will be able to move and interact
   with the game as usual.

2. **Start a Multiplayer Game**  
   Once the mod is installed, simply start a multiplayer game as you normally would.

3. **Triggering a Cutscene**  
   When a player triggers a cutscene, a notification will appear for all connected players to inform them that a
   cutscene is happening. Other players will be unable to move during this time, ensuring they are not able to progress
   while the cutscene is playing.

4. **Pause Menu, Mini-Games, and Tips**  
   While the cutscene is active, the pause menu will automatically open for the other players. They will have the option
   to:
   - Choose a mini-game to stay engaged while waiting.
   - View helpful gameplay tips.  
     Once the cutscene ends, the pause menu will close, and the game will continue as normal.

5. **Optionally Pause Time**  
   If you have installed the [Pause Time in Multiplayer Revived](https://www.nexusmods.com/stardewvalley/mods/21327)
   mod, the game time will also be paused while the cutscene is playing, ensuring no progress is made during that time (
   the host must enable the **"Any cutscene pauses"** option in the Pause Time in Multiplayer Revived settings — see
   Step 3 in the "Install" section).  
   If you are playing in local co-op, the game will automatically pause time when the pause menu opens, and the Pause
   Time in Multiplayer Revived mod is not required.
6. **Reset Command**  
   The host has access to the */reset* command, which is designed for extreme cases where something goes wrong. This
   command resets all pause states for all players and forcibly ends any active cutscenes. It can be executed via the
   in-game chat or the console(*reset*) and is useful as a fallback to ensure the game continues smoothly.

## Compatibility

- Supports both online multiplayer and split-screen co-op modes.
- Works with Stardew Valley 1.6 and later on Windows, macOS, and Linux.
- Includes a **Chinese** translation, made by *CNSCZJ*.
