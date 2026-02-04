---
name: ""
overview: ""
todos: []
isProject: false
---

# PowerUp Type Categorization Plan

## Overview

Separate powerups into two distinct categories with different activation behaviors:

- **Projectile PowerUps**: Affect knife throwing, require thrower state switch
- **World PowerUps**: Affect game world/targets, no thrower state change

---

## Problem Analysis

Currently, all powerups flow through the same activation path in `PowerUpService.TryActivate()`. The `TimeDilator` has special handling that applies immediately, but there's no formal categorization system.

```mermaid
flowchart TD
    subgraph current [Current - Ad-hoc Handling]
        Activate[TryActivate] --> Check{Is TimeDilator?}
        Check --> |Yes| ApplyEffect[Apply Effect Immediately]
        Check --> |No| SetActive[Set as Active PowerUp]
        SetActive --> ReloadKnife[Reload Knife for PowerUp]
        ApplyEffect --> Consume[Consume from Inventory]
    end
```



**Issues:**

- Type checking with `is TimeDilatorConfig` is not extensible
- No formal contract for powerup behavior categories
- Adding new world effects requires modifying `PowerUpService`
- No cancellation support for projectile powerups

---

## Proposed Architecture

```mermaid
flowchart TD
    subgraph proposed [Proposed - Categorized PowerUps]
        Activate[TryActivate] --> GetType{PowerUp Category?}
        
        subgraph projectile [Projectile PowerUp Path]
            GetType --> |IProjectilePowerUp| DisableNormal[Disable Normal Thrower]
            DisableNormal --> EnablePowerUp[Enable PowerUp Thrower]
            EnablePowerUp --> SetActive[Set as Active]
            SetActive --> WaitThrow[Wait for Throw or Cancel]
        end
        
        subgraph world [World PowerUp Path]
            GetType --> |IWorldEffectPowerUp| ApplyWorld[Apply World Effect]
            ApplyWorld --> ConsumeImmediate[Consume Immediately]
            ConsumeImmediate --> Continue[Continue Normal Play]
        end
    end
```



---

## Type Hierarchy

```mermaid
classDiagram
    direction TB
    
    class PowerUpConfigBase {
        <<abstract>>
        +DisplayName string
        +Description string
        +Icon Sprite
        +InventoryKey string
        +Execute(context) bool
    }
    
    class IProjectilePowerUp {
        <<interface>>
        Note: Marker interface for projectile powerups
    }
    
    class IWorldEffectPowerUp {
        <<interface>>
        +ApplyEffect(context) void
        +Duration float
    }
    
    class ICancellablePowerUp {
        <<interface>>
        +OnCancelled() void
    }
    
    class SuperKnifeConfig {
        +ThrowSpeed float
    }
    
    class MultipleKnivesConfig {
        +KnifeCount int
        +KnifeSpacing float
        +ThrowSpeed float
    }
    
    class KnifeAimConfig {
        +ThrowSpeed float
    }
    
    class TimeDilatorConfig {
        +Duration float
        +SpeedMultiplier float
    }
    
    PowerUpConfigBase <|-- SuperKnifeConfig
    PowerUpConfigBase <|-- MultipleKnivesConfig
    PowerUpConfigBase <|-- KnifeAimConfig
    PowerUpConfigBase <|-- TimeDilatorConfig
    
    IProjectilePowerUp <|.. SuperKnifeConfig
    IProjectilePowerUp <|.. MultipleKnivesConfig
    IProjectilePowerUp <|.. KnifeAimConfig
    ICancellablePowerUp <|.. SuperKnifeConfig
    ICancellablePowerUp <|.. MultipleKnivesConfig
    ICancellablePowerUp <|.. KnifeAimConfig
    
    IWorldEffectPowerUp <|.. TimeDilatorConfig
```



---

## Thrower State Management

```mermaid
stateDiagram-v2
    [*] --> NormalThrowing: Level Start
    
    NormalThrowing --> ProjectilePowerUpActive: Activate Projectile PowerUp
    NormalThrowing --> NormalThrowing: Activate World PowerUp
    
    ProjectilePowerUpActive --> NormalThrowing: PowerUp Used
    ProjectilePowerUpActive --> NormalThrowing: PowerUp Cancelled
    ProjectilePowerUpActive --> ProjectilePowerUpActive: Activate World PowerUp
    
    note right of NormalThrowing
        - Normal thrower enabled
        - Magazine knives active
        - KnifeThrownChannel fires
    end note
    
    note right of ProjectilePowerUpActive
        - Normal thrower disabled
        - PowerUp thrower enabled
        - Magazine unchanged
    end note
```



---

## Activation Flow: Projectile PowerUp

```mermaid
sequenceDiagram
    participant UI as PowerUpHUD
    participant PUS as PowerUpService
    participant TM as ThrowerManager
    participant NT as NormalThrower
    participant PT as PowerUpThrower
    
    UI->>PUS: TryActivate(SuperKnifeConfig)
    PUS->>PUS: Check: Is IProjectilePowerUp?
    PUS->>TM: SwitchToProjectilePowerUp(config)
    TM->>NT: SetEnabled(false)
    TM->>PT: SetEnabled(true)
    TM->>PT: LoadKnife(config)
    PUS->>PUS: SetActiveProjectilePowerUp(config)
    PUS-->>UI: Activated Event
    
    Note over UI,PT: Player throws or cancels
    
    alt Player Throws
        PT->>PUS: OnPowerUpThrowCompleted()
        PUS->>TM: SwitchToNormal()
        TM->>PT: SetEnabled(false)
        TM->>NT: SetEnabled(true)
        PUS->>PUS: ConsumeInventory()
    else Player Cancels (taps button again)
        UI->>PUS: TryCancelActive()
        PUS->>TM: SwitchToNormal()
        TM->>PT: ClearAndReturn()
        TM->>PT: SetEnabled(false)
        TM->>NT: SetEnabled(true)
        PUS->>PUS: ClearActiveWithoutConsume()
    end
```



---

## Activation Flow: World PowerUp

```mermaid
sequenceDiagram
    participant UI as PowerUpHUD
    participant PUS as PowerUpService
    participant EFS as PowerUpEffectService
    participant Targets as Active Targets
    
    UI->>PUS: TryActivate(TimeDilatorConfig)
    PUS->>PUS: Check: Is IWorldEffectPowerUp?
    PUS->>EFS: ApplyEffect(config, duration)
    EFS->>Targets: SetSpeedMultiplier(0.5)
    PUS->>PUS: ConsumeInventory()
    PUS-->>UI: Used Event
    
    Note over UI,Targets: Normal throwing continues unaffected
    
    EFS->>EFS: Wait for duration
    EFS->>Targets: SetSpeedMultiplier(1.0)
```



---

## New Interfaces

### IProjectilePowerUp

```csharp
// Assets/Scripts/Game/PowerUps/Interfaces/IProjectilePowerUp.cs
namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Marker interface for powerups that affect knife throwing.
    /// When activated, these powerups disable normal throwing and use the powerup thrower.
    /// </summary>
    public interface IProjectilePowerUp
    {
        // Marker interface - Execute() is inherited from PowerUpConfigBase
    }
}
```

### IWorldEffectPowerUp

```csharp
// Assets/Scripts/Game/PowerUps/Interfaces/IWorldEffectPowerUp.cs
namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Interface for powerups that affect the game world without changing throw behavior.
    /// These are applied immediately and consumed, allowing normal throwing to continue.
    /// </summary>
    public interface IWorldEffectPowerUp
    {
        /// <summary>
        /// Duration of the world effect in seconds.
        /// </summary>
        float EffectDuration { get; }
        
        /// <summary>
        /// Applies the world effect using the provided service.
        /// </summary>
        void ApplyWorldEffect(PowerUpEffectService effectService);
    }
}
```

### ICancellablePowerUp

```csharp
// Assets/Scripts/Game/PowerUps/Interfaces/ICancellablePowerUp.cs
namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Interface for powerups that can be cancelled after activation.
    /// </summary>
    public interface ICancellablePowerUp
    {
        /// <summary>
        /// Called when the powerup is cancelled without being used.
        /// </summary>
        void OnCancelled();
    }
}
```

---

## IThrowerStateManager Interface

```csharp
// Assets/Scripts/Game/KnifeThrower/Interfaces/IThrowerStateManager.cs
namespace Game.KnifeThrower.Interfaces
{
    /// <summary>
    /// Manages the state transitions between normal and powerup throwing.
    /// </summary>
    public interface IThrowerStateManager
    {
        /// <summary>
        /// Whether a projectile powerup is currently active.
        /// </summary>
        bool IsProjectilePowerUpActive { get; }
        
        /// <summary>
        /// Switches to powerup throwing mode.
        /// </summary>
        void ActivateProjectilePowerUp(PowerUpConfigBase config);
        
        /// <summary>
        /// Returns to normal throwing mode after powerup use.
        /// </summary>
        void DeactivateProjectilePowerUp();
        
        /// <summary>
        /// Cancels active powerup and returns to normal throwing.
        /// </summary>
        void CancelProjectilePowerUp();
    }
}
```

---

## Key Changes Summary

### New Files to Create


| File                                 | Purpose                                       |
| ------------------------------------ | --------------------------------------------- |
| `Interfaces/IProjectilePowerUp.cs`   | Marker interface for throw-affecting powerups |
| `Interfaces/IWorldEffectPowerUp.cs`  | Interface for world effect powerups           |
| `Interfaces/ICancellablePowerUp.cs`  | Interface for cancellable powerups            |
| `Interfaces/IThrowerStateManager.cs` | Thrower state management contract             |
| `Services/ThrowerStateManager.cs`    | Implementation of state management            |


### Files to Modify


| File                              | Changes                                                       |
| --------------------------------- | ------------------------------------------------------------- |
| `Configs/SuperKnifeConfig.cs`     | Implement `IProjectilePowerUp`, `ICancellablePowerUp`         |
| `Configs/MultipleKnivesConfig.cs` | Implement `IProjectilePowerUp`, `ICancellablePowerUp`         |
| `Configs/KnifeAimConfig.cs`       | Implement `IProjectilePowerUp`, `ICancellablePowerUp`         |
| `Configs/TimeDilatorConfig.cs`    | Implement `IWorldEffectPowerUp`                               |
| `Services/PowerUpService.cs`      | Use interface checks instead of type checks, add cancellation |
| `Views/PowerUpHudController.cs`   | Handle cancel on second tap of active powerup                 |


---

## PowerUpService Changes

```csharp
public bool TryActivate(PowerUpConfigBase config)
{
    if (config == null || !HasInventory(config))
        return false;

    // World effects apply immediately without affecting thrower
    if (config is IWorldEffectPowerUp worldEffect)
    {
        worldEffect.ApplyWorldEffect(_EffectService);
        ConsumeInventory(config);
        _UsedChannel?.Raise(new PowerUpUsedEventData(config, GetRemainingCount(config)));
        return true;
    }

    // Projectile powerups switch thrower state
    if (config is IProjectilePowerUp)
    {
        _ThrowerStateManager.ActivateProjectilePowerUp(config);
        _RuntimeState.SetActive(config);
        _ActivatedChannel?.Raise(new PowerUpActivatedEventData(config, GetRemainingCount(config)));
        return true;
    }

    return false;
}

public bool TryCancelActive()
{
    if (!HasActivePowerUp)
        return false;
        
    if (_RuntimeState.ActiveConfig is ICancellablePowerUp cancellable)
    {
        cancellable.OnCancelled();
        _ThrowerStateManager.CancelProjectilePowerUp();
        _RuntimeState.Clear();
        _CancelledChannel?.Raise(new PowerUpCancelledEventData(_RuntimeState.ActiveConfig));
        return true;
    }
    
    return false;
}
```

---

## HUD Cancel Behavior

```csharp
// In PowerUpHudController
private void OnButtonClicked(PowerUpConfigBase config)
{
    // If clicking the same powerup that's active, try to cancel
    if (_PowerUpService.ActiveConfig == config)
    {
        _PowerUpService.TryCancelActive();
        return;
    }
    
    // Otherwise activate normally
    _PowerUpService.TryActivate(config);
}
```

---

## Benefits

1. **Extensible**: New powerup types just implement the appropriate interface
2. **Clear Contracts**: Interfaces define expected behavior
3. **Single Responsibility**: Each interface has one purpose
4. **No Type Checking**: Polymorphism replaces `is` checks
5. **Cancellation Support**: Clean cancel flow for projectile powerups
6. **World Effects Isolated**: World powerups don't touch thrower logic at all

