# Phase 1 Prototype Guide

## How to Run the Prototype Scene
1. Open the Unity 2023 LTS URP project located in this repository.
2. In the Project window, load `Assets/_Project/Scenes/Test_Mission.unity`.
3. Press **Play**. The `Systems/GameStateController` prefab auto-initializes and persists, while the `DataBootstrap` object injects sample ScriptableObject data.

## Debug GameState Controls
- Use the on-screen buttons in the Debug State Panel (or the keyboard shortcuts `1-4` and `[`/`]`) to switch between **Lobby → Briefing → Mission → Debrief** states.
- The UI automatically updates when the state changes and will log transitions in the Console for quick tracing.

## ScriptableObject Locations
- Mission nodes: `Assets/_Project/ScriptableObjects/Missions/` (e.g., Neon Exchange, Data Chimera, Shard Garden).
- Player profiles: `Assets/_Project/ScriptableObjects/Profiles/` (e.g., `PlayerProfile_Default`).

## Offline Data Wiring
- `Assets/_Project/Scripts/Core/Data/PrototypeDataLoader.cs` runs in the scene via the `DataBootstrap` GameObject.
- It loads the assigned `PlayerProfile_Default` and mission node list, registers them with `GameStateController`, and forwards the same data to `DebugStatePanel` so the UI has context without any backend calls.
- All data is local-only for Phase 1; swapping the referenced ScriptableObjects instantly reconfigures the prototype.
