---
plan: "Ad System - Phase 3: Real Providers + Remove Ads IAP"
status: pending
dependency: ad_system_phase2_game_integration
todos:
  - id: real-provider
    content: "Implement first real IAdProvider (e.g., AdMobProvider or UnityAdsProvider)"
    status: pending
  - id: provider-selection
    content: "Add provider selection logic to AdInstaller (mock vs real, configurable via AdSettings)"
    status: pending
  - id: consent-interface
    content: "Create IAdConsentProvider interface and wire into AdService consent check"
    status: pending
  - id: consent-impl
    content: "Implement platform consent provider (ATT for iOS, Google UMP for Android)"
    status: pending
  - id: consent-ui
    content: "Create consent popup UI (custom in-game or native platform dialog)"
    status: pending
  - id: iap-remove-ads
    content: "Implement Remove Ads IAP purchase and wire into AdService.CanShowAd gate"
    status: pending
  - id: ad-free-persistence
    content: "Persist ad-free status in player data and check on startup"
    status: pending
  - id: telemetry-impl
    content: "Implement IAdTelemetryListener with Firebase Analytics"
    status: pending
  - id: retry-backoff
    content: "Add exponential backoff retry logic for failed ad loads in AdService"
    status: pending
  - id: mediation
    content: "Evaluate and integrate ad mediation (IronSource / AppLovin MAX) if warranted"
    status: pending
---

# Ad System - Phase 3: Real Providers, Consent, Remove Ads IAP

## Purpose

Replace the mock provider with real ad SDK(s), add consent/privacy compliance, implement the "Remove Ads" IAP, wire telemetry to Firebase Analytics, and harden the system for production. This phase depends on both Phase 1 (base system) and Phase 2 (game integration).

---

## 1. Real Ad Provider Implementation

### Provider Selection

`AdInstaller` gains provider selection logic based on `AdSettings`:

```csharp
if (_AdSettings.UseMockProvider)
    builder.Register<MockAdProvider>(Lifetime.Singleton).As<IAdProvider>();
else
    builder.Register<RealAdProvider>(Lifetime.Singleton).As<IAdProvider>();
```

The concrete provider class (e.g., `AdMobProvider`, `UnityAdsProvider`, or a mediation wrapper) implements `IAdProvider` identically to `MockAdProvider` but delegates to the real SDK.

### Provider Implementation Pattern

Location: `Assets/Scripts/Game/Ads/Providers/` (or a dedicated assembly if the SDK has its own asmdef)

```csharp
public class AdMobProvider : IAdProvider
{
    public async UniTask InitializeAsync(CancellationToken ct)
    {
        // MobileAds.Initialize() wrapped in UniTask
    }

    public async UniTask LoadAsync(AdPlacement placement, CancellationToken ct)
    {
        // Pattern-match placement type -> load interstitial/rewarded/banner via SDK
    }

    public bool IsLoaded(AdPlacement placement) { ... }

    public async UniTask<AdResult> ShowAsync(AdPlacement placement, CancellationToken ct)
    {
        // Show ad, await SDK callbacks, convert to AdResult
    }

    public void HideBanner(AdPlacement placement) { ... }
    public void Dispose() { ... }
}
```

**Key considerations:**
- SDK callbacks (OnAdLoaded, OnAdFailedToLoad, OnAdClosed, OnUserEarnedReward) are bridged to `UniTaskCompletionSource<AdResult>` to fit the async pattern.
- Thread safety: SDK callbacks often come on the main thread but the `UniTask` may be awaited on a different context. Use `UniTask.SwitchToMainThread()` where needed.
- The provider should handle SDK-specific initialization (ad unit IDs, test mode flags) via a provider-specific ScriptableObject config.

### Multiple Providers / Mediation

If mediation is needed (IronSource, AppLovin MAX), the mediation SDK becomes the single `IAdProvider`. It internally manages waterfalls across networks. From the ad system's perspective, nothing changes -- the `IAdProvider` interface is the same.

If multiple non-mediated providers are needed (unlikely), a `CompositeAdProvider` can delegate to different providers per placement type.

---

## 2. Consent / Privacy Flow

### IAdConsentProvider Interface

Location: `Assets/Scripts/Common/Ads/Interfaces/IAdConsentProvider.cs` (added to Common in this phase)

```csharp
public interface IAdConsentProvider
{
    ConsentStatus CurrentStatus { get; }
    UniTask<ConsentStatus> RequestConsentAsync();
    bool IsConsentRequired { get; }
}
```

`ConsentStatus` is a readonly struct (not an enum, per project rules) with static factories: `ConsentStatus.Granted`, `ConsentStatus.Denied`, `ConsentStatus.Unknown`, `ConsentStatus.NotRequired`.

### AdService Consent Integration

`AdService` constructor accepts an optional `IAdConsentProvider`. If provided:
- `CanShowAd` checks `consentProvider.CurrentStatus` -- only allows ads if granted or not required.
- On first ad request where status is `Unknown`, calls `RequestConsentAsync()` and caches the result.
- Returns `AdResult.ConsentDenied` if consent was denied.

### Platform Implementations

**iOS (ATT):**
- `IosConsentProvider` wraps `ATTrackingManager.requestTrackingAuthorization`.
- Maps ATT status to `ConsentStatus`.
- Must be called before first ad load (Apple requirement).

**Android (Google UMP):**
- `AndroidConsentProvider` wraps Google's User Messaging Platform SDK.
- Handles consent form display and status tracking.

**Editor/Mock:**
- `MockConsentProvider` always returns `ConsentStatus.Granted` (for development).

### Consent UI Decision

Two approaches (decided at implementation time based on platform requirements):
1. **Native platform dialog** -- required for ATT on iOS, standard UMP on Android. Simpler, but not customizable.
2. **Custom in-game popup** -- matches game art style, but ATT still requires the native dialog on iOS.

Recommendation: Use native dialogs for regulatory compliance, with an optional pre-consent explanation popup in-game to improve opt-in rates.

---

## 3. Remove Ads IAP

### Architecture

The "Remove Ads" purchase is an IAP product that, when owned, causes `AdService.CanShowAd` to return `false` for all non-rewarded placements. Rewarded ads should remain available since they are player-initiated and provide value.

### Implementation

**AdFreeStatus** (new, `Assets/Scripts/Game/Ads/Data/AdFreeStatus.cs`):
- Simple data class persisted in player data (via existing `PlayerDataService` / `FilePlayerDataStore`).
- `bool IsAdFree` property.
- Serialized alongside other player data.

**AdService integration:**
- `AdService` constructor accepts an `IAdFreeStatusProvider` interface.
- `CanShowAd` checks: if `IsAdFree && placement is not RewardedPlacement`, return false.
- `ShowInterstitialAsync` and `ShowBannerAsync` return `AdResult.Disabled` if ad-free.
- `ShowRewardedAsync` still works (player-initiated, opt-in).

**IAdFreeStatusProvider:**

```csharp
public interface IAdFreeStatusProvider
{
    bool IsAdFree { get; }
    void SetAdFree(bool value);
}
```

Implemented by a service that reads/writes to player data. `SetAdFree(true)` is called when the IAP purchase is verified.

**IAP integration:**
- When IAP system is built, the "Remove Ads" product completion handler calls `IAdFreeStatusProvider.SetAdFree(true)`.
- On app startup, check the IAP receipt / player data for existing purchase and restore status.
- `AdService` hides banners and stops interstitials immediately when ad-free status changes.

**Store UI:**
- Add a "Remove Ads" button/entry in the store.
- Button hidden/disabled when already purchased (`IsAdFree`).
- Uses the existing `PurchaseService` / future `IAPPurchaseService` flow.

### Restore Purchases

On iOS, "Restore Purchases" must be available. When triggered:
- Check for existing "Remove Ads" receipt.
- If found, call `SetAdFree(true)`.
- Hide all non-rewarded ad surfaces.

---

## 4. Firebase Analytics Telemetry

### Implementation

Location: `Assets/Scripts/Game/Ads/Services/FirebaseAdTelemetryListener.cs`

Implements `IAdTelemetryListener`:

```csharp
public class FirebaseAdTelemetryListener : IAdTelemetryListener
{
    public void OnAdRequested(AdPlacement placement)
    {
        FirebaseAnalytics.LogEvent("ad_requested", "placement", placement.PlacementKey);
    }

    public void OnAdShown(AdPlacement placement)
    {
        FirebaseAnalytics.LogEvent("ad_shown", "placement", placement.PlacementKey);
    }

    public void OnAdCompleted(AdPlacement placement, AdResult result)
    {
        FirebaseAnalytics.LogEvent("ad_completed",
            new Parameter("placement", placement.PlacementKey),
            new Parameter("success", result.Success ? 1 : 0));
    }

    public void OnAdFailed(AdPlacement placement, string error)
    {
        FirebaseAnalytics.LogEvent("ad_failed",
            new Parameter("placement", placement.PlacementKey),
            new Parameter("error", error));
    }
}
```

### Registration

In `AdInstaller`, register the telemetry listener:
```csharp
builder.Register<FirebaseAdTelemetryListener>(Lifetime.Singleton).As<IAdTelemetryListener>();
```

### Revenue Events

If using AdMob or mediation, impression-level revenue data is available. Add:
```csharp
void OnAdRevenue(AdPlacement placement, AdRevenueData revenue);
```
to `IAdTelemetryListener` and log to Firebase for ROAS tracking.

---

## 5. Production Hardening

### Retry Logic with Exponential Backoff

`AdService` gains retry logic for failed ad loads:
- On `LoadAsync` failure, schedule retry with backoff: 2s, 4s, 8s, 16s, max 60s.
- Cap at 3-5 retry attempts per placement before giving up until next trigger.
- Reset retry state when app returns from background or network connectivity changes.

### Offline Behavior

- `AdService` detects network status via `Application.internetReachability`.
- When offline: `CanShowAd` returns `false`, ad buttons hidden, no load attempts.
- When connectivity restored: trigger preload for all active placements.

### Error Reporting

- Ad failures logged via existing error reporting infrastructure.
- Excessive failures for a placement trigger a circuit breaker (stop trying for N minutes).

---

## 6. File Structure (Phase 3 additions)

```
Assets/Scripts/Common/Ads/
    Interfaces/
        IAdConsentProvider.cs     (new)
        IAdFreeStatusProvider.cs  (new)
    Data/
        ConsentStatus.cs          (new)

Assets/Scripts/Game/Ads/
    Providers/
        AdMobProvider.cs          (or UnityAdsProvider, or MediationProvider)
        ProviderConfig.cs         (SDK-specific ScriptableObject config)
    Services/
        FirebaseAdTelemetryListener.cs
        AdFreeStatusService.cs
    Consent/
        IosConsentProvider.cs
        AndroidConsentProvider.cs
        MockConsentProvider.cs
```

**Modified files:**
- `AdService.cs` -- add consent check, ad-free check, retry/backoff logic
- `AdInstaller.cs` -- provider selection, consent registration, telemetry registration
- `GameInstaller.cs` -- additional serialized fields if needed
- `PlayerDataService.cs` / player data model -- add ad-free status field

---

## Rollout Sequence

### 3a: First Real Provider
1. Choose SDK (AdMob, Unity Ads, or mediation).
2. Add SDK package to project.
3. Implement `IAdProvider` for chosen SDK.
4. Update `AdInstaller` with provider selection.
5. Test on device with test ad unit IDs.
6. Verify all three formats (interstitial, rewarded, banner) work.

### 3b: Consent Flow
1. Implement `IAdConsentProvider` interface.
2. Build platform-specific implementations.
3. Wire into `AdService`.
4. Test on iOS (ATT) and Android (UMP).
5. Verify ads are blocked when consent denied.

### 3c: Remove Ads IAP
1. Implement `IAdFreeStatusProvider` + persistence.
2. Wire into `AdService.CanShowAd`.
3. Add store entry for "Remove Ads".
4. Test purchase -> ads disappear.
5. Test restore purchases.

### 3d: Telemetry + Hardening
1. Implement `FirebaseAdTelemetryListener`.
2. Add retry/backoff logic.
3. Add offline detection.
4. Verify analytics events in Firebase console.

---

## Acceptance Criteria

- [ ] Real ads load and display on test devices (Android and iOS).
- [ ] Consent dialog shown on first launch (iOS ATT / Android UMP).
- [ ] Ads blocked when consent denied; no SDK calls made.
- [ ] "Remove Ads" IAP purchase disables interstitials and banners.
- [ ] Rewarded ads still available after "Remove Ads" purchase.
- [ ] "Restore Purchases" restores ad-free status on iOS.
- [ ] Firebase Analytics receives ad_requested, ad_shown, ad_completed, ad_failed events.
- [ ] Failed ad loads retry with exponential backoff.
- [ ] Offline: ad buttons hidden, no errors, graceful degradation.
- [ ] Switching between mock and real provider works via AdSettings toggle.
