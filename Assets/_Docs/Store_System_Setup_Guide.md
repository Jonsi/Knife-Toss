# Store System Setup Guide

This guide explains how to set up the new Store system and how to create purchasable items in Unity.

## 1. What Was Added

Store scripts were added under:

- `Assets/Scripts/Game/Store/Attributes`
- `Assets/Scripts/Game/Store/Data`
- `Assets/Scripts/Game/Store/Events`
- `Assets/Scripts/Game/Store/Interfaces`
- `Assets/Scripts/Game/Store/Rewards`
- `Assets/Scripts/Game/Store/Services`
- `Assets/Scripts/Game/Installers/StoreInstaller.cs`
- `Assets/Scripts/Editor/Drawers/SerializeReferenceDropdownDrawer.cs`

## 2. Create Required Assets

Create these ScriptableObject assets in Unity:

1. `StoreCatalog`
   - Menu: `Create > Knife Toss > Store > Store Catalog`
2. Purchase event channels:
   - `PurchaseStartedEventChannel`
     - Menu: `Create > Knife Toss > Events > Purchase Started Event Channel`
   - `PurchaseCompletedEventChannel`
     - Menu: `Create > Knife Toss > Events > Purchase Completed Event Channel`
   - `PurchaseFailedEventChannel`
     - Menu: `Create > Knife Toss > Events > Purchase Failed Event Channel`

Recommended folder:

- `Assets/Configurations/Store/StoreCatalog.asset`
- `Assets/Configurations/EventChannels/Store/PurchaseStartedEventChannel.asset`
- `Assets/Configurations/EventChannels/Store/PurchaseCompletedEventChannel.asset`
- `Assets/Configurations/EventChannels/Store/PurchaseFailedEventChannel.asset`

## 3. Register StoreInstaller in a Scene

`StoreInstaller` exists but is not auto-wired yet. Add it from your scene `LifetimeScope` (for example `StageInstaller` or a dedicated Store scene installer).

Example pattern (inside `Configure`):

```csharp
Install(builder, new StoreInstaller(
    _StoreCatalog,
    _PurchaseStartedEventChannel,
    _PurchaseCompletedEventChannel,
    _PurchaseFailedEventChannel));
```

You need serialized fields on the scene installer for:

- `StoreCatalog _StoreCatalog`
- `PurchaseStartedEventChannel _PurchaseStartedEventChannel`
- `PurchaseCompletedEventChannel _PurchaseCompletedEventChannel`
- `PurchaseFailedEventChannel _PurchaseFailedEventChannel`

After this, `IPurchaseService` is available for injection.

## 4. How to Create Store Items

Open your `StoreCatalog` asset and configure entries in:

- `Power Ups`
- `Currency Packs`
- `Skins`

Each `PurchasableEntry` has:

- `Display Name`
- `Description`
- `Icon`
- `Price`:
  - `Currency Id` (example: `coins`)
  - `Amount`
- `Reward` (SerializeReference dropdown)

### Reward Types Available

From the `Reward` dropdown you can choose:

1. `PowerUpReward`
   - `Config`: assign a `PowerUpConfigBase` asset (for example `SuperKnifeConfig`)
   - `Quantity`: amount to add to inventory
2. `CurrencyReward`
   - `Currency Id`: currency to grant
   - `Amount`: amount to grant

### Price Rules

A purchase is treated as free if:

- `Amount <= 0`, or
- `Currency Id` is empty

Otherwise the service checks affordability and spends currency before granting reward.

## 5. Example Item Configurations

### Example A: Buy Power-Up with Coins

- Display Name: `Super Knife x1`
- Price:
  - Currency Id: `coins`
  - Amount: `20`
- Reward: `PowerUpReward`
  - Config: `SuperKnifeConfig`
  - Quantity: `1`

### Example B: Currency Pack Purchase

- Display Name: `Orange Pack`
- Price:
  - Currency Id: `coins`
  - Amount: `100`
- Reward: `CurrencyReward`
  - Currency Id: `orange`
  - Amount: `10`

### Example C: Free Reward

- Display Name: `Daily Gift`
- Price:
  - Currency Id: ``
  - Amount: `0`
- Reward: `CurrencyReward`
  - Currency Id: `coins`
  - Amount: `25`

## 6. How UI Should Use the System

Inject into your Store UI controller:

- `StoreCatalog`
- `IPurchaseService`
- Optional purchase event channels for feedback

Basic flow:

1. Render catalog entries.
2. On buy click: `await _PurchaseService.TryPurchaseAsync(entry)`.
3. Use returned `PurchaseResult` and/or event channels to show success/failure/loading.
4. Disable buy buttons while `IsPurchaseInProgress` is true.

## 7. Events Raised During Purchase

- `PurchaseStartedEventChannel` when purchase begins
- `PurchaseCompletedEventChannel` when reward grant succeeds
- `PurchaseFailedEventChannel` when validation/spend/grant fails

Failure reason is provided in `PurchaseFailedEventData.Reason`.

## 8. Validation Checklist

Use this quick pass after setup:

1. Purchase succeeds when enough balance exists.
2. Purchase fails with insufficient funds and does not grant reward.
3. Free item grants without balance check.
4. Currency is refunded if reward grant throws an exception.
5. Only one purchase runs at a time (`IsPurchaseInProgress`).
6. Purchase events fire and UI reacts correctly.

## 9. Troubleshooting

1. Reward dropdown is missing in inspector:
   - Confirm field is `[SerializeReference]` with `[SerializeReferenceDropdown]`.
   - Confirm file exists: `Assets/Scripts/Editor/Drawers/SerializeReferenceDropdownDrawer.cs`.
2. Purchase service cannot be resolved:
   - Confirm `StoreInstaller` is installed in active scene scope.
3. Purchase always fails affordability:
   - Verify `Price.CurrencyId` matches an ID used by `CurrencyTypeCatalog`.
4. Power-up reward does nothing:
   - Verify `PowerUpReward.Config` is assigned and has a non-empty `InventoryKey`.

