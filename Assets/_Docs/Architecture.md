# KNIFE TOSS — Architecture Document

**Version 1.0**  
**Last Updated: December 2024**  
**Engine: Unity 2022+ (LTS)**

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Design Patterns](#2-design-patterns)
3. [Folder Structure](#3-folder-structure)
4. [Assembly Definitions](#4-assembly-definitions)
5. [Dependency Flow](#5-dependency-flow)
6. [Best Practices](#6-best-practices)
7. [What to Avoid](#7-what-to-avoid)
8. [Naming Conventions](#8-naming-conventions)

---

## 1. Architecture Overview

### 1.1 Architectural Style

**Knife Toss** follows a **Layered Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                       │
│  MonoBehaviours, UI Views, VFX, Input Handlers, Animations  │
├─────────────────────────────────────────────────────────────┤
│                     APPLICATION LAYER                        │
│  Services, Coordinators, Use Cases, Session Management       │
├─────────────────────────────────────────────────────────────┤
│                        CORE LAYER                            │
│  Domain Models, Interfaces, Events, Pure C# Logic            │
├─────────────────────────────────────────────────────────────┤
│                   INFRASTRUCTURE LAYER                       │
│  Persistence, Audio Systems, Spawners, External APIs         │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Key Principles

| Principle | Description |
|-----------|-------------|
| **Separation of Concerns** | Each layer has a single responsibility |
| **Dependency Inversion** | High-level modules don't depend on low-level modules |
| **Interface Segregation** | Small, focused interfaces over large ones |
| **Data-Driven Design** | Game behavior controlled by ScriptableObjects, not hardcoded values |
| **Event-Driven Communication** | Loose coupling through events and channels |

### 1.3 Technology Stack

| Technology | Purpose |
|------------|---------|
| **VContainer** | Dependency Injection |
| **UniTask** | Async/await operations |
| **DOTween** | Tweening and animations |
| **Unity Input System** | Cross-platform input handling |
| **TextMesh Pro** | Text rendering |
| **ScriptableObjects** | Configuration and event channels |

---

## 2. Design Patterns

### 2.1 Creational Patterns

#### Object Pool Pattern
**Purpose:** Efficient reuse of frequently instantiated objects (knives, particles)

**Where to use:**
- Knife spawning and recycling
- Particle effect instantiation
- UI element pooling (magazine icons, progress indicators)

**Key Components:**
- Pool Manager (generic)
- Poolable interface
- Pre-warming on scene load

---

#### Factory Pattern
**Purpose:** Centralized object creation with proper dependency injection

**Where to use:**
- Target creation with rotation behaviors
- Hitable object spawning
- UI popup instantiation

---

### 2.2 Structural Patterns

#### Service Locator (via VContainer)
**Purpose:** Provide global access to services without tight coupling

**Where to use:**
- Audio services
- Player data services
- Level management services
- Settings services

**Implementation:** VContainer's `LifetimeScope` handles registration and resolution

---

#### Composite Pattern
**Purpose:** Treat groups of objects uniformly

**Where to use:**
- Target groups (multiple targets behaving as one level objective)
- UI element hierarchies
- Animation sequences

---

### 2.3 Behavioral Patterns

#### Strategy Pattern
**Purpose:** Interchangeable algorithms at runtime

**Where to use:**
- **Target Rotation Behaviors:**
  - Constant Rotation Strategy
  - Pulsating Rotation Strategy
  - Flipping Rotation Strategy
  - Acceleration Curve Strategy

**Structure:**
```
IRotationBehavior (interface)
    ├── ConstantRotation
    ├── PulsatingRotation
    ├── FlippingRotation
    └── AccelerationCurveRotation
```

---

#### Observer Pattern (Event Channels)
**Purpose:** Decoupled communication between systems

**Where to use:**
- Knife impact events → Score system, VFX, Audio
- Level commands → Flow coordinator
- Session state changes → UI updates
- Button clicks → Audio feedback

**Implementation:** ScriptableObject-based event channels for cross-system communication

---

#### State Pattern
**Purpose:** Manage complex state transitions cleanly

**Where to use:**
- **Level State Machine:**
  - Loading → Playing → Paused → Completed/Failed → Transitioning

- **Knife State Machine:**
  - Loaded → InFlight → Stuck/Bounced → Pooled

**Benefits:**
- Clear state transitions
- Easy debugging and logging
- Prevents invalid state combinations

---

#### Command Pattern
**Purpose:** Encapsulate requests as objects

**Where to use:**
- Level commands (Retry, Next, Reset to Stage Start)
- Player actions (for potential replay system)
- Undo operations (future feature)

---

### 2.4 Unity-Specific Patterns

#### MonoBehaviour Wrapper Pattern
**Purpose:** Separate Unity lifecycle from business logic

**Structure:**
```
Pure C# Controller (logic)
    ↑
MonoBehaviour View (Unity integration)
```

**Example:**
- `KnifeController` (pure C#) handles movement logic, state
- `KnifeView` (MonoBehaviour) handles Unity physics bridge, visuals

---

#### ScriptableObject Configuration Pattern
**Purpose:** Designer-friendly, version-control-friendly configuration

**Where to use:**
- Stage definitions
- Animation parameters
- Audio clip references
- Event channels
- Difficulty settings

---

## 3. Folder Structure

```
Assets/
│
├── _Docs/                          # Documentation
│   ├── GameDesignDocument.md
│   └── Architecture.md
│
├── Art/                            # All visual assets
│   ├── Backgrounds/
│   ├── Gameplay/                   # Knives, targets, tomatoes
│   ├── Icons/
│   ├── Stages/                     # Per-stage themed assets
│   └── UI/
│
├── Audio/                          # All audio assets
│   ├── Music/
│   ├── SFX/
│   └── ThirdParty/                 # Licensed audio packs
│
├── Configurations/                 # ScriptableObject assets
│   ├── Animations/                 # Tween configs
│   ├── Audio/                      # Sound settings
│   ├── Events/                     # Event channels
│   ├── Gameplay/                   # Stages, levels, hitables
│   ├── StagesLayout/               # Visual themes per stage
│   └── UI/                         # UI configuration
│
├── Materials/                      # Shared materials
│
├── Prefabs/                        # All prefabs
│   ├── Gameplay/                   # Knife, Target, Tomato
│   ├── Particles/                  # VFX prefabs
│   └── UI/                         # UI prefabs
│
├── Resources/                      # Unity Resources (minimal use)
│
├── Scenes/
│   └── MainGame.unity
│
├── Scripts/                        # All source code
│   ├── App/                        # Application layer
│   │   ├── Installers/             # VContainer installers
│   │   └── Services/               # Application services
│   │
│   ├── Core/                       # Core layer (no Unity dependencies)
│   │   ├── Abstractions/           # Interfaces and contracts
│   │   ├── Data/                   # Data structures, configs
│   │   ├── Domain/                 # Business logic, session, rules
│   │   └── Events/                 # Event channel definitions
│   │
│   ├── Infrastructure/             # Infrastructure layer
│   │   ├── Audio/                  # Audio player implementations
│   │   ├── Input/                  # Input system wrappers
│   │   ├── Leveling/               # Level spawning, setup
│   │   ├── Persistence/            # Save/load systems
│   │   └── Pooling/                # Object pool implementations
│   │
│   ├── Presentation/               # Presentation layer
│   │   ├── Gameplay/               # Game MonoBehaviours
│   │   │   ├── Knife/
│   │   │   ├── Target/
│   │   │   └── Hitables/
│   │   ├── UI/                     # UI components
│   │   │   ├── HUD/
│   │   │   ├── Popups/
│   │   │   └── Common/
│   │   └── VFX/                    # Visual effects handlers
│   │
│   └── Editor/                     # Editor-only scripts
│       ├── Tools/
│       └── Inspectors/
│
└── Settings/                       # Unity project settings
    └── Build Profiles/
```

### 3.1 Folder Rules

| Folder | Rule |
|--------|------|
| `Core/` | **NO Unity dependencies** — pure C# only |
| `App/` | Orchestration and service registration |
| `Infrastructure/` | External system integrations |
| `Presentation/` | All MonoBehaviours live here |
| `Configurations/` | Only ScriptableObject assets |
| `Editor/` | Editor scripts with `#if UNITY_EDITOR` or in Editor folder |

---

## 4. Assembly Definitions

### 4.1 Assembly Structure

```
KnifeToss.Core.asmdef
    ↑ (referenced by)
KnifeToss.Infrastructure.asmdef
    ↑ (referenced by)
KnifeToss.App.asmdef
    ↑ (referenced by)
KnifeToss.Presentation.asmdef

KnifeToss.Editor.asmdef (Editor only)
```

### 4.2 Assembly Dependencies

| Assembly | References | Purpose |
|----------|------------|---------|
| `KnifeToss.Core` | None (pure C#) | Domain models, interfaces |
| `KnifeToss.Infrastructure` | Core | Implementations |
| `KnifeToss.App` | Core, Infrastructure | Services, DI |
| `KnifeToss.Presentation` | Core, App | MonoBehaviours |
| `KnifeToss.Editor` | All above | Editor tools |

### 4.3 Benefits

- Faster compilation (only recompile changed assemblies)
- Enforced architectural boundaries
- Clear dependency visualization
- Prevents circular dependencies

---

## 5. Dependency Flow

### 5.1 Layer Dependencies

```
┌────────────────────┐
│    Presentation    │ ──────► Can reference: App, Core
└─────────┬──────────┘
          │
┌─────────▼──────────┐
│        App         │ ──────► Can reference: Infrastructure, Core
└─────────┬──────────┘
          │
┌─────────▼──────────┐
│   Infrastructure   │ ──────► Can reference: Core only
└─────────┬──────────┘
          │
┌─────────▼──────────┐
│       Core         │ ──────► References: Nothing (pure C#)
└────────────────────┘
```

### 5.2 Dependency Injection Flow

```
GameplayInstaller (LifetimeScope)
    │
    ├── Registers Interfaces → Implementations
    │   ├── IAudioPlayer → AudioPlayer
    │   ├── IPlayerDataStore → FilePlayerDataStore
    │   ├── ILevelCatalog → StagesConfigCatalog
    │   └── ...
    │
    └── Injects into MonoBehaviours and Services
```

### 5.3 Event Flow

```
Gameplay Event (e.g., Knife Stuck)
    │
    ▼
ScriptableObject Event Channel
    │
    ├──► Score Service (updates session)
    ├──► Audio Player (plays sound)
    ├──► VFX Spawner (creates particles)
    └──► UI Updater (refreshes display)
```

---

## 6. Best Practices

### 6.1 Code Organization

| Practice | Description |
|----------|-------------|
| **One class per file** | File name matches class name exactly |
| **Namespace matches folder** | `Scripts/Core/Domain` → `namespace Core.Domain` |
| **Interfaces in Abstractions** | All interfaces live in `Core/Abstractions/` |
| **Keep MonoBehaviours thin** | Delegate logic to pure C# classes |

### 6.2 Dependency Injection

| Practice | Description |
|----------|-------------|
| **Constructor injection preferred** | Use `[Inject]` attribute sparingly |
| **Interface-based dependencies** | Depend on `IAudioPlayer`, not `AudioPlayer` |
| **Scope appropriately** | Singleton for services, Transient for views |
| **Register in Installers only** | Keep DI configuration centralized |

### 6.3 ScriptableObjects

| Practice | Description |
|----------|-------------|
| **No magic numbers** | All tuning values in ScriptableObjects |
| **One concern per SO** | Don't mix audio and animation configs |
| **Use [CreateAssetMenu]** | Easy asset creation for designers |
| **Validate on load** | Check for null references, valid ranges |

### 6.4 Events

| Practice | Description |
|----------|-------------|
| **ScriptableObject channels** | For cross-system communication |
| **C# events** | For same-object or parent-child communication |
| **Unsubscribe in OnDisable** | Prevent memory leaks and null refs |
| **Null-check before raise** | Always use `?.Invoke()` pattern |

### 6.5 Async Operations

| Practice | Description |
|----------|-------------|
| **Use UniTask** | Not coroutines for async logic |
| **CancellationToken** | Pass tokens for proper cancellation |
| **Avoid async void** | Use `async UniTaskVoid` or `async UniTask` |
| **Handle exceptions** | Wrap in try-catch, log appropriately |

### 6.6 Performance

| Practice | Description |
|----------|-------------|
| **Pool frequently spawned objects** | Knives, particles, UI elements |
| **Cache component references** | Don't call `GetComponent` in Update |
| **Avoid allocations in loops** | No `new` in hot paths |
| **Kill tweens on cleanup** | Prevent orphaned animations |

### 6.7 Testing

| Practice | Description |
|----------|-------------|
| **Pure C# for testability** | Core layer can be unit tested |
| **Mock interfaces** | Test services in isolation |
| **Editor tools for QA** | Cheats window, level skip, etc. |

---

## 7. What to Avoid

### 7.1 Architecture Anti-Patterns

| Anti-Pattern | Why to Avoid | Alternative |
|--------------|--------------|-------------|
| **God Classes** | Unmaintainable, untestable | Split by responsibility |
| **Singletons (manual)** | Hidden dependencies, hard to test | Use DI container |
| **Static state** | Global mutable state causes bugs | Inject services |
| **Circular dependencies** | Compilation issues, tight coupling | Use events or interfaces |

### 7.2 Unity-Specific Anti-Patterns

| Anti-Pattern | Why to Avoid | Alternative |
|--------------|--------------|-------------|
| **Find/FindObjectOfType** | Slow, fragile, hidden dependencies | Inject references |
| **SendMessage** | Slow, no compile-time safety | Direct calls or events |
| **Resources.Load everywhere** | No control over loading | Addressables or injection |
| **Logic in Update** | Runs every frame even when unnecessary | Event-driven updates |

### 7.3 Code Smells to Avoid

| Smell | Why to Avoid | Alternative |
|-------|--------------|-------------|
| **Magic numbers** | Hard to tune, unclear intent | Named constants or configs |
| **Deep nesting** | Hard to read and maintain | Early returns, extract methods |
| **Long methods (50+ lines)** | Hard to understand | Extract smaller methods |
| **Comments explaining "what"** | Code should be self-documenting | Better naming |
| **Commented-out code** | Clutters codebase | Delete it, use version control |

### 7.4 Performance Anti-Patterns

| Anti-Pattern | Why to Avoid | Alternative |
|--------------|--------------|-------------|
| **Instantiate/Destroy frequently** | GC spikes, frame drops | Object pooling |
| **String concatenation in loops** | Allocations | StringBuilder or interpolation |
| **LINQ in hot paths** | Hidden allocations | For loops |
| **Boxing value types** | Unnecessary allocations | Generic methods |

### 7.5 Organizational Anti-Patterns

| Anti-Pattern | Why to Avoid | Alternative |
|--------------|--------------|-------------|
| **One giant Scripts folder** | Hard to navigate | Feature-based folders |
| **Mixing Editor and Runtime** | Build errors, confusion | Separate Editor folder |
| **Hard-coded file paths** | Breaks on reorganization | Use asset references |
| **Duplicate code** | Maintenance nightmare | Extract shared utilities |

---

## 8. Naming Conventions

### 8.1 Files and Folders

| Type | Convention | Example |
|------|------------|---------|
| Folders | PascalCase | `Gameplay`, `UI`, `Abstractions` |
| C# Files | PascalCase, match class name | `KnifeController.cs` |
| Interfaces | I + PascalCase | `IRotationBehavior.cs` |
| ScriptableObjects | PascalCase + Config/Settings/Channel | `SoundSettings.asset` |

### 8.2 Code Elements

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `KnifeController` |
| Interfaces | I + PascalCase | `IAudioPlayer` |
| Methods | PascalCase | `ThrowKnife()` |
| Properties | PascalCase | `KnivesRemaining` |
| Private fields | _camelCase | `_currentKnife` |
| Parameters | camelCase | `knifeCount` |
| Constants | UPPER_SNAKE or PascalCase | `MAX_KNIVES` or `MaxKnives` |
| Events | PascalCase + Event suffix | `KnifeStuckEvent` |

### 8.3 Unity-Specific

| Type | Convention | Example |
|------|------------|---------|
| MonoBehaviours | PascalCase + View/Component | `KnifeView`, `TargetComponent` |
| ScriptableObjects | PascalCase + Config/Settings | `StageLayoutSettings` |
| Prefabs | PascalCase | `Knife.prefab` |
| Scenes | PascalCase | `MainGame.unity` |
| Tags | PascalCase | `"Knife"`, `"Target"` |
| Layers | PascalCase | `Gameplay`, `UI` |

---

## Appendix A: Quick Reference Card

### Layer Responsibilities

| Layer | Contains | Depends On |
|-------|----------|------------|
| **Presentation** | MonoBehaviours, Views, Input | App, Core |
| **App** | Services, Coordinators, Installers | Infrastructure, Core |
| **Infrastructure** | Persistence, Audio, Spawners | Core |
| **Core** | Interfaces, Domain, Events | Nothing |

### When to Use What

| Need | Solution |
|------|----------|
| Cross-system communication | ScriptableObject Event Channel |
| Object creation | Factory + Object Pool |
| Configurable behavior | ScriptableObject Configuration |
| Interchangeable algorithms | Strategy Pattern |
| Complex state management | State Machine |
| Service access | VContainer DI |
| Async operations | UniTask |
| Animations/Tweens | DOTween |

---

**END OF DOCUMENT**

*This Architecture Document serves as the technical blueprint for the Knife Toss project. All team members should follow these guidelines to maintain consistency and code quality.*

