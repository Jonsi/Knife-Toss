# Addressables Implementation Guide

This document outlines the steps to set up and use the Unity Addressables system in the Knife-Toss project, specifically for the PVP Scene implementation.

## 1. Initial Setup

1.  Open the **Addressables Groups** window: `Window > Asset Management > Addressables > Groups`.
2.  If you see a button saying "Create Addressables Settings", click it. This will create the necessary configuration assets in your project (usually under `Assets/AddressableAssetsData`).

## 2. Creating Asset Groups

Organizing assets into groups helps manage how they are built and loaded.

1.  In the Addressables Groups window, right-click in the empty space and select **Create New Group > Packed Assets**.
2.  Rename the new group to `Scenes_Group` (or similar).
3.  Select the `Scenes_Group`. In the Inspector, check the **Build & Load Paths**.
    *   For **Local** content (bundled with the app), ensure it is set to `LocalBuildPath` and `LocalLoadPath`.
    *   For **Remote** content (downloaded from cloud), ensure it is set to `RemoteBuildPath` and `RemoteLoadPath`.

## 3. Adding the PVP Scene

To make the PVP scene loadable via Addressables:

1.  Navigate to `Assets/Scenes` in the Project window.
2.  Select `PVPPlay.unity`.
3.  In the Inspector, check the box labeled **Addressable**.
4.  The default address will be the file path (e.g., `Assets/Scenes/PVPPlay.unity`). Change this address to **`PVPPlay`** to match the constant used in the code.
5.  In the Addressables Groups window, find the newly added entry (it might be in the Default Local Group). Drag it into your `Scenes_Group`.

## 4. Code Implementation

The code in `SceneLoaderService.cs` has been updated to use Addressables for the PVP scene.

**Previous Code:**
```csharp
await SceneManager.LoadSceneAsync(PVP_SCENE, LoadSceneMode.Single).ToUniTask();
```

**New Code:**
```csharp
using UnityEngine.AddressableAssets;
// ...
await Addressables.LoadSceneAsync(PVP_SCENE, LoadSceneMode.Single).Task.AsUniTask();
```

*Note: The `PVP_SCENE` constant string must match the Addressable Name you set in step 3 (i.e., "PVPPlay").*

## 5. Build Settings

### Building Addressables
Before building the Player (APK/Exe), you must build the Addressables content.

1.  Open `Window > Asset Management > Addressables > Groups`.
2.  Click **Build > New Build > Default Build Script**.
3.  This generates the bundles (usually in `Library/com.unity.addressables/...` or `ServerData/`).

### Building the Player
1.  Open `File > Build Settings`.
2.  Ensure `PVPPlay` is **removed** (or unchecked) from the "Scenes In Build" list. Addressables loads it from the bundle, not the build list. *Note: If you keep it in the list, it will be duplicated, but Addressables will still load its version.*
3.  Build and Run as normal.

## 6. Play Mode Testing

To test in the Editor without a full build:
1.  In the Addressables Groups window, looking at the toolbar "Play Mode Script".
2.  **Use Asset Database (fastest)**: Loads directly from project files. No build needed. Good for rapid logic iteration.
3.  **Simulate Groups (advanced)**: Simulates bundles. Good for testing group layout and loading logic.
4.  **Use Existing Build (requires built groups)**: Uses the actual bundles created in Step 5. Essential for testing remote loading locally.

## 7. Toggling Remote/Local Loading

To switch between hosting assets locally or remotely:

1.  Open `Window > Asset Management > Addressables > Profiles`.
2.  The **Default** profile usually maps `RemoteLoadPath` to a local server URL or a placeholder.
3.  **To set up Remote Loading:**
    *   Create a new variable or modify `RemoteLoadPath` in your profile.
    *   Set it to your cloud bucket URL (e.g., `https://my-game-bucket.s3.amazonaws.com/[BuildTarget]`).
    *   Set the `Scenes_Group` settings (Inspector) to use **Remote** paths.
4.  **To set up Local Loading:**
    *   Set the `Scenes_Group` settings to use **Local** paths.
    *   The bundles will be included in the StreamingAssets folder of the built player.

## 8. Cloud Setup (Hosting)

If you chose **Remote** paths for your `Scenes_Group`:

1.  **Build Addressables**: Run the build script (Step 5).
2.  **Locate Artifacts**: Find the built bundles.
    *   Check your `RemoteBuildPath` setting. It often defaults to `ServerData/[BuildTarget]`.
    *   You will see `.hash`, `.json`, and `.bundle` files.
3.  **Upload**: Upload **all** files in that folder to your hosting provider (S3, Google Cloud Storage, Unity CCD, or a standard web server).
4.  **Verify URL**: Ensure the `RemoteLoadPath` in your Profile matches the URL where you uploaded the files.
    *   Example: If you uploaded `hash.json` to `https://site.com/data/hash.json`, your load path should be `https://site.com/data`.

## 10. Runtime Asset Loading (Prefabs/Sprites)

Beyond scenes, you can load individual assets like Prefabs or Sprites.

### Option A: Using AssetReference (Recommended for Inspector)
1.  Add `using UnityEngine.AddressableAssets;`
2.  Declare a field: `[SerializeField] private AssetReference _myPrefabReference;`
3.  Load and Instantiate:
```csharp
public async UniTask SpawnObject()
{
    // Load and instantiate in one go
    GameObject obj = await _myPrefabReference.InstantiateAsync().Task.AsUniTask();
}
```

### Option B: Using Address (String)
```csharp
public async UniTask LoadByAddress(string address)
{
    GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(address).Task.AsUniTask();
    Instantiate(prefab);
}
```

### Important: Memory Management
Addressables uses reference counting. If you load an asset manually with `LoadAssetAsync`, you should release it when done:
```csharp
Addressables.Release(myLoadedAsset);
```
If you use `InstantiateAsync`, Addressables handles the reference count for the instance, but you should use `Addressables.ReleaseInstance(gameObject)` if you want to be explicit, though destroying the object normally often suffices if it was created via Addressables.

---
### Summary Checklist
- [ ] Addressables Package Installed.
- [ ] Settings Created.
- [ ] PVPPlay marked Addressable (Name: "PVPPlay").
- [ ] Code updated to `Addressables.LoadSceneAsync`.
- [ ] Assembly Definitions (.asmdef) updated with `Unity.Addressables` and `Unity.ResourceManager`.
- [ ] Group set to Local or Remote.
- [ ] Addressables Built (`Build > New Build`).
- [ ] Player Built.

## 9. Assembly Definitions (.asmdef)

If your project uses Assembly Definitions, you must add the Addressables modules as references:

1.  Locate your `.asmdef` file (e.g., `Game.asmdef`).
2.  In the Inspector, under **Assembly Definition References**, add:
    *   `Unity.Addressables`
    *   `Unity.ResourceManager`
    *   `UniTask.Addressables` (If using UniTask)
3.  Click **Apply**.

This resolves errors like `The type or namespace name 'AddressableAssets' does not exist`.
