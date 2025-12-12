## Phase 10 Setup Steps (Polish & Performance)

1) Scene prerequisites
- GameInstaller active LifetimeScope (already required).
- EventSystem for UI raycast checks (already required).

2) Event channels (assets)
- KnifeImpactEventChannel: ensure asset exists under Configurations/EventChannels/ and is assigned to GameInstaller (used for VFX pooling).

3) Assign GameInstaller scene references
- KnifeThrowerView → existing scene object (provides knife prefab reference for pooling).
- LevelRoot → transform that holds spawned level content.
- StageFailPopup → fail popup GameObject.
- UIController → UI controller GameObject.
- All other references from previous phases.

4) Configure VFX settings asset
- VFXSettings.asset: update KnifeHitTargetParticlesPrefab and CollectibleHitParticlesPrefab to reference ParticleSystem prefabs (changed from GameObject to ParticleSystem type).

5) Update target configurations
- In StagesConfigAsset, update existing TargetConfig instances with new rotation behavior fields:
  - RotationBehaviorType: set to "Constant", "Pulsating", "Flipping", or "Curve".
  - For Pulsating: set PulsateAmplitude and PulsateFrequency.
  - For Flipping: set FlipMinInterval and FlipMaxInterval.
  - For Curve: assign AnimationCurve to RotationCurve field.
- Add PreStuckKnives list if desired: specify Angle and optional PrefabOverride for obstacle knives.

6) Verify pooling setup
- KnifePoolProvider and ParticlePoolProvider are auto-registered via DI; ensure knife prefab is assigned in KnifeThrowerView for pooling to work.
- Particle pools require VFXSettings to have valid ParticleSystem references.

7) Playmode validation
- Throw knives and verify pooling: knives should be reused from pool without new instantiations visible in scene hierarchy.
- Impact targets and collectibles: particle effects should spawn and auto-return to pool after playing.
- Test rotation behaviors: configure different targets with varying rotation types and verify sinusoidal oscillation, random direction flips, and curve-following speeds.
- Test pre-stuck knives: level should spawn obstacle knives at configured angles; they should block thrown knives and clean up properly on target destruction.
- Performance check: monitor object counts in profiler during gameplay to confirm pooling reduces allocations.</contents>
</xai:function_call 1>2. Event channels (assets)