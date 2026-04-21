---
name: IAP Bug Fixes and Tests
overview: Fix all 18 issues identified in the code review (grouped by system) in Phase 1, then add a comprehensive NUnit test suite covering all IAP-related systems in Phase 2.
todos:
  - id: p1-iap-purchase-result
    content: "Phase 1 Group A: Add IsRestored flag to IapPurchaseResult; update Restored factory to set it true"
    status: pending
  - id: p1-iap-service
    content: "Phase 1 Group B: Fix IapService — OperationCanceledException disambiguation, GlobalTimeoutSeconds guard, RestorePurchasesAsync concurrency flag, EnsureInitializedAsync double-init guard"
    status: pending
  - id: p1-iap-settings
    content: "Phase 1 Group C: Remove dead UseMockProvider field from IapSettings"
    status: pending
  - id: p1-remove-ads-reward
    content: "Phase 1 Group D: Fix RemoveAdsReward empty catch; remove Product field; add GetIapProduct helper on PurchasableEntry; update 3 dual-source callsites"
    status: pending
  - id: p1-game-installer
    content: "Phase 1 Group E: Remove duplicate KnifeSkinService registration from GameInstaller; remove dead StoreCatalog null check"
    status: pending
  - id: p1-store-router
    content: "Phase 1 Group F: StorePurchaseRouter — wrap post-IAP GrantAsync failure with explicit error logging"
    status: pending
  - id: p1-store-menu
    content: "Phase 1 Group G: StoreMenuController — add _IapBindings list; handle IAP success feedback; fix IAP/power-up binding separation"
    status: pending
  - id: p1-ad-service
    content: "Phase 1 Group H: Remove Debug.Log + GetCallerStackTrace from AdService.ShowAdAsync"
    status: pending
  - id: p1-small-fixes
    content: "Phase 1 Group I: Remove unused System.Linq import from RewardClaimTracker; cache IapAdEntitlementGate result; MockIapProvider use string keys; MainMenuRemoveAdsFlow pass CancellationToken"
    status: pending
  - id: p2-test-asmdefs
    content: "Phase 2: Create Common.IAP.Tests and Game.IAP.Tests asmdef files with correct references"
    status: pending
  - id: p2-fake-helpers
    content: "Phase 2: Create shared fake/stub helper classes for all IAP interfaces"
    status: pending
  - id: p2-common-tests
    content: "Phase 2: Write IapPurchaseResultTests and IapServiceTests and IapEntitlementsDataTests"
    status: pending
  - id: p2-game-tests
    content: "Phase 2: Write StorePurchaseRouterTests, IapAdEntitlementGateTests, PlayerDataIapEntitlementStoreTests, MainMenuRemoveAdsFlowTests"
    status: pending
isProject: false
---

# IAP Bug Fixes and Tests Plan

## Phase 1 — Bug Fixes and Cleanup

Fixes are grouped by file/system to minimize context switching.

---

### Group A — `IapPurchaseResult` (affects everything downstream)

**Fix #10 — `Purchased` and `Restored` are indistinguishable**

Add an `IsRestored` boolean to the struct and set it to `true` only in the `Restored` factory. `Purchased` sets it `false`. All downstream callers (telemetry, router, events) automatically get the flag via the existing `IapPurchaseResult` parameter they already receive. No callers need updating unless they explicitly want to branch on it.

File: [`Assets/Scripts/Common/IAP/Data/IapPurchaseResult.cs`](Assets/Scripts/Common/IAP/Data/IapPurchaseResult.cs)

---

### Group B — `IapService`

**Fix #1 — `OperationCanceledException` → always `Timeout`**

In `ExecuteWithTimeoutAsync`, capture the `timeoutCts` reference and expose whether it fired. The pattern already exists in `AdService.ExecuteShowAsync` (checks `timeoutCts.IsCancellationRequested`). Refactor `ExecuteWithTimeoutAsync` to return the timeout CTS as an `out` parameter, or inline the timeout logic into `PurchaseAsync` directly (as `AdService` does), so `catch (OperationCanceledException)` can check `timeoutCts.IsCancellationRequested` and return `Timeout` only when the timeout fired; otherwise return a new `IapPurchaseResult.CancelledByUser`.

**Fix #2 — `GlobalTimeoutSeconds = 0` kills all purchases**

At the top of `ExecuteWithTimeoutAsync`, clamp or guard: if `_Settings.GlobalTimeoutSeconds <= 0`, skip the timeout wrapper entirely (pass `ct` directly). This mirrors what a safe fallback should do when unconfigured.

**Fix #5 — Restore not guarded against concurrent purchase**

Add an `IsRestoreInProgress` flag (private `bool`). At the start of `RestorePurchasesAsync`, return early if `IsPurchaseInProgress || IsRestoreInProgress`. Set/clear it in a `try/finally` block mirroring the `IsPurchaseInProgress` pattern in `PurchaseAsync`.

**Fix #7 — Double-init race in `EnsureInitializedAsync`**

Add a private `bool _IsInitializing` flag. If `_IsInitializing` is true when `EnsureInitializedAsync` is entered, await a polling loop or a `UniTaskCompletionSource`. The simpler practical fix is: set `_IsInitializing = true` immediately on entry (before the await), so subsequent callers see it and either wait or skip. For Unity's single-threaded main loop this is sufficient since `await` only yields at suspension points.

File: [`Assets/Scripts/Common/IAP/Services/IapService.cs`](Assets/Scripts/Common/IAP/Services/IapService.cs)

---

### Group C — `IapSettings`

**Fix #9 — Remove dead `UseMockProvider` field**

Delete the `[SerializeField] public bool UseMockProvider` property. Verify no code references it (it doesn't — confirmed during review).

File: [`Assets/Scripts/Common/IAP/Data/IapSettings.cs`](Assets/Scripts/Common/IAP/Data/IapSettings.cs)

---

### Group D — `RemoveAdsReward` + dual product source

**Fix #8 — Empty `catch {}` in `IsOwned`**

Replace the bare `catch` with `catch (Exception ex)` and emit a `Debug.LogWarning` (scoped to editor/dev builds if desired). This surfaces DI misconfiguration rather than silently returning `false`.

**Fix #11 — Dual product source duplicated in 3 callsites**

Remove the `[SerializeField] public IapProduct Product` field from `RemoveAdsReward`. Add a static helper `PurchasableEntry.GetIapProduct()` (or an extension method) that returns `entry.Price?.IapProduct` as the single authoritative source. Update the three callsites:
- [`Assets/Scripts/Game/IAP/Services/MainMenuRemoveAdsFlow.cs`](Assets/Scripts/Game/IAP/Services/MainMenuRemoveAdsFlow.cs) — `ResolveRemoveAdsProduct`
- [`Assets/Scripts/Game/Ads/Services/IapAdEntitlementGate.cs`](Assets/Scripts/Game/Ads/Services/IapAdEntitlementGate.cs) — `CanShowInterstitial`
- [`Assets/Scripts/Debug/Tabs/Mock/IapCheatsTab.cs`](Assets/Scripts/Debug/Tabs/Mock/IapCheatsTab.cs) — `FindRemoveAdsProduct`

File: [`Assets/Scripts/Game/Store/Rewards/RemoveAdsReward.cs`](Assets/Scripts/Game/Store/Rewards/RemoveAdsReward.cs), [`Assets/Scripts/Game/Store/Data/PurchasableEntry.cs`](Assets/Scripts/Game/Store/Data/PurchasableEntry.cs)

---

### Group E — `GameInstaller` / `StoreInstaller`

**Fix #3 — `KnifeSkinService` registered twice (two independent instances)**

Remove line 121 from [`Assets/Scripts/Game/Installers/GameInstaller.cs`](Assets/Scripts/Game/Installers/GameInstaller.cs):
```
builder.Register<KnifeSkinService>(Lifetime.Singleton).AsSelf();
```
`KnifeSkinService` is already registered in `StoreInstaller` (used by `StoreSceneInstaller`) and in `StageInstaller`. The root scope does not need it.

**Fix #13 — Dead null check for `_StoreCatalog` after `ValidateRequiredSetup` throws**

Remove the `if (_StoreCatalog != null)` guard at line 129 and register unconditionally. `ValidateRequiredSetup` already throws before that point if `_StoreCatalog` is null.

File: [`Assets/Scripts/Game/Installers/GameInstaller.cs`](Assets/Scripts/Game/Installers/GameInstaller.cs)

---

### Group F — `StorePurchaseRouter`

**Fix #4 — Post-IAP `GrantAsync` failure leaves player charged without reward**

After a successful `PurchaseAsync`, wrap `GrantAsync` in its own try/catch that:
1. Logs the exception explicitly (since the IAP layer cannot refund).
2. Returns `PurchaseResult.Error("Reward grant failed — entitlement preserved. Please restart and contact support.")`.

The entitlement is already persisted by `IapService` for non-consumables before `GrantAsync` is called, which is the correct order — the player's ownership is safe; only the in-session reward trigger failed.

File: [`Assets/Scripts/Game/Store/Services/StorePurchaseRouter.cs`](Assets/Scripts/Game/Store/Services/StorePurchaseRouter.cs)

---

### Group G — `StoreMenuController`

**Fix #6 — RemoveAds purchase has no visual success feedback**

**Fix #12 — IAP entries silently mixed into power-up bindings**

Add a dedicated `_IapBindings` list (same `SpawnedCardBinding` type). In `BindCatalog`, call a new `BindIapEntries(_StoreCatalog?.IapEntries)` that populates `_IapBindings`. In `RefreshPowerUpStates`, iterate only `_PowerUpBindings` (remove IAP entries from that list). In `HandleSuccessfulPurchaseAsync`, add a branch: if the entry is in `_IapBindings`, mark it purchased, disable its card, and refresh layout.

File: [`Assets/Scripts/Game/UI/Store/StoreMenuController.cs`](Assets/Scripts/Game/UI/Store/StoreMenuController.cs)

---

### Group H — `AdService`

**Fix #14 — `Debug.Log` with `new StackTrace()` on every ad show**

Remove the `Debug.Log(...)` / `GetCallerStackTrace()` block from `ShowAdAsync` and the now-unused private `GetCallerStackTrace()` method.

File: [`Assets/Scripts/Common/Ads/Services/AdService.cs`](Assets/Scripts/Common/Ads/Services/AdService.cs)

---

### Group I — Small, isolated fixes

**Fix #15 — Unused `using System.Linq;`**

Remove from [`Assets/Scripts/Game/Rewards/Services/RewardClaimTracker.cs`](Assets/Scripts/Game/Rewards/Services/RewardClaimTracker.cs).

**Fix #16 — `IapAdEntitlementGate` catalog scan on every interstitial check**

Cache the result of `CanShowInterstitial` in a nullable `bool? _cachedResult`. Invalidate the cache in a new `Invalidate()` method. Call `Invalidate()` from `PlayerDataIapEntitlementStore.SetOwned` via an optional callback, or simply clear the cache whenever entitlement changes. Alternatively, since `SetOwned` persists immediately, `IapAdEntitlementGate` can store a `bool _removeAdsOwned` computed once after the first resolution and flipped when `SetOwned` is called.

Files: [`Assets/Scripts/Game/Ads/Services/IapAdEntitlementGate.cs`](Assets/Scripts/Game/Ads/Services/IapAdEntitlementGate.cs), [`Assets/Scripts/Game/IAP/Services/PlayerDataIapEntitlementStore.cs`](Assets/Scripts/Game/IAP/Services/PlayerDataIapEntitlementStore.cs)

**Fix #17 — `MockIapProvider._OwnedProducts` uses reference equality**

Change `HashSet<IapProduct> _OwnedProducts` to `HashSet<string> _OwnedProductKeys`. Update `_OwnedProducts.Add/Remove/Contains` calls to use `product.ProductKey`. This makes mock ownership consistent with `IapEntitlementsData`.

File: [`Assets/Scripts/Common/IAP/Mock/Providers/MockIapProvider.cs`](Assets/Scripts/Common/IAP/Mock/Providers/MockIapProvider.cs)

**Fix #18 — No `CancellationToken` in `MainMenuRemoveAdsFlow.TryPurchaseAsync`**

Add a `CancellationToken ct = default` parameter to `TryPurchaseAsync` and forward it to `_IapService.PurchaseAsync(_RemoveAdsProduct, ct)`. Update the callsite in `MainMenuController` to pass its lifetime token.

File: [`Assets/Scripts/Game/IAP/Services/MainMenuRemoveAdsFlow.cs`](Assets/Scripts/Game/IAP/Services/MainMenuRemoveAdsFlow.cs)

---

## Phase 2 — Test Suite

No tests exist today. The project has `com.unity.test-framework 1.6.0`, `Common.asmdef`, and `Game.asmdef`.

### Assembly setup

Two new test asmdefs under `Assets/Tests/`:

- `Assets/Tests/Common.IAP.Tests/Common.IAP.Tests.asmdef` — references `Common`, `UniTask`, `Unity.TestFramework`; `testPlatforms: [EditMode]`
- `Assets/Tests/Game.IAP.Tests/Game.IAP.Tests.asmdef` — references `Game`, `Common`, `UniTask`, `Unity.TestFramework`; `testPlatforms: [EditMode]`

All tests are **EditMode** (no Play Mode required; Unity Test Framework supports `UniTask` via `UniTask.ToCoroutine()` or direct `async` with `[UnityTest]`).

For isolation, each test file creates lightweight hand-written fakes (no Moq/NSubstitute) for the interfaces — consistent with the project's existing mock-via-provider pattern.

---

### Test files and coverage

**`Common.IAP.Tests/IapPurchaseResultTests.cs`**
- All factory methods set the correct flag combination
- `Restored` has `IsRestored == true`; `Purchased` has `IsRestored == false`
- `Failed(null)` → non-empty message
- `default` struct has `Success == false`, all flags false

**`Common.IAP.Tests/IapServiceTests.cs`** — uses `FakeIapProvider` and `FakeIapEntitlementStore` fakes
- Returns `Disabled` when `IapSettings.IapEnabled == false`
- Returns `InvalidProduct` for null product
- Returns `InProgress` when purchase is already running
- Non-consumable already owned → raises `FailedChannel`, returns `ProductAlreadyOwned`, does not call provider
- Success path: provider called, entitlement stored (non-consumable), `CompletedChannel` raised, telemetry `OnPurchaseCompleted` called
- Consumable success: entitlement NOT stored
- Caller-cancel `OperationCanceledException` → `CancelledByUser` (not `Timeout`)
- Timeout `OperationCanceledException` → `Timeout`
- Provider exception → `Failed` with message, `FailedChannel` raised
- `RestorePurchasesAsync`: disabled → empty list
- `RestorePurchasesAsync`: non-consumable products set owned in entitlement store, `RestoredChannel` raised, telemetry called
- `RestorePurchasesAsync` blocked when purchase in progress

**`Common.IAP.Tests/IapEntitlementsDataTests.cs`**
- `IsOwned` false for null/empty key
- `SetOwned(true)` adds key, returns `true`
- `SetOwned(true)` when already owned → returns `false` (no duplicate)
- `SetOwned(false)` removes key, returns `true`
- `SetOwned(false)` when not owned → returns `false`

**`Game.IAP.Tests/StorePurchaseRouterTests.cs`** — uses fakes for `IPurchaseService`, `IIapService`, `IRewardGrantContext`
- Soft-currency entry delegates to `IPurchaseService`
- IAP entry with null product → `InvalidItem`
- IAP entry in-progress → `PurchaseInProgress`
- IAP entry already owned (`IUniqueReward`) → raises failed, returns error
- IAP entry success: `IIapService.PurchaseAsync` called, reward granted, `CompletedChannel` raised
- IAP entry IAP failure → `PurchaseFailedChannel` raised, failure mapped correctly (`Cancelled` → `PurchaseResult.Cancelled`, `AlreadyOwned` → `PurchaseResult.AlreadyOwned`, other → `Error`)
- IAP entry: `GrantAsync` throws after IAP success → returns `Error`, does not re-call IAP
- `RestorePurchasesAsync` delegates to `IIapService` and returns count

**`Game.IAP.Tests/IapAdEntitlementGateTests.cs`** — uses `FakeIapEntitlementStore`
- Returns `true` (can show) when no `RemoveAdsReward` in `IapEntries`
- Returns `true` when `RemoveAdsReward` present but entitlement not owned
- Returns `false` when `RemoveAdsReward` present and entitlement owned
- Returns `true` for null catalog / null `IapEntries`
- Cache: second call does not re-scan catalog; result updates after entitlement changed

**`Game.IAP.Tests/PlayerDataIapEntitlementStoreTests.cs`** — uses `FakePlayerDataService` (or a real one with `FakePlayerDataStore`)
- `HasEntitlement` false for null product
- `HasEntitlement` false for empty `ProductKey`
- `HasEntitlement` false before grant
- `HasEntitlement` true after `SetOwned(product, true)`
- `SetOwned` calls save when changed
- `SetOwned` does not call save when not changed

**`Game.IAP.Tests/MainMenuRemoveAdsFlowTests.cs`** — uses `FakeIapService` and a `FakeStoreCatalog`
- `IsAvailable` false when no catalog
- `IsAvailable` false when no `RemoveAdsReward` entry
- `IsAvailable` true when entry found
- `CanPurchase` false when `IsPurchaseInProgress`
- `CanPurchase` false when already owned
- `TryPurchaseAsync` calls `PurchaseAsync` with the correct product
- `TryPurchaseAsync` does not call `PurchaseAsync` when unavailable
- Analytics events fired for started, result, already-owned paths

---

### Fake helper classes (shared across test files)

Created once in each test assembly under `TestHelpers/`:

- `FakeIapProvider : IIapProvider` — configurable result, tracks call count
- `FakeIapEntitlementStore : IIapEntitlementStore` — in-memory `Dictionary<string, bool>`
- `FakeIapTelemetryListener : IIapTelemetryListener` — records calls for assertion
- `FakeIapService : IIapService` — configurable, tracks calls
- `FakeIapPurchaseService : IPurchaseService` — configurable
- `FakeIapGrantContext : IRewardGrantContext` — simple service-registry dictionary
- `FakePlayerDataStore : IPlayerDataStore` — in-memory `PlayerData` (no file I/O)