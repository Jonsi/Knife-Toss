# Addressables System Guide (Knife-Toss)

This document describes how Addressables works in this project, where remote checks happen, how local vs remote bundles are chosen, and where to inspect/debug.

## 1) Runtime flow (what happens on app start)

Primary flow is designed to start from **Loading** scene and run bootstrap before menus/gameplay.

1. `LoadingController` starts bootstrap.
2. `CatalogUpdateService` checks and applies remote catalog updates.
3. `RemoteCompositeLoader` loads `StagesCollectionComposite` via Addressables.
4. `StagesCollectionCatalogProvider` uses remote composite if loaded, else falls back to serialized composite from `GameInstaller`.
5. UI/services read world count + keys from `IStagesCollectionCatalog`.

Inspect:
- `Assets/Scripts/Game/Flow/Loading/LoadingController.cs`
- `Assets/Scripts/Common/AssetLoading/UnityAddressables/CatalogUpdateService.cs`
- `Assets/Scripts/Game/Level/Services/RemoteCompositeLoader.cs`
- `Assets/Scripts/Game/Level/Services/StagesCollectionCatalogProvider.cs`

## 2) What determines local vs remote download

Addressables group schema controls build/load paths:

- **Remote group** (`Worlds`) uses:
  - `Remote.BuildPath`
  - `Remote.LoadPath` (Unity Cloud CCD URL)
- **Local group** (`Worlds_Local`) uses:
  - `Local.BuildPath`
  - `Local.LoadPath` (runtime local path / APK)

Inspect:
- `Assets/AddressableAssetsData/AssetGroups/Worlds.asset`
- `Assets/AddressableAssetsData/AssetGroups/Worlds_Local.asset`
- `Assets/AddressableAssetsData/AssetGroups/Schemas/Worlds_BundledAssetGroupSchema.asset`
- `Assets/AddressableAssetsData/AssetGroups/Schemas/Worlds_Local_BundledAssetGroupSchema.asset`
- `Assets/AddressableAssetsData/AddressableAssetSettings.asset`

## 3) Composite asset role (important)

`StagesCollectionComposite` is the world manifest. It contains an ordered list of `AssetReference<StagesCollection>` keys.

- `CollectionsCount` comes from this list length.
- Each world entry is loaded via `GetCollectionLocationKey(index)`.
- If entries are duplicates, worlds will intentionally reuse same underlying stage collection assets.

Inspect:
- `Assets/Configurations/Worlds/StagesCollectionComposite.asset`
- `Assets/Scripts/Game/Level/Configurations/Stage/StagesCollectionComposite.cs`

## 4) Where world list is consumed

- Stage menu builds rows from `CollectionsCount`, then loads each world by key.
- Gameplay world loading also resolves by key from the catalog provider.

Inspect:
- `Assets/Scripts/Game/UI/Stages/StageListController.cs`
- `Assets/Scripts/Game/Level/Services/WorldAssetService.cs`

## 5) Fallback behavior (why remote may not appear)

If remote composite load does not succeed, app continues with fallback composite serialized on `GameInstaller` prefab.

Inspect serialized refs:
- `Assets/Prefabs/GameInstaller.prefab`

Installer wiring:
- `Assets/Scripts/Game/Installers/GameInstaller.cs`

## 6) Logging added for diagnosis

Use these tags in logs:

- `CatalogUpdateService`
  - catalog check start/result
  - updates available/no updates
  - update success/failure
- `RemoteCompositeLoader`
  - remote composite load start/success/failure
  - loaded collections count + keys
- `StagesCollectionCatalogProvider`
  - active source `remote` vs `fallback`
  - active collections count + keys
- `LoadingController`
  - bootstrap summary
- `AddressableAssetLoader`
  - per-key dependency graph and whether HTTPS appears

Core logger scripts:
- `Assets/Scripts/Common/AssetLoading/Addressables/AddressableAssetLoader.cs`
- `Assets/Scripts/Common/AssetLoading/UnityAddressables/CatalogUpdateService.cs`
- `Assets/Scripts/Game/Level/Services/RemoteCompositeLoader.cs`
- `Assets/Scripts/Game/Level/Services/StagesCollectionCatalogProvider.cs`
- `Assets/Scripts/Game/Flow/Loading/LoadingController.cs`

## 7) Editor behavior vs device behavior

In Editor, loads can resolve from local `Library/com.unity.addressables/...` paths depending on Play Mode/data setup. That does not prove CDN downloads.

For device/CDN validation, ensure logs show remote HTTPS dependency paths and remote bootstrap logs from Loading flow.

## 8) Editor-only forced bootstrap toggle

To avoid missing remote checks when starting Play Mode from non-Loading scenes:

- Menu: `Tools/Addressables/Force Remote Bootstrap In Play Mode`
- When enabled in Editor, `GameInstaller` runs catalog update + remote composite load even outside Loading scene.

Inspect:
- `Assets/Scripts/Editor/Addressables/EditorRemoteBootstrapMenu.cs`
- `Assets/Scripts/Game/Flow/Loading/EditorRemoteBootstrapStartable.cs`
- `Assets/Scripts/Game/Installers/GameInstaller.cs`

## 9) Typical publish/update sequence

1. Update composite (and optionally world assets).
2. Build Addressables for Android (`New Build`).
3. Upload/publish catalog + hash + referenced bundles to CCD release/badge used by `Remote.LoadPath`.
4. Run app through bootstrap flow and validate logs above.

