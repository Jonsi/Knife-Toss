# AUDIO-701 Production Cue Register

## Runtime ownership

The persistent `IAudioPlayer` remains the single playback owner, so the existing
`SettingsService -> AudioListener.pause` mute path applies to every cue in this register.
Gameplay collision audio continues to subscribe to the authoritative typed
`KnifeImpactEventData`; no second collision presenter or shared-feedback translation was
added.

`KnifeImpactType.Stuck` resolves to `Target_StickCrack_01`. `KnifeImpactType.Bounced` resolves
to the single pre-mixed `Knife_Collision_Failure_01`. `LevelAudioFeedbackService` observes the
same typed bounced/failure state only to suppress its generic failure accent when the collision
cue already owns that failure beat. This makes one collision produce one audio cue even though
the later level-failure event still drives the popup and game flow.

## Cue family and mix hierarchy

All production files are 44.1 kHz, 16-bit WAV sources imported by Unity as force-mono,
preloaded PCM with `Decompress On Load`. They are short, frequently used 2D one-shots and do
not stream. The persistent `AudioSource` has no clip and `Play On Awake` is disabled.

| Priority | Semantic cue | Runtime file | Settings volume | Retrigger/concurrency rule |
|---|---|---|---:|---|
| Critical | Knife-on-knife failure collision | `Knife_Collision_Failure_01.wav` | 0.95 | 0.55 s minimum interval; the 0.52 s pre-mix is effectively one collision voice. |
| High | Final target shatter/completion impact | `Target_Shatter_01.wav` | 0.82 | Level completion is authoritative; no extra reward meaning is embedded. |
| High | Target stick plus short crack | `Target_StickCrack_01.wav` | 0.78 | 0.10 s minimum interval; repeated identical impact callbacks inside the window are dropped. |
| Elevated | Collectible/reward coin | `Reward_Coin_01.wav` | 0.82 collectible / 0.72 currency gain | Existing typed collectible and currency events remain authoritative. |
| Normal | Knife throw | `Knife_Throw_01.wav` | 0.72 | 0.08 s minimum interval; gameplay already allows only its authoritative throw cadence. |
| Normal | Knife load | `Knife_Load_01.wav` | Assigned library cue | Kept separate from the throw transient for future load presentation. |
| Normal | Currency spend | `Currency_Spend_01.wav` | 0.68 | Existing serialized currency transactions own emission. |
| Low | UI press | `UI_Click_01.wav` | 0.55 | Shared `ActionButtonPresenter` raises `Navigation / ui.press`; the UX-405 audio presenter keeps the 0.15 s identical-button cooldown authoritative. |
| High UI | Blocked/insufficient funds | `UI_Denied_01.wav` | 0.60 | Shared feedback duplicate cooldowns remain authoritative (0.75 s for blocked/insufficient funds). |
| Fallback | Non-collision level failure | `Failure_Accent_01.wav` | 0.58 | Skipped whenever a failure-counting bounced impact already played the collision cue. |

The target cue is a short woody low/mid transient and decays in about 0.33 seconds. The
collision cue adds a recorded knife clash and bright, sustained metal partials for 0.52 seconds;
it is roughly five decibels more prominent than the target cue after file and settings gain and
does not include a long musical sting. The collision file itself contains the restrained failure
weight, so a separate `LoseClip` is not layered over it.

Knife throw, target impact, and collision cooldowns are independent and reset when the stage
audio service is disposed. A cooldown accepts the exact boundary. Unknown or `OutOfView`
impacts remain silent. These gates bound repeated same-cue overlap without changing gameplay
or queueing delayed sounds.

## Source and license provenance

Every new source is CC0. Attribution is optional, but the project records it for auditability.

### Kenney Impact Sounds 1.0

- Creator/distributor: Kenney.
- Source page: `https://www.kenney.nl/assets/impact-sounds`
- Download used:
  `https://www.kenney.nl/media/pages/assets/impact-sounds/87b4ddecda-1677589768/kenney_impact-sounds.zip`
- Archive SHA-256: `029d734af1582474edf3a694d1b0cebc97c1c152f2f39fa34d4c2bafc5de77f8`.
- License: Creative Commons Zero 1.0 (`https://creativecommons.org/publicdomain/zero/1.0/`).
- Selected sources: `impactWood_medium_000.ogg`, `impactWood_light_002.ogg`,
  `impactMetal_heavy_000.ogg`, `impactWood_heavy_003.ogg`,
  `impactGlass_heavy_002.ogg`, and `impactSoft_heavy_002.ogg`.

### Kenney RPG Audio

- Creator: Kenney Vleugels / Kenney.
- Source page: `https://www.kenney.nl/assets/rpg-audio`
- Download used:
  `https://www.kenney.nl/media/pages/assets/rpg-audio/8e99002d76-1677590336/kenney_rpg-audio.zip`
- Archive SHA-256: `6dbeaf8544da958d8f2adcb4a4a4b76c1ade34a05f8ab9edccd327da7375f38b`.
- License: Creative Commons Zero 1.0.
- Selected sources: `knifeSlice2.ogg`, `drawKnife1.ogg`, and `handleCoins2.ogg`.

### StarNinjas sword clashes

- Creator: StarNinjas.
- Source page: `https://opengameart.org/content/20-sword-sound-effects-attacks-and-clashes`.
- Download used:
  `https://opengameart.org/sites/default/files/sword_clash_-_starninjas_0.zip`
- Archive SHA-256: `f363c80ea1627548d336370750651b0d2d092978148ecfb3642717c2efb54b6b`.
- License: Creative Commons Zero 1.0.
- Selected source: `sword_clash.5.ogg`, described by the author as two knives recorded against
  each other and edited in Audacity.

### Dustyroom Casual Game Sound - One Shot SFX Pack

- Creator: Dustyroom game studio.
- Local license: `Assets/Audio/Casual Game Sounds U6/CasualGameSounds/license.pdf`.
- License: Creative Commons Zero 1.0 for every included sample.
- Selected sources: `DM-CGS-40.wav` and `DM-CGS-34.wav`.

The prior root-level throw, target, collision, win, lose, click, bubble, and collectible clips
remain in the repository for other historical consumers, but AUDIO-701 no longer assigns them.
No license evidence was found beside those root-level files, so they were not treated as
production-provenance-safe replacements.

## Production transforms

- Throw/load: trimmed to 0.34 s, high-pass filtered, short fade, peak-limited.
- Stick/crack: layered medium/light wood impacts with the lighter transient delayed 18 ms;
  trimmed to 0.34 s and faded.
- Collision: layered `sword_clash.5` with the short Kenney heavy-metal transient; high-pass
  filtering preserves a sharp blade read, then the result is trimmed to 0.52 s and faded from
  0.38 s. Final measured peak is -1.41 dBFS.
- Shatter: layered wood and restrained bright break transient with a 12 ms offset; no coin or
  reward semantic is embedded because COMP-602 remains undecided.
- Reward/spend: the same recorded coin family; spend is reversed and pitched down for a related
  but directionally distinct cue.
- UI/failure: short fades and conservative gain only.

## Output checksums

| Output | SHA-256 |
|---|---|
| `Currency_Spend_01.wav` | `0c504b235166e5dc35e25f656883d65bb567419277b453ce9e84f6c9cd18a82f` |
| `Failure_Accent_01.wav` | `382465f1527d3b5d05fd8472f2b10a4341b3e4638ce0f36b85eb6ef65d411f63` |
| `Knife_Collision_Failure_01.wav` | `7286fbff7eaf615e3b363fc2e4bf357b1472851c1a387a35d0bc8e3dcc36d9d4` |
| `Knife_Load_01.wav` | `8cd7126d34e07f79de5caeb6784a1b94c59b83ac32498e6f9dad3f3a73848c16` |
| `Knife_Throw_01.wav` | `fb910cf6b918011d6c8de198bcd4fa3699cf8b64961b904961e05bda163590ef` |
| `Reward_Coin_01.wav` | `302838381aad77ca26aaaa359791dd080c6708aad2d312acc5a487ed7c154f39` |
| `Target_Shatter_01.wav` | `2aa2b48709d3a8996f5650ab8843d5022572468348bd6112f0ccfa3ecb44b416` |
| `Target_StickCrack_01.wav` | `9f96fc5ac7889559b83b5647d661fb32abe70e416aef74f36812c7ec78072212` |
| `UI_Click_01.wav` | `8aec8640af830d3e8e95bec5ba968c00e86c5c6e16d8931e3b8ff70e3f5d8d5a` |
| `UI_Denied_01.wav` | `5e0252f0008894aaa97c599d01f63d04db72acb17c441fc608c11775e697a97a` |

## Validation evidence

- Unity 6000.4.2f1, Android target, project `Knife-Toss-Android`, `StagePlay` active.
- Focused audio/UI tests passed 4/4 in job `f251ec6fd18a`; the updated production-catalog
  contract passed 1/1 in job `62e1265bc04d`.
- The final complete EditMode suite passed 199/199 with no failures or skips in job
  `cfa70aafadf9` (22.9 seconds).
- A real aligned throw produced a failure-counting `Bounced` impact, then
  `LevelFailedState`, `HasBounced=true`, and the approved frame-free popup. The recorded run
  repeated that real collision after a successful paid Retry reload.
- Play Mode exercised the authoritative typed target-stick and collectible-reward channels,
  the insufficient-funds UI cue, the paid Retry transaction, Menu recovery owner, and the
  shared `ActionButtonPresenter` `ui.press` path. The temporary balance top-up was fully spent
  by Retry and the original four-coin balance was restored before exit.
- While muted, shared feedback still scheduled on the persistent AudioSource but
  `AudioListener.pause` was true; the original unmuted state was restored immediately.
- `Assets/_Docs/Phase5VisualAudit/Captures/AUDIO701_ProductionCue_Motion.mp4` is a 48.2-second,
  15 fps, 1280×2276 H.264/AAC montage with actual captured Unity AudioSource output. Two
  collision failures, paid Retry, and Menu return were inspected frame-by-frame at cue onsets.
  The montage peak is -0.43 dBFS, so the exercised mix does not digitally clip.
- Final compilation and Console inspection were clean after the temporary capture helper was
  removed.
