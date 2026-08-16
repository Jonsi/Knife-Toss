# Power-Up Button Art and Accessibility Record

Date: 2026-08-08

Implementation audit updated: 2026-08-15

## Production family

- `PowerUpSocket.png`: unreferenced handoff/source option; the shipping button is currently
  frameless.
- `ShatterStrikeIcon.png`: Ember/SunGold knife-through-cracked-target silhouette.
- `ChronoShiftIcon.png`: Lagoon clock, twelve ticks, hands, and clockwise arrow.
- `PowerUpBadges.png`: count chip, acquisition plus, unavailable pouch/slash, and lock.
- `PowerUpClockRing.png`: deterministic Lagoon twelve-tick countdown ring/mask generated in
  the Unity Editor as a final-art handoff asset. It is not currently referenced by the live
  prefab or view; the runtime-created cooldown fill copies the configured power-up icon.

The legacy power-up sheet and button frame remain in the repository for reference and
rollback. Runtime configuration references now use the new Shatter and Chrono icons, and the
button prefab uses the new badge sheet and active material. The socket and clock ring are not
runtime-referenced.

`PowerUpHUD.spriteatlasv2` exists and records intended packing settings, but its serialized
`packables` list is empty and it currently packs no sprites. Individual texture import and
Android override settings exist; atlas integration and budget validation remain ART-708 and
P2-010 work.

## Scope boundary

This delivery changes sprites and prefab layout only. It does not add a state presenter,
reason callout, input handling, inventory behavior, activation behavior, or lifecycle logic.
The existing power-up view/controller and runtime contracts remain authoritative.

## Source and provenance

Author/commissioner: project owner via Codex.

Generation method: OpenAI built-in image generation tool, followed by the Codex imagegen
skill's local chroma-key removal helper. No third-party marketplace art was used to create
these sprites. The unkeyed generated PNG sources and the full prompt record are stored in
the repository under `ArtSource/PowerUps`.

Modification history:

1. Generated on a flat magenta key background.
2. Removed the key locally with a soft matte and despill.
3. Imported as sRGB UI sprites with mipmaps and Read/Write disabled.
4. Limited runtime sizes to 512 for the socket and 256 for icons/badges/ring, with Android
   ASTC settings and no legacy packing tags.

## Final prompts

The production prompts specify Hand-Carved Forest Arcade, orthographic mobile UI sprites,
the Phase 2 palette (`Ink`, `WoodMid`, `WoodDark`, `Parchment`, `SunGold`, `Ember`, and
`Lagoon`), strong grayscale silhouettes, 64-96 dp readability, no text or watermark, and a
flat `#ff00ff` removable background. Separate prompts were used for the shared socket,
Shatter icon, Chrono icon, and four-badge sheet, matching the generated source filenames.

## Review evidence

- `Captures/Stage1BackgroundReview.png`: production-size 2x2 review over all four retained
  Stage 1 background sprites.
- `Captures/Stage1BackgroundReview_Grayscale.png`: value/silhouette review.
- `Captures/After_Stage1_DeviceReference.png`: 1080x1920 device-reference prototype.
- `Captures/BeforeAfter_DeviceReference.png`: Phase 2 audit capture beside the new device
  reference.

These are deterministic sprite composites, not live Game View screenshots. Unity 6000.4.2f1
hit a native Metal render-pass crash while creating a capture render target, so prefab
inspection and automated validation are the handoff evidence for this art-only change.

## Runtime implementation status

The art delivery and UX-408 are complete. The live HUD uses the
integrated icons, count/acquisition badges, active material and pulse, unavailable tint, and
an authoritative Chrono radial fill. Shatter armed state is reconciled from the power-up
service, and Chrono activity/progress is queried from that same service.

The product owner accepted the current buttons and behavior as the UX-408 result. Historical
technical observations—including the runtime-created fill, unused ring/socket assets, and
empty atlas—remain documented for their separate owners. They are not UX-408 closure work and
must not trigger a presenter, state-matrix, countdown, animation, or art rewrite without a new
explicit request.

The current 14-test `Game.PowerUps.Tests` total is not 14 button-state tests: seven cover the
service, three cover button active visuals, three cover knife VFX lifecycle, and one covers
hidden preload visibility. The static images in this folder remain art-review evidence rather
than runtime interaction evidence.

Ownership boundaries:

- UX-406 decides shipping reduced-motion, mute, and haptic-disable behavior.
- UX-407 builds the state/feedback sandbox.
- ART-708 owns final state-art polish and final device-size approval.
- UX-408 is complete; preserve its accepted implementation.
