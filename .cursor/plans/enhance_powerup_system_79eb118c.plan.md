---
name: Enhance PowerUp System
overview: "Refactor the power-up system to replace type-switching with polymorphism, use a flexible dictionary-based inventory, and add two new target-affecting power-ups: Time Dilator (slow motion) and Direction Lock (prevents direction flipping)."
todos:
  - id: inventory-refactor
    content: Refactor PowerUpInventoryData to use dictionary-based counts instead of hard-coded fields
    status: pending
  - id: config-base-refactor
    content: Add InventoryKey property and execution interface to PowerUpConfigBase
    status: pending
  - id: existing-configs-update
    content: Update SuperKnifeConfig, MultipleKnivesConfig, CollectiblesBladeConfig with InventoryKey and execution logic
    status: pending
  - id: service-refactor
    content: Refactor PowerUpService to remove switch statements, delegate to config execution
    status: pending
  - id: hud-refactor
    content: Update PowerUpHudController to use InventoryKey for count lookups
    status: pending
  - id: target-modifiers
    content: Add SpeedMultiplier and IsDirectionLocked modifiers to LocalBehaviorRuntime and TargetView
    status: pending
  - id: flipping-behavior-update
    content: Update FlippingRotationBehavior to respect direction lock modifier
    status: pending
  - id: effect-service
    content: Create PowerUpEffectService to manage timed effects on all active targets
    status: pending
  - id: time-dilator
    content: Create TimeDilatorConfig power-up
    status: pending
  - id: direction-lock
    content: Create DirectionLockConfig power-up
    status: pending
  - id: installer-update
    content: Update PowerUpInstaller to register new effect service
    status: pending
---

# Enhance Power-Up System

## Current Issues

The existing power-up system uses **type-switching** (switch statements on config types) which creates tight coupling:

- `PowerUpService.TryHandleThrow()` switches on config type
- `GetRemainingCount()` and `DecrementInventory()` use type-based switch expressions
- `PowerUpHudController.GetCount()` also uses type switching
- `PowerUpInventoryData` has hard-coded properties per power-up type

Adding new power-ups currently requires modifying 4+ files.

## Architecture Changes

### 1. Replace Type-Switching with Polymorphism

Add execution logic to `PowerUpConfigBase` so each power-up handles its own behavior:

- Add abstract/virtual `Execute()` method to `PowerUpConfigBase`
- Add `InventoryKey` property (string) to identify each power-up in inventory
- Move execution logic from `PowerUpService` switch statements into each config class

### 2. Flexible Dictionary-Based Inventory

Replace hard-coded inventory fields with a dictionary in [PowerUpInventoryData.cs](Assets/Scripts/Game/PowerUps/Data/PowerUpInventoryData.cs):

```csharp
public class PowerUpInventoryData
{
    public Dictionary<string, int> Counts = new();
}
```

### 3. Target Modifier System for Time Dilator and Direction Lock

Add modifier support to `LocalBehaviorRuntime` and `TargetView`:

- Add `SpeedMultiplier` property to `LocalBehaviorRuntime` (default 1.0)
- Add `IsDirectionLocked` property to `LocalBehaviorRuntime` 
- Add `ApplyModifiers()` / `ClearModifiers()` methods to `TargetView`
- Behaviors will respect these modifiers when calculating rotation

### 4. New Power-Up Configs

**TimeDilatorConfig** - Slows rotation speed:

- `Duration`: How long the effect lasts (seconds)
- `SpeedMultiplier`: Rotation speed multiplier (e.g., 0.3 for 30% speed)

**DirectionLockConfig** - Prevents direction flipping:

- `Duration`: How long the effect lasts (seconds)
- Locks current direction, preventing `FlippingRotationBehavior` from flipping

### 5. Timed Effect Service

Create `PowerUpEffectService` to manage duration-based effects:

- Tracks active timed effects
- Applies modifiers to all active targets via `LevelData.ActiveTargets`
- Automatically removes effects when duration expires

## Files to Create

- `Assets/Scripts/Game/PowerUps/Configs/TimeDilatorConfig.cs`
- `Assets/Scripts/Game/PowerUps/Configs/DirectionLockConfig.cs`
- `Assets/Scripts/Game/PowerUps/Services/PowerUpEffectService.cs`
- `Assets/Scripts/Game/PowerUps/Interfaces/IPowerUpEffect.cs`

## Files to Modify

- `Assets/Scripts/Game/PowerUps/Configs/PowerUpConfigBase.cs` - Add `InventoryKey`, execution interface
- `Assets/Scripts/Game/PowerUps/Data/PowerUpInventoryData.cs` - Dictionary-based counts
- `Assets/Scripts/Game/PowerUps/Services/PowerUpService.cs` - Remove switch statements, delegate to configs
- `Assets/Scripts/Game/PowerUps/Views/PowerUpHudController.cs` - Use `InventoryKey` for counts
- `Assets/Scripts/Game/Entities/Target/Logic/LocalBehaviorRuntime.cs` - Add modifier properties
- `Assets/Scripts/Game/Entities/Target/Views/TargetView.cs` - Add modifier API
- `Assets/Scripts/Game/Entities/Target/RotationBehaviors/FlippingRotationBehavior.cs` - Respect direction lock
- `Assets/Scripts/Game/Installers/PowerUpInstaller.cs` - Register new service
- Existing config files (`SuperKnifeConfig`, `MultipleKnivesConfig`, `CollectiblesBladeConfig`) - Add `InventoryKey`

## Unity Setup Required

After implementation:

1. Create ScriptableObject assets for `TimeDilatorConfig` and `DirectionLockConfig`
2. Add them to the `PowerUpCatalog`
3. Configure duration and speed multiplier values in the Inspector