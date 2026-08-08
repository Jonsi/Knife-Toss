# Phase 3 Power-Up Mechanic Truth

Date: 2026-08-08

Roadmap phase: [Phase 3 - Power-Up Mechanic Truth](CORE_GAMEPLAY_REFINEMENT_PHASES.md#phase-3---power-up-mechanic-truth)

## Outcome

Shatter Strike and Chrono Shift now have validated activation, consumption, cancellation,
expiration, cleanup, persistence, and acquisition contracts. Player-facing tutorial timing,
copy, gestures, and teaching order remain deliberately deferred to Phase 9.

The implementation fixes the stale Chrono Shift selection, restores the normal thrower when
a level resets with a projectile power-up selected, blocks reactivation while a world effect
is already running, and removes StagePlay's duplicate currency authority.

## Supported QA Workflow

1. Activate the `Android Development` Build Profile. Its profile-local `TEST_ENABLED`
   define compiles cheats without adding test symbols to global Android settings.
2. Open `Tools > Cheats` in Unity.
3. Under **Power-Up Cheats**, set `Shatter Strike` (`SuperKnife`) and `Chrono Shift`
   (`TimeDilator`) counts, then press **Set**.
4. Restart/reload the level after changing counts so every HUD presenter reloads persisted
   inventory through its normal lifecycle.
5. Exercise activation and throws with normal UI/gameplay input. Do not invoke gameplay
   callbacks or edit `playerData.json` to manufacture a result.
6. Record cheat-assisted mechanic QA separately from economy/store acquisition QA.
7. Back up and restore the player profile around a test session. The Phase 3 run restored
   the exact original balance, inventory, collection, and stage and removed its temporary
   backup afterward.

`PowerUpCheats.SetPowerUpCount` updates the runtime `PlayerDataService` and requests a real
save. `EconomyCheats` is appropriate only for controlled price-path setup; the actual
purchase must still use the visible store control and `PurchaseService`.

## Stable Domain Contract

`PowerUpOutcomeEventChannel` is the Phase 4 presentation seam. UI, VFX, audio, haptics,
analytics, and accessibility presenters can subscribe without inferring state from button
colors or inventory mutations.

| Outcome | Meaning | Inventory contract | Selection/effect contract |
|---|---|---|---|
| `Activated` | A supported power-up entered its active behavior. | Projectile count is unchanged; a world-effect count is reported before immediate consumption. | Projectile becomes selected; world effect begins but is not a persistent selection. |
| `Consumed` | A unit was successfully committed. | Decrement by exactly one and save immediately. | Projectile selection/thrower clears after a handled throw; world effect continues until expiry. |
| `Cancelled` | Player cancelled an active cancellable projectile. | Count is preserved. | Normal thrower and unselected HUD state are restored. |
| `Expired` | A timed world effect completed naturally. | `RemainingCount` is `-1` because expiry does not resample inventory. | Target modifiers are removed. |
| `Cleared` | Level lifecycle or explicit cleanup ended active state/effects. | Count is preserved unless it had already been consumed. | Projectile thrower and world modifiers return to normal. |
| `Unavailable` | Inventory count is zero. | No mutation. | Activation is rejected; zero-count HUD routes to acquisition when configured. |
| `Blocked` | Another projectile is already active or the action is otherwise disallowed. | No mutation. | Existing active state remains authoritative. |
| `InvalidConfiguration` | Config or inventory key is missing. | No mutation. | Presenter may surface a diagnostic in Development/QA. |
| `Unsupported` | Config implements no supported execution type. | No mutation. | No runtime state change. |
| `ExecutionFailed` | An active projectile could not handle its throw request. | No mutation. | Active state remains available for recovery/cancellation. |

Selection is derived from `IPowerUpService.ActiveConfig`; it is not an independent domain
state. This prevents a synchronous event ordering issue from leaving the HUD selected after
an immediately consumed world effect.

## Mechanic Contracts

### Shatter Strike (`SuperKnife`)

- Projectile/cancellable power-up.
- Activation swaps to one loaded super knife without consuming inventory.
- A successful real throw consumes exactly one unit, restores the normal thrower, clears
  selection, destroys the target in one hit, and leaves the ordinary knife magazine
  untouched.
- Cancellation or level reset restores the normal thrower without consuming inventory.

### Chrono Shift (`TimeDilator`)

- Immediate world-effect power-up.
- Activation consumes exactly one unit and saves immediately.
- Target behavior multiplier is `0.30` for five seconds and returns to `1.00` on expiry.
- The HUD does not remain selected while the timed effect is running or after it expires.
- Reactivation is blocked while the effect is running, so extra taps consume nothing.
- A clockwise radial sweep on the HUD button shows the effect's actual remaining duration.

## Economy and Acquisition Contract

StagePlay now reuses the persistent parent's `ICurrencyService`. It installs a local economy
only when running standalone and no parent currency authority exists. This follows the
existing installer comment and prevents stale balance dictionaries across parent/child
scopes.

The live acquisition run began with zero Chrono inventory and 48 tomato currency. The
Development cheat added 202 currency through `EconomyCheats`, then all product behavior used
real UI and services:

1. the zero-count Chrono HUD button opened `AddPowerUpPopup`;
2. the Chrono store card spent 250 currency through `PurchaseService`;
3. `PowerUpReward` granted and immediately persisted six Chrono units;
4. the popup and HUD both refreshed to six;
5. a second purchase at zero balance emitted `Insufficient funds.` and changed neither
   balance nor inventory; and
6. stopping Play Mode still loaded balance `0` and Chrono count `6`, proving immediate
   interruption persistence before the original user save was restored.

## Validation Matrix

| Lane | Input / setup | Observed result | Status |
|---|---|---|---|
| Shatter activation | Real HUD click after `PowerUpCheats` grant | Service active, projectile thrower active, one super knife loaded, count unchanged at 3. | Pass |
| Shatter use | Real gameplay tap | One unit consumed (3 -> 2), target completed with one throw, next level started, service/selection/thrower cleared. | Pass |
| Shatter level reset | Real HUD click, then supported `StageLevelCheats.RestartLevel` | `Activated` then `Cleared`; count remained 2 and normal thrower returned. | Pass |
| Chrono baseline | Real HUD click before fix | Multiplier reached 0.30 and restored to 1.00, but final HUD selection remained true. | Reproduced |
| Chrono fixed | Real HUD click after fix | `Activated` -> `Consumed` -> `Expired`; multiplier 0.30 -> 1.00 and final selection false. | Pass |
| Repeated Chrono | Focused EditMode test | Reactivation is blocked while active; no second unit is consumed and only one effect remains. | Pass |
| Zero inventory | Focused EditMode test and zero-count HUD | Typed `Unavailable` service result; configured HUD routes to acquisition rather than silently selecting. | Pass |
| Store acquisition | Real popup/card clicks with controlled sufficient currency | 250 spent, six units granted, popup/HUD/persisted data agree. | Pass |
| Insufficient funds | Second real card click at zero balance | Failure reason emitted; balance and six-unit inventory unchanged. | Pass |
| Economy ownership | Normal MainMenu -> preloaded StagePlay transition | Stage and persistent scopes resolve the same `CurrencyManager` instance and balance. | Pass |
| App interruption | Stop Play Mode immediately after purchase | Saved balance/count reloaded correctly. | Pass |
| Save discipline | Exact pre-test backup/restore | Original 48 balance, Super Knife 0, Chrono 1, collection 0, stage 2 restored; backup removed. | Pass |

## Automated Evidence

- `Game.PowerUps.Tests`: 7 passed, 0 failed, 0 skipped.
- Full Unity EditMode suite: 49 passed, 0 failed, 0 skipped.
- Normal MainMenu -> StagePlay transition after implementation: zero Console errors.
- C# compilation: zero errors and warnings.

The focused tests cover projectile consumption timing, immediate world-effect consumption,
active-effect reactivation blocking, unavailable inventory, cancellation, level cleanup,
expiration, typed outcomes, persistence saves, and thrower restoration.

## Phase Exit Review

| Criterion | Result |
|---|---|
| Both power-ups pass mechanic QA with real input | Pass. |
| Used world effect cannot remain falsely selected | Pass. |
| Acquisition, counts, selection, consumption, expiration, recovery, transition, and interruption remain correct | Pass. |
| Phase 4 presenters do not depend on tutorial-specific logic | Pass: unified typed outcome channel plus existing activation/used/cancel channels. |
| Tutorial composition remains deferred | Pass: no tutorial timing, copy, gesture, or teaching-order decision was added. |
