# Knife Toss Meta Features Integration Plan

## Implementation Order

This is the recommended execution order for the project.

1. Main menu hub refactor.
2. Stage locking and current-stage focus.
3. First-clear rewards and stage completion rewards.
4. Analytics instrumentation and dashboards.
5. Daily login streak.
6. Daily missions.
7. Real-money store basics: starter pack, remove ads, coin packs.
8. Timed offers.
9. Stars, mastery, and collection meta.
10. Live ops, weekly challenges, and event currency.

### What Comes First For Your Requested Layout

This is the first implementation block and should happen before the rest of the meta work:

1. Move the HUD to the top of the main menu. Completed.
2. Show the current coin balance in that top HUD. Completed.
3. Add a store icon/button in that top HUD.
4. Embed the stage list directly into the main menu scene.
5. Make the stage list scrollable.
6. Auto-focus the current stage in the list on menu open.
7. Show a lock on locked stages and disable their interaction.
8. Remove or demote the standalone stages scene.

## Executive Summary

The current game has a workable arcade core and the beginnings of monetization, but it is not yet a durable F2P business.

What exists now:
- Core stage progression exists.
- Soft currency exists and is earned from collectible hits.
- Consumable power-ups exist and can be bought with soft currency.
- Cosmetic knife skins exist and can be bought with soft currency.
- Rewarded continue and rewarded store ads exist.
- Interstitial ad gating exists.
- Persistence, Remote Config, and ad telemetry hooks already exist in the codebase.

Why this is not enough yet:
- Progression depth is shallow. Persistent progress currently tracks only current and highest unlocked stage.
- Stage selection appears fully open instead of progression-gated.
- The power-ups menu is informational, not a progression or monetization driver.
- Real-money commerce is not live. `CurrencyPacks` exist in the data model but are empty and not bound into the store UI.
- Content volume is modest at roughly 10 stages x 5 levels.
- There are no daily loops, streaks, quests, live offers, or prestige systems to create return behavior.

Bottom line:
- In its current form, the project can potentially make some ad revenue if the core gameplay is strong.
- It is unlikely to become a meaningful revenue product without stronger retention systems first, then a cleaner monetization layer on top.

## Current State Review

### Strengths

- The economy foundation is already present.
  - Soft currency is granted on collectible hits in [Assets/Scripts/Game/CollisionHandlers/KnifeHitTargetHandler.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/CollisionHandlers/KnifeHitTargetHandler.cs#L87).
- The store foundation is already present.
  - Store data supports `PowerUps`, `CurrencyPacks`, and `Skins` in [Assets/Scripts/Game/Store/Data/StoreCatalog.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/Store/Data/StoreCatalog.cs#L11).
- Rewarded and interstitial ad policy is already centralized.
  - Ad gating, cooldowns, attempt counting, and telemetry live in [Assets/Scripts/Common/Ads/Services/AdService.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Common/Ads/Services/AdService.cs#L12).
- Remote Config is already available for live tuning.
  - Firebase Remote Config access is implemented in [Assets/Scripts/Common/Services/RemoteConfig/FirebaseRemoteConfigService.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Common/Services/RemoteConfig/FirebaseRemoteConfigService.cs#L10).

### Weaknesses

- Persistent progression is too thin.
  - Only `CurrentStageIndex` and `HighestStageIndex` are stored in [Assets/Scripts/Game/Persistence/Data/PlayerProgressData.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/Persistence/Data/PlayerProgressData.cs#L9).
- Stage gate pressure is weak or absent.
  - Stage selection directly initializes every stage item and lets the player jump to any chosen stage in [Assets/Scripts/Game/UI/Stages/StagesMenuController.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/UI/Stages/StagesMenuController.cs#L51).
- The power-ups menu does not currently drive decisions.
  - It displays items only and passes `null` for action handling in [Assets/Scripts/Game/UI/PowerUps/PowerUpsMenuController.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/UI/PowerUps/PowerUpsMenuController.cs#L64).
- Real-money monetization is not integrated.
  - `CurrencyPacks` are empty in [Assets/Configurations/Systems/Store/StoreCatalog.asset](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Configurations/Systems/Store/StoreCatalog.asset#L36).
  - The store menu binds only skins and power-ups in [Assets/Scripts/Game/UI/Store/StoreMenuController.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/UI/Store/StoreMenuController.cs#L96).
- Content exhaustion risk is high.
  - The stage generator currently defines `TotalStages = 10` and `LevelsPerStage = 5` in [Assets/Scripts/Editor/StageGenerator.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Editor/StageGenerator.cs#L16).

### Revenue Readiness Verdict

Current state by revenue type:
- Ads: structurally viable, but not enough alone unless retention is decent.
- IAP: not ready.
- Hybrid F2P: not ready.

Current state by product readiness:
- Core gameplay prototype: yes.
- Early live test candidate: maybe, after analytics and retention hooks.
- Scalable business foundation: no.

## Product Strategy

The correct order is:

1. Turn the main menu into the progression hub.
2. Tighten progression pressure and content pacing.
3. Build retention loops.
4. Add value-based monetization.
5. Use analytics and Remote Config to tune.

Do not start by stuffing more monetization into the current build. If players do not return, monetization improvements will not matter.

## Main Menu Hub Refactor

This requested UI change should be treated as a core product change, not a cosmetic pass.

Target behavior:
- HUD pinned at the top of the main menu.
- Current coin balance visible at all times.
- Store icon visible from the main menu header.
- Scrollable stage list directly inside the main menu scene.
- Current stage clearly highlighted.
- Locked stages clearly show a lock state and cannot be entered.
- Separate stages scene becomes unnecessary or secondary.

Why this should happen early:
- This makes progression more visible.
- This creates a better home for future daily login, missions, and featured offers.
- This is the natural place to expose currency and store access.
- This reduces menu hops and makes lock pressure more obvious.

Current code implications:
- `MainMenuController` is currently very lightweight and still routes to a separate stages scene in [Assets/Scripts/Game/UI/MainMenu/MainMenuController.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/UI/MainMenu/MainMenuController.cs#L14).
- Stage list rendering already exists in [Assets/Scripts/Game/UI/Stages/StagesMenuController.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/UI/Stages/StagesMenuController.cs#L14) and [Assets/Scripts/Game/UI/Stages/UiStageItem.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/UI/Stages/UiStageItem.cs#L9), but it needs to be adapted for lock state, current-stage focus, and embedding in the main menu.
- A reusable coin balance presenter already exists in [Assets/Scripts/Game/UI/Common/CurrencyBalanceDisplay.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/UI/Common/CurrencyBalanceDisplay.cs#L11) and can be reused in a header chip similar to the store balance UI adapter in [Assets/Scripts/Game/UI/Store/StoreBalanceChipView.cs](/mnt/c/Users/jons1/Documents/Knife-Toss/Assets/Scripts/Game/UI/Store/StoreBalanceChipView.cs#L8).

## Target Meta Stack

This project should aim for the following meta layers:

### Layer 1: Core Progression

- Stage unlock gating.
- Level completion stars or medals.
- First-clear rewards.
- Stage completion chests.
- Milestone unlocks tied to progression.

### Layer 2: Return Loops

- Daily login streak.
- Daily missions.
- Weekly challenge track.
- Limited-time rotating event.
- Comeback gift for lapsed players.

### Layer 3: Spend Sinks

- Power-up inventory consumption.
- Skin collection.
- Upgrade sinks or mastery sinks.
- Offer shop with time-limited bundles.
- Event currency shop.

### Layer 4: Revenue Layer

- Rewarded ads for recovery and acceleration.
- Interstitials with controlled pacing.
- Starter pack.
- Coin packs.
- Remove ads purchase.
- Premium skin bundles.

### Layer 5: Live Ops Control

- Remote Config tuned economy.
- Offer rotation.
- Mission weights.
- Ad cooldown tuning.
- Difficulty tuning.

## Recommended Feature Set

### Phase 1: Must-Have Before Serious Monetization

These are the features with the highest leverage.

#### 1. Main Menu Progression Hub

Goal:
- Make the main menu the default meta screen.
- Surface progression, currency, and store access without extra navigation.

Requested layout:
- Fixed top HUD/header.
- Coin balance chip in the top HUD.
- Store icon button in the top HUD.
- Scroll view below the HUD containing stage cards.
- Current stage card auto-scrolled into view on open.
- Locked stage cards show lock icon and disabled CTA.

Recommended UI structure:
- `MainMenuHeader`
- `CurrencyBalanceChip`
- `StoreButton`
- `ScrollableStageList`
- `StageCard`

Recommended behavior:
- Remove or demote `_StagesButton`.
- Keep `_PlayButton` as "Play Current Stage" or "Continue."
- Tapping an unlocked stage sets `GameData.CurrentStageIndex`.
- Tapping the play button launches the currently selected stage.
- Opening the menu auto-focuses the current or highest unlocked stage.

Files likely impacted:
- `Assets/Scripts/Game/UI/MainMenu/MainMenuController.cs`
- `Assets/Scripts/Game/UI/Stages/StagesMenuController.cs`
- `Assets/Scripts/Game/UI/Stages/UiStageItem.cs`
- main menu scene prefab/layout assets

Architecture recommendation:
- Do not duplicate stage list logic.
- Extract stage list population into a reusable presenter or service, then use it inside the main menu.
- Keep the old stages scene only as fallback during transition, then remove it once stable.

Why this matters:
- This change makes the meta loop legible.
- It creates the right foundation for missions, streaks, and featured offers.
- It immediately solves three UX asks at once: progression visibility, store access, and current coin visibility.

#### 2. Stage Locking and Completion Rewards

Goal:
- Make progression matter.
- Prevent players from bypassing the content ladder.
- Create a visible next target.

Implementation:
- Lock stages above `HighestStageIndex + 1`.
- Add clear state per stage and per level.
- Add first-clear reward for each level.
- Add stage-complete reward chest.

Data additions:
- `CompletedLevels`
- `ClaimedLevelRewards`
- `ClaimedStageRewards`
- `BestLevelScore` or `BestLevelStars`

Files likely impacted:
- `Assets/Scripts/Game/Persistence/Data/PlayerProgressData.cs`
- `Assets/Scripts/Game/Persistence/Services/PlayerDataService.cs`
- `Assets/Scripts/Game/UI/MainMenu/MainMenuController.cs`
- `Assets/Scripts/Game/UI/Stages/StagesMenuController.cs`
- `Assets/Scripts/Game/UI/Stages/UiStageItem.cs`
- level/stage completion flow classes under `Assets/Scripts/Game/Flow`

Why this matters:
- Right now progression pressure is weak.
- This is the minimum structure needed for goals, rewards, and future missions.

#### 3. Daily Missions

Goal:
- Create D1 and D7 return motivation.
- Give players clear short-term objectives besides "play more."

Suggested launch mission pool:
- Complete 3 levels.
- Hit 15 collectibles.
- Use 2 power-ups.
- Watch 1 rewarded ad.
- Finish a stage without failing.
- Buy 1 item from the shop.

Reward types:
- Soft currency.
- Power-up packs.
- Event currency later.

Data additions:
- `DailyMissionProgress`
- `DailyMissionRefreshDateUtc`
- `ClaimedDailyMissionIds`

New systems:
- `DailyMissionDefinition`
- `DailyMissionCatalog`
- `DailyMissionService`
- `MissionProgressRouter`

Files likely added:
- `Assets/Scripts/Game/Missions/...`
- `Assets/Configurations/Systems/Missions/...`

Why this matters:
- Daily missions are the cleanest way to turn an arcade game into a repeatable habit.

#### 4. Daily Login Streak

Goal:
- Give a low-friction reason to reopen the app every day.

Recommended behavior:
- 7-day repeating streak.
- Day 1-6 give coins and power-ups.
- Day 7 gives a larger reward or exclusive shard.
- One missed day drops back by 1 step or resets depending on target harshness.

Data additions:
- `LastLoginUtc`
- `CurrentLoginStreak`
- `LastClaimedLoginDayIndex`

UI:
- Main menu streak card.
- Claim popup on launch.

Why this matters:
- This is one of the cheapest retention systems to build and tune.

### Phase 2: Must-Have Monetization Layer

These should come after retention basics are live.

#### 5. Real-Money Store

Goal:
- Create actual payer conversion, not only ad monetization.

Recommended launch SKU set:
- Starter pack: low price, high value, limited-time for new users.
- Remove ads.
- Small, medium, large coin packs.
- Premium skin bundle.
- Power-up boost pack.

Important:
- Do not sell only coins. Players convert better on framed value offers.
- Soft currency alone is usually weak unless demand pressure is already strong.

Data additions:
- `OwnedNonConsumables`
- `ClaimedOfferIds`
- `OfferExpiryUtc`

Systems:
- `IapProductCatalog`
- `IapService`
- `OfferService`
- `OfferPresentationResolver`

UI changes:
- Add `CurrencyPacks` section to the store UI.
- Add featured offer panel to the main menu header or store.
- Add post-fail and post-win contextual offers.

Files likely impacted:
- `Assets/Scripts/Game/UI/Store/StoreMenuController.cs`
- `Assets/Scripts/Game/Store/Data/StoreCatalog.cs`
- new IAP services under `Assets/Scripts/Game/Store` or `Assets/Scripts/Game/IAP`

Why this matters:
- Right now there is no real-money path wired into the product.

#### 6. Remove Ads Purchase

Goal:
- Improve payer experience and create a simple low-friction SKU.

Recommended rules:
- Remove interstitials.
- Keep optional rewarded ads.
- Message it clearly in fail/store flows.

Why this matters:
- This is usually one of the cleanest first purchases in ad-driven arcade games.

#### 7. Time-Limited Offers

Goal:
- Increase urgency and conversion.

Launch offer types:
- New player welcome offer.
- Fail streak rescue bundle.
- Stage completion bundle.
- Weekend bundle.

Suggested contents:
- Coins.
- Power-ups.
- Exclusive cosmetic.

Why this matters:
- Permanent catalogs rarely convert as well as contextual, timed bundles.

### Phase 3: Depth and Long-Term Retention

#### 7. Star / Medal System

Goal:
- Create replay incentive.

Star examples:
- Clear level.
- Clear without failing.
- Hit all collectibles.

Effects:
- Improves completionist retention.
- Supports chest thresholds and stage mastery rewards.

#### 8. Collection Meta

Goal:
- Make skins part of a collection game, not just isolated purchases.

Recommended features:
- Collection progress bar.
- Set completion rewards.
- Rare skins from events.
- Duplicate protection if random reward sources are added.

#### 9. Event Currency and Event Shop

Goal:
- Introduce live-ops cadence without disrupting base economy.

Recommended event examples:
- Weekend precision challenge.
- Rotating stage modifier event.
- Collect X event tokens from clears.

Use a separate event currency:
- Prevents wrecking the base economy.
- Makes limited rewards easier to manage.

#### 10. Mastery / Upgrade Sink

Goal:
- Add a durable coin sink.

Recommended approach:
- Meta upgrades that are helpful but not run-breaking.
- Keep them restrained to avoid pay-to-win feel.

Examples:
- Slight starting bonus per run.
- Slight collectible bonus.
- Additional daily mission reroll.
- Cosmetic prestige frame unlocks.

Warning:
- Avoid raw permanent power creep early.
- Knife toss arcade games can lose fairness quickly if upgrades distort core timing.

## What Not To Add Early

- PvP-dependent economy.
- Clan systems.
- Loot boxes as a first monetization layer.
- Too many currencies.
- Deep upgrade trees that trivialize the skill loop.

These systems add complexity fast and can obscure whether the core loop actually retains players.

## Integration Architecture Plan

### 1. Persistence Expansion

Extend `PlayerData` with focused meta containers instead of dumping all fields into `PlayerProgressData`.

Recommended structure:

```csharp
public class PlayerMetaData
{
    public PlayerStageProgressData StageProgress = new();
    public DailyLoginData DailyLogin = new();
    public MissionProgressData Missions = new();
    public OfferStateData Offers = new();
    public CommerceStateData Commerce = new();
}
```

Recommended persistence additions:
- `PlayerMetaData`
- `PlayerStageProgressData`
- `LevelCompletionEntry`
- `DailyLoginData`
- `MissionProgressData`
- `OfferStateData`
- `CommerceStateData`

Why:
- Keep economy, progression, and live-ops state separated.
- Avoid turning `PlayerProgressData` into an unmaintainable bag of flags.

### 2. Event-Driven Mission Tracking

Mission progress should subscribe to existing gameplay/store/ad events rather than polling.

Good event sources already exist:
- collectible hit events
- purchase completed events
- level completed events
- level failed events
- power-up used events
- ad completed events

Implementation shape:
- `MissionProgressRouter` listens to channels.
- Converts events into mission stat increments.
- `DailyMissionService` validates completion and claims.

Why:
- This matches the current event-channel architecture.
- It is low-risk and easy to test.

### 3. Offer and Economy Tuning via Remote Config

Use Remote Config for:
- daily reward values
- mission reward tables
- ad cooldowns
- stage reward amounts
- offer prices and contents
- fail-offer triggers
- starter-pack expiry window

Suggested key naming:
- `economy_level_first_clear_coin_reward`
- `economy_stage_complete_coin_reward`
- `daily_login_day_1_reward_soft`
- `daily_login_day_7_reward_soft`
- `missions_daily_set_a`
- `offers_starter_pack_enabled`
- `offers_starter_pack_price_usd`
- `offers_fail_bundle_enabled`
- `ads_interstitial_retry_cooldown_seconds`

Why:
- The Remote Config layer already exists, so use it to reduce balancing churn.

### 4. Analytics Layer

Before serious live testing, implement a game analytics service and connect it to Firebase Analytics or another provider.

Minimum event set:
- `session_start`
- `session_end`
- `level_start`
- `level_complete`
- `level_fail`
- `stage_complete`
- `currency_earned`
- `currency_spent`
- `powerup_used`
- `mission_completed`
- `daily_login_claimed`
- `shop_viewed`
- `shop_purchase_soft`
- `shop_purchase_real`
- `offer_shown`
- `offer_clicked`
- `offer_purchased`
- `ad_requested`
- `ad_shown`
- `ad_completed`
- `ad_failed`

Minimum properties:
- stage index
- level index
- failure count
- currency balance before and after
- placement id
- sku id
- session number
- days since install

Why:
- Without telemetry, you will not know whether retention or economy is broken.

## UI Integration Plan

### Main Menu

Add:
- Fixed top HUD/header.
- Coin balance chip.
- Store button.
- Daily login claim card.
- Mission summary widget.
- Featured timed offer card.
- Progress chest / milestone widget.
- Embedded vertical or horizontal scrollable stage list.
- Current-stage highlight and auto-focus on open.

Likely touchpoint:
- `Assets/Scripts/Game/UI/MainMenu/MainMenuController.cs`

### Stages Menu

Add:
- Locked state visuals.
- Stage reward chest state.
- Stage medal/star total.
- Recommended stage CTA.

Note:
- If the main menu hub refactor lands first, the standalone stages scene should be reduced to a reusable stage-list implementation or retired entirely.

Likely touchpoints:
- `Assets/Scripts/Game/UI/Stages/StagesMenuController.cs`
- `Assets/Scripts/Game/UI/Stages/UiStageItem.cs`

### Store

Add:
- Currency packs section.
- Featured offers section.
- Remove ads section.
- Premium bundle cards.

Likely touchpoints:
- `Assets/Scripts/Game/UI/Store/StoreMenuController.cs`
- `Assets/Scripts/Game/UI/Store/StoreCardView.cs`

### Gameplay HUD and End Screens

Add:
- Mission progress tracker.
- Star result display.
- First-clear bonus messaging.
- Next reward preview.
- Contextual offer entry points.

Likely touchpoints:
- `Assets/Scripts/Game/UI/HUD/...`
- fail/win popup controllers

## Detailed Rollout Order

### Milestone 1: Measurement and Progression Gating

Ship:
- main menu hub refactor
- analytics service
- stage lock logic
- first-clear rewards
- stage completion rewards

Success criteria:
- main menu shows current coin balance
- store entry is always visible
- current stage auto-focuses correctly in the scroll list
- locked stages display clearly and cannot be entered
- no progression skips
- reward claims saved correctly
- clean progression funnel visibility

### Milestone 2: Daily Return Systems

Ship:
- daily login streak
- daily missions
- mission progress UI

Success criteria:
- measurable D1 improvement
- mission claim loop stable
- no reset or timezone corruption issues

### Milestone 3: Real-Money Basics

Ship:
- remove ads
- starter pack
- coin packs
- featured offer panel

Success criteria:
- payer conversion starts
- no purchase restoration issues
- ad suppression works for remove-ads users

### Milestone 4: Mid-Term Meta

Ship:
- star system
- stage mastery rewards
- collection progress
- rotating offers

Success criteria:
- replay depth improves
- soft currency sinks remain healthy

### Milestone 5: Live Ops Layer

Ship:
- event currency
- weekly challenge
- limited-time event store
- remote-configured offer schedule

Success criteria:
- return cadence improves
- event participation becomes measurable

## Economy Design Notes

### Soft Currency

Use soft currency for:
- base skins
- power-up packs
- mission rerolls
- retry assistance items later

Do not let soft currency become useless after early progression. A dead currency kills both ad demand and store relevance.

### Hard Currency

Do not add hard currency at first unless needed.

For this project, a simpler model is better:
- soft currency for gameplay economy
- direct IAP offers for monetization

Add premium currency only if you later need:
- multi-offer flexibility
- event store complexity
- battle-pass style reward structures

### Power-Ups

Power-ups are already a good sink because they are consumable.

Improve them by:
- showing inventory pressure more clearly
- granting them from missions and streaks
- using them in offer bundles
- adding scarcity around strong effects

### Skins

Skins are useful, but by themselves they are a weak monetization driver in a small arcade game unless collection prestige is stronger.

Improve them by:
- collection progress
- rarity tags
- event exclusivity
- themed bundles

## Risks

### Risk 1: Overbuilding Meta Before Measuring Core Retention

Mitigation:
- add analytics first
- validate fail points and session depth before large content work

### Risk 2: Too Many Systems at Once

Mitigation:
- ship in milestones
- each milestone should improve one KPI cluster

### Risk 3: Economy Inflation

Mitigation:
- remote-config all major rewards
- track sinks vs sources weekly

### Risk 4: Power Creep Damaging Skill-Based Core

Mitigation:
- keep upgrades mostly horizontal or utility-based
- avoid permanent damage/speed escalation early

## Concrete KPI Targets

Use these as directional checkpoints, not promises.

Early launch targets:
- D1 retention: 30%+ desirable
- D7 retention: 8%+ desirable
- rewarded ad opt-in: 20%+ on exposed players
- interstitial frustration: low enough to avoid measurable retention drop
- payer conversion: 1% to 2% is a credible first target for a small arcade game

If retention is below threshold:
- fix onboarding
- fix difficulty spikes
- fix progression pressure
- do not keep layering monetization

## Recommended Immediate Next Build

If prioritizing for business impact, build this next:

1. Main menu hub refactor with top HUD, coin balance, store icon, and embedded scrollable stage list.
2. Analytics service and dashboards.
3. Stage gating plus first-clear rewards.
4. Daily login streak.
5. Daily missions.
6. Real-money starter pack plus remove ads.

That is the cleanest route from "feature-complete arcade game" to "testable F2P product."

## Suggested Code Work Breakdown

### Sprint A

- refactor main menu into progression hub
- embed reusable stage list into main menu
- add current-stage auto-scroll and lock visuals
- add top HUD coin balance and store button
- add meta persistence models
- add stage lock state
- add first-clear reward flow
- add analytics abstraction

### Sprint B

- add daily login service
- add daily mission service
- wire mission progress to event channels
- add main menu widgets

### Sprint C

- add IAP catalog and service
- add remove ads entitlement
- add starter pack and coin packs
- add store UI sections for real-money offers

### Sprint D

- add stars/medals
- add stage chest logic
- add collection progress
- add limited-time offer presentation

## Final Recommendation

The current layout is enough to support the start of a business, but not enough to become one.

You already have the right base systems:
- currency
- consumables
- cosmetics
- ads
- persistence
- remote config

What is missing is the part that makes players come back and the part that gives spenders clear value.

Build retention first, then monetize the retained users.

## Progress Updates

### 2026-03-24 - Main menu hub script groundwork
- Completed: `None`
- Summary: Extracted reusable stage-list presentation and lock-state logic into scripts, updated the stages menu to use that shared presenter, and changed the main menu play flow to respect the current selected stage within unlocked bounds.
- Follow-ups: Wire the new optional `UiStageItem` lock/current references in Unity, add the reusable stage list to the main menu scene, and complete the header layout there without changing scripts again.

### 2026-03-24 - Main menu HUD move
- Completed: `Move the HUD to the top of the main menu`
- Summary: Marked the first requested main menu layout task complete.

### 2026-03-24 - Main menu top HUD coin balance
- Completed: `Show the current coin balance in that top HUD`
- Summary: Marked the second requested main menu HUD task complete.
