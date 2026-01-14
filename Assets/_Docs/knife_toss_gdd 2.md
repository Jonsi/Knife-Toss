Knife Toss GDD

Overview

An arcade skill game where the player throws knives at a rotating target. The goal is to stick all knives in the target without hitting previously placed knives.

Core Loop

Throw: Player taps to throw a knife.

Result: Knife moves upward and either sticks to the target or causes a fail by hitting another knife.

Progression:

Success: If all knives are placed, the game immediately transitions to the next level in the Stage.

Stage Complete: If the final level of the Stage is beaten, the Stage is cleared and progress is saved.

Fail State: On collision, the player fails. They must Retry the Stage from Level 1 or Continue (Watch Ad) to resume the current level and save their streak.

Gameplay

The target rotates at varying speeds.

Knives thrown stick to empty segments of the rotating target.

Collision with an existing knife results in failure.

Level & Stage Structure

Stage-Based Progression: The game is divided into Stages (e.g., Forest, Desert).

The Gauntlet Rule: A Stage consists of multiple levels (e.g., 5 levels). The player must complete all levels in the stage in one run to unlock the next Stage.

Difficulty Scaling: Target rotation speed and knife count increase with every level within a stage.

Controls

Single tap to launch a knife.

Input locked during knife flight until resolution.

UI

Minimal UI with Stage/Level counter and remaining knife count.

Main Menu with Play and Sound options.

Fail screen with:

Restart Stage ( Lose progress).

Continue (Watch Ad to keep current Stage progress).

Level Complete screen removed for seamless flow.

Audio/Visual

Simple knife sprite and rotating target.

Hit particle effect and fail particle effect.

Sound effects for throw, hit, and fail.

Monetization

Interstitial ads on Stage Completion or Stage Failure.

Rewarded ad option to continue a run after failing a specific level inside a stage.

Technical Notes

Knives instantiated at a fixed launcher point.

Knives become children of the target on successful hit.

Collision with knife stack triggers failure.

Rotation managed by a single script on the target.

Art

Minimalist design.

Flat colors.

Default shapes for target and knives.

Post-Launch Enhancements

Skins for knives.

Animated targets.

Power-ups.

FEATURES (Expanded)

Core Features

Throw knives at a rotating target.

Knives stick on hit; collision with stuck knives causes failure.

Single-tap control.

Level progression with increasing difficulty.

Fruit Selection and Pricing

To maintain the minimalist design and flat colors specified in the GDD, these five fruits are chosen for their high visual contrast against the nine different background themes.

Tomato (Value: 1 Coin): The standard "Bonus Object". It is the baseline reward for basic accuracy.

Lemon (Value: 5 Coins): A bright yellow fruit that stands out against darker biomes like the Volcanic Cave or Crystal Cavern.

Kiwi (Value: 10 Coins): Features more detail (seeds), representing an uncommon reward found in mid-tier levels.

Cherries (Value: 25 Coins): Small, double targets that are harder to hit, fitting for high-difficulty Target Behaviors.

Pomegranate (Value: 100 Coins): The "Jackpot" fruit. Its complex shape and deep red color make it a rare, high-value prize.

Placement Strategy by Level Difficulty

The strategy leverages Target Variants and Obstacles to ensure that higher rewards require higher skill.

1. Thematic Biome Alignment

To create a cohesive feel, match fruit types to the nine backgrounds and targets:

Common (Levels 1-10): Place Tomatoes on the basic Wooden Target in the Misty Forest background.

Uncommon (Levels 11-30): Use Lemons and Kiwis in the Jungle or Underwater ruins, where the Target Behavior begins to include "speed pulsation".

Exotic (Levels 30+): Reserve Cherries and Pomegranates for the Golden Palace or Crystal Cavern, where lighting and background complexity are highest.

2. Risk-Based "Trap" Placement

Use Obstacles to make high-value fruits a gamble:

Metal Plate Guarding: Place a Pomegranate directly adjacent to a metal plate. Since hitting a metal plate causes a "bounce/fail," players must be extremely precise to collect the 100 coins without ending their run.

The Narrow Gap: Place Cherries inside a very small empty segment between two "void/no-hit sectors". This forces the player to use their timing skills to hit the small fruit without failing.

3. Behavior-Based Spawning

Link the rarest fruits to the most erratic Target Behaviors:

Direction Flipping: Spawn a Kiwi only when the target is set to "direction flipping mid-rotation." The unpredictable movement makes the 10-coin reward harder to earn.

Jitter/Wobble: Place Cherries on targets with "jitter/wobble motion." The small size of the fruit combined with the shaky target movement creates a high-skill challenge.

4. Bonus Level "Frenzy"

In Bonus Levels, ignore the obstacles and fill the target with high-value fruits:

The Orbiting Harvest: Use the "Orbiting mini-targets" variant. Place a Tomato on the main target and a Lemon or Cherry on each orbiting mini-target. This encourages the player to time their throws to hit the moving outer rewards for a massive Score Multiplier and consecutive hit streak.

5. Technical Implementation Note

Per the GDD, ensure that these bonus objects "never block required knife placement slots". When the game instantiates the fruits at the start of a level, they should be placed in segments that are not already occupied by metal plates or spike zones, ensuring the level remains beatable.

Target Variants

Multiple rotating targets.

Orbiting mini-targets.

Opposite or alternating rotation directions.

Target Behavior

Speed pulsation.

Direction flipping mid-rotation.

Jitter/wobble motion.

Rotation acceleration curves.

Knife Variants

Cosmetic skins.

Optional gameplay variants:

Heavy knife (bigger collision).

Slim knife (smaller collision).

Double-throw mechanic.

Power-Ups

Super Knife: Allows the player to destroy the target with a single hit. While the core loop typically requires sticking a specific number of knives into the target to complete a level, this power-up provides a shortcut by bypassing the standard "hit-and-stick" mechanic.

Multiple Knives: Enables the player to throw multiple knives simultaneously. This aligns with the "Double-throw mechanic" listed as an optional gameplay variant to help manage the required knife count for higher difficulty levels.

Time Dilator (Slow Motion): Temporarily slows down the target's rotation speed. Since level difficulty scales by speeding up the target, this provides a strategic advantage by making it easier to time throws accurately.

Precision Aim: Temporarily unlocks manual trajectory control. Instead of the standard fixed-angle throw, the player can rotate the knife to adjust its launch angle. This allows the player to target specific, tight gaps in the target's defense that are inaccessible with a standard straight throw.

Obstacles

Metal plates that cause bounce/fail.

Spike zones.

Void/no-hit sectors.

Combo System

Precision-timing rewards.

Consecutive hit streaks.

Score multiplier.

Additional Modes

Endless mode.

Daily challenges.

Global/local leaderboards.

Visual Feedback

Screen flash on fail.

Retry button animation.

Target freeze on fail.

LEVEL DESIGN SPECIFICATIONS

This section defines the difficulty profiles for "Knife Toss" levels. These profiles act as guidelines for creating LevelConfig and TargetConfig assets to ensure a consistent progression curve.

Key Configuration Parameters

Level Parameters (LevelConfig)

Knives To Throw: Total knives required to complete the level (derived from Target HP).

Extra Knives: Allowed misses (usually 0 for standard play, maybe 1-2 for tutorial).

Target Parameters (TargetConfig)

HP (KnivesToBreak): How many hits the target takes.

Scale (BaseScale): Size of the target.

1.0: Standard

< 1.0: Harder (smaller hit area)

> 1.0: Easier (usually) or Boss (large but crowded)

Obstacles (PreStuckKnives): Number and arrangement of pre-existing knives.

Values: List of angles (e.g., [0, 90, 180, 270]).

Rotation Behaviors (ILocalBehaviorConfig)

1. Constant Spin (ConstantSpinBehaviorConfig)

Speed: Rotation speed in degrees per second.

Positive = Clockwise

Negative = Counter-Clockwise

2. Flipping Spin (FlippingSpinBehaviorConfig)

Base Speed: Speed during rotation.

Interval (Min/Max): Random time between direction flips.

3. Curve Spin (CurveSpinBehaviorConfig)

Curve: AnimationCurve defining speed over time (allows for Stop & Go, Acceleration, etc.).

Difficulty Profiles

1. Tutorial / Very Easy (Stage 1: Levels 1-3)

Goal: Introduce mechanics.

Behavior: Constant Spin

Speed: 30 - 50 (Very predictable)

HP: 3 - 5

Obstacles: 0

Scale: 1.0

Notes: No surprises. Pure aim practice.

2. Easy (Stage 1: Levels 4-10, Stage 2: Early)

Goal: Basic challenge.

Behavior: Constant Spin (Faster) or Flipping Spin (Lazy)

Speed: 60 - 90

HP: 6 - 8

Obstacles: 0 - 2 (Widely spaced)

Scale: 1.0

Notes: Introduction of obstacles requiring timing.

3. Normal (Mid-Game)

Goal: Rhythm disruption.

Behavior: Flipping Spin

Speed: 90 - 120

Flip Interval: 2.0s - 4.0s (Predictable flips)

HP: 8 - 12

Obstacles: 2 - 3 (Asymmetrical placement)

Scale: 0.9 - 1.0

Notes: Player must anticipate direction changes or wait for a safe opening.

4. Hard (Late Game / Challenge Levels)

Goal: Precision and Reflexes.

Behavior: Flipping Spin (Aggressive) or Curve Spin (Stop & Go)

Speed: 120 - 180

Flip Interval: 0.5s - 1.5s (Frequent changes)

HP: 10 - 15

Obstacles: 3 - 5 (Tight clusters)

Scale: 0.8 - 0.9 (Small targets)

Notes: "Stop & Go" patterns where the target pauses briefly, baiting a throw, then spins rapidly.

5. Insane / Boss

Goal: Pattern memorization and perfect execution.

Behavior: Curve Spin (Complex)

Speed: Variable (0 to 300+ bursts)

HP: 15 - 25

Obstacles: 5 - 8

Scale: Variable (0.7 tiny or 1.2 massive wall)

Notes:

The "Wall": Large target, high HP, many obstacles.

The "Gnat": Tiny target, super fast spin.

The "Trickster": Slow spin that suddenly accelerates 3x when a knife is thrown (requires anticipation).

Configuration Constraints & Safety Limits

To ensure playable and valid levels, adhere to the following constraints when configuring LevelConfig and TargetConfig.

1. Target Health & Capacity (Physical Limits)

The target has a limited circumference. Attempting to stick too many knives will result in unavoidable collisions (impossible levels).

Max Capacity Rule: (KnivesToBreak + PreStuckKnives.Count) <= MaxCapacity

Capacity by Scale:

Scale 1.0: ~25 Knives Max

Scale 0.8: ~18 Knives Max

Scale 0.6: ~12 Knives Max

Scale 1.2: ~30 Knives Max

Recommendation: Keep total knife count at least 20% below max capacity for "Fair" difficulty.

2. Spatial Boundaries (Screen Space)

Targets must remain visible and reachable. Assumes standard Portrait aspect ratio (9:16).

Position Offset (InitialOffset):

X Range: -2.2 to 2.2 (Horizontal bounds)

Y Range: -1.0 to 3.5 (Vertical bounds; keep above thrower, below UI)

Scale Limits:

Min Scale: 0.5 (Hitbox becomes pixel-perfect difficult)

Max Scale: 1.5 (Begins to obscure UI or clip screen edges)

3. Obstacle Placement (Pre-Stuck Knives)

Angle Separation: Maintain at least 15 degrees between pre-stuck knives.

Safe Zones: Ensure there are gaps large enough for the player to land KnivesToBreak (e.g., if player needs to throw 5 knives, don't block 90% of the circle).

4. Rotation Speed

Playable Cap: ±360 degrees/second (1 full rotation per second) is extremely fast.

Standard Range: 45 to 180 degrees/second.

Human Reaction Limit: Speeds > 200 require anticipation rather than reaction.

Progression Logic (Draft)

Stage 1 (Forest): Profiles 1 & 2. Mostly Constant Speed.

Stage 2 (Desert): Introduce Flipping.

Stage 3 (Ice): Introduce Variable Speed (Curves/Slippery feel).

Stage 4 (Space): Small targets, erratic movement.