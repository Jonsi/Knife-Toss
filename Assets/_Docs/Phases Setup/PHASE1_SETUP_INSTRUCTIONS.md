# Phase 1 Setup Instructions

## 1. Create KnifeThrowConfigAsset ScriptableObject Asset

1. In Unity, right-click in `Assets/Configurations/Gameplay/` folder
2. Select: `Create > Knife Toss > Gameplay > Knife Throw Config`
3. Name it `KnifeThrowConfig`
4. Set default values:
   - Throw Speed: 10
   - Spawn Position Offset: (0, 0)

## 2. Setup GameplayInstaller in Scene

1. In the scene, create an empty GameObject named `GameplayInstaller`
2. Add the `GameplayInstaller` component to it
3. Assign the `KnifeThrowConfig` asset to the `Knife Throw Config` field in the inspector
4. The VContainer LifetimeScope component will be automatically added

## 3. Setup KnifeThrowerView

1. Find or create the GameObject that will spawn knives (e.g., existing KnifeThrower prefab)
2. Add the `KnifeThrowerView` component
3. **Important**: Assign the `KnifeView` component (from the Knife prefab) directly to the `Knife Prefab` field (not the GameObject, but the KnifeView component)
4. Assign the spawn position Transform to `Spawn Point` (or leave null to use the GameObject's position)

## 4. Configure Knife Prefab

1. Open `Assets/Prefabs/Knife.prefab`
2. Ensure it has:
   - SpriteRenderer component
   - Collider2D component (set as Trigger) - for collision detection only
3. Add the `KnifeView` component
4. **Important**: In the KnifeView component inspector, assign:
   - The SpriteRenderer component to the `_spriteRenderer` field
5. Save the prefab

**Note**: 
- The Knife prefab must have KnifeView as the root component (not a child GameObject) for direct prefab reference.
- Movement is handled via transform, not physics (no Rigidbody2D needed).

## 5. Regenerate Input Actions C# Class

1. Select `Assets/InputSystem_Actions.inputactions` in the Project window
2. In the Inspector, click `Generate C# Class`
3. This will create the InputSystem_Actions.cs file needed by InputAdapter

## 6. Test

- Enter Play Mode
- Tap/Click the screen or press the Throw action
- A knife should spawn and fly upward
- Check console for any errors

---

# Phase 2 Setup Instructions

## 1. Create TargetConfigAsset ScriptableObject Asset

1. In Unity, right-click in `Assets/Configurations/Gameplay/` folder
2. Select: `Create > Knife Toss > Gameplay > Target Config`
3. Name it `TargetConfig`
4. Set default values:
   - Rotation Speed: 50
   - Radius: 1.0

## 2. Setup Target Prefab

1. Open `Assets/Prefabs/Target.prefab`
2. Ensure it has:
   - SpriteRenderer component
   - CircleCollider2D component (set as Trigger) - for collision detection
3. Add the `TargetView` component
4. **Important**: In the TargetView component inspector, assign:
   - The CircleCollider2D component to the `_collider` field
5. Save the prefab

## 3. Setup Target in Scene

1. In the scene, find or create the Target GameObject
2. Add the `TargetView` component if not already present
3. Assign the CircleCollider2D to the `_collider` field in TargetView

## 4. Update GameInstaller

1. Select the GameObject with `GameInstaller` component in the scene
2. In the Inspector, assign:
   - The `TargetConfig` asset to the `Target Config` field
   - The Target GameObject (with TargetView component) to the `Target View` field
3. The VContainer LifetimeScope will automatically inject dependencies

## 5. Configure Knife Prefab for Collision

1. Open `Assets/Prefabs/Knife.prefab`
2. Ensure the Collider2D component is:
   - Set as Trigger
   - Properly sized to detect collisions with target
3. Save the prefab

## 6. Test Phase 2

- Enter Play Mode
- Target should rotate continuously at configured speed
- Throw a knife at the target
- Knife should stick to the target surface at the collision point
- Stuck knife should rotate with the target (parented correctly)
- Knife tip should point toward target center
- Multiple knives can stick at different positions
- Change rotation speed in TargetConfig → Target rotates faster/slower

