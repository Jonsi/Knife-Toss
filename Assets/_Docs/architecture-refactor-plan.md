# Knife Toss - Architecture Refactor Plan

## Executive Summary

This document outlines a comprehensive refactoring plan to transform the Knife Toss codebase into a highly modular, scalable, and maintainable architecture. The refactor focuses on SOLID principles, DRY code, dependency injection, pooling, and deterministic flow.

---

## Part 1: Current State Analysis

### 1.1 Critical Anti-Patterns Identified

#### ❌ Inefficient Unity API Calls

| File | Issue | Line(s) |
|------|-------|---------|
| `KnifeCollisionHandler.cs` | `FindObjectOfType<ProjectLifetimeScope>()` | 20 |
| `AudioPlayer.cs` | `GameObject.Find("AudioPlayer (2D)")` | 84 |
| `KnivesHierarchyManager.cs` | `GameObject.Find(parentName)` | 52 |
| `KnivesHierarchyManager.cs` | `FindObjectsByType<Knife>()` | 66 |
| `Knife.cs` | `Camera.main` accessed multiple times | 54, 512 |
| `KnifeThrower.cs` | `Camera.main` in Update loop | 201 |
| `Knife.cs` | `GetComponent<Rigidbody2D>()` at runtime | 110 |
| `KnifeMover.cs` | `GetComponent<Rigidbody2D>()` at runtime | 22 |
| `KnifePool.cs` | `GetComponent<KnifeView>()` per operation | 151, 207 |
| `TargetGroupManager.cs` | `GetComponent<RotatingTarget>()` per target | 137 |

#### ❌ No Pooling for Frequently Spawned Objects

| Object Type | Current Approach | Impact |
|-------------|-----------------|--------|
| Particles (splinters) | `Instantiate` + `Destroy` | GC spikes |
| Particles (tomato) | `Instantiate` + `Destroy` | GC spikes |
| Hitable Objects | `Instantiate` + `Destroy` | GC spikes |
| Pre-stuck Knives | `Instantiate` only | Memory leak potential |
| Audio (one-shot) | Single shared source | No pooling |

#### ❌ Magic Strings

```csharp
// Found throughout codebase
collisionObject.CompareTag("Knife")  // Knife.cs:178
collisionObject.CompareTag("Target") // Knife.cs:199
instance.name = "KnifeThrowerInstance" // GameplayObjectProvider.cs:60
```

### 1.2 SOLID Violations

#### Single Responsibility Principle (SRP) Violations

**`Knife.cs` (620+ lines) - Does Too Much:**
- Movement control
- Collision detection & handling
- Sticking logic
- Animation (bounce-off)
- Sound playback
- Pool return management
- Camera visibility checking

**`KnifeThrower.cs` (480+ lines) - Does Too Much:**
- Input handling relay
- Knife instantiation
- Pool management
- Magazine queue management
- Aiming calculations
- Animation coordination

**`LevelSessionService.cs` (295+ lines) - Does Too Much:**
- Level loading
- Session state management
- Event subscription
- Thrower configuration
- Target configuration
- Progress persistence

#### Open/Closed Principle (OCP) Violations

- `RotationBehaviourFactory` uses switch statement - adding new rotation types requires modification
- `ObjectiveEvaluator` switch on `ObjectiveType` - not extensible

#### Dependency Inversion Principle (DIP) Violations

- `AudioPlayer.CreateOrGetSource()` creates concrete `GameObject` and `AudioSource`
- `HitableObjectSpawner` directly calls `Object.Instantiate`
- `PreStuckKnifeSpawner` directly calls `Object.Instantiate`
- `KnifeImpactParticlesSpawner` directly calls `Instantiate`

### 1.3 DRY Violations

#### Duplicated Code Patterns

**Impact Point Resolution (duplicated 4+ times):**
```csharp
// In Knife.cs, HitableObjectView.cs - identical code
private Vector3 ResolveImpactPoint(Collision2D collision)
{
    if (collision.contactCount > 0)
        return collision.GetContact(0).point;
    return ResolveImpactPoint(collision.collider);
}
```

**Collider Enable/Disable (duplicated 5+ times):**
```csharp
// In Knife.cs, KnifeView.cs, HitableObjectView.cs, KnifePool.cs
for (int i = 0; i < colliders.Length; i++)
{
    collider.enabled = enabled;
}
```

**Particle Spawn & Destroy (duplicated 3+ times):**
```csharp
// In HitableObjectView.cs, KnifeImpactParticlesSpawner.cs
ParticleSystem instance = Instantiate(prefab, position, rotation, parent);
Destroy(instance.gameObject, main.duration + life);
```

### 1.4 Inconsistent Architecture Patterns

| Pattern | Usage | Issue |
|---------|-------|-------|
| Event System | `ScriptableObject` channels + `IGameEventBus` | Two parallel systems |
| DI | VContainer `[Inject]` + manual wiring + `FindObjectOfType` | Inconsistent |
| Pooling | Custom `KnifePool` + `GenericMonoPool<T>` | Not unified |
| Spawning | Direct `Instantiate` everywhere | No factory abstraction |

---

## Part 2: Target Architecture

### 2.1 Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│  Views, Presenters, Input Handlers, Visual Components        │
├─────────────────────────────────────────────────────────────┤
│                    APPLICATION LAYER                         │
│  Use Cases, Coordinators, Services, Event Bus                │
├─────────────────────────────────────────────────────────────┤
│                      DOMAIN LAYER                            │
│  Entities, Value Objects, Domain Events, Rules               │
├─────────────────────────────────────────────────────────────┤
│                   INFRASTRUCTURE LAYER                       │
│  Pools, Factories, Persistence, Audio, Input Adapters        │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Core Principles

1. **Every spawnable object uses a pool** - No exceptions
2. **All dependencies injected via VContainer** - No `FindObject*` ever
3. **Unified event system via ScriptableObject channels** - Type-safe, inspectable
4. **Small, focused classes** - Single responsibility, <200 lines ideally
5. **Deterministic initialization** - Async with UniTask, clear ordering
6. **Code reads like a story** - Self-documenting method names

---

## Part 3: Refactoring Tasks

### Phase 1: Foundation Infrastructure

#### 1.1 Create Universal Pool System

**New Files:**
```
Scripts/Infrastructure/Pooling/
├── IPoolable.cs
├── IPool.cs (update existing)
├── Pool<T>.cs (generic, non-MonoBehaviour)
├── ComponentPool<T>.cs (for Components)
├── ParticlePool.cs
├── AudioSourcePool.cs
└── PoolRegistry.cs (central pool management)
```

**`IPoolable.cs`:**
```csharp
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}
```

**`Pool<T>.cs`:**
```csharp
public class Pool<T> : IPool<T> where T : class, IPoolable, new()
{
    private readonly Stack<T> _available = new();
    private readonly Func<T> _factory;
    private readonly Action<T> _onGet;
    private readonly Action<T> _onRelease;

    public T Get() { ... }
    public void Release(T item) { ... }
    public void Warmup(int count) { ... }
}
```

#### 1.2 Create Pooled Audio System

**Replace `AudioPlayer.cs` with:**
```
Scripts/Infrastructure/Audio/
├── IAudioService.cs
├── PooledAudioService.cs
├── AudioSourcePool.cs
└── AudioHandle.cs (for tracking playing sounds)
```

**`PooledAudioService.cs`:**
```csharp
public sealed class PooledAudioService : IAudioService, IDisposable
{
    private readonly AudioSourcePool _pool;
    private readonly Transform _poolRoot;

    public UniTask<AudioHandle> Play2DAsync(AudioClip clip, float volume = 1f);
    public UniTask<AudioHandle> PlayAtAsync(AudioClip clip, Vector3 position, float volume = 1f);
    public void StopAll();
}
```

#### 1.3 Create Particle Pool System

**New Files:**
```
Scripts/Infrastructure/VFX/
├── IVFXService.cs
├── PooledVFXService.cs
├── ParticlePoolEntry.cs
└── VFXHandle.cs
```

**`IVFXService.cs`:**
```csharp
public interface IVFXService
{
    UniTask<VFXHandle> SpawnAsync(ParticleSystem prefab, Vector3 position, Quaternion rotation);
    void Release(VFXHandle handle);
}
```

---

### Phase 2: Break Down God Classes

#### 2.1 Decompose `Knife.cs` into Components

**Current:** 1 class, 620+ lines
**Target:** 6+ focused classes

```
Scripts/Presentation/Gameplay/Projectiles/
├── Knife.cs (facade, ~50 lines)
├── KnifeMovement.cs (~80 lines)
├── KnifeCollision.cs (~100 lines)
├── KnifeSticking.cs (~80 lines)
├── KnifeBounceAnimation.cs (~60 lines)
├── KnifeAudio.cs (~40 lines)
└── KnifeVisibility.cs (~40 lines)
```

**`Knife.cs` (Facade):**
```csharp
public sealed class Knife : MonoBehaviour, IPoolable
{
    [SerializeField] private KnifeMovement _movement;
    [SerializeField] private KnifeCollision _collision;
    [SerializeField] private KnifeSticking _sticking;
    [SerializeField] private KnifeBounceAnimation _bounceAnimation;
    [SerializeField] private KnifeAudio _audio;
    [SerializeField] private KnifeVisibility _visibility;

    public void Initialize(Vector2 direction, float speed)
    {
        _movement.Launch(direction, speed);
        _collision.Enable();
    }

    public void OnSpawn() => ResetState();
    public void OnDespawn() => CleanupTweens();
}
```

**`KnifeMovement.cs`:**
```csharp
public sealed class KnifeMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody; // Cached in inspector!

    private Vector2 _direction;
    private float _speed;
    private bool _isActive;

    public void Launch(Vector2 direction, float speed)
    {
        _direction = direction.normalized;
        _speed = speed;
        _isActive = true;
        _rigidbody.simulated = true;
    }

    public void Stop()
    {
        _isActive = false;
        _rigidbody.velocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!_isActive) return;
        _rigidbody.velocity = _direction * _speed;
    }
}
```

**`KnifeCollision.cs`:**
```csharp
public sealed class KnifeCollision : MonoBehaviour
{
    public event Action<CollisionData> OnCollided;

    [SerializeField] private Collider2D[] _colliders; // Cached!

    public void Enable() => SetCollidersEnabled(true);
    public void Disable() => SetCollidersEnabled(false);

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnCollided?.Invoke(CollisionData.FromTrigger(other, transform));
    }
}

public readonly struct CollisionData
{
    public Vector3 ImpactPoint { get; }
    public GameObject Other { get; }
    public CollisionType Type { get; }
}
```

#### 2.2 Decompose `KnifeThrower.cs`

**Current:** 1 class, 480+ lines
**Target:** 5+ focused classes

```
Scripts/Presentation/Gameplay/Throwing/
├── KnifeThrower.cs (facade, ~80 lines)
├── ThrowingInput.cs (~60 lines)
├── AimCalculator.cs (~50 lines)
├── KnifeLauncher.cs (~60 lines)
└── Magazine/
    ├── KnifeMagazine.cs (~100 lines) - existing, simplified
    ├── MagazineAnimator.cs (~80 lines)
    └── MagazineConfig.cs (ScriptableObject)
```

**`KnifeThrower.cs` (Simplified Facade):**
```csharp
public sealed class KnifeThrower : MonoBehaviour
{
    [SerializeField] private ThrowingInput _input;
    [SerializeField] private AimCalculator _aim;
    [SerializeField] private KnifeLauncher _launcher;
    [SerializeField] private KnifeMagazine _magazine;

    public event Action KnifeThrown;

    private void OnEnable()
    {
        _input.ThrowRequested += HandleThrowRequested;
        _input.AimUpdated += _aim.UpdateAim;
    }

    private void HandleThrowRequested(Vector2 screenPosition)
    {
        if (!_magazine.HasKnives) return;

        var direction = _aim.CalculateDirection(screenPosition);
        var knife = _magazine.TakeNext();
        _launcher.Launch(knife, direction);
        KnifeThrown?.Invoke();
    }
}
```

#### 2.3 Decompose `LevelSessionService.cs`

**Current:** 1 class, 295+ lines
**Target:** 4+ focused classes

```
Scripts/Application/Level/
├── LevelSessionService.cs (~80 lines) - orchestrator only
├── LevelLoader.cs (~60 lines)
├── LevelStateEvaluator.cs (~50 lines)
├── LevelProgressTracker.cs (~50 lines)
└── LevelEventCoordinator.cs (~60 lines)
```

---

### Phase 3: Event System Unification

#### 3.1 Expand ScriptableObject Event Channels

**New Event Channels:**
```
Assets/Configurations/Events/
├── Core/
│   ├── VoidEventChannel.cs (base class)
│   ├── IntEventChannel.cs
│   ├── Vector3EventChannel.cs
│   └── GenericEventChannel<T>.cs
├── Gameplay/
│   ├── KnifeEvents/
│   │   ├── KnifeSpawnedChannel.asset
│   │   ├── KnifeThrownChannel.asset
│   │   ├── KnifeStuckChannel.asset
│   │   ├── KnifeBouncedChannel.asset
│   │   └── KnifeReturnedToPoolChannel.asset
│   ├── TargetEvents/
│   │   ├── TargetHitChannel.asset
│   │   └── TargetDestroyedChannel.asset
│   ├── HitableEvents/
│   │   ├── HitableSpawnedChannel.asset
│   │   ├── HitableHitChannel.asset
│   │   └── HitableDestroyedChannel.asset
│   └── ScoreEvents/
│       ├── ScoreChangedChannel.asset
│       └── ComboChangedChannel.asset
├── Level/
│   ├── LevelStartedChannel.asset
│   ├── LevelCompletedChannel.asset
│   ├── LevelFailedChannel.asset
│   └── LevelTransitionChannel.asset
└── UI/
    ├── ButtonClickChannel.asset (existing)
    └── PopupEvents/
        ├── PopupOpenedChannel.asset
        └── PopupClosedChannel.asset
```

#### 3.2 Generic Event Channel Base

**`GenericEventChannel<T>.cs`:**
```csharp
public abstract class GenericEventChannel<T> : ScriptableObject
{
    private readonly List<Action<T>> _listeners = new();

    public void Raise(T value)
    {
        for (int i = _listeners.Count - 1; i >= 0; i--)
            _listeners[i]?.Invoke(value);
    }

    public void Subscribe(Action<T> listener)
    {
        if (!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public void Unsubscribe(Action<T> listener)
    {
        _listeners.Remove(listener);
    }
}
```

#### 3.3 Deprecate `IGameEventBus`

The class-based `GameEventBus` will be deprecated in favor of ScriptableObject channels because:
- Channels are inspector-visible and debuggable
- Channels survive domain reload in editor
- Channels are explicitly typed and discoverable
- Channels can be injected via VContainer

---

### Phase 4: Factory System

#### 4.1 Create Factory Interfaces

```
Scripts/Infrastructure/Factories/
├── IFactory<T>.cs
├── IAsyncFactory<T>.cs
├── Gameplay/
│   ├── IKnifeFactory.cs
│   ├── ITargetFactory.cs
│   ├── IHitableFactory.cs
│   └── IParticleFactory.cs
└── Implementations/
    ├── PooledKnifeFactory.cs
    ├── PooledTargetFactory.cs
    ├── PooledHitableFactory.cs
    └── PooledParticleFactory.cs
```

**`IFactory<T>.cs`:**
```csharp
public interface IFactory<T>
{
    T Create();
    void Recycle(T instance);
}

public interface IFactory<TConfig, TResult>
{
    TResult Create(TConfig config);
    void Recycle(TResult instance);
}
```

**`PooledKnifeFactory.cs`:**
```csharp
public sealed class PooledKnifeFactory : IKnifeFactory
{
    private readonly IPool<Knife> _pool;
    private readonly IObjectResolver _resolver;

    public PooledKnifeFactory(GameObject prefab, Transform parent, IObjectResolver resolver)
    {
        _resolver = resolver;
        _pool = new ComponentPool<Knife>(
            () => CreateKnife(prefab, parent),
            warmupCount: 15
        );
    }

    public Knife Create()
    {
        var knife = _pool.Get();
        knife.OnSpawn();
        return knife;
    }

    public void Recycle(Knife knife)
    {
        knife.OnDespawn();
        _pool.Release(knife);
    }

    private Knife CreateKnife(GameObject prefab, Transform parent)
    {
        var instance = Object.Instantiate(prefab, parent);
        _resolver.InjectGameObject(instance);
        instance.SetActive(false);
        return instance.GetComponent<Knife>();
    }
}
```

---

### Phase 5: Constants & Configuration

#### 5.1 Replace Magic Strings with Constants

**`Tags.cs`:**
```csharp
namespace KnifeToss.Core.Constants
{
    public static class Tags
    {
        public const string Knife = "Knife";
        public const string Target = "Target";
        public const string Hitable = "Hitable";
        public const string Player = "Player";
    }
}
```

**`Layers.cs`:**
```csharp
namespace KnifeToss.Core.Constants
{
    public static class Layers
    {
        public const int Default = 0;
        public const int Projectile = 8;
        public const int Target = 9;
        // etc.
    }
}
```

#### 5.2 Create Configuration ScriptableObjects

```
Assets/Configurations/
├── Gameplay/
│   ├── KnifeConfig.asset
│   ├── ThrowerConfig.asset
│   ├── MagazineConfig.asset
│   └── TargetConfig.asset
├── Audio/
│   ├── AudioPoolConfig.asset
│   └── SoundSettings.asset (existing)
├── VFX/
│   ├── ParticlePoolConfig.asset
│   └── ImpactEffectsConfig.asset
└── Pooling/
    └── PoolingConfig.asset
```

---

### Phase 6: Utility Extraction

#### 6.1 Create Shared Utilities

**`CollisionUtility.cs`:**
```csharp
namespace KnifeToss.Core.Utilities
{
    public static class CollisionUtility
    {
        public static Vector3 ResolveImpactPoint(Collision2D collision, Vector3 fallback)
        {
            if (collision == null) return fallback;
            if (collision.contactCount > 0)
                return collision.GetContact(0).point;
            return collision.collider?.ClosestPoint(fallback) ?? fallback;
        }

        public static Vector3 ResolveImpactPoint(Collider2D collider, Vector3 position)
        {
            return collider?.ClosestPoint(position) ?? position;
        }
    }
}
```

**`MathUtility.cs`:**
```csharp
namespace KnifeToss.Core.Utilities
{
    public static class MathUtility
    {
        public static Vector3 GetOrbitPosition(float radius, float angleDegrees)
        {
            float rad = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle < 0f) angle += 360f;
            while (angle >= 360f) angle -= 360f;
            return angle;
        }
    }
}
```

#### 6.2 Cache Service for Camera & Common References

**`RuntimeCacheService.cs`:**
```csharp
public sealed class RuntimeCacheService : IDisposable
{
    private Camera _mainCamera;
    private Transform _mainCameraTransform;

    public Camera MainCamera => _mainCamera ??= Camera.main;
    public Transform MainCameraTransform => _mainCameraTransform ??= MainCamera.transform;

    public void InvalidateCache()
    {
        _mainCamera = null;
        _mainCameraTransform = null;
    }

    public void Dispose() => InvalidateCache();
}
```

---

### Phase 7: Async Flow with UniTask

#### 7.1 Convert Initialization to Async

**Before:**
```csharp
public void Start()
{
    InitializeAsync().Forget();
}
```

**After:**
```csharp
public async UniTaskVoid Start()
{
    await InitializeAsync();
}

private async UniTask InitializeAsync()
{
    await _levelLoader.LoadLevelAsync(_startLevel);
    await _targetManager.SpawnTargetsAsync();
    await _thrower.InitializeAsync();
    _uiPresenter.ShowReady();
}
```

#### 7.2 Async Pool Warming

```csharp
public async UniTask WarmPoolsAsync(CancellationToken ct = default)
{
    var tasks = new List<UniTask>
    {
        _knifePool.WarmupAsync(15, ct),
        _particlePool.WarmupAsync(10, ct),
        _audioPool.WarmupAsync(8, ct),
    };
    await UniTask.WhenAll(tasks);
}
```

---

## Part 4: File-by-File Refactoring Checklist

### High Priority (Phase 1-2)

| File | Action | New Files Created |
|------|--------|-------------------|
| `Knife.cs` | Decompose into 6 components | `KnifeMovement.cs`, `KnifeCollision.cs`, `KnifeSticking.cs`, `KnifeBounceAnimation.cs`, `KnifeAudio.cs`, `KnifeVisibility.cs` |
| `KnifeThrower.cs` | Decompose into 4 components | `ThrowingInput.cs`, `AimCalculator.cs`, `KnifeLauncher.cs`, `MagazineAnimator.cs` |
| `AudioPlayer.cs` | Replace with pooled system | `PooledAudioService.cs`, `AudioSourcePool.cs` |
| `KnifeImpactParticlesSpawner.cs` | Use particle pool | Integrate with `PooledVFXService.cs` |
| `HitableObjectView.cs` | Use particle pool | Remove local spawn code |
| `KnivesHierarchyManager.cs` | Remove `FindObjectsByType` | Inject knife registry |
| `KnifeCollisionHandler.cs` | Remove `FindObjectOfType` | Inject via constructor |

### Medium Priority (Phase 3-4)

| File | Action | New Files Created |
|------|--------|-------------------|
| `LevelSessionService.cs` | Decompose into 4 services | `LevelLoader.cs`, `LevelStateEvaluator.cs`, `LevelProgressTracker.cs`, `LevelEventCoordinator.cs` |
| `GameEventBus.cs` | Deprecate, migrate to SO channels | Multiple `*EventChannel.cs` |
| `HitableObjectSpawner.cs` | Add pooling | Integrate with `PooledHitableFactory.cs` |
| `PreStuckKnifeSpawner.cs` | Add pooling | Integrate with `PooledKnifeFactory.cs` |
| `TargetGroupManager.cs` | Add factory pattern | `PooledTargetFactory.cs` |

### Lower Priority (Phase 5-7)

| File | Action |
|------|--------|
| All files | Replace magic strings with `Tags.cs`, `Layers.cs` |
| All files | Replace `Camera.main` with cached service |
| All files | Replace direct `GetComponent` with cached references |
| All files | Convert to async where beneficial |

---

## Part 5: New Folder Structure

```
Assets/Scripts/
├── Core/
│   ├── Abstractions/
│   │   ├── Services/
│   │   │   ├── IAudioService.cs
│   │   │   ├── IVFXService.cs
│   │   │   └── ICacheService.cs
│   │   ├── Factories/
│   │   │   ├── IFactory.cs
│   │   │   ├── IKnifeFactory.cs
│   │   │   ├── ITargetFactory.cs
│   │   │   └── IHitableFactory.cs
│   │   └── Pooling/
│   │       ├── IPool.cs
│   │       └── IPoolable.cs
│   ├── Constants/
│   │   ├── Tags.cs
│   │   ├── Layers.cs
│   │   └── AnimationHashes.cs
│   ├── Data/
│   │   └── (existing, reorganized)
│   ├── Domain/
│   │   └── (existing, reorganized)
│   ├── Events/
│   │   ├── Base/
│   │   │   ├── VoidEventChannel.cs
│   │   │   └── GenericEventChannel.cs
│   │   ├── Gameplay/
│   │   │   ├── KnifeEventChannel.cs
│   │   │   ├── TargetEventChannel.cs
│   │   │   └── HitableEventChannel.cs
│   │   └── Level/
│   │       └── LevelEventChannel.cs
│   └── Utilities/
│       ├── CollisionUtility.cs
│       ├── MathUtility.cs
│       └── TransformExtensions.cs
│
├── Application/
│   ├── Level/
│   │   ├── LevelSessionService.cs (simplified)
│   │   ├── LevelLoader.cs
│   │   ├── LevelStateEvaluator.cs
│   │   └── LevelProgressTracker.cs
│   └── Services/
│       └── (existing services)
│
├── Infrastructure/
│   ├── Audio/
│   │   ├── PooledAudioService.cs
│   │   └── AudioSourcePool.cs
│   ├── VFX/
│   │   ├── PooledVFXService.cs
│   │   └── ParticlePool.cs
│   ├── Pooling/
│   │   ├── Pool.cs
│   │   ├── ComponentPool.cs
│   │   └── PoolRegistry.cs
│   ├── Factories/
│   │   ├── PooledKnifeFactory.cs
│   │   ├── PooledTargetFactory.cs
│   │   └── PooledHitableFactory.cs
│   └── (existing folders)
│
├── Presentation/
│   ├── Gameplay/
│   │   ├── Projectiles/
│   │   │   ├── Knife.cs (facade)
│   │   │   ├── Components/
│   │   │   │   ├── KnifeMovement.cs
│   │   │   │   ├── KnifeCollision.cs
│   │   │   │   ├── KnifeSticking.cs
│   │   │   │   ├── KnifeBounceAnimation.cs
│   │   │   │   ├── KnifeAudio.cs
│   │   │   │   └── KnifeVisibility.cs
│   │   │   └── Data/
│   │   │       └── KnifeBounceOffAnimationConfig.cs
│   │   ├── Throwing/
│   │   │   ├── KnifeThrower.cs (facade)
│   │   │   ├── ThrowingInput.cs
│   │   │   ├── AimCalculator.cs
│   │   │   ├── KnifeLauncher.cs
│   │   │   └── Magazine/
│   │   │       ├── KnifeMagazine.cs
│   │   │       └── MagazineAnimator.cs
│   │   ├── Targets/
│   │   │   ├── RotatingTarget.cs
│   │   │   ├── TargetGroupManager.cs
│   │   │   └── Rotation/
│   │   │       └── (existing)
│   │   └── Hitables/
│   │       └── HitableObjectView.cs
│   └── UI/
│       └── (existing)
│
└── Installers/
    ├── ProjectLifetimeScope.cs
    ├── GameplayInstaller.cs
    └── PoolingInstaller.cs (new)
```

---

## Part 6: Implementation Order

### Sprint 1: Foundation (Week 1)
1. ✅ Create `IPoolable`, `IPool<T>`, `Pool<T>`, `ComponentPool<T>`
2. ✅ Create `PooledAudioService` and `AudioSourcePool`
3. ✅ Create `PooledVFXService` and `ParticlePool`
4. ✅ Create `Tags.cs` and `Layers.cs`
5. ✅ Create `RuntimeCacheService` for Camera caching

### Sprint 2: Knife Refactor (Week 2)
1. ✅ Decompose `Knife.cs` into 6 components
2. ✅ Create `PooledKnifeFactory`
3. ✅ Update `KnifePool` to use new factory
4. ✅ Remove all `GetComponent` calls from hot paths

### Sprint 3: Thrower Refactor (Week 3)
1. ✅ Decompose `KnifeThrower.cs` into components
2. ✅ Create `MagazineConfig` ScriptableObject
3. ✅ Simplify `KnifeMagazine`

### Sprint 4: Event System (Week 4)
1. ✅ Create `GenericEventChannel<T>` base
2. ✅ Create all new event channel assets
3. ✅ Migrate from `IGameEventBus` to SO channels
4. ✅ Deprecate `GameEventBus.cs`

### Sprint 5: Services & Spawners (Week 5)
1. ✅ Decompose `LevelSessionService`
2. ✅ Add pooling to `HitableObjectSpawner`
3. ✅ Add pooling to `PreStuckKnifeSpawner`
4. ✅ Create `PooledTargetFactory`

### Sprint 6: Cleanup & Polish (Week 6)
1. ✅ Remove all `FindObject*` calls
2. ✅ Remove all `GameObject.Find` calls
3. ✅ Convert remaining sync init to async
4. ✅ Final code review and documentation

---

## Part 7: Success Metrics

| Metric | Current | Target |
|--------|---------|--------|
| Largest class (lines) | 620+ (`Knife.cs`) | <200 |
| `FindObject*` calls | 4 | 0 |
| `GameObject.Find` calls | 2 | 0 |
| Uncached `GetComponent` in Update | 5+ | 0 |
| Pooled object types | 1 (knives) | 5+ (knives, particles, audio, hitables, targets) |
| Event channel types | 4 | 15+ |
| Average method length | 25 lines | <15 lines |
| Test coverage | 0% | 30%+ (domain layer) |

---

## Part 8: Risk Mitigation

### Breaking Changes
- Keep old `Knife.cs` as `LegacyKnife.cs` during migration
- Use feature flags to toggle between old/new systems
- Maintain backward compatibility with existing prefabs

### Performance Validation
- Profile before/after each sprint
- Monitor GC allocations per frame
- Ensure pool warmup doesn't cause startup lag

### Rollback Strategy
- Each sprint is a separate git branch
- Merge to main only after validation
- Keep prefab backups before component changes

---

## Appendix A: Code Style Guidelines

### Naming Conventions
```csharp
// Interfaces: I-prefix
public interface IKnifeFactory { }

// Event channels: descriptive + "Channel"
public class KnifeStuckEventChannel : GenericEventChannel<Vector3> { }

// Pools: Type + "Pool"
public class AudioSourcePool : IPool<AudioSource> { }

// Factories: "Pooled" + Type + "Factory"
public class PooledKnifeFactory : IKnifeFactory { }
```

### Method Naming
```csharp
// Commands: verb
void Launch();
void Stop();
void Reset();

// Queries: question or noun
bool CanThrow();
int GetRemainingKnives();
Knife GetActiveKnife();

// Event handlers: Handle + EventName
void HandleKnifeThrown();
void HandleLevelCompleted();

// Async: suffix with "Async"
UniTask InitializeAsync();
UniTask<Knife> SpawnKnifeAsync();
```

### Class Size Limits
- **MonoBehaviours:** Max 200 lines
- **Services:** Max 150 lines
- **Factories:** Max 100 lines
- **Data classes:** Max 50 lines
- **Utilities:** Max 100 lines

---

## Appendix B: Dependency Injection Registration

### GameplayInstaller (Updated)
```csharp
protected override void Configure(IContainerBuilder builder)
{
    // Pooling
    builder.Register<PoolRegistry>(Lifetime.Singleton);
    builder.Register<PooledAudioService>(Lifetime.Singleton).As<IAudioService>();
    builder.Register<PooledVFXService>(Lifetime.Singleton).As<IVFXService>();

    // Factories
    builder.Register<PooledKnifeFactory>(Lifetime.Singleton).As<IKnifeFactory>();
    builder.Register<PooledTargetFactory>(Lifetime.Singleton).As<ITargetFactory>();
    builder.Register<PooledHitableFactory>(Lifetime.Singleton).As<IHitableFactory>();

    // Services
    builder.Register<RuntimeCacheService>(Lifetime.Singleton);
    builder.Register<LevelLoader>(Lifetime.Singleton);
    builder.Register<LevelStateEvaluator>(Lifetime.Singleton);
    builder.Register<LevelProgressTracker>(Lifetime.Singleton);
    builder.Register<LevelSessionService>(Lifetime.Singleton);

    // Event Channels (ScriptableObject instances)
    builder.RegisterInstance(knifeStuckChannel);
    builder.RegisterInstance(knifeThrownChannel);
    builder.RegisterInstance(levelCompletedChannel);
    // ... etc
}
```

---

*Document Version: 1.0*
*Created: December 2024*
*Last Updated: December 2024*

