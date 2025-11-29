# Knife Toss Architecture Overview

The project is organized using a layered, dependency-inverted structure so that gameplay rules remain decoupled from Unity-specific code. Every layer references only the layer beneath it, which keeps the codebase modular and testable.

## Layer Breakdown

- **Core** (`Assets/Scripts/Core`)
  - **Data** holds pure data definitions (`LevelData`, `LevelConfig`).
  - **Domain** contains mutable state objects such as `LevelSession`/`LevelRules` that model gameplay progress with no Unity dependencies.
  - **Abstractions** defines interfaces like `ILevelCatalog` and `ILevelProgressStore` so higher layers depend on contracts instead of implementations.
  - **Events** contains ScriptableObject channels (e.g., `KnifeImpactEventChannel`) that broadcast gameplay events without tight coupling.

- **Application** (`Assets/Scripts/Application`)
  - Implements use-case/services such as `LevelSessionService` that orchestrate domain objects and invoke interfaces. Application classes stay unaware of Unity scene objects.

- **Infrastructure** (`Assets/Scripts/Infrastructure`)
  - Provides Unity-facing adapters that implement Core interfaces, e.g., `LevelConfigCatalog` (ScriptableObject-backed level data), `PlayerPrefsProgressStore`, `KnivesHierarchyManager`, and `PreStuckKnifeSpawner`.

- **Installers** (`Assets/Scripts/Installers`)
  - `GameplayInstaller` acts as the composition root: it gathers scene references, instantiates domain/application objects, wires dependencies, and exposes the configured services to presentation components. This replaces the old monolithic `GameController` logic.

- **Presentation** (`Assets/Scripts/Presentation`)
  - Divided into `Gameplay` and `UI` subfolders. MonoBehaviours here interact with Unity components, forward input, and react to service events. Examples:
    - `LevelLifecyclePresenter` subscribes to `LevelSession` events and updates UI presenters.
    - `KnifeThrower`, `KnifeThrowRelay`, and `KnifeEventRelay` convert player input and projectile events into service calls.
    - `LevelHudPresenter` and `GameStatePresenter` update TextMeshPro labels and UI state based on session data.

## Event Flow

1. **Input** – `KnifeThrower` captures input actions and spawns `Knife` projectiles. `KnifeThrowRelay` forwards `KnifeThrown` events to the service.
2. **Gameplay State** – `LevelSessionService` updates `LevelSession` based on throws, knife impacts, retries, or level progression.
3. **Events** – `Knife` uses `KnifeImpactEventChannel` so any system (service, VFX, audio) can respond to stuck/fail outcomes without direct references.
4. **Presentation** – `LevelLifecyclePresenter` listens to `LevelSession` events and updates UI (`LevelHudPresenter`, `GameStatePresenter`).
5. **Persistence** – When a level completes, `LevelSessionService` saves progress through `ILevelProgressStore` (currently backed by `PlayerPrefs`).

This structure ensures each class owns a single responsibility, lets you replace infrastructure pieces (e.g., switch to addressable configs or cloud saves), and makes future features pluggable by subscribing to events or extending installers without editing core gameplay rules.
