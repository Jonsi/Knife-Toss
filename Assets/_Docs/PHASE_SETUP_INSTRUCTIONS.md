# Phase Setup Instructions

## Phase 6: Collectibles Setup

Follow these steps to configure the collectibles system in the Unity Editor after pulling the code changes.

### 1. Create Event Channel Asset
1. Navigate to `Assets/Configurations/EventChannels/`.
2. Create a new folder named `Collectible`.
3. Right-click in the folder and select: **Create -> Knife Toss -> Events -> Collectible Hit Event Channel**.
4. Name the new asset `CollectibleHitEventChannel`.

### 2. Configure GameInstaller
1. Open the `GameInstaller` (either the prefab in `Assets/Prefabs/` or the object in your scene).
2. Locate the **Event Channels** section in the Inspector.
3. Find the **Collectible Hit Event Channel** field.
4. Drag and drop the `CollectibleHitEventChannel` asset created in Step 1 into this field.

### 3. Configure Collectible Prefab
1. Open `Assets/Prefabs/Tomato.prefab`.
2. Add the `CollectibleView` component to the root GameObject.
3. In the `CollectibleView` component:
   - **Hit Particles Prefab**: Assign `Assets/Prefabs/TomatoParticles.prefab`.
   - **Collider**: Drag the existing `CircleCollider2D` component from the prefab into this field.
   - **Value**: Set to `1`.
4. Ensure the `CircleCollider2D` has **Is Trigger** unchecked (the knife handles the trigger interaction).
   *Note: If the `CollectibleView` script replaced an old script, remove the missing script component.*

### 4. Update Level Configuration
1. Open `Assets/Configurations/Gameplay/StagesConfig.asset`.
2. Expand the **Stages** list and select a Stage (e.g., Stage 0).
3. Expand **Levels** and select a Level (e.g., Level 0).
4. Expand **Targets** and select a Target.
5. Find the **Collectibles** list (newly added).
6. Add new elements to the list to spawn tomatoes:
   - **Prefab**: Assign `Assets/Prefabs/Tomato.prefab`.
   - **Value**: `1`.
   - **Angle Offset**: Set degrees for position (e.g., `0`, `90`, `180`, `270`).
   - **Radius Offset**: Set distance from target edge (e.g., `0.5`).

### 5. Verify Setup
1. Play the game in the Editor.
2. Verify tomatoes appear on the target.
3. Throw a knife at a tomato.
4. Verify:
   - Particles spawn.
   - Tomato disappears.
   - No errors in the console.

