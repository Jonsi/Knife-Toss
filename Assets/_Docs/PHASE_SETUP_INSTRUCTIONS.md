## Phase Setup Instructions

1. Open the Unity scene that uses `GameInstaller` and ensure all serialized references are assigned.
2. Verify configuration assets exist and are linked:
   - `Assets/Configurations/Audio/SoundSettings.asset`
   - `Assets/Configurations/VFX/VFXSettings.asset`
   - `Assets/Configurations/Animations/TargetHitAnimationConfig.asset`
   - `Assets/Configurations/Animations/KnifeLoadAnimationConfig.asset`
   - `Assets/Configurations/EventChannels/KnifeThrownEventChannel.asset`
3. Prefab wiring:
   - `Target.prefab`: assign `TargetHitFeedback` component fields (config, transform, sprite renderer) and reference it in `TargetView`.
   - `Knife.prefab`: assign `KnifeLoadAnimator` component fields (config, sprite renderer, optional visual root) and reference it in `KnifeView`.
4. Scene objects:
   - `GameInstaller` serialized fields: audio settings, VFX settings, audio player, event channels (collision, knife impact, knife thrown, collectible hit, level completed/failed), retry/restart channels, stage and UI references.
   - `AudioPlayer` GameObject: ensure it has an `AudioSource` configured for SFX.
5. Asset checks:
   - Particle prefabs referenced in `VFXSettings` (e.g., knife hit target particles) are present under `Assets/Prefabs`.
   - Audio clips referenced in `SoundSettings` exist under `Assets/Audio`.
6. Assembly definition:
   - `Assets/Scripts/Game/Game.asmdef` includes references to `DOTween` and `DOTween.Modules`.
7. Playmode smoke test:
   - Run a level, confirm throw, impact, collectible, and level result sounds play.
   - Confirm knife hit particles spawn on stick events.
   - Confirm target hit shake/tint and knife load animation play.


