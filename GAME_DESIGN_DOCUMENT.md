# Toss Madness - Game Design Document

## 1. Product Snapshot

**Toss Madness** is a mobile, portrait-first arcade skill game built around a simple promise: throw knives into a rotating target, break the target, collect rewards, and progress through increasingly tricky stages.

The game is designed for short, repeatable sessions. Each attempt should feel immediate, readable, and tense: the player waits for the perfect opening, taps to throw, and either lands a satisfying hit or fails by colliding with an existing knife. Long-term motivation comes from stage progression, themed worlds, knife skins, power-ups, daily missions, collectible currency, rewarded ads, and a light local PVP mode.

## 2. Player Fantasy

The player is a precision knife thrower clearing a sequence of stylized target challenges. The fantasy is not realism; it is toy-like, crunchy, and readable. The target spins, shifts, pulses, and becomes more dangerous over time. The player gets better at timing and learns when to use power-ups to rescue difficult moments.

The emotional arc is:

- "I can understand this instantly."
- "I almost had that."
- "One more try."
- "That hit felt perfect."
- "I want to unlock the next stage, skin, or reward."

## 3. Target Audience

The game is aimed at casual mobile players who enjoy fast skill checks, clean progression, and cosmetic collection. The experience should work for players who play for 30 seconds, but also give returning players reasons to come back through missions, unlocks, and harder stage patterns.

Primary audience:

- Casual arcade players.
- Hyper-casual and hybrid-casual mobile players.
- Players who enjoy precision timing games.
- Players motivated by cosmetic skins, daily objectives, and simple progression.

## 4. Platform And Session Shape

The game is structured for mobile play in short bursts.

- **Session length:** 30 seconds to several minutes.
- **Input:** simple tap-to-throw, with optional manual aim through a power-up.
- **Orientation:** portrait.
- **Core session:** open main menu, press play, attempt a stage, earn rewards or retry.
- **Meta session:** check missions, spend currency, buy skins or power-ups, select a knife skin, continue progression.

## 5. Core Gameplay Loop

1. The player enters a stage.
2. A level begins with a rotating target and a limited set of knives.
3. The player taps to throw a knife.
4. A successful hit sticks into the target.
5. A bad hit collides with an existing knife or fails to land, ending the level attempt.
6. Enough successful hits break the target.
7. Breaking all targets in the level completes the level.
8. Completing all levels in the stage completes the stage.
9. The player receives stage rewards and unlocks further progression.
10. Between attempts, the player can use currency, power-ups, skins, ads, or missions to improve the next session.

The core loop is intentionally simple. The depth comes from target behavior, knife limits, collectible placement, existing obstacles, multi-target layouts, and power-up timing.

## 6. Moment-To-Moment Rules

The player throws knives upward into a target.

Successful play requires:

- Reading the target's rotation.
- Avoiding knives already stuck in the target.
- Choosing the right timing window.
- Collecting optional items positioned around the target.
- Conserving knives.
- Using power-ups at high-pressure moments.

Failure happens when:

- A thrown knife collides with an existing knife and counts as a failed bounce.
- The player runs out of available knives before all targets are destroyed.
- A thrown knife leaves play without resolving the objective.

Completion happens when:

- Every active target in the level is destroyed.
- Every level in the stage is completed.

## 7. Stage And Level Structure

The game is organized into themed worlds. Each world contains stages, and each stage contains several levels. A stage is not a single target; it is a short sequence of levels that the player must clear.

Current structure visible in the project:

- Multiple world collections.
- Each world currently contains four stages.
- Each stage is built from a chain of levels.
- Early stages use simple rotating targets.
- Later stages introduce faster speeds, direction changes, pulsing rotation, flipping behavior, moving targets, orbiting targets, multiple targets, collectibles, and pre-stuck knives.

Stage difficulty is shaped by:

- Number of knives available.
- Number of hits required to break targets.
- Target size and placement.
- Rotation speed and direction.
- Movement behavior.
- Number of targets in the level.
- Existing knives already stuck in the target.
- Collectible placement.
- Recovery is always free Retry or Menu; no failure Continue cost is presented.

## 8. World Progression

Worlds act as themed rows in the stage selection screen. They are the player's main progression map.

Each world should communicate:

- A distinct visual theme.
- A set of stages in progression order.
- Locked and unlocked states.
- The player's current stage.
- A sense of moving deeper into the game.

The current project includes world folders and art concepts that point toward elemental or fantasy-themed stage identities, including forest, fire, water, forge, desert, frost, swamp, and void-style environments. The product direction should lean into this because it gives the same simple mechanic fresh context as the player progresses.

## 9. Current Stage Content Model

The content model supports both easy early stages and much harder late stages.

A level can include:

- One or more targets.
- A required hit count per target.
- A set knife count for the player.
- Optional extra knives.
- Optional collectibles attached around the target.
- Optional knives already stuck in the target.
- Rotational behavior.
- World movement behavior.

Target behavior types include:

- Constant rotation.
- Curved or changing spin.
- Pulsating spin.
- Flipping rotation.
- Horizontal or sinusoidal movement.
- Orbiting movement.

This gives the game a strong expansion path without changing the basic input model.

## 10. Main Menu

The main menu is the player's hub.

Primary actions:

- **Play:** start the currently selected unlocked stage.
- **Stages:** open the stage/world selection screen.
- **Shop:** open the store.
- **Power-ups:** view available power-ups.
- **Missions:** open daily missions.
- **PVP:** enter local PVP mode.
- **Remove Ads:** purchase the ad-free entitlement.
- **Knife skin selector:** cycle through owned knife skins.

The main menu background reflects the selected stage or world theme, reinforcing progression and giving the player a clear sense of place.

## 11. Stage Selection

The stage selection screen presents worlds as grouped rows. Each world row contains its stages and communicates progression across the larger campaign.

Expected player behavior:

- See which world and stage are current.
- Scroll or browse available worlds.
- Choose unlocked stages.
- Understand locked progression.
- Return to the main menu.

The stage screen should be treated as a product-facing progression map, not just a list.

## 12. Stage Failure And Recovery Flow

When the player fails a level, the game presents recovery choices.

Approved popup contract:

- `Game Over` is the only headline/informational copy.
- `Retry` is the single primary action and reloads the failed level for free.
- `Menu` is the quiet exit.
- The collision cause is communicated by the gameplay impact beat before the popup, not by
  descriptive popup copy.
- Start Over, Continue, prices, currency recovery, rewarded ads, and interstitial recovery
  are absent.

Legacy Continue cost data and bindings may remain during cleanup, but they are not an
approved player-facing rule. A future Continue requires a new product decision and an
outcome genuinely different from free Retry.

## 13. Rewards

Rewards are granted through stages, missions, ads, and purchases.

Reward types include:

- Soft currency.
- Power-up inventory.
- Knife skins.
- Remove Ads entitlement.

Current reward examples:

- 50 coins.
- 75 coins.
- 100 coins.
- Shatter Strike power-up.
- Chrono Shift power-up.
- Tri-Barrage power-up.

Rewards should be presented clearly after meaningful actions, especially stage completion and mission claiming.

## 14. Economy

The project currently has a simple collectible and currency model.

Currencies and collectibles:

- A primary coin/tomato-style currency worth 1.
- An orange-style collectible worth 3.
- Collectibles can be placed around targets and earned during play.
- Store prices use the primary soft currency.

Main economic sinks:

- Knife skins.
- Power-up bundles.
- Continue costs.

Main economic sources:

- In-level collectibles.
- Stage rewards.
- Daily mission rewards.
- Rewarded ads.

The economy should stay readable and lightweight. The player should understand what they earned, what they can spend, and why the next purchase is desirable.

## 15. Store

The store supports cosmetic and utility purchases.

Current store categories:

- Knife skins.
- Power-up bundles.
- Remove Ads purchase.
- Rewarded ad CTA for free currency.

Current store pricing visible in the project:

- Power-up bundles: 250 soft currency for 6 units.
- Knife skins: priced from 100 to 650 soft currency.
- Remove Ads: real-money IAP.
- Watch ad reward: 50 soft currency.

Knife skin purchases auto-equip after purchase, making the reward immediate and satisfying.

The store should prioritize:

- Clear card visuals.
- Obvious ownership states.
- Clear price and affordability.
- Strong feedback after purchase.
- Easy return to the main menu.

## 16. Knife Skins

Knife skins are the primary cosmetic collection system.

Current content includes:

- A set of standard knife skins.
- A set of premium knife skins.
- Main menu selection for owned skins.
- Store purchase flow for new skins.
- Auto-equip behavior when a skin is purchased.

Design purpose:

- Give players a visible identity.
- Add long-term soft-currency goals.
- Create non-power-based monetization and retention hooks.

Knife skins should stay cosmetic. They should not affect gameplay balance unless the product direction later introduces explicit weapon classes.

## 17. Power-Ups

Power-ups provide tactical help during stages. They are inventory-based and can be earned or bought.

Current power-ups:

| Power-Up | Player-Facing Purpose |
| --- | --- |
| Shatter Strike | Instantly destroys a target with a single hit, skipping remaining required knives. |
| Tri-Barrage | Launches three knives at once to quickly reduce the required hit count. |
| Chrono Shift | Slows target rotation for 5 seconds, making timing easier. |
| Dead Eye | Unlocks manual aim so the player can adjust the throw angle. |

Power-up design goals:

- Help players overcome difficulty spikes.
- Create smart decision moments.
- Support mission objectives.
- Provide store value without making normal play feel impossible.

Power-ups should feel strong, visual, and limited. The best use case is "I know exactly why I used this here."

## 18. Daily Missions

Daily missions provide recurring goals and give players a reason to return.

The game currently activates three daily missions from a mission pool.

Current mission pool:

- Complete 3 levels.
- Hit 15 collectibles.
- Use 2 power-ups.
- Watch 1 rewarded ad.
- Complete a stage without failing.
- Buy 1 item from the shop.

Mission rewards include coins and power-ups.

Design purpose:

- Encourage varied play.
- Drive engagement with store, ads, and power-ups.
- Reward both skill and participation.
- Create daily retention without requiring long sessions.

## 19. Ads And Monetization

The game supports ads in player-friendly reward contexts.

Ad placements:

- Store rewarded ad for soft currency.
- Any future interstitial placement requires separate approval and must not interrupt the
  approved failure recovery path.

Remove Ads:

- A real-money purchase removes ads.
- The entitlement should disable interruption-style ads while preserving any product decision around optional rewarded ads.

Design principle:

Ads should be framed as a convenience, rescue, or bonus. The core game should remain playable without ads, especially in early progression.

## 20. IAP

Current real-money purchase:

- Remove Ads.

Product role:

- Simple, understandable first IAP.
- Good fit for players who like the game and want a smoother experience.
- Low design risk compared with selling power directly.

Future IAP opportunities, if desired:

- Starter bundle with skins, coins, and power-ups.
- Cosmetic skin packs.
- Limited-time themed bundles.
- No-fail beginner pack with currency and a few power-ups.

## 21. PVP Mode

The project includes a local PVP mode.

Current PVP concept:

- Two players alternate turns.
- Both throw at the same target.
- A successful hit passes the turn.
- A failed bounce gives the win to the other player.
- The mode supports rematch and exit.

Design purpose:

- Adds a social, pass-and-play mode.
- Uses the same core knife timing mechanic.
- Creates a quick "challenge a friend" experience without requiring online systems.

PVP should stay extremely simple unless it becomes a product pillar. Its value is immediacy.

## 22. Audio And Feedback

The game relies on strong feedback to make a simple action feel satisfying.

Important feedback moments:

- Knife loaded and ready.
- Knife thrown.
- Knife sticks into target.
- Knife collides and fails.
- Target hit reaction.
- Target shatter or break.
- Collectible hit.
- Stage completion.
- Reward granted.
- Store purchase success or failure.

The project includes casual game sounds, RPG-style sounds, action VFX, hit particles, shatter effects, target spawn animation, knife hit particles, and screen flash support. These should be used to make every tap feel crisp.

## 23. Visual Direction

The game's visual identity is colorful, casual, and arcade-fantasy.

Current visual signals:

- Stylized targets.
- Fruit/coin-like collectibles.
- Knife sprites and skins.
- Themed stage backgrounds.
- Fantasy/action VFX.
- UI with bold arcade buttons and reward cards.

Visual priorities:

- Targets and knives must remain readable above everything else.
- Existing stuck knives must be easy to see.
- Collectibles should invite risk without obscuring target danger.
- Stage themes should add variety without hurting gameplay clarity.
- Store and mission screens should feel rewarding, not cluttered.

## 24. Player Progression

Progression has several layers:

- **Skill progression:** the player gets better at timing.
- **Stage progression:** the player unlocks new stages and worlds.
- **Collection progression:** the player buys and equips knife skins.
- **Inventory progression:** the player earns and spends power-ups.
- **Daily progression:** the player completes missions and claims rewards.
- **Ad/IAP progression:** the player can use ads or Remove Ads to shape the experience.

The most important layer is skill progression. Everything else should support it rather than replace it.

## 25. Current Feature Map

Implemented or represented in project content:

- Main menu.
- Stage play.
- Stage/world selection.
- Themed stage collections.
- Multi-level stages.
- Target rotation and movement behaviors.
- In-level collectibles.
- Pre-stuck knife obstacles.
- Stage completion rewards.
- Failure, free Retry, and Menu recovery flow.
- Soft currency.
- Store.
- Knife skins.
- Power-up inventory and power-up menu.
- Daily missions.
- Rewarded ads.
- Interstitial ads.
- Remove Ads IAP.
- Local PVP mode.
- Settings, mute, privacy policy, and supporting popups.

## 26. Product Pillars

**Instant Clarity**

The player should understand the goal within one second: throw knives, avoid knives, break the target.

**Readable Tension**

Difficulty should come from visible timing problems, not hidden rules.

**Short-Session Satisfaction**

Every session should offer either progress, currency, mission progress, or a useful retry lesson.

**Fair Recovery**

Failure should sting but not feel punishing. Free Retry is immediate, Menu is always
available, and the player is never routed through a spend or ad decision to recover.

**Lightweight Collection**

Skins, power-ups, and rewards give long-term motivation without burying the core game under complexity.

## 27. Open Product Questions

These are product/design questions worth deciding as the game moves forward:

- Should the game name be finalized as **Toss Madness**, **Knife Toss**, or another title?
- Should world names be fantasy names, elemental names, or simple numbered worlds?
- Should premium knives be store-only, reward-only, or IAP-bundle content?
- Should rewarded ads remain optional after Remove Ads, or should Remove Ads disable only interstitials?
- Should PVP award anything, or remain a purely casual side mode?
- Should later worlds introduce new target types with explicit player-facing names?
- Should daily missions reset at local time or global UTC time from the player's perspective?
- Should continues cost soft currency, ads, or both depending on attempt count?

## 28. Recommended Next Design Pass

The strongest next product pass would be to define the campaign identity:

1. Name each world.
2. Define a difficulty theme for each world.
3. Assign a reward theme to each world.
4. Decide the exact stage unlock cadence.
5. Finalize store categories and visual priority.
6. Define the first-time user experience and tutorial beats.

The foundation is already coherent: a simple arcade core, a layered progression wrapper, and enough content systems to support a full casual mobile product.

See also: [First 10 Minutes Design Pass](FIRST_10_MINUTES_DESIGN_PASS.md) for the recommended onboarding, unlock order, and early-stage tuning plan.
