---
name: Soft Currency Architecture
overview: Implement a robust soft currency system with fruit type configuration (ScriptableObjects), CurrencyManager service, remote config override support, event-driven UI updates, and integration with existing collectible collision system.
todos:
  - id: fruit-configs
    content: Create FruitTypeConfig and FruitTypeCatalog ScriptableObject classes
    status: pending
  - id: currency-events
    content: Create CoinBalanceChangedEventData and EventChannel classes
    status: pending
  - id: persistence-layer
    content: Extend PlayerProgressData and GameData with TotalCoins/CurrentCoins fields
    status: pending
  - id: currency-manager
    content: Implement CurrencyManager service with fruit lookup and remote config override support
    status: pending
  - id: economy-installer
    content: Create EconomyInstaller and register services with DI container
    status: pending
  - id: persistence-connector
    content: Create CurrencyPersistenceConnector to auto-save coin changes
    status: pending
  - id: collectible-integration
    content: Update CollectibleConfig, CollectibleView, and KnifeHitCollectibleHandler to use fruit types
    status: pending
  - id: stage-generator
    content: Update StageGenerator to assign fruit type IDs based on value ranges
    status: pending
  - id: coin-ui
    content: Create CoinBalanceDisplay HUD component
    status: pending
  - id: unity-setup-instructions
    content: Document Unity Inspector setup steps for ScriptableObject assets
    status: pending
isProject: false
---

# Soft Currency Architecture Implementation

## Overview

Implement a complete economy system that converts fruit collectibles into coins, with ScriptableObject-based configuration and remote config override capability.

## Current Architecture Analysis

**Collectible Flow:**

- Single collectible prefab spawned via [`CollectibleSpawner.cs`](Assets/Scripts/Game/Entities/Collectible/Spawning/CollectibleSpawner.cs)
- Collision handled by [`KnifeHitCollectibleHandler.cs`](Assets/Scripts/Game/CollisionHandlers/KnifeHitCollectibleHandler.cs)
- Events raised via `CollectibleHitEventChannel` with position and value
- Currently tracked as `TotalCollectiblesCollected` in [`PlayerProgressData.cs`](Assets/Scripts/Game/Persistence/Data/PlayerProgressData.cs)

**Key Integration Points:**

- Event system: ScriptableObject-based `EventChannel<T>` pattern
- DI: VContainer with service registration in installers
- Persistence: JSON file storage via [`PlayerDataService.cs`](Assets/Scripts/Game/Persistence/Services/PlayerDataService.cs)
- UI Updates: Event-driven subscriptions (see [`CollectibleCountDisplay.cs`](Assets/Scripts/Game/UI/HUD/CollectibleCountDisplay.cs))

## Implementation Plan

### 1. Create Fruit Type Configuration System

**New File: `Assets/Scripts/Game/Economy/Data/FruitTypeConfig.cs`**

- ScriptableObject defining individual fruit types
- Properties: `FruitId` (string), `DisplayName`, `DefaultCoinValue`, `Sprite`, `HitParticlePrefab`
- Will serve as default/fallback values for remote config

**New File: `Assets/Scripts/Game/Economy/Data/FruitTypeCatalog.cs`**

- ScriptableObject containing array of all `FruitTypeConfig` assets
- Provides centralized registry for fruit type lookups
- Reference from [`PrefabsConfiguration.cs`](Assets/Scripts/Game/Pooling/PrefabsConfiguration.cs:1-52) pattern

**Create Unity Assets** (Instructions for Unity Inspector):

- Create 5 FruitTypeConfig assets in `Assets/Configurations/Economy/FruitTypes/`:
  - `TomatoConfig.asset` (DefaultCoinValue: 1)
  - `LemonConfig.asset` (DefaultCoinValue: 5)
  - `KiwiConfig.asset` (DefaultCoinValue: 10)
  - `CherriesConfig.asset` (DefaultCoinValue: 25)
  - `PomegranateConfig.asset` (DefaultCoinValue: 100)
- Create `FruitTypeCatalog.asset` in `Assets/Configurations/Economy/` referencing all 5 configs

### 2. Create Currency Event System

**New File: `Assets/Scripts/Game/Economy/Events/CoinBalanceChangedEventData.cs`**

- Event data struct: `PreviousBalance`, `NewBalance`, `AmountChanged`

**New File: `Assets/Scripts/Game/Economy/Events/CoinBalanceChangedEventChannel.cs`**

- ScriptableObject event channel for coin balance updates
- Follows existing pattern from [`CollectibleHitEventChannel.cs`](Assets/Scripts/Game/Entities/Collectible/Events/CollectibleHitEventChannel.cs:1-15)

**Create Unity Asset** (Instructions):

- Create `CoinBalanceChangedEventChannel.asset` in `Assets/Configurations/EventChannels/`

### 3. Extend Persistence Layer

**Modify: [`PlayerProgressData.cs`](Assets/Scripts/Game/Persistence/Data/PlayerProgressData.cs:1-15)**

- Add `public int TotalCoins;` field for persistent coin balance
- Keep existing `TotalCollectiblesCollected` for analytics

**Modify: [`GameData.cs`](Assets/Scripts/Game/Flow/Game/GameData.cs:1-18)**

- Add `public int CurrentCoins { get; internal set; }` for runtime coin state
- Update `Reset()` method to reset coins

**Modify: [`PlayerDataService.cs`](Assets/Scripts/Game/Persistence/Services/PlayerDataService.cs:120-127)**

- Update `UpdateProgress()` to sync `TotalCoins` between `GameData` and `PlayerProgressData`
- Update `SyncGameData()` (line 136-140) to load coins on initialization

### 4. Create Currency Manager Service

**New File: `Assets/Scripts/Game/Economy/Services/CurrencyManager.cs`**

- Service managing coin balance and fruit type configurations
- **Initialization**: Load `FruitTypeCatalog`, build `Dictionary<string, int>` mapping FruitId → CoinValue
- **Methods**:
  - `AddCoins(string fruitId, Vector2 position)`: Looks up fruit coin value, adds to balance, raises event
  - `SpendCoins(int amount)`: Deduct coins (for future shop integration)
  - `GetBalance()`: Returns current coin count
  - `ApplyRemoteConfigOverrides(Dictionary<string, int>)`: Override coin values from remote config
- **Dependencies**: Inject `GameData`, `IEventChannel<CoinBalanceChangedEventData>`, `FruitTypeCatalog`
- **Event Raising**: On balance change, raise `CoinBalanceChangedEventChannel` with before/after values

**New File: `Assets/Scripts/Game/Economy/Interfaces/ICurrencyService.cs`**

- Interface for `CurrencyManager` to support testing and future flexibility

### 5. Create Economy Installer

**New File: `Assets/Scripts/Game/Installers/EconomyInstaller.cs`**

- Register `CurrencyManager` as singleton with `IInitializable` interface
- Inject `FruitTypeCatalog` ScriptableObject asset
- Follow pattern from [`PersistenceInstaller.cs`](Assets/Scripts/Game/Installers/PersistenceInstaller.cs:1-34)

**Modify: Existing Game Installer** (locate via exploration)

- Add `EconomyInstaller` to the installer chain

### 6. Create Currency Persistence Connector

**New File: `Assets/Scripts/Game/Economy/Services/CurrencyPersistenceConnector.cs`**

- Listens to `CoinBalanceChangedEventChannel`
- Calls `PlayerDataService.RequestSave()` on coin changes
- Implements `IInitializable` and `IDisposable` for event subscription lifecycle
- Mirrors pattern from [`StagePersistenceConnector.cs`](Assets/Scripts/Game/Persistence/Services/StagePersistenceConnector.cs:1-64)

**Update: `EconomyInstaller.cs`**

- Register `CurrencyPersistenceConnector` as entry point

### 7. Integrate with Collectible System

**Modify: [`CollectibleConfig.cs`](Assets/Scripts/Game/Entities/Collectible/Data/CollectibleConfig.cs:1-31)**

- Add `public string FruitTypeId { get; private set; }` property
- Update constructor to accept `fruitTypeId` parameter
- Keep `Value` property for backwards compatibility (can be deprecated later)

**Modify: [`CollectibleView.cs`](Assets/Scripts/Game/Entities/Collectible/Views/CollectibleView.cs:56-66)**

- Add `public string FruitTypeId { get; private set; }` property
- Update `Initialize()` to store `FruitTypeId` from config
- Optionally support sprite swapping based on fruit type (if prefab has SpriteRenderer)

**Modify: [`KnifeHitCollectibleHandler.cs`](Assets/Scripts/Game/CollisionHandlers/KnifeHitCollectibleHandler.cs:28-44)**

- Inject `CurrencyManager` via constructor
- After collectible hit validation (line 36-37), call `_CurrencyManager.AddCoins(collectible.FruitTypeId, data.ImpactPoint)`
- Keep existing `CollectibleHitEventChannel` raise for UI/audio feedback

### 8. Update Stage Generator for Fruit Types

**Modify: [`StageGenerator.cs`](Assets/Scripts/Editor/StageGenerator.cs:341-358)**

- Update `ConfigureItems()` method to assign fruit types based on value:
  - Value 1: "Tomato"
  - Value 2-5: "Lemon"
  - Value 6-10: "Kiwi"
  - Value 11-30: "Cherries"
  - Value 31+: "Pomegranate"
- Update `CollectibleConfig` instantiation to include `fruitTypeId` parameter
- Add comment: "// Note: Fruit type assignment will be configurable via FruitTypeCatalog in future editor tools"

### 9. Create Coin Balance UI Display

**New File: `Assets/Scripts/Game/UI/HUD/CoinBalanceDisplay.cs`**

- Similar to [`CollectibleCountDisplay.cs`](Assets/Scripts/Game/UI/HUD/CollectibleCountDisplay.cs:1-82)
- Subscribe to `CoinBalanceChangedEventChannel` and `LevelStartedEventChannel`
- Inject `GameData` to sync initial balance
- Update TextMeshPro text with coin count
- Optional: Add coin icon sprite alongside text

**Unity Setup Instructions:**

- Add `CoinBalanceDisplay` component to HUD Canvas
- Assign `CoinBalanceChangedEventChannel` and `LevelStartedEventChannel` references
- Configure TextMeshPro text field

### 10. Visual Feedback System (TODO Placeholder)

**Note for Future Implementation:**

- The codebase has no existing floating text system (confirmed via research)
- TextMeshPro is available in project
- Visual feedback currently limited to particles/shatter effects

**Future File: `Assets/Scripts/Game/Feedback/UI/FloatingTextFeedbackService.cs`** (Not implemented in this phase)

- Would listen to `CoinBalanceChangedEventChannel` or `CollectibleHitEventChannel`
- Spawn floating text (e.g., "+5 coins") at world position
- Use TextMeshPro with DOTween for animation
- TODO: Requires prefab creation and pooling setup

**Current Workaround:**

- Add TODO comment in `CurrencyManager.AddCoins()`: "// TODO: Trigger floating text feedback showing +{coinValue}"
- Existing particle effects will continue to work via `CollectibleView.PlayHitFeedback()`

## Integration Summary

### Data Flow

```
Knife Hits Collectible
    ↓
KnifeHitCollectibleHandler validates & marks hit
    ↓
CurrencyManager.AddCoins(fruitTypeId, position)
    ├→ Looks up coin value from FruitTypeCatalog (or remote override)
    ├→ Updates GameData.CurrentCoins
    └→ Raises CoinBalanceChangedEventChannel
          ├→ CoinBalanceDisplay updates UI
          └→ CurrencyPersistenceConnector triggers save
                ↓
          PlayerDataService syncs to PlayerProgressData.TotalCoins
                ↓
          JSON file save via FilePlayerDataStore
```

### Remote Config Integration (Future)

When remote config is implemented:

1. On app start or config refresh, call `CurrencyManager.ApplyRemoteConfigOverrides(remoteValues)`
2. Remote config format: `{"Tomato": 5, "Lemon": 10, ...}`
3. Manager updates internal lookup dictionary
4. All subsequent collections use overridden values
5. ScriptableObject defaults used as fallback if remote config unavailable

## Testing Checklist

- [ ] Collect Tomato → Verify +1 coin (or overridden value)
- [ ] Collect multiple fruits → Verify cumulative balance
- [ ] Complete level → Verify coins persist after reload
- [ ] Restart app → Verify coin balance loads from save file
- [ ] Modify FruitTypeConfig DefaultCoinValue in Inspector → Verify new value used
- [ ] Test remote config override → Verify runtime value override works

## Files to Create (11 new files)

1. `Assets/Scripts/Game/Economy/Data/FruitTypeConfig.cs`
2. `Assets/Scripts/Game/Economy/Data/FruitTypeCatalog.cs`
3. `Assets/Scripts/Game/Economy/Events/CoinBalanceChangedEventData.cs`
4. `Assets/Scripts/Game/Economy/Events/CoinBalanceChangedEventChannel.cs`
5. `Assets/Scripts/Game/Economy/Services/CurrencyManager.cs`
6. `Assets/Scripts/Game/Economy/Interfaces/ICurrencyService.cs`
7. `Assets/Scripts/Game/Economy/Services/CurrencyPersistenceConnector.cs`
8. `Assets/Scripts/Game/Installers/EconomyInstaller.cs`
9. `Assets/Scripts/Game/UI/HUD/CoinBalanceDisplay.cs`

## Files to Modify (8 files)

1. [`PlayerProgressData.cs`](Assets/Scripts/Game/Persistence/Data/PlayerProgressData.cs) - Add TotalCoins field
2. [`GameData.cs`](Assets/Scripts/Game/Flow/Game/GameData.cs) - Add CurrentCoins property
3. [`PlayerDataService.cs`](Assets/Scripts/Game/Persistence/Services/PlayerDataService.cs) - Sync coin data
4. [`CollectibleConfig.cs`](Assets/Scripts/Game/Entities/Collectible/Data/CollectibleConfig.cs) - Add FruitTypeId
5. [`CollectibleView.cs`](Assets/Scripts/Game/Entities/Collectible/Views/CollectibleView.cs) - Store FruitTypeId
6. [`KnifeHitCollectibleHandler.cs`](Assets/Scripts/Game/CollisionHandlers/KnifeHitCollectibleHandler.cs) - Call CurrencyManager
7. [`StageGenerator.cs`](Assets/Scripts/Editor/StageGenerator.cs) - Assign fruit types
8. Game installer (TBD) - Add EconomyInstaller

## Unity Inspector Setup Required

After code implementation:

1. Create FruitTypeConfig assets for all 5 fruit types in `Assets/Configurations/Economy/FruitTypes/`
2. Create FruitTypeCatalog asset referencing all configs
3. Create CoinBalanceChangedEventChannel asset in `Assets/Configurations/EventChannels/`
4. Assign FruitTypeCatalog to EconomyInstaller in Unity scene
5. Add CoinBalanceDisplay component to HUD Canvas and wire up references
6. Regenerate stage data via StageGenerator to populate fruit type IDs