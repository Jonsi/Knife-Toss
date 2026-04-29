# IAP System — Unity Setup Guide

---

## Step 1 — Create ScriptableObject Assets

Right-click in Project → Create:

| Menu Path | Asset Name |
|-----------|-----------|
| Knife Toss/IAP/IAP Product | `IapProduct_RemoveAds` |
| Knife Toss/IAP/IAP Catalog | `IapCatalog` |
| Knife Toss/IAP/IAP Settings | `IapSettings` |
| Knife Toss/Events/IAP/Purchase Started | `IapPurchaseStartedEventChannel` |
| Knife Toss/Events/IAP/Purchase Completed | `IapPurchaseCompletedEventChannel` |
| Knife Toss/Events/IAP/Purchase Failed | `IapPurchaseFailedEventChannel` |
| Knife Toss/Events/IAP/Purchase Restored | `IapPurchaseRestoredEventChannel` |

---

## Step 2 — Configure Assets

**`IapProduct_RemoveAds`**
- `Product Key` → `remove_ads`
- `Store Product Id` → your Apple/Google SKU (e.g. `com.yourcompany.removeads`)
- `Is Consumable` → off

**`IapCatalog`**
- Add `IapProduct_RemoveAds` to the Products list

**`IapSettings`**
- `Iap Enabled` → on
- `Global Timeout Seconds` → `30`

---

## Step 3 — Configure StoreCatalog

Open your `StoreCatalog` asset → **Iap Entries** → Add entry:
- `Price` → set `Iap Product` to `IapProduct_RemoveAds` (leave currency fields at 0/None)
- `Reward` → select `RemoveAdsReward` from the dropdown

---

## Step 4 — Add IapInstaller to the persistent scene

1. In your persistent scene (where `GameInstaller` lives), select the `GameInstaller` GameObject
2. Add component: `IapInstaller`
3. Fill in its fields:
   - `Iap Catalog` → `IapCatalog`
   - `Iap Settings` → `IapSettings`
   - `Purchase Started/Completed/Failed/Restored Channel` → the four event channel assets
4. Drag the `IapInstaller` component reference into **`GameInstaller → IAP → Iap Installer`**
5. Drag `StoreCatalog` into **`GameInstaller → Services → Store Catalog`**

---

## Step 5 — (TEST_ENABLED only) Mock setup

> Skip this for production builds.

Create additional asset: `Knife Toss/IAP/Mock IAP Settings` → `MockIapSettings`
- `Default Outcome` → Success
- `Simulated Delay Seconds` → `0.5`

In `GameInstaller`:
- `Mock Iap Settings` → `MockIapSettings`
- `Mock Iap Popup Bridge` → (optional) your `MockIapPopup` prefab instance in scene

---

## Step 6 — Verify

Hit Play. In the SRDebugger **6. IAP** tab:
- `Iap Enabled` should show `true`
- `Remove Ads Owned` should show `false`
- Press **Force: Success** then trigger a purchase — `Remove Ads Owned` flips to `true`
- Interstitial ads should now return `Disabled`
