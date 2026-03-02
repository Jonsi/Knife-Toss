# Store System Setup Guide

This guide only covers the required setup steps, in the order they should be done.

## 1. Create Core Assets First

Create these ScriptableObjects before touching the scene:

1. `StoreCatalog`
   - Menu: `Create > Knife Toss > Store > Store Catalog`
2. `PurchaseStartedEventChannel`
   - Menu: `Create > Knife Toss > Events > Purchase Started Event Channel`
3. `PurchaseCompletedEventChannel`
   - Menu: `Create > Knife Toss > Events > Purchase Completed Event Channel`
4. `PurchaseFailedEventChannel`
   - Menu: `Create > Knife Toss > Events > Purchase Failed Event Channel`

Recommended folders:

- `Assets/Configurations/Store/StoreCatalog.asset`
- `Assets/Configurations/EventChannels/Store/`

## 2. Create Knife Skin Configs

Create a `KnifeSkinConfig` asset for each purchasable skin before configuring store entries.

Each `KnifeSkinConfig` needs:

- `Id` (stable unique id, e.g. `fire_blade`)
- `Icon`

The icon for a skin should come from `KnifeSkinConfig`.
Only the `Id` is saved to player data when the skin is unlocked.

## 3. Configure Store Items In StoreCatalog

Open `StoreCatalog` and fill these sections:

- `Skins`
- `PowerUps`

Each item (`PurchasableEntry`) needs:

- `Price`
  - `Currency` (`CurrencyType` enum, not a string)
  - `Amount`
- `Reward`

Reward types currently available:

1. `PowerUpReward`
   - Assign `Config`
   - Set `Quantity`
2. `CurrencyReward`
   - Set `Currency`
   - Set `Amount`
3. `KnifeSkinReward`
   - Assign `KnifeSkinConfig`
   - Optional: enable `AutoEquipOnPurchase`

Do this before creating the scene so the UI has real data to bind to.

## 4. Create UI Prefabs

Create the card prefabs directly.

The price pill is not a separate prefab or component.
It is a serializable nested binding inside `StoreCardView`.

### 4.1 KnifeSkinStoreCardView

Create a knife skin card prefab with:

- visual art/layout you want
- a price pill area inside the prefab
  - `AmountText`
- `BuyButton`
- `IconImage`
- `ShadowImage` (back/shadow image)
- `KnifeSkinStoreCardView` component

`KnifeSkinStoreCardView` inherits from the abstract base `StoreCardView`.

Assign:

- `PricePill.AmountText`
- `IconImage`
- `ShadowImage`
- `BuyButton`

Save as:

- `Assets/Prefabs/UI/Store/KnifeSkinStoreCardView.prefab`

### 4.2 PowerUpStoreCardView

Create a power-up card prefab with:

- visual art/layout you want
- a price pill area inside the prefab
  - `AmountText`
- `BuyButton`
- `IconImage`
- `ShadowImage` (back/shadow image)
- `RewardAmountText` (for the granted amount, e.g. `+5`)
- `PowerUpStoreCardView` component

`PowerUpStoreCardView` inherits from the abstract base `StoreCardView`.

Assign:

- `PricePill.AmountText`
- `IconImage`
- `ShadowImage`
- `RewardAmountText`
- `BuyButton`

Save as:

- `Assets/Prefabs/UI/Store/PowerUpStoreCardView.prefab`

Important:

- The card prefab owns the image references.
- `KnifeSkinStoreCardView` sets the images from `KnifeSkinReward.Config.Icon`.
- `PowerUpStoreCardView` sets the images from `PowerUpReward.Config.Icon`.
- `PowerUpStoreCardView` sets `RewardAmountText` from `PowerUpReward.Quantity`.

## 5. Build The Store Scene

After assets and prefabs exist, create the Store scene.

1. Create or open `Assets/Scenes/Store.unity`
2. Add a root object with `StoreSceneInstaller`
3. Add a `Canvas`
4. Create these scene elements:
   - `BalanceChip` with `StoreBalanceChipView`
   - `KnifeSkinsContentRoot`
   - `PowerUpsContentRoot`
   - `CloseButton`
   - `StoreMenuController` component

Recommended structure:

- `StoreRoot`
  - `Header`
    - `BalanceChip`
  - `KnifeSkinsSection`
    - `KnifeSkinsContentRoot`
  - `PowerUpsSection`
    - `PowerUpsContentRoot`
  - `BottomActions`
    - `CloseButton`

## 6. Wire The Scene

### 6.1 StoreSceneInstaller

Assign:

- `StoreCatalog`
- `PurchaseStartedEventChannel`
- `PurchaseCompletedEventChannel`
- `PurchaseFailedEventChannel`
- `StoreMenuController`

### 6.2 StoreMenuController

Assign:

- `KnifeSkinCardPrefab`
- `KnifeSkinsContentRoot`
- `PowerUpCardPrefab`
- `PowerUpsContentRoot`
- `CloseButton`

### 6.3 StoreBalanceChipView

Assign:

- `BalanceText`
- Optional `Currency`

If `Currency` is `None`, it uses the first currency from `CurrencyTypeCatalog`.

## 7. How It Works At Runtime

Once wired:

1. `StoreSceneInstaller` registers the store services and controller.
2. `StoreMenuController` instantiates:
   - knife skin cards from `StoreCatalog.Skins`
   - power-up cards from `StoreCatalog.PowerUps`
3. Each card calls `IPurchaseService.TryPurchaseAsync(...)` when bought.
4. `KnifeSkinReward` saves the unlocked skin id into persistent player data.
5. `StoreBalanceChipView` updates itself from the currency service and balance changed events.

## 8. Quick Validation

Check these after setup:

1. Skin cards appear under `KnifeSkinsContentRoot`
2. Power-up cards appear under `PowerUpsContentRoot`
3. Prices are visible on both card types
4. Clicking buy triggers a purchase
5. Balance chip updates after a successful purchase
6. Buttons disable while a purchase is in progress
7. Purchased skins become non-purchasable after unlock

## 9. Troubleshooting

1. No cards appear
   - Check the correct prefab is assigned on `StoreMenuController`
   - Check the content roots are assigned
   - Check the catalog section actually has entries
2. Prices do not show
   - Check `PricePill.AmountText` is assigned on the card prefab
3. Card icon is missing
   - Check `IconImage` is assigned on the card prefab
   - Check `ShadowImage` is assigned if you expect the back/shadow image to update
   - Check the reward config has a valid icon
4. Power-up reward amount is not shown
   - Check `RewardAmountText` is assigned on the power-up card prefab
   - Check the store entry uses `PowerUpReward`
5. Buy does nothing
   - Check `BuyButton` is assigned on the card prefab
   - Check the item has a valid `Reward`
6. Balance chip does not update
   - Check `BalanceText` is assigned
   - Check the currency services are available in the parent scope
7. Skin can still be bought after purchase
   - Check the store entry uses `KnifeSkinReward`
   - Check `KnifeSkinConfig.Id` is set
