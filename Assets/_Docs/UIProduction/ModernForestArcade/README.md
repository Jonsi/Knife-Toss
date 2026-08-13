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

The full MainMenu production pass is documented under `MainMenu/README.md`. It replaces the
screen's blocked navigation, selector-arrow, currency-chip, and stage-sign chrome with approved
surfaces plus bespoke illustrated icon sprites, preserves all action/value owners, and adds
fixed-preset safe-layout, phone-size, grayscale, state-width, and encoded transition evidence.

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

## Typography

The former Lilita One outline / Liberation Sans pairing has been replaced in the semantic token
asset and the live MainMenu surface:

- Display, Heading, Action, and Value: Barlow Condensed ExtraBold.
- Body, Legal, and Caption: Nunito Sans SemiBold.
- Both source TTFs and their SIL Open Font License 1.1 texts are vendored under
  `Assets/Art/UI/Fonts`; the static TMP SDF assets contain 191 curated Latin/UI characters.
- MainMenu text uses the shared clean SDF materials with no heavy outline. The choice follows the
  measured reference requirement for a condensed, open-counter face that is bold without being
  bulbous.

Source binaries were fetched from the official Google Fonts repository / its recorded upstream:

- Barlow Condensed ExtraBold SHA-256:
  `724c9c25952d5f4a2d87185d9767aa006144c5f0d944dc05bf7d5d603551c260`
- Nunito Sans SemiBold SHA-256:
  `e51e66d6a3e519f70b76a156066182406e464b1e8731fba28d16372b6b03fab0`

## Capture inventory

| File | Evidence |
|---|---|
| `Captures/ModernForestArcade_MainMenu.png` | Live MainMenu Play/Stages integration |
| `Captures/ModernForestArcade_MainMenu_Typography.png` | Live MainMenu with production Barlow Condensed / Nunito Sans typography |
| `MainMenu/Captures/MainMenu_IllustratedIcons_Standard.png` | Live MainMenu with Settings, Store, Missions, Remove Ads, and Currency illustrated sprites |
| `MainMenu/Captures/MainMenu_IllustratedIcons_Selector.png` | Temporary Play Mode display proof of the illustrated Previous/Next sprites |
| `MainMenu/Captures/MainMenu_IllustratedIcons_v2.png` | Live review revision with the simplified Remove Ads mark and CuzGames `CG` currency monogram |
| `MainMenu/Captures/MainMenu_RemoveAds_v3.png` | Live correction replacing the fragmented prohibition ring with one crossed-video card |
| `MainMenu/Captures/MainMenu_FinalReviewed_v4.png` | Final live review with the larger frameless crossed-play Remove Ads glyph |
| `MainMenu/Captures/MainMenu_FinalReviewed_v4_360x640.png` | Phone-size legibility and hierarchy review |
| `MainMenu/Captures/MainMenu_FinalReviewed_v4_Grayscale.png` | Grayscale silhouette and value-hierarchy review |
| `MainMenu/Captures/MainMenu_RemoveAds_ADS_Approved_v5.png` | Live proof of the user-approved large `ADS` mark with red-orange slash on top |
| `MainMenu/Captures/MainMenu_KnifeTarget_v1.png` | Live MainMenu with the newly authored default knife and throwing stump |
| `MainMenu/Captures/MainMenu_KnifeFloat_v1.mov` | Wall-clock recording of the production knife's restrained selector float |
| `MainMenu/Captures/MainMenu_KnifeFloat_v1_ContactSheet.png` | Eight decoded frames inspected from the encoded knife-float movie |
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

The later MainMenu vertical-slice pass adds focused 41/41 (`df57866a1563`) and complete EditMode
173/173 (`5d04452104b3`) results. See `MainMenu/README.md` for exact geometry and runtime evidence.

The typography integration was revalidated with the complete current EditMode suite: 173/173
passed in 20.6 s, job `4126955dbacb`. MainMenu then reported zero missing references, zero legacy
Lilita/Liberation text assignments, 13/13 text components with valid fonts, one EventSystem, clean
semantic-token validation, and no Console errors.

The illustrated icon integration replaced the temporary procedural renderer with seven 256x256
RGBA production sprites and a dedicated V2 atlas. The complete current EditMode suite passed
168/168 in 21.2 s, job `573bcc335a97`; the lower count reflects removal of seven obsolete
procedural-glyph cases and addition of two production sprite/atlas cases.

The targeted v2 art revision changes only Remove Ads and Currency: the ad mark is a simplified
crossed video card, and the coin now carries the CuzGames `CG` monogram. The five already-approved
icons still build byte-identically from v1. The complete EditMode suite passed 168/168 in 20.99 s,
job `ec33f4acf66e`, after the revision.

The v3 review correction changes only Remove Ads again: its small-size prohibition ring was
replaced by one compact video card crossed by a continuous coral slash. Atlas padding increased to
8 px with rotation/tight packing disabled and alpha dilation enabled to prevent neighboring-icon
bleed. The other six icon PNGs remained byte-identical. Focused coverage passed 5/5, job
`4742f1c895ce`, and the complete EditMode suite passed 170/170 in 21.40 s, job `c0eff8212144`.

The final v4 proportion correction removes the redundant inner video-card frame and scales one
ivory play triangle plus one continuous coral slash to the available socket. The other six icons
remain byte-identical. Full-size, 360×640, and grayscale live-menu reviews found no remaining
proportion, alignment, hierarchy, legibility, or silhouette issue. Focused production coverage
passed 7/7, job `4e726650ceaa`, and the complete EditMode suite passed 170/170, job
`9e4c1019f671`.

The user-approved v5 correction replaces the rejected play-mark direction with large warm-ivory
`ADS` lettering and a red-orange slash on top. It has no inner card or secondary frame. The six
other icons remain byte-identical; focused production coverage passed 7/7 in 1.32 s, job
`4afc72bb192a`, and the complete EditMode suite passed 170/170 in 20.80 s, job
`c7c82b0a2822`. The live 1080×1920 result was visually inspected at native and enlarged scale.

The later hero-art pass replaces the legacy default knife and MainMenu throwing stump with
separately generated Modern Forest Arcade production sprites. Existing GUIDs and the knife's
serialized sprite ID remain stable; 3× source density preserves its 0.36×1.25 gameplay world size.
The 12 unlockable skins remain unchanged. Focused asset/scene coverage passed 7/7, job
`944772022707`, and the complete EditMode suite passed 170/170 in 20.50 s, job `f790a456c023`.

Play-stop logged one pre-existing `ModalInputBlockerPresenter` destroyed-object exception in its
shutdown path and the known Unity Account API warning; neither stack contains the button presenter
or validation harness. This record does not claim a warning-free Play-stop cycle.

## Limits

This is Editor evidence. It does not prove physical touch, player-build packing, Android ASTC
decode/memory, or device performance; those remain P2-010. The reduced-motion mode is an
Editor-only, nonpersistent preview. UX-406 still owns the unresolved shipping preference/default.
The temporary surface is representative button validation, not the generalized P2-009 UI gallery.
