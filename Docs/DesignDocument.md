# Demon's Descent: The Fractured Oath

This document summarizes the game's core ideas and clarifications discussed so far.

## 1. Title & Concept
A dark 2D side-scroller inspired by *Diablo II* with survival elements and a tragic narrative. Visual style mixes gothic themes with whimsical, vector-based art reminiscent of *Adventure Time*.

## 2. Narrative Overview
- The player is a demon hunter whose hometown was destroyed.
- Flashback sequences are triggered through quest completions.
- Betrayal and tragedy drive the story, culminating in a single inevitable ending.

## 3. Core Mechanics
### Combat & Skills
- Real-time melee, ranged, and magic attacks.
- Each class starts with **5-7 core skills**.
- If the same class is assigned twice, its skills gain a double bonus.
- No class switching, but players can add skills as they progress.

### Survival & Camping
- Camping is a standalone scene entered from exploration.
- Failing to maintain the campfire spawns enemies and ends the camp early.

### Inventory & Items
- Current item rarity tiers remain (Common, Uncommon, Magic, Rare, Epic, Set, Unique, Legendary).

### Multiplayer
- Split-screen co-op with optional online integration.
- Each player has separate progression and inventories.

### Art & Modularity
- Characters and enemies are assembled from sprite parts using a dictionary lookup.
- Randomized parts allow visual variety (e.g., multiple head sprites per enemy type).

### Level Generation
- Mix of hand-crafted and procedurally generated levels.
- Generation can be toggled to choose the ratio of handmade vs. random scenes.

## 4. Outstanding Tasks (from issue tracker)
- Create comprehensive README with setup instructions.
- Implement a CampingManager for the dedicated camping scene.
- Expand quest system to trigger flashbacks and branching dialogue.
- Integrate split-screen co-op with optional online networking.
- Add sprite dictionary system for modular characters and enemies.
- Support toggling between handcrafted and generated levels.
