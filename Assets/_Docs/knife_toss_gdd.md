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

## Build Requirements
- Android build.
- Ad SDK integration.
- Basic analytics for level tracking.

