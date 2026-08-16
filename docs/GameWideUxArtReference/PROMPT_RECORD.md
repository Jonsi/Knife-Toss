# Toss Madness target art reference — prompt and provenance record

Date: 2026-08-09

> Historical record: the Failure screen content in prompt 01 is superseded and must not be
> implemented or used as current product direction. The authoritative Failure reference is
> `Assets/_Docs/Phase5VisualAudit/Captures/FAIL502_GameOverTarget.png`: the approved target
> with only `Game Over`, `Retry`, and `Menu`. The verbatim prompt below is retained
> solely to preserve provenance for the already-generated board.

Final runtime extraction note (2026-08-15):
`Assets/Art/UI/Failure/FailurePopupAtlas.png` was created deterministically from the approved
target pixels and contains only `GameOverTitle`, `CrossedKnives`, `RetryButtonArt`, and
`MenuButtonArt`. The existing repository popup frame is reused and was not regenerated.
An intermediate generated atlas approximation was rejected and is not a project asset.

## Provenance

- Author/commissioner: project owner via Codex.
- Generation path: OpenAI built-in image generation using the Codex `imagegen` skill.
- Use: pre-production UI/UX and feedback reference only; none of these boards is a runtime
  texture or shippable screen asset.
- Source references: `Assets/_Docs/Phase2VisualAudit/ContactSheet.png`,
  `Assets/_Docs/PowerUpButtonArt/Captures/After_Stage1_DeviceReference.png`, and each
  preceding generated target board as a style-consistency anchor.
- No third-party trademarks or external reference art were requested.
- All generated copy, prices, rewards, names, addresses, and values are illustrative and
  non-authoritative.

## Output manifest

| File | Size | SHA-256 |
|---|---:|---|
| `01_Core_Loop_Target.png` | 1672×941 | `d24a7beb82a05de9b84083cc7ad519ffe836f7f883835436613d820a103e0f8e` |
| `02_Meta_Progression_Target.png` | 1672×941 | `5e3ddde2d36a5ba0af6e4ba492d2fa6ba2a7d49161ffaf6f9a116ad6918922fd` |
| `03_Modals_Forms_Target.png` | 1672×941 | `e77918d651f96c7aa793e9d14b6480fd72425c5c4aaeef286e0b974608f8bb49` |
| `04_Reusable_UI_Feedback_Target.png` | 1672×941 | `7f1be27118ecbf7bcb4f9e77c9505c23bbe4161a15862e72a978dc0e0231c93f` |

## Final prompts

### 01 — Core loop

```text
Use case: ui-mockup
Asset type: pre-production target art reference board for a portrait mobile Unity game
Primary request: create a polished landscape presentation board showing four full portrait phone screens for Toss Madness: Main Menu, active Gameplay HUD, Knife Collision failure modal, and Stage Complete reward/handoff.
Input images: Image 1 is the current-state screen contact sheet and must guide real content, screen structure, forest setting, target, knife, and reachable flows; Image 2 is a style and composition reference for the approved Shatter Strike and Chrono Shift power-up controls. Generate a new target board; do not edit or collage the source screenshots.
Style/medium: shippable 2D casual mobile game UI mockup, Hand-Carved Forest Arcade; warm carved wood, parchment, painted lush forest, chunky readable silhouettes, coherent authored icon family, tactile but uncluttered.
Composition/framing: 16:9 landscape board, four evenly spaced 9:16 phone screens at useful readable scale, neutral dark presentation background, no perspective distortion. Preserve generous safe areas and at least 48dp touch targets.
Screen 1 Main Menu: logo, resource chip, settings, compact secondary navigation, knife selection, one dominant PLAY button, subordinate STAGES button; clear hierarchy and calm composition.
Screen 2 Gameplay HUD: central throw lane stays open; compact top progress showing STAGE 1 and target progress; coin chip; settings; knife magazine; current Shatter/Chrono circular power-up visuals retained at lower right with attached counts; small contextual swipe/tap guidance that does not obscure the target.
Screen 3 Failure: dark scrim over gameplay, carved parchment modal, explicit KNIFE COLLISION cause, clear primary CONTINUE button with truthful cost area, strong RETRY button, secondary START OVER and MENU actions; no guidance above modal.
Screen 4 Completion: clean target-cleared world state, celebratory gold/leaf/wood-chip accents, readable STAGE COMPLETE modal, reward summary and one clear CONTINUE action; no old target slices overlapping the next target.
Color palette: deep Ink, dark and mid carved wood, warm Parchment #E8C79B, SunGold #FFC43D, Ember #F26A2E, Lagoon #19BFC4, restrained Leaf #45C968; ivory text.
Typography: chunky Lilita-One-like display face for headings and actions; clean sans serif for supporting copy and values.
Text (verbatim, short labels only): "PLAY", "STAGES", "STAGE 1", "KNIFE COLLISION", "CONTINUE", "RETRY", "START OVER", "MENU", "STAGE COMPLETE".
Constraints: this is a UI/UX production target, not cinematic concept art; all controls must look actionable; use one coherent visual system across screens; retain the current forest background and round power-up identities; keep the throw lane readable; no placeholder copy, default Unity controls, unrelated purple panels, logos beyond the game title, trademarks, watermark, or tiny body text.
Avoid: ornate fantasy clutter, photorealism, excessive gradients, glossy generic mobile UI, distorted phone frames, duplicated controls, unreadable text.
```

### 02 — Meta and progression

```text
Use case: ui-mockup
Asset type: pre-production target art reference board for meta/progression screens in a portrait mobile Unity game
Primary request: create a polished landscape presentation board showing three full portrait phone screens for Toss Madness: Daily Missions, Store, and Stage Selection, plus small clear state callouts beside each screen where helpful.
Input images: Image 1 is the current-state contact sheet and guides the actual content and reachable flows; Image 2 is the approved target-art style anchor. Generate a new board, not an edit or collage.
Style/medium: shippable 2D casual mobile game UI mockup, the exact same Hand-Carved Forest Arcade system as Image 2: carved wood frames, warm parchment, painted forest/world art, chunky readable display type, clean supporting type, tactile authored icons, restrained celebration.
Composition/framing: 16:9 landscape board, three evenly spaced 9:16 phone screens at readable scale on a neutral dark presentation background, orthographic front view, generous safe areas, 48dp minimum targets.
Screen 1 Daily Missions: titled DAILY MISSIONS in a reusable wood/parchment modal shell; three mission rows with icon, concise objective, numeric progress, progress bar, reward chip; one clearly CLAIMABLE gold action, one IN PROGRESS disabled action, one CLAIMED check/stamp state; visible close control; no foreground HUD icons overlapping.
Screen 2 Store: titled STORE; horizontal knife-card carousel with a visible next-card affordance and page dots; strong selected/equipped outline and OWNED stamp; truthful price button; lower power-up row using the retained round Shatter Strike and Chrono Shift identities; rewarded-ad CTA clearly labeled FREE COINS; REMOVE ADS has a durable OWNED state; coin chip and back control.
Screen 3 Stage Selection: titled STAGES; vertically scrolling world cards with distinct lush forest, ember volcanic, and lagoon crystal identities; readable world names, stage nodes showing COMPLETE, CURRENT, and LOCKED states; world chest is one prominent claimable reward at the end of progression, not tiny repeated no-op buttons; back control and preserved scroll discovery.
Small state callouts: PURCHASE COMPLETE check/stamp, NOT ENOUGH COINS typed blocked toast, MISSION REWARD coin travel, and STAGE UNLOCK glow, shown as compact annotations without crowding the phone screens.
Color palette: deep Ink, WoodDark and WoodMid, Parchment #E8C79B, SunGold #FFC43D, Ember #F26A2E, Lagoon #19BFC4, Leaf #45C968, Failure #E84B3C, ivory text.
Typography: chunky Lilita-One-like headings/actions; clean Liberation-Sans-like supporting text and values.
Text (verbatim, short labels only): "DAILY MISSIONS", "CLAIM", "IN PROGRESS", "CLAIMED", "STORE", "OWNED", "EQUIPPED", "FREE COINS", "REMOVE ADS", "STAGES", "COMPLETE", "CURRENT", "LOCKED", "PURCHASE COMPLETE", "NOT ENOUGH COINS".
Constraints: cohesive product UI, not concept art; every visible action has a clear state and outcome; retain current game rules and current round power-up identities; no price or reward redesign beyond illustrative placeholder values; no default Unity buttons, repeated placeholder world names, generic green rectangles, unrelated purple panels, trademarks, watermark, or dense tiny copy.
Avoid: excessive ornament, photorealism, generic glossy mobile UI, disconnected icon families, clipped content as the only scroll cue, repeated CLAIM buttons under every stage.
```

### 03 — Modals and forms

```text
Use case: ui-mockup
Asset type: pre-production target art reference board for modal, settings, legal, and form screens in a portrait mobile Unity game
Primary request: create a polished landscape presentation board showing four full portrait phone screens for Toss Madness: Settings, Privacy, Report Error, and Add Power-Up.
Input images: Image 1 is the current-state contact sheet and defines the actual screens; Images 2 and 3 are approved target-art style anchors. Generate a new coherent board, not an edit or collage.
Style/medium: shippable 2D casual mobile game UI mockup in the same Hand-Carved Forest Arcade system: one reusable carved-wood and warm-parchment modal shell, strong hierarchy, authored icons, clean forms, restrained decorative leaves and metal fasteners.
Composition/framing: 16:9 landscape board, four evenly spaced 9:16 phone screens at readable scale, front-facing and undistorted, neutral dark presentation background. Every modal has a scrim, safe-area margin, title, close/back behavior, content region, and action row. Gameplay guidance and HUD actions must not render above the modal.
Screen 1 Settings: title SETTINGS, close button, clean rows for SOUND, MUSIC, HAPTICS, and REDUCED MOTION; each uses a carved toggle or slider with visible on/off state and icon; separate PRIVACY and REPORT ERROR navigation rows; RESTORE PURCHASES secondary action; compact, balanced spacing, no empty wasteland.
Screen 2 Privacy: title PRIVACY, readable parchment document surface with strong line length and scroll indicator, short representative body blocks rather than fake dense microtext, product/support identity area, secondary BACK and clear primary ACCEPT action.
Screen 3 Report Error: title REPORT ERROR, one parchment text field with prompt, focus state and character count, concise instructions, validation/blocked message example, keyboard-safe lower action row, secondary CANCEL and primary SUBMIT.
Screen 4 Add Power-Up: title NEED A POWER-UP?, clear reason context, retained round Shatter Strike and Chrono Shift choices with attached inventory counts, two truthful acquisition options WATCH AD and BUY 250, one secondary CONTINUE WITHOUT action; no ambiguous unlabeled video/coin strip.
Color palette: deep Ink, WoodDark/WoodMid, Parchment #E8C79B, SunGold #FFC43D, Ember #F26A2E, Lagoon #19BFC4, Leaf #45C968, Failure #E84B3C, ivory text.
Typography: chunky Lilita-One-like headings/actions; clean Liberation-Sans-like body, legal, field, and value text with strong contrast.
Text (verbatim, short labels only): "SETTINGS", "SOUND", "MUSIC", "HAPTICS", "REDUCED MOTION", "PRIVACY", "REPORT ERROR", "RESTORE PURCHASES", "BACK", "ACCEPT", "SUBMIT", "CANCEL", "NEED A POWER-UP?", "SHATTER STRIKE", "CHRONO SHIFT", "WATCH AD", "BUY 250", "CONTINUE WITHOUT".
Constraints: cohesive production UI, not concept art; legal copy remains illustrative and must be replaced by approved real copy; form behavior remains illustrative but clear; one modal shell and action hierarchy across all screens; no default checkbox, default gray Unity input field, placeholder company names, generic green rectangles, unrelated purple, trademarks, watermark, or tiny unreadable body text.
Avoid: ornate clutter, scroll ambiguity, inconsistent close/back affordances, buttons without labels, HUD/guidance overlap, fake app chrome.
```

### 04 — Reusable UI and feedback

```text
Use case: infographic-diagram
Asset type: reusable UI component and game-feedback target art board for a Unity mobile game
Primary request: create one polished 16:9 landscape production reference board that defines the Toss Madness reusable UI kit and feedback language. It must feel like a practical game UI style sheet, not a phone-screen concept.
Input images: Images 1–3 are approved target-art anchors; match their exact Hand-Carved Forest Arcade direction, palette, material finish, typography, icon weight, and level of polish.
Scene/backdrop: clean deep-Ink presentation background with a subtle grid and organized wood/parchment specimen panels.
Style/medium: shippable 2D casual game UI component sheet, orthographic, consistent asset scale, crisp silhouettes and readable labels.
Composition/framing: organized into four large quadrants with generous spacing and readable component sizes.
Quadrant 1 BUTTON SYSTEM: show the same primary carved gold button across deterministic states READY, PRESSED, DISABLED, LOADING, BLOCKED. PRESSED is slightly compressed with shorter shadow; DISABLED is desaturated/dim but readable; LOADING retains label context with a small carved spinner and blocks duplicate input; BLOCKED returns to ready while an Ember denial pulse/toast appears. Also show SECONDARY wood, DANGER Ember, ICON ONLY, PRICE, OWNED, and EQUIPPED variants.
Quadrant 2 STRUCTURE AND DATA: reusable wood/parchment modal shell with title/close/content/action slots and dark scrim sample; resource chip; stage progress bar/nodes; notification badge; attached inventory count; lock; claimable chest; claimed check/stamp; selected outline; safe-area margins and 48dp touch box callout.
Quadrant 3 INPUT AND FEEDBACK: parchment form field in IDLE, FOCUSED, INVALID, and VALID states; typed toast family for BLOCKED, NOT ENOUGH COINS, PURCHASE COMPLETE, REWARD RECEIVED; clean carved toggles/sliders; navigation back/close/settings/store/missions icons in one authored silhouette family.
Quadrant 4 MOTION/VFX STORYBOARD: small three-frame strips for UI PRESS (down, hold, release), KNIFE COLLISION (opposing sparks, brief warm-red flash, settle), VALID HIT (gold-white core, wood chips, crack pulse), COLLECTIBLE (gold pickup flies toward coin chip), STAGE COMPLETE (clean shatter, progress pulse, reward travel, next target enters only after cleanup), MODAL OPEN (scrim fade and panel settle). Include tiny speaker and haptic glyphs beside applicable moments, but no waveform detail.
Color palette: Ink background; carved WoodDark/WoodMid; Parchment #E8C79B; SunGold #FFC43D; Ember #F26A2E; Lagoon #19BFC4; Leaf #45C968; Failure #E84B3C; ivory text. Use color plus icon/shape/value so meaning survives grayscale.
Typography: chunky Lilita-One-like headings/actions; clean Liberation-Sans-like support labels.
Text (verbatim, short labels only): "BUTTON SYSTEM", "READY", "PRESSED", "DISABLED", "LOADING", "BLOCKED", "SECONDARY", "DANGER", "PRICE", "OWNED", "EQUIPPED", "STRUCTURE & DATA", "INPUT & FEEDBACK", "MOTION & VFX", "NOT ENOUGH COINS", "PURCHASE COMPLETE", "REWARD RECEIVED", "UI PRESS", "KNIFE COLLISION", "VALID HIT", "COLLECTIBLE", "STAGE COMPLETE", "MODAL OPEN".
Constraints: practical, implementable sprite/prefab reference; match existing round Shatter/Chrono identities but do not redesign them here; component states must be visibly distinct without changing button outcomes; loading prevents duplicate submission; blocked is transient typed feedback, not a persistent disabled state; no default Unity widgets, generic glossy UI, unrelated purple, excessive ornament, trademarks, watermark, or unreadably tiny labels.
Avoid: inconsistent border weights, multiple competing wood styles, text-heavy explanations, photorealism, cinematic concept art, ambiguous state changes.
```
