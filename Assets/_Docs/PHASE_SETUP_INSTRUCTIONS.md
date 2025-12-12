## Phase 10 Setup Steps (Polish & Performance)

1) Scene prerequisites
- GameInstaller active LifetimeScope (already required).
- EventSystem for UI raycast checks (already required).

2) Event channels (assets)
- LevelStartedEventChannel: ensure asset exists under Configurations/EventChannels/ and is assigned to GameInstaller (used for automatic knife cleanup when levels start).

3) Assign GameInstaller scene references
- KnifeThrowerView → existing scene object with knife prefab assigned (used for pooling).
- LevelRoot → transform that holds spawned level content.
- StageFailPopup → fail popup GameObject.
- UIController → UI controller GameObject.
- All other references from previous phases.

4) Configure PoolSettings asset
- Create PoolSettings.asset using Assets > Create > Knife Toss > Pooling > Pool Settings
- Assign KnifePrefab to your knife prefab with KnifeView component
- Assign KnifeHitTargetParticlesPrefab to your knife hit particle effect
- Optionally assign CollectibleHitParticlesPrefab to collectible hit particles
- Configure warmup counts and max pool sizes as needed

5) Update target configurations
- In StagesConfigAsset, update existing TargetConfig instances with new rotation behavior fields:
  - RotationBehaviorType: set to "Constant", "Pulsating", "Flipping", or "Curve".
  - For Pulsating: set PulsateAmplitude and PulsateFrequency.
  - For Flipping: set FlipMinInterval and FlipMaxInterval.
  - For Curve: assign AnimationCurve to RotationCurve field.
- Add PreStuckKnives list if desired: specify Angle and PrefabOverride for obstacle knives.

6) Verify pooling setup
- Pooling is automatic - KnifePoolProvider and ParticlePoolProvider register via DI.
- Pool parents are created automatically as "KnifePool_{prefab.name}" and "ParticlePool_{prefab.name}".
- Level cleanup happens automatically when new levels start.

7) Playmode validation
- Throw knives: verify they're reused from pool (no new GameObjects created).
- Change levels: verify automatic cleanup (pooled objects return to their parents).
- Test rotation behaviors: different targets should rotate with constant, pulsating, flipping, or curve patterns.
- Test pre-stuck knives: should appear at configured angles on targets.
- Performance: monitor object counts - pooling should prevent accumulation of unused objects.</contents>
</xai:function_call 1>2. Event channels (assets)