---
name: IAP System and RemoveAds
overview: Introduce a dedicated real-money IAP service under Common/IAP (mirroring the Common/Ads pattern) with a mock provider, keep the existing soft-currency Game.Store.IPurchaseService unchanged, and ship RemoveAds as a real-money IAP that integrates with the existing reward pipeline and suppresses interstitials via a persisted entitlement.
todos:
  - id: phase1-common-iap
    content: "Phase 1: create Common/IAP interfaces, data, events, services, mock provider, and IapInstaller mirroring Common/Ads pattern"
    status: pending
  - id: phase1-entitlements
    content: "Phase 1: add Game-side IapEntitlementsData to PlayerResourcesData and implement PlayerDataIapEntitlementStore"
    status: pending
  - id: phase1-composition
    content: "Phase 1: register IapInstaller and entitlement store from GameInstaller"
    status: pending
  - id: phase1-router
    content: "Phase 1: add optional IapProduct on PriceDefinition and introduce StorePurchaseRouter that delegates to IPurchaseService or IIapService"
    status: pending
  - id: phase2-product
    content: "Phase 2: author RemoveAds IapProduct, add to IapCatalog and StoreCatalog.IapEntries"
    status: pending
  - id: phase2-reward
    content: "Phase 2: implement RemoveAdsReward (ItemReward + IUniqueReward) backed by IIapEntitlementStore"
    status: pending
  - id: phase2-ads-gate
    content: "Phase 2: add IAdEntitlementGate and short-circuit AdService interstitials when RemoveAds entitlement is owned (rewarded ads unaffected)"
    status: pending
  - id: phase2-restore-qa
    content: "Phase 2: wire Restore Purchases UI action, mock popup outcomes, and a debug cheat to toggle RemoveAds entitlement"
    status: pending
isProject: false
---

## Verdict on existing `IPurchaseService`

`Game.Store.Interfaces.IPurchaseService` is tightly bound to soft currency (`ICurrencyService.TrySpend`, `CurrencyType`) and must remain the in-game purchase coordinator. Real-money IAP has fundamentally different semantics (platform products, localized prices, async store UI, receipts, restore, deferred/pending states, consumable vs. non-consumable). A new, separate `IIapService` lives in `Common/IAP`. A thin router in `Game/Store` delegates per-entry to one or the other so the store UI remains unified.

## Phase 1 — `Common/IAP` service with mock provider

Mirror the `Common/Ads` pattern exactly (facade + SDK adapter + mock + MonoBehaviour installer + event channels + null telemetry listener). Ship only the mock provider in Phase 1; do not install Unity IAP yet.

**New namespaces under `Assets/Scripts/Common/IAP`:Assets\Scripts\Game\UI\MainMenu\MainMenuController.cs(45,26): error CS0246: The type or namespace name 'KnifeSkinService' could not be found (are you missing a using directive or an assembly reference?)**



- `Interfaces/`: `IIapService`, `IIapProvider`, `IIapEntitlementStore`, `IIapTelemetryListener`.
- `Data/`: `IapProduct` (ScriptableObject per SKU, asset-encapsulated like `AdPlacementKey`), `IapCatalog`, `IapSettings`, `MockIapSettings`, `IapPurchaseResult`, `IapProductInfo`.
- `Events/`: `IapPurchaseStartedEventChannel`, `IapPurchaseCompletedEventChannel`, `IapPurchaseFailedEventChannel`, `IapPurchaseRestoredEventChannel` (use existing `EventChannel<T>` base).
- `Services/`: `IapService` (owns kill switch, in-progress guard, timeout, dispatch, telemetry), `NullIapTelemetryListener`.
- `Providers/`: `MockIapProvider` with `PopupCallback` for interactive QA, seeded ownership, sequenced outcomes, simulated delay (shape matches `MockAdProvider`).
- `Installers/IapInstaller : MonoBehaviour, IInstaller` mirroring [Assets/Scripts/Game/Ads/Installers/AdInstaller.cs](Assets/Scripts/Game/Ads/Installers/AdInstaller.cs).

**Game-side wiring (stays out of `Common`):**

- Extend `PlayerResourcesData` with `IapEntitlementsData` (list of owned product keys) plus `EnsureIapEntitlements` / `SaveIapEntitlements` on [Assets/Scripts/Game/Persistence/Services/PlayerDataService.cs](Assets/Scripts/Game/Persistence/Services/PlayerDataService.cs) (pattern: `EnsureKnifeSkinCollection`).
- Add `Game/IAP/Services/PlayerDataIapEntitlementStore` implementing `IIapEntitlementStore`.
- Register `IapInstaller` from [Assets/Scripts/Game/Installers/GameInstaller.cs](Assets/Scripts/Game/Installers/GameInstaller.cs) alongside `_AdInstaller`, and register the entitlement store.

**Store coexistence:**

- Add an optional `IapProduct` reference to `PriceDefinition` in [Assets/Scripts/Game/Store/Data/PriceDefinition.cs](Assets/Scripts/Game/Store/Data/PriceDefinition.cs). When present, the entry is real-money; otherwise it remains soft-currency (no enum, presence of the asset is the discriminator).
- Introduce `Game/Store/Services/StorePurchaseRouter` that inspects the entry and calls either `IPurchaseService` (unchanged) or `IIapService`. UI calls the router.
- On IAP success the router still runs `ItemReward.GrantAsync` via `IRewardGrantContext` and raises `PurchaseCompletedEventChannel`, keeping the reward/analytics pipeline unified.

**What belongs where:**

- `Common/IAP`: all interfaces, `IapService`, `MockIapProvider`, data/event/installer classes. No `Game.`* references, no reward/persistence coupling.
- `Game`: `IIapEntitlementStore` implementation, `StorePurchaseRouter`, `PriceDefinition` change, new `RemoveAdsReward`, and UI wiring.

## Phase 2 — RemoveAds as a real-money IAP

- Create a `RemoveAds` `IapProduct` (non-consumable) and add it to `IapCatalog`.
- Add a `PurchasableEntry` for it in a new `IapEntries` list on [Assets/Scripts/Game/Store/Data/StoreCatalog.cs](Assets/Scripts/Game/Store/Data/StoreCatalog.cs) (keeps soft vs. real-money visually obvious). `PriceDefinition.Product` references the RemoveAds asset.
- Add `Game/Store/Rewards/RemoveAdsReward : ItemReward, IUniqueReward`. `IsOwned` queries `IIapEntitlementStore`; `AlreadyOwnedReason = "Ads already removed."` `GrantAsync` is a no-op confirmation (entitlement is persisted by `IapService` on platform success).
- Add `IAdEntitlementGate` interface in `Common/Ads` with a `Game`-side implementation backed by `IIapEntitlementStore`. In [Assets/Scripts/Common/Ads/Services/AdService.cs](Assets/Scripts/Common/Ads/Services/AdService.cs), short-circuit interstitial `CanShowAd`/`ShowAdAsync` to `AdResult.Disabled` when the gate reports removed. Rewarded ads remain enabled.
- Purchase flow: UI -> `StorePurchaseRouter` -> `IIapService.PurchaseAsync` -> entitlement persisted -> `IapPurchaseCompletedEventChannel` -> reward grant -> `PurchaseCompletedEventChannel`. Restore flow re-applies entitlements via the store.
- Mock QA: `MockIapProvider.PopupCallback` exposes Success/Cancelled/Deferred/Failed outcomes; add a debug cheat analogous to `AdCheatsTab` to toggle the entitlement.

## Risks / dependencies

- Store compliance: restore button, deferred purchases, duplicate-grant protection, consumable vs. non-consumable semantics. `IIapProvider` must expose an explicit confirm/finish-transaction step.
- Receipt validation: plumb receipt blob through `IapPurchaseResult` so server-side validation can be added behind `IIapService` later.
- Local-only entitlement truth initially; call out cloud/backend validation as follow-up.
- `com.unity.purchasing` (or alternative) is not added in Phase 1; keep `IIapProvider` stable so the real provider is a drop-in swap.
- Project rules discourage enums/IDs; use asset-based references (`IapProduct` ScriptableObjects) not string SKUs scattered in code.
- Mock must cover success, user-cancel, network failure, deferred, already-owned, restore paths to avoid hidden bugs at SDK-integration time.

## Validation checklist

- `Common/IAP` has zero references to `Game.*` or `UnityEngine.Purchasing`.
- `GameInstaller` installs `IapInstaller` and the `Game`-side `IIapEntitlementStore`.
- Soft-currency flow through `IPurchaseService` is behavior-unchanged.
- Router correctly dispatches by `PriceDefinition.Product` presence.
- Buying RemoveAds on the mock: persists entitlement, fires both IAP and store completion channels, makes `AdService` return `Disabled` for interstitials, leaves rewarded ads functioning.
- Entitlement survives restart; Restore re-applies it after wiping runtime state.
- Already-owned attempt fails cleanly without double-grant; in-progress guard and timeout work.
- Debug tooling can force outcomes and revoke the entitlement.
- Analytics events fire via `IIapTelemetryListener`.
- No new enums where an asset-based key fits; no raw SKU strings outside `IapProduct` assets.
- `IPurchaseService` XML summary updated to explicitly say "in-game (soft-currency) purchases" to disambiguate from IAP.

