# Folder Structure Proposal

Restructure the scripts under `Assets/Scripts` into feature-first folders so every script that works together lives together. Third-party packages (DOTween, TextMesh Pro, Unity-generated inputs) stay where Unity expects them; only project scripts move as outlined below.

## Proposed Folder Tree (Scripts)
```
Scripts
├─ Common
│  ├─ Attributes
│  │  └─ ReadOnlyAttribute.cs
│  ├─ Config
│  │  └─ IConfigurationProvider.cs
│  ├─ Events
│  │  ├─ IEventChannel.cs
│  │  └─ EventChannel.cs
│  └─ Patterns
│     ├─ State
│     │  ├─ IState.cs
│     │  └─ StateMachine.cs
│     └─ (other shared patterns as they appear)
├─ Editor
│  └─ Drawers
│     └─ ReadOnlyDrawer.cs
└─ Game
   ├─ Installers
   │  └─ GameInstaller.cs
   ├─ Systems
   │  ├─ GameFlow
   │  │  └─ GameFlowController.cs
   │  ├─ Input
   │  │  ├─ InputAdapter.cs
   │  │  └─ InputSystem_Actions.cs
   │  ├─ Logging
   │  │  └─ TestEnvLogHandler.cs
   │  └─ Settings
   │     └─ GameSettings.cs
   ├─ Collision
   │  ├─ Interfaces
   │  │  ├─ IHitable.cs
   │  │  ├─ ICollisionHandler.cs
   │  │  └─ ICollisionRouter.cs
   │  ├─ Data
   │  │  └─ CollisionData.cs
   │  ├─ Logic
   │  │  └─ CollisionHandler.cs
   │  ├─ Services
   │  │  ├─ CollisionRouter.cs
   │  │  └─ CollisionProxy.cs
   │  ├─ Events
   │  │  └─ CollisionEventChannel.cs
   │  └─ Presentation
   │     ├─ CollisionHandlerRegistrar.cs
   │     ├─ KnifeHitKnifeHandler.cs
   │     └─ KnifeHitTargetHandler.cs
   ├─ Knife
   │  ├─ Interfaces
   │  │  ├─ IKnifeController.cs
   │  │  ├─ IKnifeThrowerView.cs
   │  │  └─ IKnifeViewContext.cs
   │  ├─ Data
   │  │  ├─ KnifeData.cs
   │  │  ├─ KnifeSpawnRequestData.cs
   │  │  ├─ KnifeImpactEventData.cs
   │  │  └─ KnifeImpactType.cs
   │  ├─ States
   │  │  ├─ KnifeStateMachine.cs
   │  │  ├─ KnifeStateBase.cs
   │  │  ├─ KnifeBouncedState.cs
   │  │  ├─ KnifeInFlightState.cs
   │  │  ├─ KnifeLoadedState.cs
   │  │  ├─ KnifePooledState.cs
   │  │  └─ KnifeStuckState.cs
   │  ├─ Events
   │  │  ├─ KnifeImpactEventChannel.cs
   │  │  └─ KnifeSpawnRequestEventChannel.cs
   │  └─ Presentation
   │     ├─ KnifeView.cs
   │     └─ KnifeThrowerView.cs
   ├─ Level
   │  ├─ Interfaces
   │  │  ├─ ILevelCatalog.cs
   │  │  └─ ILevelSpawner.cs
   │  ├─ Data
   │  │  ├─ LevelConfig.cs
   │  │  ├─ LevelRules.cs
   │  │  ├─ StageConfig.cs
   │  │  ├─ StageLayoutSettings.cs
   │  │  ├─ StageData.cs
   │  │  └─ LevelStateChangedEventData.cs
   │  ├─ States
   │  │  ├─ StageStateMachine.cs
   │  │  ├─ LevelStateBase.cs
   │  │  ├─ LevelLoadingState.cs
   │  │  ├─ LevelPlayingState.cs
   │  │  ├─ LevelCompletedState.cs
   │  │  └─ LevelFailedState.cs
   │  ├─ Catalogs
   │  │  ├─ StagesConfigCatalog.cs
   │  │  └─ StagesConfigAsset.cs
   │  ├─ Spawning
   │  │  └─ LevelSpawner.cs
   │  └─ Events
   │     └─ LevelStateChangedEventChannel.cs
   ├─ Target
   │  ├─ Interfaces
   │  │  ├─ ITargetView.cs
   │  │  └─ IRotationBehavior.cs
   │  ├─ Data
   │  │  ├─ TargetConfig.cs
   │  │  └─ StickPositionData.cs
   │  ├─ Logic
   │  │  └─ ConstantRotationBehavior.cs
   │  ├─ Factories
   │  │  ├─ RotationBehaviorFactory.cs
   │  │  └─ TargetFactory.cs
   │  └─ Presentation
   │     └─ TargetView.cs
   ├─ Throwing
   │  ├─ Interfaces
   │  │  ├─ IThrowService.cs
   │  │  └─ IPlayerThrowInputListener.cs
   │  ├─ Data
   │  │  └─ KnifeThrowConfiguration.cs
   │  ├─ Configs
   │  │  └─ KnifeThrowConfigAsset.cs
   │  └─ Services
   │     └─ ThrowService.cs
   └─ UI
      ├─ Common
      │  └─ BasePopup.cs
      ├─ Popups
      │  └─ StageFailPopup.cs
      └─ Controllers
         └─ UIController.cs
```

## Full Script Layout (proposed)

### Common
- Attributes  
  - `ReadOnlyAttribute.cs`  ← from `Scripts/Core/Attributes/ReadOnlyAttribute.cs`
- Config  
  - `IConfigurationProvider.cs`  ← from `Scripts/Core/Abstractions/Config/IConfigurationProvider.cs`
- Events  
  - `IEventChannel.cs`  ← from `Scripts/Core/Abstractions/Events/IEventChannel.cs`  
  - `EventChannel.cs`  ← from `Scripts/Infrastructure/Events/EventChannel.cs`
- Patterns / State  
  - `IState.cs`  ← from `Scripts/Core/Abstractions/State/IState.cs`  
  - `StateMachine.cs`  ← from `Scripts/Core/Domain/StateMachine.cs`

### Editor
- Drawers  
  - `ReadOnlyDrawer.cs`  ← from `Scripts/Infrastructure/Editor/ReadOnlyDrawer.cs`

### Game
- Installers  
  - `GameInstaller.cs`  ← from `Scripts/Bootstrapper/GameInstaller.cs`

- Systems  
  - Flow: `GameFlowController.cs`  ← from `Scripts/App/Services/Flow/GameFlowController.cs`  
  - Input: `InputAdapter.cs`, `InputSystem_Actions.cs`  ← from `Scripts/Infrastructure/Input/...`  
  - Logging: `TestEnvLogHandler.cs`  ← from `Scripts/Infrastructure/Logging/TestEnvLogHandler.cs`  
  - Settings: `GameSettings.cs`  ← from `Scripts/Infrastructure/Settings/GameSettings.cs`

- Features

  - Collision  
    - Abstractions: `IHitable.cs`, `ICollisionHandler.cs`, `ICollisionRouter.cs`  ← from `Scripts/Core/Abstractions/Collision/...`  
    - Data: `CollisionData.cs`  ← from `Scripts/Core/Data/Collision/CollisionData.cs`  
    - Domain: `CollisionHandler.cs`  ← from `Scripts/Core/Domain/Collision/CollisionHandler.cs`  
    - Application: `CollisionRouter.cs`, `CollisionProxy.cs`  ← from `Scripts/App/Services/Collision/...`  
    - Events: `CollisionEventChannel.cs`  ← from `Scripts/Infrastructure/Events/CollisionEventChannel.cs`  
    - Presentation: `CollisionHandlerRegistrar.cs`, `KnifeHitKnifeHandler.cs`, `KnifeHitTargetHandler.cs`  ← from `Scripts/Presentation/Gameplay/Collision/...`

  - Knife  
    - Abstractions: `IKnifeController.cs`, `IKnifeThrowerView.cs`, `IKnifeViewContext.cs`  ← from `Scripts/Core/Abstractions/Gameplay/Knife/...`  
    - Data: `KnifeData.cs`, `KnifeSpawnRequestData.cs`, `KnifeImpactEventData.cs`, `KnifeImpactType.cs`  ← from `Scripts/Core/Data/Knife/...`  
    - Domain: `KnifeStateMachine.cs`, `KnifeStateBase.cs`, `KnifeBouncedState.cs`, `KnifeInFlightState.cs`, `KnifeLoadedState.cs`, `KnifePooledState.cs`, `KnifeStuckState.cs`  ← from `Scripts/Core/Domain/Knife/...`  
    - Events: `KnifeImpactEventChannel.cs`, `KnifeSpawnRequestEventChannel.cs`  ← from `Scripts/Infrastructure/Events/...`  
    - Presentation: `KnifeView.cs`, `KnifeThrowerView.cs`  ← from `Scripts/Presentation/Gameplay/Knife/...`

  - Level  
    - Abstractions: `ILevelCatalog.cs`, `ILevelSpawner.cs`  ← from `Scripts/Core/Abstractions/Gameplay/...`  
    - Data: `LevelConfig.cs`, `LevelRules.cs`, `StageConfig.cs`, `StageLayoutSettings.cs`, `StageData.cs`, `LevelStateChangedEventData.cs`  ← from `Scripts/Core/Data/Level/...`  
    - Domain / State Machine: `StageStateMachine.cs`, `LevelStateBase.cs`, `LevelLoadingState.cs`, `LevelPlayingState.cs`, `LevelCompletedState.cs`, `LevelFailedState.cs`  ← from `Scripts/Core/Domain/Level/...`  
    - Catalogs / Assets: `StagesConfigCatalog.cs`, `StagesConfigAsset.cs`  ← from `Scripts/Infrastructure/Leveling/...`  
    - Application / Spawning: `LevelSpawner.cs`  ← from `Scripts/Presentation/Gameplay/Leveling/LevelSpawner.cs`  
    - Events: `LevelStateChangedEventChannel.cs`  ← from `Scripts/Infrastructure/Events/LevelStateChangedEventChannel.cs`

  - Target  
    - Abstractions: `ITargetView.cs`, `IRotationBehavior.cs`  ← from `Scripts/Core/Abstractions/Gameplay/Target/...`  
    - Data: `TargetConfig.cs`, `StickPositionData.cs`  ← from `Scripts/Core/Data/Target/...`  
    - Domain: `ConstantRotationBehavior.cs`  ← from `Scripts/Core/Domain/Target/ConstantRotationBehavior.cs`  
    - Factories: `RotationBehaviorFactory.cs`, `TargetFactory.cs`  ← from `Scripts/Infrastructure/Leveling/RotationBehaviorFactory.cs`, `Scripts/Presentation/Gameplay/Leveling/TargetFactory.cs`  
    - Presentation: `TargetView.cs`  ← from `Scripts/Presentation/Gameplay/Target/TargetView.cs`

  - Throwing  
    - Abstractions: `IThrowService.cs`, `IPlayerThrowInputListener.cs`  ← from `Scripts/Core/Abstractions/Throwing/...`  
    - Data: `KnifeThrowConfiguration.cs`  ← from `Scripts/Core/Data/Throwing/KnifeThrowConfiguration.cs`  
    - Configs: `KnifeThrowConfigAsset.cs`  ← from `Scripts/Infrastructure/Leveling/KnifeThrowConfigAsset.cs`  
    - Services: `ThrowService.cs`  ← from `Scripts/App/Services/Throwing/ThrowService.cs`

  - UI  
    - Common: `BasePopup.cs`  ← from `Scripts/Presentation/UI/Common/BasePopup.cs`  
    - Popups: `StageFailPopup.cs`  ← from `Scripts/Presentation/UI/Popups/StageFailPopup.cs`  
    - Controllers: `UIController.cs`  ← from `Scripts/Presentation/UI/UIController.cs`

