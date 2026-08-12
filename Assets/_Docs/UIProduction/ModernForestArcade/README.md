# Modern Forest Arcade button production evidence

Date: 2026-08-12
Unity: 6000.4.2f1, Android target, MainMenu, fixed 1080×1920 Game View

## Integrated result

The approved Modern Forest Arcade family is imported under `Assets/Art/UI/Core`, referenced by
`UiSemanticTokens.asset`, packed into the valid V2 `CoreUI.spriteatlasv2`, and consumed by the
shared `ActionButtonPresenter`. `MainMenu` now uses the production Primary skin for Continue/Play
and Secondary skin for Stages while retaining screen-owned labels, callbacks, touch padding, and
navigation behavior. After live review, the MainMenu controls were reduced from the oversized
900×240 / 900×192 first pass to 780×168 Primary and 720×144 Secondary; the Secondary remains at
the 48 dp minimum hit height. The temporary validation surface uses the exact production token and sprite
references; it does not save a scene, mutate economy/player data, or invoke navigation.

The approved semantic palette serialized in `UiSemanticTokens.asset` is:

| Role | Hex |
|---|---|
| Ink | `#321F0E` |
| Ivory | `#F7E4BE` |
| Wood Dark / Mid / Light | `#011B17` / `#012F2A` / `#014F43` |
| Parchment | `#F5DFB4` |
| Sun Gold | `#FDA720` |
| Lagoon / Leaf | `#31A78D` / `#55C9B4` |
| Failure / Blocked | `#F25F35` / `#F98559` |
| Disabled | `#606861` |
| Success | `#2EB883` |
| Scrim | `#011B17CC` |

## Capture inventory

| File | Evidence |
|---|---|
| `Captures/ModernForestArcade_MainMenu.png` | Live MainMenu Play/Stages integration |
| `Captures/ModernForestArcade_ButtonFamily_Standard.png` | Full live-content family at 1080×1920 |
| `Captures/ModernForestArcade_ButtonFamily_ReducedMotion.png` | Nonpersistent reduced-motion Ready preview |
| `Captures/ModernForestArcade_ButtonFamily_Standard_Grayscale.png` | Grayscale derivative of the same Standard capture |
| `Captures/ModernForestArcade_ButtonFamily_Standard_360x640.png` | Phone-scale downsample of the same Standard capture |
| `Captures/ModernForestArcade_ButtonStates.mov` | Standard and reduced state sequence |
| `Captures/ModernForestArcade_ButtonStates_ContactSheet.png` | Eleven key frames decoded from the actual MOV |

The MOV is H.264 High, yuv420p/BT.709, 1080×1920, 30 fps, 198 frames, 6.6 seconds, fast-start.
It is a synthetic inspection timeline built from live Unity Game View samples held for 18 encoded
frames each—not wall-clock timing proof. Decoded key frames cover Standard Ready, Pressed,
pointer-exit cancellation, Loading with duplicate rejection, asynchronous cancellation, Blocked,
then reduced Ready, Pressed, static Loading, asynchronous cancellation, and Blocked with no shake.
The contact sheet was generated from the encoded movie and visually inspected.

The short-lived reflection write used to hold Blocked for capture changes only the temporary
Editor overlay's private countdown after the public typed `PresentBlocked` call. It does not alter
production code, tokens, scenes, prefabs, saves, or gameplay state.

## Automated and runtime validation

- Final focused button/token/presenter suite: 34/34 passed in 2.00 s, job `25c665706e45`.
- Complete EditMode suite: 163/163 passed in 19.89 s, job `19898ebd5736`.
- Compilation pipeline: zero entries after the full run.
- Asset missing-reference scan: zero results.
- Teardown: validation overlay count zero, MainMenu not dirty, exactly one EventSystem in Edit Mode.
- Loading probe: two attempts, one accepted, duplicate result false; interaction gate restored on cancel.
- Reduced Loading uses a static cue at alpha 1; reduced Blocked has zero anchored-position shake.

Play-stop logged one pre-existing `ModalInputBlockerPresenter` destroyed-object exception in its
shutdown path and the known Unity Account API warning; neither stack contains the button presenter
or validation harness. This record does not claim a warning-free Play-stop cycle.

## Limits

This is Editor evidence. It does not prove physical touch, player-build packing, Android ASTC
decode/memory, or device performance; those remain P2-010. The reduced-motion mode is an
Editor-only, nonpersistent preview. UX-406 still owns the unresolved shipping preference/default.
The temporary surface is representative button validation, not the generalized P2-009 UI gallery.
