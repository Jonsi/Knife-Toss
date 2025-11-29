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
  - `GameplayInstaller` derives from **VContainer**'s `LifetimeScope` and strictly wires dependencies: it registers ScriptableObject data, infrastructure adapters, presentation components (HUD/state presenters, command channel), the `GameplayObjectProvider`, and the `LevelFlowCoordinator` that orchestrates everything.

- **Presentation** (`Assets/Scripts/Presentation`)
  - Divided into `Gameplay` and `UI` subfolders. MonoBehaviours here interact with Unity components, while non-Mono classes (registered via DI) coordinate logic. Examples:
    - `GameplayObjectProvider` instantiates prefabs (target, knife thrower) on demand and keeps them alive for services.
    - `LevelFlowCoordinator` (registered via VContainer) hooks into `LevelSessionService`, `LevelHudPresenter`, and the `LevelCommandEventChannel` to drive lifecycle transitions and UI updates.
    - `LevelHudPresenter` handles HUD text, fail/complete indicators, and raises command events when its buttons are clicked.
    - `KnifeThrower` and `KnifeThrowRelay` convert player input into projectile launches, while knives raise events directly into `KnifeImpactEventChannel`.

## Event Flow

1. **Input** – `KnifeThrower` captures input actions and spawns `Knife` projectiles. `KnifeThrowRelay` forwards `KnifeThrown` events to the service.
2. **Gameplay State** – `LevelSessionService` updates `LevelSession` based on throws, knife impacts, retries, or level progression. The service now exposes asynchronous `UniTask` workflows so level transitions can yield between destruction/creation steps without blocking the main thread.
3. **Events** – `Knife` uses `KnifeImpactEventChannel` so any system (service, VFX, audio) can respond to stuck/fail outcomes without direct references.
4. **Presentation** – `LevelFlowCoordinator` listens to `LevelSession` events, relies on `GameplayObjectProvider` for runtime objects, and updates UI presenters. Button clicks send commands through `LevelCommandEventChannel`, which the coordinator listens to.
5. **Persistence** – When a level completes, `LevelSessionService` saves progress through `ILevelProgressStore` (currently backed by `PlayerPrefs`).

This structure ensures each class owns a single responsibility, lets you replace infrastructure pieces (e.g., switch to addressable configs or cloud saves), and makes future features pluggable by subscribing to events or extending the `GameplayInstaller` rather than editing core gameplay rules. Prefab-driven bootstrapping + VContainer centralizes wiring, while UniTask-ready services allow future async work (loading scenes, spawning VFX, cloud saves) to slot in without refactors.
