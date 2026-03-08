---
name: ""
overview: ""
todos:
  - id: ad-result
    content: Create AdResult readonly struct in Common/Ads/Data/
    status: pending
  - id: ad-placement
    content: Create AdPlacement ScriptableObject base + InterstitialPlacement, RewardedPlacement, BannerPlacement subclasses in Common/Ads/Data/
    status: pending
  - id: ad-settings
    content: Create AdSettings ScriptableObject (master kill switch, global timeout, default cooldown) in Common/Ads/Data/
    status: pending
  - id: ad-provider-interface
    content: Create IAdProvider interface in Common/Ads/Interfaces/
    status: pending
  - id: ad-service-interface
    content: Create IAdService interface in Common/Ads/Interfaces/
    status: pending
  - id: ad-telemetry-interface
    content: Create IAdTelemetryListener interface in Common/Ads/Interfaces/
    status: pending
  - id: ad-event-channels
    content: Create AdEventData, AdRequestedEventChannel, AdCompletedEventChannel, AdFailedEventChannel in Common/Ads/Events/
    status: pending
  - id: ad-service-impl
    content: Implement AdService (frequency gate, cooldown, auto-preload, timeout, telemetry dispatch) in Common/Ads/Services/
    status: pending
  - id: mock-ad-settings
    content: Create MockAdSettings ScriptableObject (simulated outcome, delay, sequenced outcomes, show fake popup flag) in Common/Ads/Data/
    status: pending
  - id: mock-ad-provider
    content: Implement MockAdProvider (configurable outcomes, delay simulation, outcome sequencing) in Common/Ads/Providers/
    status: pending
isProject: false
---

# Ad System - Phase 1: Base System (Common Assembly)

## Purpose

Build the entire provider-agnostic ad infrastructure in the **Common** assembly so it has zero dependency on game-specific code. This layer defines all contracts, data types, the core `AdService`, the mock provider, and event channels. It is reusable across any project that references Common.

Phase 2 (game integration) depends on this phase. Phase 3 (real providers + IAP) depends on both.

---

## Decisions & Rationale

### Why Common?

The ad system is infrastructure, not game logic. Placing it in Common:

- Keeps it decoupled from Game-specific types (store, flow, UI).
- Allows reuse if the Common assembly is shared across projects.
- Matches existing patterns: `IRemoteConfigService`, `EventChannel<T>`, `StateMachine` all live in Common.

### Unified AdService (Option A)

A single `AdService` class owns frequency gating, cooldown tracking, consent checks (stubbed for now), telemetry dispatch, and provider delegation. This mirrors `PurchaseService` in the Game assembly -- one focused service per concern. Decomposition into separate gate/consent/telemetry services would be premature for two ad formats and one frequency rule.

### No Consent Logic Yet

`IAdService` will accept an optional consent status check point, but no `IAdConsentProvider` interface or implementation in Phase 1. Consent will be added in Phase 3 alongside real providers. The `AdService` will simply treat consent as always-granted until then.

### Banner Support in Contracts

Banners are fundamentally different from interstitials/rewarded (persistent show/hide vs. fire-and-forget). The interfaces include banner methods from the start so providers don't need interface changes later.

### No Enums for Ad Format

Following project rules ("avoid enums"), ad format is represented via `AdPlacement` subclasses: `InterstitialPlacement`, `RewardedPlacement`, `BannerPlacement`. The calling code chooses the right `IAdService` method; the placement carries configuration. Provider implementations can use pattern matching on placement type.

---

## Contracts

### IAdProvider

Location: `Assets/Scripts/Common/Ads/Interfaces/IAdProvider.cs`

The SDK adapter contract. One implementation per ad SDK (or mock). Intentionally low-level -- no frequency/cooldown logic here.

```csharp
public interface IAdProvider
{
    UniTask InitializeAsync(CancellationToken ct = default);
    UniTask LoadAsync(AdPlacement placement, CancellationToken ct = default);
    bool IsLoaded(AdPlacement placement);
    UniTask<AdResult> ShowAsync(AdPlacement placement, CancellationToken ct = default);
    void HideBanner(AdPlacement placement);
    void Dispose();
}
```

### IAdService

Location: `Assets/Scripts/Common/Ads/Interfaces/IAdService.cs`

The facade that game code depends on. Encapsulates all policy decisions (cooldown, frequency, consent, availability).

```csharp
public interface IAdService
{
    bool IsAdAvailable(AdPlacement placement);
    bool CanShowAd(AdPlacement placement);
    UniTask<AdResult> ShowInterstitialAsync(AdPlacement placement, CancellationToken ct = default);
    UniTask<AdResult> ShowRewardedAsync(AdPlacement placement, CancellationToken ct = default);
    UniTask<AdResult> ShowBannerAsync(AdPlacement placement, CancellationToken ct = default);
    void HideBanner(AdPlacement placement);
    void ResetRunState();
}
```

- `IsAdAvailable`: provider has an ad loaded.
- `CanShowAd`: available AND passes frequency gate, cooldown, consent, and not disabled.
- `ResetRunState`: called at run/session start to clear once-per-run flags.

### IAdTelemetryListener

Location: `Assets/Scripts/Common/Ads/Interfaces/IAdTelemetryListener.cs`

Optional callback interface. `AdService` invokes these if a listener is injected. Game code implements this in Phase 2 (or Phase 3 with Firebase Analytics).

```csharp
public interface IAdTelemetryListener
{
    void OnAdRequested(AdPlacement placement);
    void OnAdShown(AdPlacement placement);
    void OnAdCompleted(AdPlacement placement, AdResult result);
    void OnAdFailed(AdPlacement placement, string error);
}
```

---

## Data Types

### AdResult

Location: `Assets/Scripts/Common/Ads/Data/AdResult.cs`

Mirrors the existing `PurchaseResult` pattern.

```csharp
public readonly struct AdResult
{
    public bool Success { get; }
    public string FailureReason { get; }

    // Static factories
    public static AdResult Completed => ...;
    public static AdResult Skipped => ...;       // User dismissed
    public static AdResult Unavailable => ...;   // No ad loaded / offline
    public static AdResult OnCooldown => ...;    // Frequency gate blocked
    public static AdResult Disabled => ...;      // Master kill switch
    public static AdResult Timeout => ...;       // Provider exceeded deadline
    public static AdResult Failed(string reason) => ...;
}
```

### AdPlacement (base + subclasses)

Location: `Assets/Scripts/Common/Ads/Data/AdPlacement.cs`

ScriptableObject. Each ad surface in the game gets its own asset instance. Configuration values can be overridden by Remote Config in Phase 2.

```csharp
public abstract class AdPlacement : ScriptableObject
{
    [field: SerializeField] public string PlacementKey { get; private set; }
    [field: SerializeField] public float CooldownSeconds { get; private set; }
    [field: SerializeField] public int MinAttemptsBetweenAds { get; private set; }
    [field: SerializeField] public bool OncePerRun { get; private set; }
}

public class InterstitialPlacement : AdPlacement { }

public class RewardedPlacement : AdPlacement { }
// Reward type and amount live in game-side config (Phase 2) — ad system is reward-agnostic.

public class BannerPlacement : AdPlacement { }
```

### AdSettings

Location: `Assets/Scripts/Common/Ads/Data/AdSettings.cs`

Global configuration ScriptableObject.

```csharp
public class AdSettings : ScriptableObject
{
    [field: SerializeField] public bool AdsEnabled { get; private set; }
    [field: SerializeField] public bool UseMockProvider { get; private set; }
    [field: SerializeField] public float GlobalTimeoutSeconds { get; private set; }
    [field: SerializeField] public float DefaultCooldownSeconds { get; private set; }
}
```

### MockAdSettings

Location: `Assets/Scripts/Common/Ads/Data/MockAdSettings.cs`

```csharp
public class MockAdSettings : ScriptableObject
{
    [field: SerializeField] public AdMockOutcome DefaultOutcome { get; private set; }
    [field: SerializeField] public float SimulatedDelaySeconds { get; private set; }
    [field: SerializeField] public List<AdMockOutcome> SequencedOutcomes { get; private set; }
}
```

`AdMockOutcome` is a simple serializable struct (not an enum) with a string `Type` field and optional `FailureReason`. Predefined static factories: `AdMockOutcome.Completed`, `.Skipped`, `.Failed(reason)`, `.Unavailable`, `.Timeout`.

---

## Event Channels

Location: `Assets/Scripts/Common/Ads/Events/`

Following the existing `EventChannel<T>` pattern:

### AdEventData

```csharp
public readonly struct AdEventData
{
    public AdPlacement Placement { get; }
    public AdResult Result { get; }

    public AdEventData(AdPlacement placement, AdResult result) { ... }
}
```

### Channels

```csharp
public class AdRequestedEventChannel : EventChannel<AdEventData> { }
public class AdCompletedEventChannel : EventChannel<AdEventData> { }
public class AdFailedEventChannel : EventChannel<AdEventData> { }
```

These are ScriptableObject assets created in Unity Editor (Phase 2 creates the actual `.asset` files).

---

## AdService Implementation

Location: `Assets/Scripts/Common/Ads/Services/AdService.cs`

### Constructor Dependencies

- `IAdProvider` -- the active provider (mock or real).
- `AdSettings` -- global config.
- `AdRequestedEventChannel`, `AdCompletedEventChannel`, `AdFailedEventChannel` -- event dispatch.
- `IAdTelemetryListener` (optional, nullable) -- analytics hook.

### Internal State

- `Dictionary<string, float>` for last-shown timestamps per placement key (cooldown tracking).
- `Dictionary<string, int>` for attempt counters per placement key (frequency gating).
- `HashSet<string>` for once-per-run used placements.

### Flow (ShowRewardedAsync example)

```
1. Check AdSettings.AdsEnabled -> if false, return AdResult.Disabled
2. Check cooldown: Time.realtimeSinceStartup - lastShown[key] < placement.CooldownSeconds -> return AdResult.OnCooldown
3. Check frequency: attemptCounter[key] < placement.MinAttemptsBetweenAds -> return AdResult.OnCooldown (but increment counter)
4. Check once-per-run: usedPlacements.Contains(key) -> return AdResult.OnCooldown
5. Check provider: !provider.IsLoaded(placement) -> attempt LoadAsync, if fails return AdResult.Unavailable
6. Raise AdRequestedEventChannel
7. Invoke telemetry.OnAdRequested
8. Create CancellationTokenSource with GlobalTimeoutSeconds
9. var result = await provider.ShowAsync(placement, cts.Token)
10. If result.Success:
      - Record timestamp, mark once-per-run if applicable, reset attempt counter
      - Raise AdCompletedEventChannel
      - Invoke telemetry.OnAdCompleted
      - Fire-and-forget: provider.LoadAsync (preload next)
11. Else:
      - Raise AdFailedEventChannel
      - Invoke telemetry.OnAdFailed
12. Return result
```

### ShowInterstitialAsync

Same flow but no reward validation. Interstitials are fire-and-forget from the game's perspective -- `AdService` awaits completion but the caller can choose to not await.

### ShowBannerAsync / HideBanner

Banner show delegates to provider. No cooldown/frequency for banners (they're persistent). `HideBanner` delegates directly.

### ResetRunState

Clears the once-per-run set. Called by game code at session/run start.

### IncrementAttemptCounter

Public method for game code to call when a relevant game event occurs (e.g., stage attempt) without showing an ad. This separates "counting" from "showing".

---

## MockAdProvider Implementation

Location: `Assets/Scripts/Common/Ads/Providers/MockAdProvider.cs`

### Behavior

- Implements `IAdProvider`.
- `InitializeAsync`: no-op, returns immediately.
- `LoadAsync`: simulates delay from `MockAdSettings.SimulatedDelaySeconds`, then marks placement as "loaded".
- `IsLoaded`: returns tracked loaded state.
- `ShowAsync`:
  1. Waits `SimulatedDelaySeconds` (simulates ad display time).
  2. Determines outcome: if `SequencedOutcomes` has entries, cycles through them (advancing an internal index); otherwise uses `DefaultOutcome`.
  3. Converts `AdMockOutcome` to `AdResult` and returns.
  4. If a `MockAdPopupRequested` callback is set (wired in Phase 2), invokes it instead of auto-resolving, allowing manual QA interaction.
- `HideBanner`: marks banner as hidden.
- `Dispose`: clears state.

### No UI in Phase 1

The mock provider is pure logic. The optional fake popup UI (`MockAdPopup`) is created in Phase 2 because it depends on Game assembly types (`BasePopup`, `MonoBehaviour` UI).

The provider exposes a `Func<AdPlacement, UniTaskCompletionSource<AdResult>>` callback that Phase 2 can hook into to show a popup and manually resolve the result.

---

## File Structure

```
Assets/Scripts/Common/Ads/
    Interfaces/
        IAdService.cs
        IAdProvider.cs
        IAdTelemetryListener.cs
    Data/
        AdResult.cs
        AdPlacement.cs            (base class)
        InterstitialPlacement.cs
        RewardedPlacement.cs
        BannerPlacement.cs
        AdSettings.cs
        MockAdSettings.cs
        AdMockOutcome.cs
    Services/
        AdService.cs
    Providers/
        MockAdProvider.cs
    Events/
        AdEventData.cs
        AdRequestedEventChannel.cs
        AdCompletedEventChannel.cs
        AdFailedEventChannel.cs
```

Total: ~16 files. No changes to existing files in this phase.

---

## Risks & Mitigations


| Risk                                                       | Mitigation                                                                                                                                          |
| ---------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `AdPlacement` subclass proliferation if more formats added | Only 3 subclasses now; unlikely to grow beyond 4-5. If it does, refactor to composition.                                                            |
| `AdService` grows too large                                | Internal logic is cleanly separated into private methods (cooldown, frequency, consent). Extraction to helper classes is straightforward if needed. |
| ScriptableObject event channels require asset creation     | Documented in Phase 2; Phase 1 is code-only.                                                                                                        |
| `MockAdProvider` callback pattern for popup is indirect    | Keeps Common assembly free of UI dependencies while allowing Phase 2 to hook in cleanly.                                                            |


---

## Acceptance Criteria

- All interfaces compile with no dependencies outside Common assembly (UniTask, VContainer, UnityEngine only).
- `AdService` unit-testable with a mock `IAdProvider` (no Unity runtime required for core logic tests).
- `MockAdProvider` returns configurable outcomes with simulated delays.
- Cooldown gating blocks ads within the cooldown window and allows after.
- Frequency gating blocks until N attempts reached, then resets after showing.
- Once-per-run flag prevents second show; `ResetRunState` clears it.
- Event channels fire for requested/completed/failed outcomes.
- `AdResult.Disabled` returned when `AdSettings.AdsEnabled` is false.
- Timeout returns `AdResult.Timeout` when provider exceeds deadline.

