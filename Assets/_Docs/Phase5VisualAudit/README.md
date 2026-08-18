# Phase 5 Frame-Free Failure Presentation

## Approved content and behavior

The failure presentation contains only:

- `STAGE {n} • LEVEL {n}` from authoritative live stage data.
- `GAME OVER`.
- The crossed-knives collision animation and restrained impact glow.
- `RETRY` with the configured live coin cost.
- `MENU`.

There is no description sentence, `Knife Collision` label, `Start Over` action, second Retry
action, decorative copy, or enclosing popup frame. Retry raises the existing paid-retry request,
spends the configured per-level coin cost through the existing currency transaction, and reloads
only after that transaction succeeds. Menu returns to Main Menu.

## Approved art direction

The product-owner-approved direction from 2026-08-16 is frame-free: gameplay remains visible
under one full-screen dark scrim while the live failure content floats over it. This is the
default visual rule for theme-neutral modal presentation: do not add a large parchment, shield,
wood, teal/gold, or biome-specific enclosure when the background dimmer and content hierarchy
already communicate modality.

The frame-free reference was generated from the approved Main Menu, Store, and Gameplay HUD
screens. Its built-in ImageGen output is
`exec-1d777392-3115-44f3-911c-b5db71fef176.png`. It established this hierarchy:

1. Small live stage/level progress.
2. Large illustrated `GAME OVER` wordmark.
3. Existing red/blue crossed-knives impact.
4. Wide orange paid Retry CTA with integrated live price.
5. Smaller teal Menu action.

The earlier layered charcoal/teal/gold shell and its fold animation are rejected. The runtime
`FailurePopupFoldAnimation.png`, `PopupSpriteSequenceAnimator.cs`, and their fold-only
validation captures were removed to the OS Trash and are no longer referenced by the project.
The older concept/source images remain only as historical audit material; StageFailPopup does
not use them.

## Implemented Unity composition

The production result is assembled in `Assets/Scenes/StagePlay.unity` while retaining the
`Assets/Prefabs/UI/PopupTemplate.prefab` link:

- `BlackBackground` is the only modal surface. It is a raycast-blocking black image at 0.82
  alpha, so the current world remains visible without competing with the actions.
- `Holder` retains no sprite, material, fill, or visible color.
- `Holder/Content/FailureContent` contains the live labels, glow, knives, Retry, and Menu.
- No `NeutralGameOverShell`, legacy popup frame, or shell sprite animator exists in the
  hierarchy.
- `PopupFadeInAnimation` remains the shared popup transition owner, while each visible element
  retains its own short staggered `UiElementAnimator` entrance.

The hierarchy combines live data with authored display art:

- Stage/level, Retry, and Menu remain live Unity text and use the same
  `LilitaOne-Regular SDF` display face already used by the approved Main Menu chrome.
- `GAME OVER` is the transparent authored sprite
  `Assets/Art/UI/Failure/GameOverTitle_v2.png`, not a flat runtime font treatment. Its gold
  `GAME`, ivory `OVER`, dark painted outline, and beveled depth follow the established
  `TOSS MADNESS` logo language while remaining biome-neutral.
- The shared `Assets/Prefabs/UI/Buttons/FailureActionButton.prefab` owns the hit area,
  interaction presentation, and live label for both actions.
- The builder instantiates the existing shared action prefab and does not rewrite it when it
  already exists.
- Retry uses `Assets/Art/UI/Failure/FailureRetryButton_Source.png`.
- Menu uses `Assets/Art/UI/Core/ButtonSecondary.png` at its native saturated teal rather than a
  muted tint. Both action labels use the same dark-brown Lilita treatment for a coherent pair.
- Retry nests `Assets/Prefabs/UI/Store/PricePillView.prefab`; its duplicate fill and border are
  hidden so the shared live coin icon and amount occupy the Retry surface's integrated pocket.
- All sixteen active stage configurations start Level 1 Retry at `50`, and `StageConfig`
  preserves a positive configured price instead of forcing Level 1 to zero.

The action collection is permanently bounded to Retry and Menu, so pooling or virtualization
does not apply. Both instances retain their shared prefab links and 48 dp touch-target padding.

## Motion

The crossed-knives animation remains at
`Assets/Art/UI/Failure/FailureKnivesAnimation.png` as sixteen ordered 512×512 frames. The knives
approach independently, collide, play the orange impact, and hold the final pose. It runs on
unscaled time after a 0.08-second delay at 0.028 seconds per frame because the modal pauses
gameplay. The detailed recipe remains in `FAIL502_KnifeAnimationProduction.md`.

Progress, title, glow, knives, Retry, and Menu use staggered element motion. The glow, knives,
and primary action retain restrained settled heartbeat motion. There is no panel animation.

## Validation contract

`Tools/Toss Madness/UI/Validate Approved Failure Popup` verifies:

- One interactive full-screen dark scrim at the approved opacity.
- A transparent holder with no frame sprite.
- No retired shell object or legacy baked failure atlas.
- Authoritative live stage/level typography and the approved transparent `GAME OVER` sprite.
- Exactly two shared action-prefab instances.
- The nested shared price-pill link and positive paid Retry cost.
- Ordered knife-animation frames and input-safe glow.
- Paid-retry channel, installer recovery bindings, and PopupTemplate linkage.

`StageFailPopupProductionTests` covers the same scene/prefab contract plus one-based progress,
configured cost presentation/spend, duplicate-input protection, knife frame order/final hold,
and shared prefab ownership.

Final frame-free Game-view evidence is stored as:

- `Captures/FAIL502_FrameFree_Final.png`
- `Captures/FAIL502_FrameFree_Motion.mp4`

The prettier title/type/button revision is stored as:

- `Captures/FAIL502_PrettyTitle_Final.png`
- `Captures/FAIL502_PrettyTitle_Motion.mp4`

Final validation on Unity 6000.4.2f1, Android target, repeated after the prettier title/type/button
revision on 2026-08-18:

- The production validator passed with no Console errors or warnings.
- The complete EditMode suite passed `195/195` with no failures or skips.
- `GameOverTitle_v2.png` was verified as an 1182×809 RGBA single sprite with transparency,
  mipmaps disabled, bilinear filtering, and uncompressed import.
- The current 1080×1920 Game-view still was inspected for hierarchy, title edges, typography,
  button balance, spacing, cost, and the absence of an enclosing frame.
- The 3-second, 30 fps, 90-frame Game-view recording was inspected across entry, stable hold,
  knife impact, and exit. It confirms the scrim-only modal treatment, clean wordmark alpha, and
  smooth shared popup transition over live gameplay.

## Generated-art provenance

The approved frame-free reference used built-in ImageGen with:

- Main Menu final review as style reference.
- Store final review as style reference.
- Gameplay HUD reference as layout/background reference.
- Exact copy: `STAGE 3 • LEVEL 1`, `GAME OVER`, `RETRY`, `50`, and `MENU`.
- Constraints: no enclosing panel, no fold layers, no descriptive copy, no extra actions, and no
  theme-specific decoration.

The output `exec-1d777392-3115-44f3-911c-b5db71fef176.png` remains an art-direction reference
only.

The production title revision from 2026-08-18 also used built-in ImageGen. Inputs were the
approved Main Menu final capture as the authoritative wordmark-style reference and the prior
frame-free Game Over capture as the composition reference. The prompt required exactly `GAME`
over `OVER`, gold and ivory beveled faces, the Main Menu logo's dark painted outline/depth,
mobile-scale readability, genuine transparency, and no panel, icons, scene art, or extra copy.
The selected generated source is `exec-c47d63fa-db73-404e-8dc6-f831312f8332.png`. Because the
service encoded its checkerboard as RGB despite two explicit alpha requests, an edge-connected
background extraction converted only the exterior checkerboard to alpha while preserving the
enclosed ivory letters. The verified RGBA workspace asset is
`Assets/Art/UI/Failure/GameOverTitle_v2.png`; runtime never references the external generated
source.
