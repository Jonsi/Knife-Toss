# Knife Toss GDD

## Overview
An arcade skill game where the player throws knives at a rotating target. The goal is to stick all knives in the target without hitting previously placed knives.

## Core Loop
1. Player taps to throw a knife.
2. Knife moves upward and either sticks to the target or causes a fail by hitting another knife.
3. When all required knives are placed, the level is completed.
4. On fail, the player can retry or continue using a rewarded ad.

## Gameplay
- The target rotates at varying speeds.
- Knives thrown stick to empty segments of the rotating target.
- Collision with an existing knife results in failure.

## Level Structure
- Each level requires a specific number of knives.
- Target rotation speed increases with level progression.
- Level difficulty scales by speeding up the target and increasing knife count.

## Controls
- Single tap to launch a knife.
- Input locked during knife flight until resolution.

## UI
- Minimal UI with level counter and remaining knife count.
- Main Menu with Play and Sound options.
- Fail screen with Retry and Continue (rewarded ad).
- Level complete screen with Continue.

## Audio/Visual
- Simple knife sprite and rotating target.
- Hit particle effect and fail particle effect.
- Sound effects for throw, hit, and fail.

## Monetization
- Interstitial ads on fail.
- Interstitial ads every few level completions.
- Rewarded ad option to continue after failing a level.

## Technical Notes
- Knives instantiated at a fixed launcher point.
- Knives become children of the target on successful hit.
- Collision with knife stack triggers failure.
- Rotation managed by a single script on the target.

## Art
- Minimalist design.
- Flat colors.
- Default shapes for target and knives.

## Post-Launch Enhancements
- Skins for knives.
- Animated targets.
- Power-ups.

---

# FEATURES (Expanded)

## Core Features
- Throw knives at a rotating target.
- Knives stick on hit; collision with stuck knives causes failure.
- Single-tap control.
- Level progression with increasing difficulty.

## Scoring System
- 3-Star level rating:
  - 1 Star: Level completed.
  - 2 Stars: Clean run or accuracy conditions.
  - 3 Stars: Perfect run.
- Stars tied to performance and bonus objectives.

## Bonus Objects
- Tomatoes or similar items placed on targets.
- Hitting them grants bonus points.
- Never blocks required knife placement slots.

## Target Variants
- Multiple rotating targets.
- Orbiting mini-targets.
- Opposite or alternating rotation directions.

## Target Behavior
- Speed pulsation.
- Direction flipping mid-rotation.
- Jitter/wobble motion.
- Rotation acceleration curves.

## Knife Variants
- Cosmetic skins.
- Optional gameplay variants:
  - Heavy knife (bigger collision).
  - Slim knife (smaller collision).
  - Double-throw mechanic.

## Obstacles
- Metal plates that cause bounce/fail.
- Spike zones.
- Void/no-hit sectors.

## Combo System
- Precision-timing rewards.
- Consecutive hit streaks.
- Score multiplier.

## Additional Modes
- Endless mode.
- Daily challenges.
- Global/local leaderboards.

## Visual Feedback
- Screen flash on fail.
- Retry button animation.
- Target freeze on fail.

