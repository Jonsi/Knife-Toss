# Unity Setup Guide

This guide explains how to set up the Knife Toss game in Unity after the architecture refactor.

---

## Prerequisites

### Required Unity Packages

The project uses the following packages (install via Package Manager):

1. **VContainer** (Dependency Injection)
   - Version: Latest
   - Install via: OpenUPM or Git URL
   - Git URL: `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer`

2. **UniTask** (Async/Await for Unity)
   - Version: Latest
   - Install via: OpenUPM
   - Package: `com.cysharp.unitask`

3. **DOTween** (Animation)
   - Version: Latest
   - Install via: Asset Store or Git URL
   - Note: Already included in project at `Assets/DOTween/`

4. **TextMeshPro** (UI Text)
   - Version: Latest
   - Install via: Package Manager (Built-in)

### Unity Version

- **Recommended:** Unity 2021.3 LTS or later
- **Minimum:** Unity 2020.3 LTS

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/              # Domain models, abstractions, constants
│   ├── Application/       # Use cases, services, coordinators
│   ├── Infrastructure/    # Pools, factories, persistence, audio
│   ├── Presentation/      # Views, UI, gameplay components
│   └── Installers/        # Dependency injection configuration
├── Prefabs/               # Game object prefabs
├── Configurations/         # ScriptableObject configs and events
├── Scenes/                # Unity scenes
└── _Docs/                 # Documentation
```

---

## Scene Setup

### MainGame Scene

1. **Create Bootstrap GameObject**
   - Name: `Bootstrap`
   - Add Component: `ProjectLifetimeScope` (from `KnifeToss.Infrastructure.Installers`)
   - This initializes the root dependency injection container

2. **Create Gameplay GameObject**
   - Name: `Gameplay`
   - Add Component: `GameplayInstaller` (from `KnifeToss.Installers`)
   - Configure all serialized fields (see Configuration section below)

3. **Camera Setup**
   - Ensure Main Camera exists
   - Tag: `MainCamera`
   - Position: `(0, 0, -10)` (typical 2D setup)

4. **Canvas Setup**
   - Create Canvas for UI
   - Add `LevelHudPresenter` component to a GameObject in the Canvas
   - Configure UI references (see UI Setup section)

---

## Prefab Setup

### Knife Prefab

**Location:** `Assets/Prefabs/Knife.prefab`

**Required Components:**
1. `Knife` (main facade)
2. `KnifeMovement` (movement/physics)
3. `KnifeCollision` (collision detection)
4. `KnifeSticking` (sticking logic)
5. `KnifeBounceAnimation` (bounce animation)
6. `KnifeAudio` (audio playback)
7. `KnifeVisibility` (camera visibility checks)
8. `Rigidbody2D` (for physics)
9. `Collider2D` (CircleCollider2D recommended)

**Setup Steps:**
1. Create empty GameObject named "Knife"
2. Add all required components listed above
3. Configure `KnifeMovement`:
   - Cache `Rigidbody2D` reference in inspector
3. Configure `KnifeCollision`:
   - Cache `Collider2D[]` array in inspector
4. Configure `KnifeSticking`:
   - Set `embedOffset` (default: 0.02)
   - Assign `KnifeImpactEventChannel` ScriptableObject
   - Optionally assign `snapPosition` Transform child
5. Configure `KnifeBounceAnimation`:
   - Assign `KnifeBounceOffAnimationConfig` ScriptableObject
6. Configure `KnifeAudio`:
   - Set `soundVolume` (default: 1.0)
7. Save as prefab

### KnifeThrower Prefab

**Location:** `Assets/Prefabs/KnifeThrower.prefab`

**Required Components:**
1. `KnifeThrower` (main facade)
2. `ThrowingInput` (input handling)
3. `AimCalculator` (aim direction calculation)
4. `KnifeLauncher` (knife launching)
5. `KnifeMagazine` (magazine queue management)

**Setup Steps:**
1. Create empty GameObject named "KnifeThrower"
2. Add all required components
3. Configure `ThrowingInput`:
   - Assign `throwInputSource` (MonoBehaviour implementing `IThrowInput`)
   - Example: `InputActionThrowInput` component
4. Configure `AimCalculator`:
   - Assign `launcherTransform` (usually the KnifeThrower's transform)
5. Configure `KnifeLauncher`:
   - Assign `launcherTransform`
   - Set `throwSpeed` (default: 10.0)
6. Configure `KnifeMagazine`:
   - Assign `knifePool` reference
   - Set magazine settings (visible queue count, spacing, etc.)
7. Save as prefab

### Target Prefab

**Location:** `Assets/Prefabs/Target.prefab`

**Required Components:**
1. `RotatingTarget` (rotation behavior)
2. `CircleCollider2D` (for collision detection)
3. `SpriteRenderer` (visual)

**Setup Steps:**
1. Create GameObject with Circle sprite
2. Add `RotatingTarget` component
3. Configure rotation settings
4. Add `CircleCollider2D`
5. Tag: `Target` (use `Tags.Target` constant)
6. Save as prefab

---

## Configuration Setup

### ScriptableObject Assets

Create the following ScriptableObject assets in `Assets/Configurations/`:

#### 1. LevelConfig
- **Path:** `Assets/Configurations/LevelConfig.asset`
- **Type:** `LevelConfig`
- **Usage:** Defines all level data
- **Setup:** Configure level definitions, procedural generation settings

#### 2. SoundSettings
- **Path:** `Assets/Configurations/SoundSettings.asset`
- **Type:** `SoundSettings`
- **Usage:** Audio clip references
- **Setup:** Assign AudioClip references for:
  - HitTargetSound
  - HitKnifeSound
  - ButtonClickSound

#### 3. Event Channels

Create in `Assets/Configurations/Events/`:

- **KnifeImpactEventChannel.asset**
  - Type: `KnifeImpactEventChannel`
  - Used for knife stuck events

- **LevelCommandEventChannel.asset**
  - Type: `LevelCommandEventChannel`
  - Used for level commands (next, retry)

- **HitableObjectHitEventChannel.asset**
  - Type: `HitableObjectHitEventChannel`
  - Used for hitable object hit events

- **ButtonClickEventChannel.asset**
  - Type: `ButtonClickEventChannel`
  - Used for button click events

#### 4. Animation Configs

- **KnifeBounceOffAnimationConfig.asset**
  - Type: `KnifeBounceOffAnimationConfig`
  - Configure bounce animation parameters

- **RetryButtonAnimationConfig.asset**
  - Type: `RetryButtonAnimationConfig`
  - Configure retry button animation

- **ScoreTextConfiguration.asset**
  - Type: `ScoreTextConfiguration`
  - Configure score text display

---

## Dependency Injection Setup

### ProjectLifetimeScope (Bootstrap)

**Location:** `Assets/Scripts/Infrastructure/Installers/ProjectLifetimeScope.cs`

**Current Registration:**
```csharp
- IGameEventBus → GameEventBus (singleton)
- IAudioPlayer → UnityAudioPlayer (from hierarchy)
- IInputService → InputService (from hierarchy)
```

**To Add New Services:**
1. Open `ProjectLifetimeScope.cs`
2. Add registration in `Configure()` method
3. Example:
```csharp
builder.Register<RuntimeCacheService>(Lifetime.Singleton);
builder.Register<PooledAudioService>(Lifetime.Singleton).As<IAudioService>();
```

### GameplayInstaller

**Location:** `Assets/Scripts/Installers/GameplayInstaller.cs`

**Required Serialized Fields:**
- `levelConfig` - LevelConfig ScriptableObject
- `knifeImpactChannel` - KnifeImpactEventChannel ScriptableObject
- `levelCommandEventChannel` - LevelCommandEventChannel ScriptableObject
- `soundSettings` - SoundSettings ScriptableObject
- `buttonClickEventChannel` - ButtonClickEventChannel ScriptableObject
- `startLevel` - Starting level number (default: 1)
- `targetPrefab` - Target prefab GameObject
- `knifeThrowerPrefab` - KnifeThrower prefab GameObject
- `knifePrefab` - Knife prefab GameObject
- `hitableHitEventChannel` - HitableObjectHitEventChannel ScriptableObject
- `hudPresenter` - LevelHudPresenter component reference

**Current Registrations:**
- Level configuration and data
- Event channels
- Services (Audio, Settings, Score, Star Rating)
- Gameplay objects (Target, KnifeThrower)
- Level management (Session, Catalog, Spawners)

---

## UI Setup

### LevelHudPresenter

**Component:** `LevelHudPresenter` (from `KnifeToss.Presentation.UI`)

**Required References:**
- `levelText` - TextMeshProUGUI for level display
- `levelCompletePopup` - LevelCompletePopup component
- `retryButton` - Button component
- `muteButton` - Button component (optional)
- `muteIcon` - GameObject for mute icon
- `unmuteIcon` - GameObject for unmute icon

**Setup Steps:**
1. Create Canvas if not exists
2. Create GameObject named "LevelHUD"
3. Add `LevelHudPresenter` component
4. Assign all required references
5. Wire up button onClick events (handled automatically via events)

### LevelCompletePopup

**Component:** `LevelCompletePopup` (from `KnifeToss.Presentation.UI`)

**Required References:**
- Star display UI elements
- Score text
- Next/Retry buttons

---

## Input Setup

### Input System

The project uses Unity's Input System.

**Required Actions:**
- Create Input Actions asset: `InputSystem_Actions.inputactions`
- Define actions:
  - `Throw` (Button) - For throwing knives
  - `Aim` (Vector2) - For aim direction (mouse/touch position)

**InputActionThrowInput Component:**
- Attach to KnifeThrower GameObject
- Assign Input Actions asset
- Map actions to Throw and Aim

---

## Pooling Setup

### Knife Pool

The new architecture uses `PooledKnifeFactory` for knife pooling.

**Registration (in GameplayInstaller):**
```csharp
builder.Register<PooledKnifeFactory>(Lifetime.Singleton)
    .As<IKnifeFactory>()
    .WithParameter("prefab", knifePrefab)
    .WithParameter("parent", knivesParentTransform)
    .WithParameter("warmupCount", 15);
```

**Manual Setup (if needed):**
1. Create empty GameObject named "KnifePool"
2. The factory will create pool instances automatically
3. Pool warmup happens on factory creation

### Audio Pool

**Registration:**
```csharp
builder.Register<PooledAudioService>(Lifetime.Singleton)
    .As<IAudioService>()
    .WithParameter("poolSize", 8);
```

### VFX Pool

**Registration:**
```csharp
builder.Register<PooledVFXService>(Lifetime.Singleton)
    .As<IVFXService>()
    .WithParameter("defaultPoolSize", 5);
```

---

## Runtime Cache Service

**Purpose:** Caches frequently accessed Unity objects (Camera.main, etc.)

**Registration:**
```csharp
builder.Register<RuntimeCacheService>(Lifetime.Singleton);
```

**Usage:**
Inject `RuntimeCacheService` into components that need camera access:
```csharp
[VContainer.Inject]
public void Construct(RuntimeCacheService cacheService)
{
    Camera cam = cacheService.MainCamera; // Cached, no FindObject calls
}
```

---

## Tags and Layers Setup

### Tags

Create the following tags in Unity (Edit → Project Settings → Tags and Layers):

- `Knife` - For knife GameObjects
- `Target` - For target GameObjects
- `Hitable` - For hitable objects (tomatoes, etc.)
- `Player` - For player GameObject (if needed)

**Note:** Use `Tags.Knife`, `Tags.Target`, etc. constants in code instead of magic strings.

### Layers

Create the following layers (if needed):

- `Projectile` (Layer 8)
- `Target` (Layer 9)
- `Hitable` (Layer 10)

**Note:** Use `Layers.Projectile`, etc. constants in code.

---

## Testing the Setup

### Quick Verification Checklist

1. ✅ **Bootstrap Scene:**
   - ProjectLifetimeScope component exists
   - No errors in console

2. ✅ **Gameplay Scene:**
   - GameplayInstaller component exists
   - All serialized fields assigned
   - No missing references

3. ✅ **Prefabs:**
   - Knife prefab has all required components
   - KnifeThrower prefab has all required components
   - Target prefab configured correctly

4. ✅ **Configuration Assets:**
   - LevelConfig asset created and configured
   - SoundSettings asset created with audio clips
   - All event channel assets created

5. ✅ **UI:**
   - LevelHudPresenter component exists
   - All UI references assigned
   - Canvas configured

6. ✅ **Input:**
   - Input Actions asset created
   - InputActionThrowInput component configured

### Common Issues

**Issue: Missing Component References**
- **Solution:** Ensure all prefabs have required components added
- Check inspector for missing component warnings

**Issue: Dependency Injection Errors**
- **Solution:** Verify all services are registered in installers
- Check console for VContainer registration errors

**Issue: Pool Not Working**
- **Solution:** Ensure `IPoolable` is implemented on pooled objects
- Verify factory is registered correctly

**Issue: Events Not Firing**
- **Solution:** Check ScriptableObject event channels are assigned
- Verify subscribers are registered before events fire

---

## Migration from Old Architecture

If migrating from the old architecture:

1. **Replace Old Knife.cs:**
   - Old `Knife.cs` (620+ lines) → New `Knife.cs` (250 lines) + Components
   - Update prefab to include all component references

2. **Replace Old KnifeThrower.cs:**
   - Old `KnifeThrower.cs` (480+ lines) → New `KnifeThrower.cs` (200 lines) + Components
   - Update prefab to include all component references

3. **Update Audio:**
   - Old `AudioPlayer` → New `PooledAudioService`
   - Update registrations in installers

4. **Update Events:**
   - Migrate from `IGameEventBus` to ScriptableObject channels
   - Create event channel assets
   - Update subscriptions

5. **Remove Redundant Files:**
   - Delete `KnifeFacade.cs` (replaced by component system)
   - Delete `KnifeMover.cs` (replaced by `KnifeMovement.cs`)
   - Update any references

---

## Performance Tips

1. **Pool Warmup:**
   - Warm up pools during loading screens
   - Use `WarmupAsync()` for non-blocking initialization

2. **Component Caching:**
   - Always cache `GetComponent` results in inspector or Awake
   - Never call `GetComponent` in Update loops

3. **Event Channels:**
   - Use ScriptableObject channels instead of class-based events
   - Channels survive domain reload in editor

4. **Camera Access:**
   - Use `RuntimeCacheService` instead of `Camera.main`
   - Cache camera reference once, reuse

5. **Object Finding:**
   - Never use `FindObjectOfType`, `GameObject.Find`, etc.
   - Use dependency injection instead

---

## Next Steps

After setup is complete:

1. Test gameplay flow
2. Verify pooling is working (check profiler for GC allocations)
3. Test event system (verify events fire correctly)
4. Profile performance and adjust pool sizes
5. Add additional features using the new architecture

---

## Support

For issues or questions:
- Check `Assets/_Docs/architecture-refactor-plan.md` for architecture details
- Review component documentation in code comments
- Check Unity console for error messages

---

*Last Updated: December 2024*

