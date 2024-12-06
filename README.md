# Multiplayer Cutscene Notification

A **Stardew Valley mod** that notifies all players when someone starts or finishes a cutscene. While the cutscene is
active, movement is blocked for other players. During this downtime, they can play a mini-game instead of waiting idly.

## How It Works

- When a player initiates a cutscene:
    - A notification is sent to the chat.
    - Movement is blocked for all other players.
    - A mini-game(Junimo cart) is automatically opened for the waiting players.
- When the cutscene ends:
    - Another notification is sent.
    - The mini-game closes, and movement resumes.

## Dependencies

To ensure that in-game time does not progress during cutscenes, the mod requires
the [Pause in Multiplayer](https://www.nexusmods.com/stardewvalley/mods/21327?tab=files) mod. This functionality may be
integrated into this mod in future updates.

## Installation

1. Install the latest version of [SMAPI](https://smapi.io/).
2. Download the mod from the [releases page](https://github.com/your-repo-link/releases).
3. Extract the downloaded `.zip` file into your `Mods` folder.
4. Run the game using SMAPI.

## Compatibility

- **Game Version**: Compatible with Stardew Valley 1.6 and later.
- **Multiplayer**: Designed specifically for multiplayer mode.
- **Platforms**: Windows, macOS, and Linux.