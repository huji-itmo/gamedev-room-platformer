# MyRoomGame

A Unity 6000 first/third-person escape room game. Explore a detailed room, find clues, solve puzzles (safe combination, TV repair), collect items, and manage your inventory via a 3D world-space hotbar.

## Scenes
- **MainMenuScene** — Main menu with settings (volume, resolution, fullscreen)
- **SampleScene** — Main gameplay room (first-person with `FirstPersonController`)
- **SampleSceneVR** — Same room configured for VR (OpenXR)

## Controls
- **WASD** — Move
- **Mouse** — Look around
- **E** — Interact
- **1-4** — Hotbar slot selection

## Features
- Item interaction and pickup (`InteractableWithItem`)
- Inventory with add/remove/consume (`InventoryManager`)
- 4-slot world-space hotbar with 3D previews (`HotbarUI`)
- Pause menu with audio/resolution settings (`PauseManager`)
- Safe puzzle with pin code (`SafeMechanism`)
- Jump pads, animated NPC, TV repair

## Build
```bash
# Linux standalone
"/path/to/Unity" -quit -batchmode -projectPath . -executeMethod BuildScript.PerformBuild

# Android (requires NDK toolchain)
# Build via Unity Editor with Android target selected
```

Requires Unity 6000.4.0f1 with Android module and OpenXR.
