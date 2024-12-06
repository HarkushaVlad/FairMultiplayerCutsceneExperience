# Fair Multiplayer Cutscene Experience

A **Stardew Valley mod** that adds functionality to multiplayer by notifying all players when a cutscene starts or ends.
During the cutscene, movement for all other players is blocked, and they are given the opportunity to play a mini-game (
Junimo Cart) while waiting.

This mod ensures players can enjoy cutscenes in multiplayer without worrying about in-game time progressing or other
players continuing to play while time is frozen, which could give them an unfair advantage. While
the [Pause in Multiplayer](https://www.nexusmods.com/stardewvalley/mods/21327) mod pauses time during cutscenes, it
doesn't block other players from moving or interacting with the game. This mod solves that by preventing movement and
offering a mini-game to keep waiting players engaged.

## Install

1. Install the latest version of [SMAPI](https://smapi.io).
2. Download the mod from the [releases page](https://github.com/HarkushaVlad/MultiplayerCutsceneNotification/releases) or [Nexus](https://www.nexusmods.com/stardewvalley/mods/29913/)
   and unzip it into your `Stardew Valley/Mods` folder.
3. Download and unzip the required mod [Pause in Multiplayer](https://www.nexusmods.com/stardewvalley/mods/21327).
4. Run the game using SMAPI.

## How to Use

This mod will automatically notify players when a cutscene is triggered by another player. When a cutscene starts, other
players will be unable to move and will be prompted to play a mini-game. When the cutscene ends, the mini-game will
close, and player movement will resume.

To use this mod, simply play in multiplayer and participate in any cutscene. There's no need for additional actions to
trigger the mini-game or notifications.

## Dependencies

This mod requires the [Pause in Multiplayer](https://www.nexusmods.com/stardewvalley/mods/21327) mod to ensure that
in-game time is paused during cutscenes. Without this mod, time will continue to progress during cutscenes, which could
cause issues with gameplay.

In future updates, we plan to integrate the time pause functionality directly into this mod, removing the need for the
external dependency.

## Compatibility

- Works with *Stardew Valley* 1.6 and later on Windows, macOS, and Linux.