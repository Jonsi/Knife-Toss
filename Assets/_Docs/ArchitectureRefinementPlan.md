# Architecture Refinement Plan

## Executive Summary

This document provides a comprehensive plan to reorganize the existing scripts in `Assets/Scripts/Game/` to follow the Clean Architecture structure with proper MVP patterns for all entities.

### Target Layer Structure

| Layer | Purpose | Dependencies |
|-------|---------|--------------|
| **Domain** | Pure C# data models, value objects, domain events | None (no Unity) |
| **App** | Application services, flow controllers, use cases | Domain |
| **Infrastructure** | Factories, repositories, external system adapters | Domain, App |
| **Presentation** | Views, Presenters, UI components (MVP) | Domain, App, Infrastructure |
| **Persistence** | Data storage, serialization | Domain |

---

## Current State Analysis

### Scripts by Current Location

```
Game/
├── App/                    # ⚠️ Incomplete - only has Flow/Events
├── CollisionHandlers/      # ✗ Should be in App or Infrastructure
├── DI/                     # ✓ OK (BaseInstallerSO)
├── Domain/                 # ⚠️ Incomplete - only stats classes
├── Entities/               # ✗ Not following MVP pattern
├── Feedback/               # ✗ Should be split between App/Infrastructure
├── Flow/                   # ✗ Should be in App/Flow
├── Infrastructure/         # ⚠️ Incomplete - only StageFailFactorySO
├── Input/                  # ✗ Should be in Infrastructure
├── Installers/             # ✓ OK
├── KnifeThrower/           # ✗ Mixed concerns - should be split
├── Level/                  # ✗ Mixed concerns - should be split
├── Persistence/            # ✓ OK structure, needs minor adjustments
├── Pooling/                # ✗ Should be in Infrastructure
├── PowerUps/               # ✗ Mixed concerns - should be split
├── Presentation/           # ✓ Good structure for UI
├── Settings/               # ✗ Should be in Domain/Infrastructure
└── UI/                     # ✗ Legacy - should merge into Presentation
```

---

## Target Folder Structure

```
Game/
├── Domain/                           # Pure C# - NO Unity dependencies
│   ├── Knife/
│   │   ├── KnifeState.cs             # State constants
│   │   └── KnifeImpactType.cs        # (move from Entities)
│   ├── Target/
│   │   └── TargetState.cs            # State constants
│   ├── Collectible/
│   │   └── CollectibleState.cs       # State constants
│   ├── Level/
│   │   ├── LevelStats.cs             # ✓ EXISTS
│   │   └── LevelConfig.cs            # (pure data portion)
│   ├── Stage/
│   │   ├── StageStats.cs             # ✓ EXISTS
│   │   └── StageConfig.cs            # (pure data portion)
│   ├── Player/
│   │   └── PlayerStats.cs            # ✓ EXISTS
│   ├── PowerUp/
│   │   └── PowerUpInventoryData.cs   # (move from PowerUps/Data)
│   └── Settings/
│       └── GameSettingsData.cs       # Pure settings data
│
├── App/                              # Application Layer
│   ├── Flow/
│   │   ├── Events/                   # ✓ EXISTS
│   │   ├── Game/
│   │   │   ├── GameData.cs
│   │   │   ├── GameStateMachine.cs
│   │   │   └── States/
│   │   ├── Level/
│   │   │   ├── LevelData.cs
│   │   │   ├── LevelFlowController.cs
│   │   │   ├── LevelStateMachine.cs
│   │   │   └── States/
│   │   ├── Stage/
│   │   │   ├── StageData.cs
│   │   │   ├── StageFlowController.cs
│   │   │   ├── StageStateMachine.cs
│   │   │   └── States/
│   │   └── Interfaces/
│   ├── Services/
│   │   ├── Throwing/
│   │   │   ├── IThrowService.cs
│   │   │   └── ThrowService.cs
│   │   ├── PowerUp/
│   │   │   ├── IPowerUpService.cs
│   │   │   └── PowerUpService.cs
│   │   ├── Collision/
│   │   │   ├── CollisionHandlerRegistrar.cs
│   │   │   ├── KnifeHitTargetHandler.cs
│   │   │   ├── KnifeHitKnifeHandler.cs
│   │   │   └── KnifeHitCollectibleHandler.cs
│   │   ├── Level/
│   │   │   └── LevelSpawner.cs
│   │   └── Settings/
│   │       └── SettingsService.cs
│   └── UseCases/
│       └── (future use case classes)
│
├── Infrastructure/                   # External systems & factories
│   ├── Input/
│   │   ├── InputAdapter.cs
│   │   └── InputSystem_Actions.cs
│   ├── Audio/
│   │   ├── AudioPlayer.cs
│   │   └── SoundSettings.cs          # SO Configuration
│   ├── Feedback/
│   │   ├── KnifeAudioFeedbackService.cs
│   │   ├── CollectibleAudioFeedbackService.cs
│   │   ├── LevelAudioFeedbackService.cs
│   │   └── VFXFeedbackService.cs
│   ├── Pooling/
│   │   ├── KnifePoolProvider.cs
│   │   ├── KnifePoolService.cs
│   │   ├── ParticlePoolProvider.cs
│   │   ├── PrefabsConfiguration.cs
│   │   └── PrefabEntry.cs
│   ├── Factories/
│   │   ├── TargetFactory.cs
│   │   ├── TargetBehaviorFactory.cs
│   │   ├── StageFailFactorySO.cs     # ✓ EXISTS
│   │   └── CollectibleSpawner.cs
│   ├── Events/
│   │   ├── Knife/
│   │   │   ├── KnifeImpactEventChannel.cs
│   │   │   └── KnifeThrownEventChannel.cs
│   │   ├── Collectible/
│   │   │   └── CollectibleHitEventChannel.cs
│   │   └── PowerUp/
│   │       ├── PowerUpActivatedEventChannel.cs
│   │       └── PowerUpUsedEventChannel.cs
│   ├── Configs/
│   │   ├── KnifeThrowConfigAsset.cs
│   │   ├── PowerUpCatalog.cs
│   │   ├── PowerUpConfigBase.cs
│   │   ├── SuperKnifeConfig.cs
│   │   ├── MultipleKnivesConfig.cs
│   │   ├── CollectiblesBladeConfig.cs
│   │   ├── StagesConfigAsset.cs
│   │   └── StagesConfigCatalog.cs
│   └── Animation/
│       ├── KnifeLoadAnimationConfig.cs
│       ├── TargetHitAnimationConfig.cs
│       ├── TargetSpawnAnimationConfig.cs
│       └── SpriteShatterAnimationConfig.cs
│
├── Presentation/                     # UI & Entity Views (MVP)
│   ├── Common/
│   │   ├── BasePopupView.cs          # ✓ EXISTS
│   │   └── BaseView.cs               # ✓ EXISTS
│   ├── HUD/                          # ✓ EXISTS (properly structured)
│   │   ├── Collectibles/
│   │   ├── KnifeMagazine/
│   │   ├── MuteButton/
│   │   ├── PowerUps/
│   │   └── StageProgress/
│   ├── Popups/                       # ✓ EXISTS
│   │   └── StageFail/
│   ├── Controllers/
│   │   └── UIController.cs           # (move from UI/Controllers)
│   └── Entities/                     # NEW: MVP for game entities
│       ├── Knife/
│       │   ├── Interfaces/
│       │   │   └── IKnifeView.cs
│       │   ├── Models/
│       │   │   └── KnifeModel.cs
│       │   ├── Views/
│       │   │   ├── KnifeView.cs
│       │   │   └── KnifeThrowerView.cs
│       │   ├── Presenters/
│       │   │   └── KnifePresenter.cs
│       │   ├── States/               # State machine stays with presentation
│       │   └── Feedback/
│       │       └── KnifeLoadAnimator.cs
│       ├── Target/
│       │   ├── Interfaces/
│       │   │   └── ITargetView.cs
│       │   ├── Models/
│       │   │   └── TargetModel.cs
│       │   ├── Views/
│       │   │   └── TargetView.cs
│       │   ├── Presenters/
│       │   │   └── TargetPresenter.cs
│       │   ├── Behaviors/            # Runtime behaviors
│       │   │   ├── LocalBehaviorRuntime.cs
│       │   │   ├── WorldBehaviorRuntime.cs
│       │   │   └── RotationBehaviors/
│       │   └── Feedback/
│       │       ├── TargetHitFeedback.cs
│       │       ├── TargetSpawnAnimator.cs
│       │       └── SpriteShatter/
│       └── Collectible/
│           ├── Interfaces/
│           │   └── ICollectibleView.cs
│           ├── Models/
│           │   └── CollectibleModel.cs
│           ├── Views/
│           │   └── CollectibleView.cs
│           └── Presenters/
│               └── CollectiblePresenter.cs
│
├── Persistence/                      # ✓ EXISTS (properly structured)
│   ├── Data/
│   │   ├── PlayerData.cs
│   │   ├── PlayerProgressData.cs
│   │   └── PlayerSettingsData.cs
│   ├── Interfaces/
│   │   └── IPlayerDataStore.cs
│   ├── Services/
│   │   └── PlayerDataService.cs
│   └── Storage/
│       └── FilePlayerDataStore.cs
│
├── DI/                               # ✓ EXISTS
│   └── BaseInstallerSO.cs
│
└── Installers/                       # ✓ EXISTS
    └── (all installer SOs)
```

---

## Migration Tasks by Script

### Phase 1: Domain Layer Population

#### 1.1 Move Pure Data Classes to Domain

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Entities/Knife/Data/KnifeImpactType.cs` | `Domain/Knife/KnifeImpactType.cs` | Move |
| `Entities/Knife/Data/KnifeImpactEventData.cs` | `Domain/Knife/KnifeImpactEventData.cs` | Move |
| `Entities/Knife/Data/KnifeSpawnRequestData.cs` | `Domain/Knife/KnifeSpawnRequestData.cs` | Move |
| `Entities/Collectible/Data/CollectibleConfig.cs` | `Domain/Collectible/CollectibleConfig.cs` | Move |
| `Entities/Collectible/Data/CollectibleHitEventData.cs` | `Domain/Collectible/CollectibleHitEventData.cs` | Move |
| `Entities/Target/Data/TargetConfig.cs` | `Domain/Target/TargetConfig.cs` | Move |
| `Entities/Target/Data/PreStuckKnifeConfig.cs` | `Domain/Target/PreStuckKnifeConfig.cs` | Move |
| `PowerUps/Data/PowerUpInventoryData.cs` | `Domain/PowerUp/PowerUpInventoryData.cs` | Move |
| `PowerUps/Data/PowerUpActivatedEventData.cs` | `Domain/PowerUp/PowerUpActivatedEventData.cs` | Move |
| `PowerUps/Data/PowerUpUsedEventData.cs` | `Domain/PowerUp/PowerUpUsedEventData.cs` | Move |
| `PowerUps/Data/PowerUpRuntimeState.cs` | `Domain/PowerUp/PowerUpRuntimeState.cs` | Move |
| `PowerUps/Data/PowerUpThrowContext.cs` | `Domain/PowerUp/PowerUpThrowContext.cs` | Move |
| `KnifeThrower/Configs/KnifeThrowConfiguration.cs` | `Domain/Knife/KnifeThrowConfiguration.cs` | Move (pure data) |

#### 1.2 Create New Domain Files

| File | Purpose |
|------|---------|
| `Domain/Knife/KnifeState.cs` | State constants for knife |
| `Domain/Target/TargetState.cs` | State constants for target |
| `Domain/Collectible/CollectibleState.cs` | State constants for collectible |

---

### Phase 2: App Layer Migration

#### 2.1 Move Flow Controllers

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Flow/Game/GameData.cs` | `App/Flow/Game/GameData.cs` | Move |
| `Flow/Game/GameStateMachine.cs` | `App/Flow/Game/GameStateMachine.cs` | Move |
| `Flow/Game/States/*` | `App/Flow/Game/States/*` | Move all |
| `Flow/Level/LevelData.cs` | `App/Flow/Level/LevelData.cs` | Move |
| `Flow/Level/LevelFlowController.cs` | `App/Flow/Level/LevelFlowController.cs` | Move |
| `Flow/Level/States/*` | `App/Flow/Level/States/*` | Move all |
| `Flow/Stage/StageData.cs` | `App/Flow/Stage/StageData.cs` | Move |
| `Flow/Stage/StageFlowController.cs` | `App/Flow/Stage/StageFlowController.cs` | Move |
| `Flow/Stage/States/*` | `App/Flow/Stage/States/*` | Move all |
| `Flow/Interfaces/*` | `App/Flow/Interfaces/*` | Move all |
| `GameFlowController.cs` | `App/Flow/GameFlowController.cs` | Move |

#### 2.2 Move Services

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `KnifeThrower/Services/ThrowService.cs` | `App/Services/Throwing/ThrowService.cs` | Move |
| `KnifeThrower/Interfaces/IThrowService.cs` | `App/Services/Throwing/IThrowService.cs` | Move |
| `KnifeThrower/Interfaces/IPlayerThrowInputListener.cs` | `App/Services/Throwing/IPlayerThrowInputListener.cs` | Move |
| `KnifeThrower/Interfaces/IKnifeThrowerView.cs` | `Presentation/Entities/Knife/Interfaces/IKnifeThrowerView.cs` | Move |
| `PowerUps/Services/PowerUpService.cs` | `App/Services/PowerUp/PowerUpService.cs` | Move |
| `PowerUps/Services/CollectibleRegistry.cs` | `App/Services/Collectible/CollectibleRegistry.cs` | Move |
| `PowerUps/Interfaces/IPowerUpService.cs` | `App/Services/PowerUp/IPowerUpService.cs` | Move |
| `PowerUps/Interfaces/IPowerUp.cs` | `Domain/PowerUp/IPowerUp.cs` | Move (interface) |
| `CollisionHandlers/CollisionHandlerRegistrar.cs` | `App/Services/Collision/CollisionHandlerRegistrar.cs` | Move |
| `CollisionHandlers/KnifeHitTargetHandler.cs` | `App/Services/Collision/KnifeHitTargetHandler.cs` | Move |
| `CollisionHandlers/KnifeHitKnifeHandler.cs` | `App/Services/Collision/KnifeHitKnifeHandler.cs` | Move |
| `CollisionHandlers/KnifeHitCollectibleHandler.cs` | `App/Services/Collision/KnifeHitCollectibleHandler.cs` | Move |
| `Level/Spawning/LevelSpawner.cs` | `App/Services/Level/LevelSpawner.cs` | Move |
| `Level/Interfaces/ILevelSpawner.cs` | `App/Services/Level/ILevelSpawner.cs` | Move |
| `Level/Interfaces/ILevelCatalog.cs` | `App/Services/Level/ILevelCatalog.cs` | Move |
| `UI/Services/SettingsService.cs` | `App/Services/Settings/SettingsService.cs` | Move |

---

### Phase 3: Infrastructure Layer Migration

#### 3.1 Move Input System

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Input/InputAdapter.cs` | `Infrastructure/Input/InputAdapter.cs` | Move |
| `Input/InputSystem_Actions.cs` | `Infrastructure/Input/InputSystem_Actions.cs` | Move |

#### 3.2 Move Audio Infrastructure

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Feedback/Audio/Systems/AudioPlayer.cs` | `Infrastructure/Audio/AudioPlayer.cs` | Move |
| `Feedback/Audio/Data/SoundSettings.cs` | `Infrastructure/Audio/SoundSettings.cs` | Move |
| `Feedback/Audio/Interfaces/IAudioPlayer.cs` | `Infrastructure/Audio/IAudioPlayer.cs` | Move |

#### 3.3 Move Feedback Services

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Feedback/Audio/Services/KnifeAudioFeedbackService.cs` | `Infrastructure/Feedback/KnifeAudioFeedbackService.cs` | Move |
| `Feedback/Audio/Services/CollectibleAudioFeedbackService.cs` | `Infrastructure/Feedback/CollectibleAudioFeedbackService.cs` | Move |
| `Feedback/Audio/Services/LevelAudioFeedbackService.cs` | `Infrastructure/Feedback/LevelAudioFeedbackService.cs` | Move |
| `Feedback/VFX/Services/VFXFeedbackService.cs` | `Infrastructure/Feedback/VFXFeedbackService.cs` | Move |

#### 3.4 Move Pooling

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Pooling/KnifePoolProvider.cs` | `Infrastructure/Pooling/KnifePoolProvider.cs` | Move |
| `Pooling/KnifePoolService.cs` | `Infrastructure/Pooling/KnifePoolService.cs` | Move |
| `Pooling/KnifePrefabEntry.cs` | `Infrastructure/Pooling/KnifePrefabEntry.cs` | Move |
| `Pooling/ParticlePoolProvider.cs` | `Infrastructure/Pooling/ParticlePoolProvider.cs` | Move |
| `Pooling/ParticlePrefabEntry.cs` | `Infrastructure/Pooling/ParticlePrefabEntry.cs` | Move |
| `Pooling/ParticleReturnHandler.cs` | `Infrastructure/Pooling/ParticleReturnHandler.cs` | Move |
| `Pooling/PrefabEntry.cs` | `Infrastructure/Pooling/PrefabEntry.cs` | Move |
| `Pooling/PrefabsConfiguration.cs` | `Infrastructure/Pooling/PrefabsConfiguration.cs` | Move |

#### 3.5 Move Factories

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Entities/Target/Factories/TargetFactory.cs` | `Infrastructure/Factories/TargetFactory.cs` | Move |
| `Entities/Target/Factories/TargetBehaviorFactory.cs` | `Infrastructure/Factories/TargetBehaviorFactory.cs` | Move |
| `Entities/Collectible/Spawning/CollectibleSpawner.cs` | `Infrastructure/Factories/CollectibleSpawner.cs` | Move |

#### 3.6 Move Event Channels

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Entities/Knife/Events/KnifeImpactEventChannel.cs` | `Infrastructure/Events/Knife/KnifeImpactEventChannel.cs` | Move |
| `KnifeThrower/Events/KnifeThrownEventChannel.cs` | `Infrastructure/Events/Knife/KnifeThrownEventChannel.cs` | Move |
| `Entities/Collectible/Events/CollectibleHitEventChannel.cs` | `Infrastructure/Events/Collectible/CollectibleHitEventChannel.cs` | Move |
| `PowerUps/Events/PowerUpActivatedEventChannel.cs` | `Infrastructure/Events/PowerUp/PowerUpActivatedEventChannel.cs` | Move |
| `PowerUps/Events/PowerUpUsedEventChannel.cs` | `Infrastructure/Events/PowerUp/PowerUpUsedEventChannel.cs` | Move |

#### 3.7 Move Configurations (SO Assets)

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `KnifeThrower/Configs/KnifeThrowConfigAsset.cs` | `Infrastructure/Configs/KnifeThrowConfigAsset.cs` | Move |
| `PowerUps/Configs/PowerUpCatalog.cs` | `Infrastructure/Configs/PowerUpCatalog.cs` | Move |
| `PowerUps/Configs/PowerUpConfigBase.cs` | `Infrastructure/Configs/PowerUpConfigBase.cs` | Move |
| `PowerUps/Configs/SuperKnifeConfig.cs` | `Infrastructure/Configs/SuperKnifeConfig.cs` | Move |
| `PowerUps/Configs/MultipleKnivesConfig.cs` | `Infrastructure/Configs/MultipleKnivesConfig.cs` | Move |
| `PowerUps/Configs/CollectiblesBladeConfig.cs` | `Infrastructure/Configs/CollectiblesBladeConfig.cs` | Move |
| `Level/Configurations/Stage/StagesConfigAsset.cs` | `Infrastructure/Configs/StagesConfigAsset.cs` | Move |
| `Level/Configurations/Stage/StagesConfigCatalog.cs` | `Infrastructure/Configs/StagesConfigCatalog.cs` | Move |
| `Level/Configurations/Stage/StageConfig.cs` | `Infrastructure/Configs/StageConfig.cs` | Move (SO portion) |
| `Level/Configurations/Stage/StageLayoutSettings.cs` | `Infrastructure/Configs/StageLayoutSettings.cs` | Move |
| `Level/Configurations/Level/LevelConfig.cs` | `Infrastructure/Configs/LevelConfig.cs` | Move (SO portion) |
| `Settings/GameSettings.cs` | `Infrastructure/Configs/GameSettings.cs` | Move |

#### 3.8 Move Animation Configs

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Feedback/Animation/Data/KnifeLoadAnimationConfig.cs` | `Infrastructure/Animation/KnifeLoadAnimationConfig.cs` | Move |
| `Feedback/Animation/Data/SpriteShatterAnimationConfig.cs` | `Infrastructure/Animation/SpriteShatterAnimationConfig.cs` | Move |
| `Feedback/Animation/Data/TargetHitAnimationConfig.cs` | `Infrastructure/Animation/TargetHitAnimationConfig.cs` | Move |
| `Feedback/Animation/Data/TargetSpawnAnimationConfig.cs` | `Infrastructure/Animation/TargetSpawnAnimationConfig.cs` | Move |

---

### Phase 4: Presentation Layer - Entity MVP Refactoring

#### 4.1 Move Entity Views to Presentation

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Entities/Knife/Views/KnifeView.cs` | `Presentation/Entities/Knife/Views/KnifeView.cs` | Move + Refactor to passive |
| `Entities/Knife/Views/KnifeThrowerView.cs` | `Presentation/Entities/Knife/Views/KnifeThrowerView.cs` | Move |
| `Entities/Knife/Interfaces/IKnifeViewContext.cs` | `Presentation/Entities/Knife/Interfaces/IKnifeViewContext.cs` | Move |
| `Entities/Knife/States/*` | `Presentation/Entities/Knife/States/*` | Move all |
| `Entities/Knife/Feedback/KnifeLoadAnimator.cs` | `Presentation/Entities/Knife/Feedback/KnifeLoadAnimator.cs` | Move |
| `Entities/Target/Views/TargetView.cs` | `Presentation/Entities/Target/Views/TargetView.cs` | Move + Refactor to passive |
| `Entities/Target/Interfaces/ITargetView.cs` | `Presentation/Entities/Target/Interfaces/ITargetView.cs` | Move |
| `Entities/Target/Interfaces/ITargetHitFeedback.cs` | `Presentation/Entities/Target/Interfaces/ITargetHitFeedback.cs` | Move |
| `Entities/Target/Interfaces/ILocalTargetBehavior.cs` | `Presentation/Entities/Target/Behaviors/ILocalTargetBehavior.cs` | Move |
| `Entities/Target/Interfaces/IWorldTargetBehavior.cs` | `Presentation/Entities/Target/Behaviors/IWorldTargetBehavior.cs` | Move |
| `Entities/Target/Logic/*` | `Presentation/Entities/Target/Behaviors/*` | Move all |
| `Entities/Target/RotationBehaviors/*` | `Presentation/Entities/Target/Behaviors/Rotation/*` | Move all |
| `Entities/Target/WorldBehaviors/*` | `Presentation/Entities/Target/Behaviors/World/*` | Move all |
| `Entities/Target/Services/StickPositionService.cs` | `Presentation/Entities/Target/Services/StickPositionService.cs` | Move |
| `Entities/Target/Feedback/*` | `Presentation/Entities/Target/Feedback/*` | Move all |
| `Entities/Target/Data/Behaviors/*` | `Presentation/Entities/Target/Behaviors/Data/*` | Move all |
| `Entities/Collectible/Views/CollectibleView.cs` | `Presentation/Entities/Collectible/Views/CollectibleView.cs` | Move + Refactor to passive |
| `PowerUps/Views/PowerUpButtonView.cs` | `Presentation/HUD/PowerUps/PowerUpButtonView.cs` | Move |

#### 4.2 Move Legacy UI to Presentation

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `UI/Controllers/UIController.cs` | `Presentation/Controllers/UIController.cs` | Move |
| `UI/Interfaces/ILevelHudData.cs` | `Presentation/HUD/Interfaces/ILevelHudData.cs` | Move |
| `UI/ViewModels/LevelHudViewModel.cs` | `Presentation/HUD/ViewModels/LevelHudViewModel.cs` | Move |

#### 4.3 Create New MVP Components for Entities

**Knife MVP:**

| File | Purpose |
|------|---------|
| `Presentation/Entities/Knife/Interfaces/IKnifeView.cs` | Passive view interface |
| `Presentation/Entities/Knife/Models/KnifeModel.cs` | Reactive model with state |
| `Presentation/Entities/Knife/Presenters/KnifePresenter.cs` | Logic coordinator |

**Target MVP:**

| File | Purpose |
|------|---------|
| `Presentation/Entities/Target/Models/TargetModel.cs` | Reactive model with hits, state |
| `Presentation/Entities/Target/Presenters/TargetPresenter.cs` | Logic coordinator |

**Collectible MVP:**

| File | Purpose |
|------|---------|
| `Presentation/Entities/Collectible/Interfaces/ICollectibleView.cs` | Passive view interface |
| `Presentation/Entities/Collectible/Models/CollectibleModel.cs` | Reactive model |
| `Presentation/Entities/Collectible/Presenters/CollectiblePresenter.cs` | Logic coordinator |

---

### Phase 5: Cleanup

#### 5.1 Delete Empty/Redundant Folders

After migration, delete these folders:

```
Game/CollisionHandlers/     # All moved to App/Services/Collision/
Game/Entities/              # All moved to Presentation/Entities/ and Domain/
Game/Feedback/              # All moved to Infrastructure/
Game/Flow/                  # All moved to App/Flow/
Game/Input/                 # All moved to Infrastructure/Input/
Game/KnifeThrower/          # All split between App/, Domain/, Infrastructure/
Game/Level/                 # All split between App/, Domain/, Infrastructure/
Game/Pooling/               # All moved to Infrastructure/Pooling/
Game/PowerUps/              # All split between App/, Domain/, Infrastructure/
Game/Settings/              # All moved to Infrastructure/Configs/
Game/UI/                    # All moved to Presentation/
```

---

## Assembly Definition Updates

### New/Updated Assembly Definitions

| Assembly | Purpose | References |
|----------|---------|------------|
| `Game.Domain` | Pure domain models | None (no Unity refs) |
| `Game.App` | Application services | Game.Domain, Common, VContainer |
| `Game.Infrastructure` | External systems | Game.Domain, Game.App, Common, Unity, VContainer |
| `Game.Presentation` | Views & presenters | Game.Domain, Game.App, Game.Infrastructure, Common, R3, UniTask, VContainer |
| `Game.Persistence` | Data storage | Game.Domain, Common |

### Assembly Reference Map

```
                    ┌─────────────────┐
                    │   Game.Domain   │
                    │   (Pure C#)     │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
      ┌───────────┐  ┌─────────────┐  ┌─────────────┐
      │ Game.App  │  │  Game.      │  │ Game.       │
      │           │  │ Persistence │  │Presentation │
      └─────┬─────┘  └─────────────┘  └──────┬──────┘
            │                                │
            └──────────────┬─────────────────┘
                           │
                    ┌──────▼──────┐
                    │   Game.     │
                    │Infrastructure│
                    └─────────────┘
```

---

## Entity MVP Refactoring Details

### Current KnifeView Problems

1. Contains state machine logic
2. Handles collision detection and routing
3. Manages its own out-of-view detection
4. Contains power-up awareness (IsSuperKnife)
5. Directly communicates with event channels

### Target KnifeView (Passive)

```csharp
public interface IKnifeView : IPooledView
{
    Transform Transform { get; }
    Rigidbody2D Rigidbody { get; }
    SpriteRenderer SpriteRenderer { get; }
    Observable<Collision2D> OnTriggerEnter { get; }
    
    void SetActive(bool isActive);
    void SetSuperKnifeVisual(bool isSuper);
    void PlayStickAnimation();
    void PlayBounceAnimation();
    void PlayShatterEffect();
    void SetColliderEnabled(bool enabled);
    void SetVelocity(Vector2 velocity);
    void SetPosition(Vector3 position);
    void SetRotation(Quaternion rotation);
    void SetParent(Transform parent);
}
```

### Target KnifeModel (Reactive State)

```csharp
public class KnifeModel : IModel
{
    public ReactiveProperty<int> State { get; } = new(KnifeState.Idle);
    public ReactiveProperty<bool> IsSuperKnife { get; } = new(false);
    public ReactiveProperty<float> ThrowSpeed { get; } = new(0f);
    public ReactiveProperty<bool> IsInFlight { get; } = new(false);
    
    public void Reset()
    {
        State.Value = KnifeState.Idle;
        IsSuperKnife.Value = false;
        ThrowSpeed.Value = 0f;
        IsInFlight.Value = false;
    }
}
```

### Target KnifePresenter (Logic)

```csharp
public class KnifePresenter : BasePresenter<IKnifeView, KnifeModel>
{
    private readonly KnifeStateMachine _StateMachine;
    private readonly IEventChannel<CollisionData> _CollisionChannel;
    private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;
    
    public KnifePresenter(
        IKnifeView view,
        KnifeModel model,
        IEventChannel<CollisionData> collisionChannel,
        IEventChannel<KnifeImpactEventData> impactChannel) : base(view, model)
    {
        _CollisionChannel = collisionChannel;
        _ImpactChannel = impactChannel;
        
        InitializeStateMachine();
        BindModelToView();
        BindViewToPresenter();
    }
    
    private void BindModelToView()
    {
        Model.IsSuperKnife
            .Subscribe(isSuper => View.SetSuperKnifeVisual(isSuper))
            .AddTo(Disposable);
            
        Model.State
            .Where(s => s == KnifeState.Stuck)
            .Subscribe(_ => View.PlayStickAnimation())
            .AddTo(Disposable);
    }
    
    private void BindViewToPresenter()
    {
        View.OnTriggerEnter
            .Subscribe(HandleCollision)
            .AddTo(Disposable);
    }
    
    public void Launch(Vector2 direction, float speed)
    {
        Model.ThrowSpeed.Value = speed;
        Model.IsInFlight.Value = true;
        Model.State.Value = KnifeState.Flying;
        View.SetVelocity(direction * speed);
    }
}
```

---

## Migration Priority Order

### Batch 1: Foundation (No Breaking Changes)
1. Create new folders in Domain/, App/, Infrastructure/
2. Create assembly definitions
3. Create new interfaces and models

### Batch 2: Domain Migration
1. Move pure data classes to Domain/
2. Update namespaces
3. Update assembly references

### Batch 3: App Migration
1. Move Flow/ to App/Flow/
2. Move services to App/Services/
3. Update namespaces

### Batch 4: Infrastructure Migration
1. Move Input/, Pooling/, Audio/
2. Move Factories/, Events/, Configs/
3. Update namespaces

### Batch 5: Presentation Migration
1. Move Entity views
2. Create MVP components
3. Refactor views to passive
4. Update namespaces

### Batch 6: Cleanup
1. Delete empty folders
2. Verify all references
3. Update installers if needed

---

## Checklist

### Domain Layer
- [ ] Move KnifeImpactType, KnifeImpactEventData, KnifeSpawnRequestData
- [ ] Move CollectibleConfig, CollectibleHitEventData
- [ ] Move TargetConfig, PreStuckKnifeConfig
- [ ] Move PowerUp data classes
- [ ] Create KnifeState, TargetState, CollectibleState constants

### App Layer
- [ ] Move all Flow/ contents to App/Flow/
- [ ] Move ThrowService and interfaces
- [ ] Move PowerUpService and interfaces
- [ ] Move CollisionHandlers
- [ ] Move LevelSpawner
- [ ] Move SettingsService

### Infrastructure Layer
- [ ] Move InputAdapter
- [ ] Move AudioPlayer and SoundSettings
- [ ] Move all Feedback services
- [ ] Move all Pooling classes
- [ ] Move all Factories
- [ ] Move all Event Channels
- [ ] Move all Configuration SOs

### Presentation Layer
- [ ] Move KnifeView and create MVP triad
- [ ] Move TargetView and create MVP triad
- [ ] Move CollectibleView and create MVP triad
- [ ] Move UIController
- [ ] Move PowerUpButtonView to HUD

### Assembly Definitions
- [ ] Update Game.Domain.asmdef
- [ ] Create Game.App.asmdef
- [ ] Create Game.Infrastructure.asmdef
- [ ] Update Game.Presentation.asmdef (if exists) or create
- [ ] Update Game.asmdef main references

### Cleanup
- [ ] Delete empty folders
- [ ] Verify no broken references
- [ ] Update installers

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Prefab references break | Move files via Unity Editor, not file system |
| Assembly reference errors | Update asmdef files BEFORE moving code |
| Namespace conflicts | Use IDE refactoring tools |
| Missing DI registrations | Update installers alongside file moves |
| Runtime null references | Test after each batch |

---

## Notes

1. **Preserve XML summaries** - Do not delete any existing documentation
2. **Move via Unity Editor** - To preserve GUID references
3. **Batch updates** - Make changes in small testable batches
4. **Assembly boundaries** - Ensure Domain has NO Unity dependencies
5. **Interface segregation** - Views should only expose rendering methods

---

*Document Version: 1.0*
*Created: December 2024*

