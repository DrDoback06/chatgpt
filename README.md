# chatgpt

This repository contains a Unity project used for gameplay testing. It now includes a simple chapter flow managed through scripts.

## New Scenes

The following placeholder scenes have been added under `Assets/Scenes/Chapters`:

- **RuinedHomeTown**
- **HauntedForest**
- **RitualGrounds**
- **FinalTemple**

These scenes are loaded sequentially using the `ChapterManager` component.

## Flashback Support

The `FlashbackManager` script allows temporary scene changes that swap the active `Character` and then return to the previous scene. This enables flashback sequences while preserving player progress.
