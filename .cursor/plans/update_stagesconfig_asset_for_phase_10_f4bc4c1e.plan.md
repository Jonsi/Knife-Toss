---
name: Update StagesConfig Asset for Phase 10
overview: Update StagesConfig.asset to have 2 stages with 4 levels each, introducing all Phase 10 features progressively with cumulative difficulty increases.
todos:
  - id: stage1-level1
    content: "Configure Stage 1 Level 1: Single target, constant rotation, collectibles introduction, low difficulty"
    status: pending
  - id: stage1-level2
    content: "Configure Stage 1 Level 2: Orbit movement, pulsating rotation introduction, medium difficulty"
    status: pending
  - id: stage1-level3
    content: "Configure Stage 1 Level 3: Multiple targets, flipping rotation, pre-stuck knives introduction"
    status: pending
  - id: stage1-level4
    content: "Configure Stage 1 Level 4: Curve rotation introduction, complex patterns, high difficulty"
    status: pending
  - id: stage2-level1
    content: "Configure Stage 2 Level 1: Recap with harder settings, all rotation types"
    status: pending
  - id: stage2-level2
    content: "Configure Stage 2 Level 2: Triple target challenge, varied behaviors"
    status: pending
  - id: stage2-level3
    content: "Configure Stage 2 Level 3: Complex patterns, maximum pre-stuck knives"
    status: pending
  - id: stage2-level4
    content: "Configure Stage 2 Level 4: Final challenge, all features at peak difficulty"
    status: pending
---

# Update StagesConfig.asset for Phase 10 Features

## Current State

- Already has 2 stages with 4 levels each (correct structure)
- Missing Phase 10 fields: RotationBehaviorType, rotation behavior parameters, PreStuckKnives
- All targets currently use default "Constant" rotation behavior
- Some levels have collectibles, but not systematically distributed

## Feature Introduction Strategy

### Stage 1: Learning & Introduction

**Level 1-1**: Foundation

- 1 target, stationary (OrbitRadius: 0)
- Constant rotation (default)
- Low difficulty: 4 knives to break, rotation speed 30-40
- Introduce collectibles: 1-2 collectibles on target
- Extra knives: 2

**Level 1-2**: Movement Introduction

- 1 target with orbit movement
- Constant rotation (faster: 50-60)
- Medium difficulty: 5 knives to break
- Orbit speed: 40-50 degrees/sec
- Introduce Pulsating rotation: 1 target with pulsating behavior (amplitude 10-15, frequency 0.5-1.0)
- Extra knives: 2

**Level 1-3**: Multiple Targets & Obstacles

- 2 targets with different patterns
- Introduce Flipping rotation: 1 target with flipping behavior (intervals 1.5-2.5s)
- Introduce Pre-Stuck Knives: 1-2 pre-stuck knives on one target as obstacles
- Higher difficulty: 4-5 knives per target
- Faster speeds: rotation 70-90, orbit 50-70
- Extra knives: 3

**Level 1-4**: Advanced Patterns

- 2 targets with complex patterns
- Introduce Curve rotation: 1 target using AnimationCurve
- Combine features: pre-stuck knives + different rotation behaviors
- High difficulty: 5-6 knives per target
- Fast speeds: rotation 100-120, orbit 60-80
- Extra knives: 3

### Stage 2: Mastery & Challenge

**Level 2-1**: Stage 1 Recap with Harder Settings

- 2 targets, all rotation types represented
- Faster speeds than Stage 1 Level 4
- More pre-stuck knives: 2-3 per target
- 5-6 knives per target
- Extra knives: 3

**Level 2-2**: Triple Target Challenge

- 3 targets with varied behaviors
- Mix of all rotation types
- Pre-stuck knives on multiple targets
- Higher speeds: rotation 120-150, orbit 70-100
- 4-6 knives per target
- Extra knives: 3

**Level 2-3**: Complex Patterns

- 3 targets with complex orbit patterns
- All rotation behaviors in use
- Maximum pre-stuck knives: 3-4 per target
- Very fast speeds: rotation 150-180, orbit 90-120
- 5-7 knives per target
- Extra knives: 4

**Level 2-4**: Final Challenge

- 3-4 targets (if supported) or 3 targets with maximum difficulty
- All features combined at peak difficulty
- Maximum pre-stuck knives: 4+ per target
- Peak speeds: rotation 180-220, orbit 100-140
- 6-8 knives per target
- Extra knives: 4

## Implementation Details

### Rotation Behavior Values

- **Pulsating**: Amplitude 10-30, Frequency 0.5-2.0 Hz
- **Flipping**: MinInterval 1.0-2.0s, MaxInterval 2.0-4.0s
- **Curve**: Create AnimationCurve in Unity editor with varying speed over time (e.g., slow-fast-slow pattern)

### Pre-Stuck Knife Placement

- Angles: Distribute evenly (0°, 90°, 180°, 270°) or strategic positions
- Start with 1-2 knives, increase to 3-4+ in later levels
- Use on targets with faster rotation for added challenge

### Collectible Distribution

- Stage 1: 1-2 collectibles per level (introduction)
- Stage 2: 2-3 collectibles per level (reward for difficulty)
- Place on targets that require more precision

### Difficulty Progression Metrics

- **Knives per target**: 4 → 5 → 6 → 7 → 8
- **Rotation speed**: 30 → 50 → 90 → 120 → 180 → 220
- **Orbit speed**: 0 → 40 → 60 → 80 → 100 → 140
- **Number of targets**: 1 → 1 → 2 → 2 → 2 → 3 → 3 → 3
- **Pre-stuck knives**: 0 → 0 → 1-2 → 2 → 2-3 → 3 → 3-4 → 4+

## File to Update

- `Assets/Configurations/Gameplay/StagesConfig.asset`

## Unity Editor Steps Required

1. Open StagesConfig.asset in Unity Inspector
2. For each TargetConfig in each level:

- Set RotationBehaviorType field ("Constant", "Pulsating", "Flipping", or "Curve")
- Configure behavior-specific parameters
- Add PreStuckKnives list entries with angles
- Adjust collectibles as needed

3. Create AnimationCurve assets for Curve rotation behavior (or set directly in inspector)
4. Verify KnifeCount auto-calculates correctly (sum of KnivesToBreak + ExtraKnives)

## Validation

- Ensure all 8 levels (2 stages × 4 levels) are properly configured
- Verify each rotation behavior type appears at least once
- Confirm difficulty increases cumulatively across levels
- Check that pre-stuck knives don't exceed reasonable limits (4-5 max per target)
- Validate collectible placement doesn't interfere with gameplay