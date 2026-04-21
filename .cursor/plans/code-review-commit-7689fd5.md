# Code Review — Commit `7689fd5`

| # | Severity | Location | Issue |
|---|----------|----------|-------|
| 1 | Medium | `IapService.PurchaseAsync` | `OperationCanceledException` always returns `Timeout`, not `Cancelled` |
| 2 | Medium | `IapSettings` | `GlobalTimeoutSeconds = 0` kills all purchases silently |
| 3 | Medium | `GameInstaller` / `StoreInstaller` | `KnifeSkinService` registered in both scopes → two independent instances |
| 4 | Low→High | `StorePurchaseRouter` IAP path | Post-IAP `GrantAsync` failure: charged, reward not granted, no recovery |
| 5 | Low | `IapService.RestorePurchasesAsync` | No `IsPurchaseInProgress` guard; purchase + restore can run concurrently |
| 6 | Low | `StoreMenuController` | RemoveAds purchase has no visual success feedback |
| 7 | Low | `IapService.EnsureInitializedAsync` | Double-init possible if two concurrent calls race before `_Initialized` is set |
| 8 | Medium | `RemoveAdsReward.IsOwned` | Empty `catch {}` hides DI misconfig; silently returns false |
| 9 | Medium | `IapSettings.UseMockProvider` | Field exists but is never read — dead/misleading config |
| 10 | Medium | `IapPurchaseResult` | `Purchased` and `Restored` are structurally identical — analytics cannot distinguish them |
| 11 | Medium | `MainMenuRemoveAdsFlow`, `IapAdEntitlementGate`, `IapCheatsTab` | Dual product source (`RemoveAdsReward.Product ?? entry.Price.IapProduct`) duplicated in 3 places |
| 12 | Low | `StoreMenuController` | IAP entries silently mixed into power-up bindings |
| 13 | Low | `GameInstaller` | Null check for `_StoreCatalog` at line 129 is dead code post-validation |
| 14 | Low | `AdService.ShowAdAsync` | Production `Debug.Log` with `new StackTrace()` on every ad show |
| 15 | Low | `RewardClaimTracker.cs` | Unused `using System.Linq;` |
| 16 | Low | `IapAdEntitlementGate` | Catalog scan on every interstitial check; could be cached |
| 17 | Low | `MockIapProvider` | `HashSet<IapProduct>` uses reference equality; `ProductKey` string equality is safer |
| 18 | Low | `MainMenuRemoveAdsFlow.TryPurchaseAsync` | No `CancellationToken` passed to `PurchaseAsync` |
