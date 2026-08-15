# Gameplay HUD element breakdown

Status: reference approved on 2026-08-15  
Approved reference: `GameplayHUD_Reference_v3.png`  
Scope: production planning only; no separated sprites, prefab edits, scripts, scene changes, or runtime behavior changes are included.

## Visual contract

- Preserve the current jungle/waterfall gameplay environment, target, stuck knives, incoming knife, and gameplay rules.
- Keep the central throw lane free of persistent UI and decorative overlays.
- Use the approved Modern Forest Arcade material language: warm carved wood, dark leather, ivory text, gold hardware/highlights, and restrained teal accents.
- Keep all reusable HUD chrome stage-neutral. Leaves, flowers, vines, and other biome decoration belong only to the gameplay environment.
- Keep owned, unavailable, cooldown, and disabled states opaque and readable; do not communicate state through transparency alone.
- Render every value and label with Unity TMP. No production sprite may contain baked text or numbers.
- Treat the approved reference as a hierarchy and material target, not a source of production pixels.

## Composition and ownership

The review coordinates below identify approved reference zones; final anchors must be resolved inside `SafeAreaRoot` and validated at supported portrait aspect ratios rather than copied as literal screen pixels.

| Element | Approved reference zone | Production boundary | Existing runtime owner | Text/state binding |
|---|---|---|---|---|
| Settings | Top-left, review point `9.4%, 2.8%` | Reuse `Assets/Prefabs/UI/Buttons/MenuButton.prefab`; apply the approved Icon Socket surface and `Assets/Art/UI/Icons/IconSettings.png` | `SettingsButtonView` | No baked text; existing settings action remains authoritative |
| Stage progress plaque | Top-center, review point `51.3%, 4%` | New reusable `StageProgressPlaque` prefab composed from one blank shell, a TMP stage label, and `Assets/Prefabs/UI/ProgressBarSlot.prefab` instances | `StageProgressBarView` and `LevelHudViewModel` | Stage label and slots remain runtime-driven; no numeric fraction is shown |
| Currency chip | Top-right, review point `88.9%, 2.4%` | Directly reuse `Assets/Prefabs/UI/Coin/CoinUiDisaply.prefab` with root placement/scale overrides only | `CoinsUiDisplay` and `CurrencyBalanceDisplay` | Existing shared surface, icon, TMP balance, count tween, flash, and accumulation destination remain authoritative |
| Knife magazine | Lower-left, review point `15.9%, 82.5%` | New reusable vertical `KnifeMagazineDock` shell containing repeated instances of `Assets/Prefabs/MagazineKnifeIcon.prefab` | `KnifeMagazineDisplay` | No text; active/inactive knife truth remains event-driven |
| Shatter Strike | Lower-right upper control, review point `83.1%, 77.3%` | Reuse `Assets/Prefabs/UI/PowerUp/PowerUpButton.prefab`; shared neutral circular bezel and count badge; retain `ShatterStrikeIcon.png` | `PowerUpHudController` and `PowerUpButtonView` | Existing count, selected, unavailable, activation, and consumption truth |
| Chrono Shift | Lower-right lower control, review point `92.1%, 88.3%` | Second instance of the same `PowerUpButton.prefab`; retain `ChronoShiftIcon.png` | `PowerUpHudController` and `PowerUpButtonView` | Existing count plus active timed-effect radial progress |

## Proposed reusable prefab hierarchy

```text
StagePlayCanvas
└── SafeAreaRoot
    ├── TopHud
    │   ├── MenuButton (existing prefab instance)
    │   ├── StageProgressPlaque (new shared prefab)
    │   │   ├── BlankShell
    │   │   ├── StageLabel (TMP)
    │   │   ├── ProgressSlots
    │   │   │   └── ProgressBarSlot (existing prefab instances)
    │   └── CoinUiDisaply (existing prefab instance)
    └── BottomHud
        ├── KnifeMagazineDock (new shared prefab)
        │   ├── BlankShell
        │   └── KnifeIcons
        │       └── MagazineKnifeIcon (existing prefab instances)
        └── PowerUpStack
            └── PowerUpButton (existing prefab instances built from catalog)
```

All repeated elements must remain prefab-linked. Editor tooling may instantiate these prefab sources but must not reconstruct equivalent hierarchies manually.

## Sprite separation plan

### Reuse without re-authoring

- Settings glyph: `Assets/Art/UI/Icons/IconSettings.png`.
- Currency display: reuse the complete `Assets/Prefabs/UI/Coin/CoinUiDisaply.prefab` source without visual overrides.
- Shared blank surfaces from `Assets/Art/UI/Core`, especially Icon Socket where its silhouette fits.
- Existing Shatter Strike and Chrono Shift icon identities from `Assets/Art/PowerUps`.
- Existing knife icon content referenced by `MagazineKnifeIcon.prefab` unless a later close-up review demonstrates a material mismatch.
- Existing progress-slot prefab source; only its shared visual skin may be revised.

### New blank production art candidates

1. `HudStageProgressShell`: shallow neutral wood/leather plaque, nine-sliced, no text, numbers, progress marks, foliage, or stage identity.
2. `HudKnifeMagazineShell`: tall neutral wood/leather dock, nine-sliced where practical, no knives or count baked in.
3. `HudPowerUpBezel`: one neutral circular wood/gold bezel shared by both power-ups.
4. `HudCountBadge`: one opaque ivory/leather count-badge surface authored once in `PowerUpButton.prefab` and reused by every instance.
5. `HudProgressSlot`: one neutral marker shape with state supplied by tint/material rather than separate item-specific art, if the current slot cannot meet the approved finish.

The reference background, target, tomatoes, stuck knife, incoming knife, stepping stones, flowers, and waterfall are gameplay content and are not part of HUD sprite separation.

## Knife magazine contract

- Keep a narrow vertical dock at the lower-left safe edge.
- Keep knives in a single vertical sequence.
- Rotate each knife approximately 15–20 degrees counterclockwise: blade tip upper-left, handle lower-right, matching the current HUD.
- Preserve one shared `MagazineKnifeIcon.prefab`; never bake multiple knives into the dock shell.
- Preserve `KnifeMagazineDisplay` runtime truth. Its current high-watermark behavior creates icons only until the largest encountered knife count and then reuses/hides them.
- Before implementation, measure the maximum supported `LevelCatalog` knife count. Full truth must fit the dock; do not silently truncate or invent an overflow convention without separate approval.

## Stage-progress contract

- Use one shallow top-center plaque; it must not become a large banner.
- Stage identity and progress slots are separate children; fraction text is intentionally omitted.
- `StageProgressBarView` remains the presentation owner for slot state and stage events.
- Slot instances must come from `ProgressBarSlot.prefab`. Prefer a bounded high-watermark pool sized from the supported stage catalog rather than destroy/rebuild churn when production implementation begins.

## Power-up state contract

- Both controls are instances of the same `PowerUpButton.prefab`; item identity comes from catalog data.
- Shatter Strike retains the orange cracked-target/knife identity.
- Chrono Shift retains the teal clock/time identity and clockwise timed-effect overlay.
- The count badge stays physically attached to its own control in every state and aspect ratio.
- Count `> 0`: show the authoritative integer in TMP.
- Count `0`: retain the existing acquisition route/plus state; do not display a fake count.
- Selected/active: use an opaque rim, material, or restrained pulse. Preserve reduced-motion meaning with a static state.
- Timed effect: retain the existing radial progress truth.
- Unavailable/gameplay-blocked: use an opaque low-emphasis treatment plus a clear state cue; avoid the current transparent look as the sole signal.
- The catalog currently bounds live instances; do not pre-author one separate hierarchy per power-up item.

## Input, layering, and safe-area rules

- Only visible button hit roots may intercept raycasts. Decorative shells, TMP labels, slots, and magazine icons must have raycast targeting disabled.
- The center of the screen, including the incoming-knife-to-target lane, must remain free of HUD raycast targets.
- Modal scrims and modal controls must render and receive input above every HUD element.
- Settings and power-up buttons retain at least a 48 dp effective touch target, even when their visible art is smaller.
- The background remains full bleed; HUD anchors live under `SafeAreaRoot`.
- Validate at 1080×1920, 360×640, 1080×2160 with safe insets, and 1440×2560 portrait tablet-safe before approval.

## Production and approval sequence

1. Produce only the five blank/new visual candidates listed above, reusing approved existing icons and surfaces wherever possible.
2. Present transparent-background element previews and an annotated contact sheet for approval. Do not integrate them yet.
3. After explicit separated-element approval, import the approved sprites, configure borders/atlases, and update shared prefab sources.
4. Compose `StagePlay` only from those prefab sources and retain existing bindings/data owners.
5. Validate standard, large-count, zero-count/acquisition, selected, active timer, blocked/modal, small-phone, grayscale, tall-safe, and tablet-safe conditions.
6. Record and inspect short videos for pulse, timer, currency feedback, transitions, and any other multi-frame behavior.
7. Run focused and full EditMode tests and review compilation/runtime Console output.

No gameplay, economy, inventory, progression, purchase, power-up, or input behavior change is authorized by this breakdown.
