# Knife Toss GDD - Phase 1 (MVP)

**Goal:** Deliver a stable, addictive core experience with basic progression and monetization foundation.

## 1. Overview
An arcade skill game where the player throws knives at a rotating target. The goal is to stick all knives in the target without hitting previously placed knives.

## 2. Core Loop
- **Throw:** Player taps to throw a knife.
- **Result:** Knife moves upward.
    - **Success:** Sticks to the target.
    - **Fail:** Hits another knife (Result: Game Over).
- **Level Win:** Sticking all required knives transitions to the next level.
- **Stage Win:** Completing all levels in a Stage clears the stage.
- **Fail State:** Hitting an obstacle or knife ends the run. Player can **Retry** (Level 1 of Stage) or **Continue** (Watch Ad).

## 3. Gameplay Mechanics (Phase 1)
### Target Behaviors
- **Constant Spin:** Target rotates at a fixed speed (CW or CCW).
- **Flipping Spin:** Target changes direction at random or fixed intervals.
- **Curve Spin:** Stop & Go, Acceleration/Deceleration curves.
- **Pulsating:** Target grows/shrinks (Scale changes).
- **Orbiting Targets:** Mini-targets rotating around the main one.

### Controls
- Single tap to launch.
- Input locked during flight.

### Power-Ups
- **Super Knife:** Destroys target in one hit.
- **Multiple Knives:** Throws 3 knives in a spread.
- **Time Dilator:** Slows rotation for a set duration.
- **Precision Aim:** Helper for aiming.

### Obstacles
- **Pre-stuck Knives:** Basic obstacles to avoid.
- **Metal Plates:** Simple bounce/fail zones.

## 4. Progression (Initial Content)
**Structure:**
- **The Gauntlet Rule:** Must beat all levels in a stage to unlock the next.
- **Phase 1 Content:** Stages 1 - 10.

**Stage 1 (Forest):**
- **Theme:** Wooden Target, Forest Background.
- **Difficulty:** Tutorial / Easy.
- **Behavior:** Constant Spin.
- **Levels:** 5 Levels.

**Stage 2 (Desert):**
- **Theme:** Sand/Stone Target, Desert Background.
- **Difficulty:** Easy / Normal.
- **Behavior:** Constant Spin + Simple Flipping.
- **Levels:** 5 Levels.

**Stage 3 (Ice):**
- **Theme:** Ice/Crystal Target.
- **Behavior:** Variable Speed (Slippery feel), Curve Spin, Pulsating.

**Stage 4 (Space/Void):**
- **Theme:** Neon/Dark.
- **Behavior:** Small targets, erratic movement, Orbiting mechanics.

**Stages 5-10:**
- Increasing difficulty with mixed behaviors (Sin Movement, Orbiting + Curves).

## 5. Monetization (Priority for Phase 1)
- **Interstitial Ads:** Triggered on Stage Completion or Failure (every ~3 attempts).
- **Rewarded Ads:** "Continue" option after failure (once per run) to resume from current level + specific knife count.

## 6. UI & HUD
- **Main Menu:** Play Button, Stages Button, Sound Toggle.
- **HUD:** Stage/Level Indicator, Remaining Knives (Magazine), Power-up Status.
- **Fail Screen:** Restart Button, Continue Button (Ad).
- **Win Screen:** "Stage Clear" text (minimal).

## 7. Audio/Visuals
- **Art:** Minimalist flat design (Target, Knife, Background).
- **VFX:** Simple hit puff, Fail flash.
- **Audio:** Throw, Hit, Fail sounds.

## 8. Economy (Basic)
- **Currency:** Coins (Tomatoes).
- **Fruit:** "Tomato" (Value 1) spawns to test collection mechanic.
- **Shop:** *Hidden/Disabled in Phase 1* (Coins accumulate but no spend outlet yet).

---

## Technical Constraints (Phase 1)
- **Rotation Speed:** 45 - 180+ deg/s.
- **Knife Count:** Max 17 per target (based on Stage 10).
- **Hitbox:** Generous/Standard (Scale 1.0).
