# Modern Forest Arcade UI Reference Specification

Date: 2026-08-12
Status: approved visual direction; not approved production pixels
Scope: measured decomposition of the generated button-system reference

## 1. Source and reference files

The user approved the direction shown in the generated reference and explicitly rejected the
earlier bulky carved-wood button family. This specification records what is actually visible in
the approved image, how it relates to the image and embedded phone viewport, and which parts may
inform production.

| File | Purpose | Dimensions | SHA-256 |
|---|---|---:|---|
| [`ModernForestArcade_ButtonSystem_Reference.png`](References/ModernForestArcade_ButtonSystem_Reference.png) | Unmodified ImageGen output | 941×1672 | `a244da83c2e969ef265460dab1f85f28e10c7bd5ab42e69a2d79a6d9d4d1faa7` |
| [`ModernForestArcade_ButtonSystem_Annotated.png`](References/ModernForestArcade_ButtonSystem_Annotated.png) | Main measured boxes and IDs | 941×1672 | `0ea2235dabfabb02afb9bb7ec201741571434b2f0a41c1d03cc8f0f36d639b5f` |
| [`ModernForestArcade_PhoneLayout_Annotated.png`](References/ModernForestArcade_PhoneLayout_Annotated.png) | Enlarged phone crop with layout IDs | 744×1806 | `6ef135038b5ea359440f3ee69b0dc263db562e38ddd39c4b029a104a287e9ff1` |
| [`ModernForestArcade_ElementBreakdown.png`](References/ModernForestArcade_ElementBreakdown.png) | Cropped visual inventory | 1600×1450 | `aaa79d0234690595d5bbff891a4dc190252c95662d65267be813dff70933652e` |
| [`ModernForestArcade_ElementMeasurements.csv`](References/ModernForestArcade_ElementMeasurements.csv) | Machine-readable coordinates and disposition | — | `9344a5955e3fb9c431f7e1ee58ca31c100e2223fcadec8c9a73d64819bafecc7` |

ImageGen output ID: `exec-a9d78663-79de-480b-b5c1-da8351683a36`.

The original ImageGen file remains under the Codex generated-image store, but the repository copy
above is the stable reference for future work. No crop from this composite may ship as a runtime
asset. Production elements must be recreated separately as blank, clean sources.

## 2. Measurement contract

### 2.1 Coordinate spaces

All boxes use top-left origin and the form `x, y, width, height`.

- **Board space:** 941×1672 pixels. Percentages are `x / 941`, `y / 1672`, `width / 941`, and
  `height / 1672`.
- **Embedded phone viewport:** observed box `550,421,330,858` in board space. Phone-relative
  coordinates subtract `(550,421)` and divide by `330×858`.
- **Project reference canvas:** 1080×1920, defined by `UiSemanticTokens.asset`.
- **Reference density:** 3 pixels per dp. Therefore 144 px = 48 dp, the minimum touch target.
- **Small review canvas:** 360×640, used only to assess phone-scale readability.

### 2.2 Accuracy and interpretation

Boxes were measured from the raster at original resolution. Antialiasing, shadows, and generated
texture make the visual edge fuzzy, so measurements are accurate to approximately ±2 px for hard
faces and ±4 px for shadows/illustrations. They are exact recorded observations for this reference,
not exact production constraints.

The embedded phone viewport is 330×858 (aspect 0.385), while a 1080×1920 canvas is aspect 0.563.
Do **not** stretch the phone sample anisotropically into the game. Use:

- phone-relative percentages for layout intent;
- source aspect ratios for visual silhouette;
- project dp tokens for final control and hit sizes.

### 2.3 Scaling formulas

For board-space comparison only:

```text
board x at target width  = x * targetWidth / 941
board y at target height = y * targetHeight / 1672
```

For phone-layout positions:

```text
phoneX% = (x - 550) / 330
phoneY% = (y - 421) / 858
```

For a control that must preserve its aspect ratio, choose the production width in dp or pixels and
derive its height from the source ratio. Do not independently scale its width and height from the
embedded phone.

## 3. Full-board element map

The `1080×1920` and `360×640` columns below show board-level proportional conversion. They help
reconstruct the reference sheet; they are not recommended runtime sizes.

| ID | Element | Board box `x,y,w,h` | Position on board | Size on board | Board-scale 1080×1920 | Board-scale 360×640 |
|---|---|---|---|---|---:|---:|
| H-01 | Title | `264,55,422,77` | 28.06% / 3.29% | 44.85% × 4.61% | 484×88 | 161×29 |
| H-02 | Subtitle | `292,148,362,29` | 31.03% / 8.85% | 38.47% × 1.73% | 415×33 | 138×11 |
| H-03 | Left botanical flourish | `126,54,104,88` | 13.39% / 3.23% | 11.05% × 5.26% | 119×101 | 40×34 |
| H-04 | Right botanical flourish | `710,54,105,88` | 75.45% / 3.23% | 11.16% × 5.26% | 121×101 | 40×34 |
| B-01 | Primary specimen | `60,269,441,136` | 6.38% / 16.09% | 46.87% × 8.13% | 506×156 | 169×52 |
| B-02 | Secondary specimen | `61,501,419,132` | 6.48% / 29.96% | 44.53% × 7.89% | 481×152 | 160×51 |
| B-03 | Danger specimen | `61,729,420,129` | 6.48% / 43.60% | 44.63% × 7.72% | 482×148 | 161×49 |
| B-04 | Icon-only specimen | `200,943,132,124` | 21.25% / 56.40% | 14.03% × 7.42% | 151×142 | 50×47 |
| B-05 | Price specimen | `61,1132,411,108` | 6.48% / 67.70% | 43.68% × 6.46% | 472×124 | 157×41 |
| B-06 | Owned specimen | `61,1317,410,91` | 6.48% / 78.77% | 43.57% × 5.44% | 471×104 | 157×35 |
| B-07 | Equipped specimen | `61,1426,411,93` | 6.48% / 85.29% | 43.68% × 5.56% | 472×107 | 157×36 |
| C-01 | Phone outer hardware | `529,400,372,903` | 56.22% / 23.92% | 39.53% × 54.01% | 427×1037 | 142×346 |
| C-02 | Phone viewport | `550,421,330,858` | 58.45% / 25.18% | 35.07% × 51.32% | 379×985 | 126×328 |
| D-01 | Footer forest silhouette | `0,1517,941,155` | 0% / 90.73% | 100% × 9.27% | 1080×178 | 360×59 |
| D-02 | Right botanical flourish | `771,1408,170,264` | 81.93% / 84.21% | 18.07% × 15.79% | 195×303 | 65×101 |

Section labels and leader rules are reference-sheet annotations, not product components. Their full
boxes are available in the measurement CSV.

## 4. Embedded phone screen layout

### 4.1 Observed layout

The viewport is `330×858`. Positions and sizes below are relative to that viewport. The
`1080×1920` column maps x/y and width/height independently to show occupied screen fractions; it
does not preserve asset aspect ratios.

| ID | Element | Board box | Viewport-local position | Position in viewport | Size in viewport | Fractional 1080×1920 | Fractional 360×640 |
|---|---|---|---|---|---|---:|---:|
| P-01 | Settings socket | `561,463,51,51` | `11,42` | 3.33% / 4.90% | 15.45% × 5.94% | 167×114 | 56×38 |
| P-02 | Currency chip | `728,465,101,43` | `178,44` | 53.94% / 5.13% | 30.61% × 5.01% | 331×96 | 110×32 |
| P-03 | Add-currency socket | `828,463,40,51` | `278,42` | 84.24% / 4.90% | 12.12% × 5.94% | 131×114 | 44×38 |
| P-04 | Knife illustration | `696,575,74,174` | `146,154` | 44.24% / 17.95% | 22.42% × 20.28% | 242×389 | 81×130 |
| P-05 | Target and stand | `608,635,205,274` | `58,214` | 17.58% / 24.94% | 62.12% × 31.93% | 671×613 | 224×204 |
| P-06 | Primary PLAY | `575,940,275,72` | `25,519` | 7.58% / 60.49% | 83.33% × 8.39% | 900×161 | 300×54 |
| P-07 | Secondary STAGES | `575,1024,275,60` | `25,603` | 7.58% / 70.28% | 83.33% × 6.99% | 900×134 | 300×45 |
| P-08 | Danger RETRY | `575,1094,275,58` | `25,673` | 7.58% / 78.44% | 83.33% × 6.76% | 900×130 | 300×43 |
| P-09 | Icon-only knife action | `679,1164,67,68` | `129,743` | 39.09% / 86.60% | 20.30% × 7.93% | 219×152 | 73×51 |

### 4.2 Layout relationships

- Top controls begin at approximately 5% of viewport height.
- The currency group is right-aligned; Settings is independently left-aligned.
- The hero knife begins near 18% height; the target occupies roughly 25–57% height.
- The primary action begins near 60.5% height.
- All three horizontal actions are 83.33% of viewport width.
- Left content margin is 7.58%; right margin is approximately 9.09%.
- Observed vertical gaps are 12 px between Primary and Secondary and 10 px between Secondary and
  Danger: approximately 3.6% and 3.0% of viewport width.
- The icon-only action is centered and begins near 86.6% height.

### 4.3 Recommended 1080×1920 production interpretation

These are implementation targets derived from the visual relationships and the existing 3 px/dp
token contract. They are recommendations, not measurements from the source.

| Component | Observed phone ratio | Candidate visual target | Candidate dp at 3 px/dp | Hit-root rule | Notes |
|---|---:|---:|---:|---|---|
| Primary | 3.82:1 (`275/72`) | 780×168 | 260×56 | Full 780×168 | Live review rejected the earlier 900×240 interpretation as oversized |
| Secondary | 4.58:1 (`275/60`) | 720×144 | 240×48 | Full 720×144 | Accepted MainMenu size; retains the 48 dp minimum |
| Danger | 4.74:1 (`275/58`) | 720×144 candidate | 240×48 | At least 720×144 | Share normalized geometry and scale with Secondary |
| Price | 3.81:1 (`411/108`) | 840×220 | 280×73; quantize to 280×72 | At least full 840×216 | Reserve live icon/value slots |
| Owned/Equipped row | 4.42–4.51:1 | 840×186–190 | 280×62–63; quantize to 280×64 | At least full 840×192 | Status mark + live label |
| Icon-only visual | 1.06:1 including shadow | 192×192 normalized | 64×64 | At least 192×192 | Never below 144×144 / 48 dp |
| Settings/Add visual | Approximately square | 144–168 square | 48–56 square | At least 144×144 | Live glyph centered |
| Horizontal gap | — | 36–48 | 12–16 | — | Between stacked actions |
| Screen side margin | — | 90 | 30 | — | Reference shows ~8.3% average |

The reference image shows Primary taller than the other actions. Production retains that hierarchy
while using shared corner geometry and edge thickness. The initial 900-pixel-wide runtime mapping
was visibly too large against the real MainMenu and was replaced by the accepted 780×168 / 720×144
integration above. These are screen-layout values, not changes to the retained sprite canvases.

### 4.4 Candidate export and nine-slice matrix

This table translates the measured direction into a reviewable production hypothesis. It is not
approved implementation data.

| Element | Candidate retained master | Candidate runtime PNG | Candidate fixed border | Stretch center | Reasoning |
|---|---:|---:|---:|---:|---|
| Primary | 2048×544 blank RGBA | 768×204 or 512×136 | 48 px each side; 30 px top/bottom at 768 scale | At least 672×144 | Preserves 3.76:1–3.82:1 face with quiet center |
| Secondary | 2048×448 blank RGBA | 768×168 or 512×112 | 42 px each side; 24 px top/bottom | At least 684×120 | Normalized 4.57:1 silhouette |
| Danger | Recolor/edit of approved Secondary geometry | Same as Secondary | Same as Secondary | Same as Secondary | Semantics should not change geometry |
| Price | 2048×536 blank RGBA | 768×200 or 512×133 | 48 px each side; 28 px top/bottom | At least 672×144 | Live resource/value layers; no baked socket |
| Status row | 2048×456 blank RGBA | 768×172 or 512×115 | 42 px each side; 24 px top/bottom | At least 684×124 | Mark remains a separate layer |
| Icon socket | 1024×1024 blank RGBA | 256×256 | 40–48 px each side | 160–176 square center | Square normalized form of shared corners |
| Owned mark | 1024×1024 RGBA | 256×256 | None | N/A | Separate check/ring sprite |
| Equipped mark | 1024×1024 RGBA | 256×256 | None | N/A | Separate shield/distinct symbol |

The lower 512-wide alternatives save memory but provide less vertical detail. Select one scale only
after test imports prove phone-size edge quality and atlas headroom. Do not combine these candidate
borders with the rejected current sprite borders.

## 5. Button construction breakdown

### 5.1 Shared silhouette

- Long clipped-corner rectangle.
- Four 45-degree corner cuts, visually about 6–8% of control height.
- Nearly flat top, bottom, and side runs.
- No protruding corner machinery or asymmetric lower-right notch.
- Primary specimen aspect including shadow: 3.24:1 (`441×136`).
- Secondary: 3.17:1 (`419×132`).
- Danger: 3.26:1 (`420×129`).
- Price: 3.81:1 (`411×108`).
- Owned: 4.51:1 (`410×91`).
- Equipped: 4.42:1 (`411×93`).
- Icon-only is the square version of the same clipped-corner language: 1.06:1 including shadow
  (`132×124`).

Production should normalize Primary/Secondary/Danger to one source silhouette and vary color,
height role, and content—not redraw inconsistent endcaps.

### 5.1.1 Per-control sub-element measurements

These boxes are visually measured from the source raster and use board coordinates. `Outer visual`
includes the shallow shadow. `Face` excludes most of that shadow. Text/icon boxes contain generated
demonstration content and document spacing only; they are not crop or font specifications.

| Control | Outer visual | Face/body | Corner cut | Bottom depth | Demonstration content | Content box | Content/face relationship |
|---|---|---|---:|---:|---|---|---|
| Primary B-01 | `60,269,441,136` | `61,269,439,129` | 29–31 px | 8–15 px | `PLAY` | approx. `207,308,146,55` | Label occupies ~33% face width; arrows occupy side zones |
| Secondary B-02 | `61,501,419,132` | `61,501,419,122` | 27–30 px | 10–15 px | `STAGES` | approx. `172,543,198,49` | Label occupies ~47% face width |
| Danger B-03 | `61,729,420,129` | `61,729,420,120` | 26–29 px | 9–15 px | `RETRY` | approx. `181,768,178,49` | Label occupies ~42% face width |
| Icon B-04 | `200,943,132,124` | `201,943,130,113` | 17–20 px | 10–13 px | Knife | `224,962,82,89` | Glyph occupies ~63% face width and ~79% face height |
| Price B-05 | `61,1132,411,108` | `61,1132,411,99` | 23–27 px | 8–11 px | Coin + `1,250` | coin `142,1147,67,68`; value approx. `231,1157,174,59` | Icon ~16% outer width; value ~42%; ~22 px gap |
| Owned B-06 | `61,1317,410,91` | `61,1317,410,82` | 22–25 px | 8–11 px | Check + `OWNED` | mark `107,1332,59,60`; label approx. `228,1342,139,45` | Mark center x≈137; label center x≈298 |
| Equipped B-07 | `61,1426,411,93` | `61,1426,411,84` | 22–25 px | 8–11 px | Shield + `EQUIPPED` | mark `105,1441,60,63`; label approx. `211,1450,211,47` | Wider label retains ~49 px right padding |

Observed side-arrow boxes, which are explicitly rejected for production:

| Control | Left arrow | Right arrow | Approximate quiet label zone after removal |
|---|---|---|---|
| Primary | `107,316,39,37` | `414,316,38,37` | x≈105–456; center 250–300 px is completely quiet |
| Secondary | `104,548,39,32` | `404,548,41,32` | x≈100–445; center remains available for long copy |
| Danger | `104,775,39,33` | `405,775,40,33` | x≈100–445; center remains available for explicit verb |

### 5.1.2 Face anatomy and proportional tokens

| Feature | Observed proportion | Candidate production token |
|---|---:|---:|
| Clipped-corner horizontal run | 6–8% of face height | 8 dp on 56–64 dp controls; 10–12 dp on 80 dp Primary |
| Thin bright keyline | 1–2% of face height | 1–2 px in runtime sprite, subject to 360×640 review |
| Dark boundary | 2–3% of face height | 2 dp maximum; below 4% accessibility limit |
| Bottom depth/shadow | 7–12% of total visual height | 4 dp offset with restrained soft shadow, matching existing surface token direction |
| Horizontal live-content padding | 10–14% each side on label-only controls | 24–32 dp at typical width |
| Icon-to-label gap | ~5.4% of Price width (about 22 px) | 16 dp candidate |
| Status-mark diameter | ~14.4% of row width | 40–48 dp candidate within a 56–64 dp row |

### 5.2 Shared visual layers

The reference reads as five restrained layers:

1. **Flat semantic face** — the dominant fill.
2. **Very low-contrast texture** — paper/paint/wood impression, not visible plank construction.
3. **Thin inner top/side highlight** — about 1–2% of face height.
4. **Thin dark boundary** — about 2–3% of face height; well below the token maximum outline
   proportion of 4%.
5. **Shallow bottom depth/shadow** — approximately 7–12% of visual height; no stacked frame.

Use Full Rect sprites and nine-slice only after the clean source geometry is approved. The board's
generated shadows must not be assumed to define final nine-slice pixels.

### 5.3 Content zones

| Variant | Left zone | Center zone | Right zone | Runtime ownership |
|---|---|---|---|---|
| Primary | Empty | Live action label | Empty | Screen/presenter |
| Secondary | Empty | Live action label | Empty | Screen/presenter |
| Danger | Empty | Live explicit verb | Empty | Screen/presenter |
| Icon-only | — | One live glyph | — | Existing action/glyph owner |
| Price | Live resource icon, 18–22% width | Live amount/action, 55–65% width | 12–18% quiet padding | Economy/presenter |
| Owned | Mark, 18–22% width | Live `OWNED` copy | Quiet padding | Store/domain truth |
| Equipped | Distinct mark, 18–22% width | Live `EQUIPPED` copy | Quiet padding | Equipment/domain truth |

The inward arrows on Primary, Secondary, and Danger are decorative generated marks. They have no
action semantics and should be removed. Removing them increases the quiet live-label zone.

## 6. Color palette

Colors below are sampled or region-clustered from the exact reference PNG. Because the image is a
generated raster with texture and compression-like antialiasing, treat them as reference targets,
not final token values.

### 6.1 Core semantic palette

| Token candidate | Hex | RGB | Reference use | Status |
|---|---|---:|---|---|
| Pine 950 | `#011B17` | 1, 27, 23 | Board/background dark | Direction approved |
| Pine 900 | `#012F2A` | 1, 47, 42 | Owned face | Direction approved |
| Deep Teal 800 | `#014F43` | 1, 79, 67 | Equipped face | Direction approved |
| Lagoon 600 | `#31A78D` | 49, 167, 141 | Secondary face cluster | Direction approved |
| Lagoon Highlight | `#55C9B4` | 85, 201, 180 | Secondary inner highlight | Direction approved |
| Primary Amber | `#FDA720` | 253, 167, 32 | Primary face cluster | Direction approved |
| Primary Highlight | `#F6C152` | 246, 193, 82 | Primary inner highlight | Direction approved |
| Primary Depth | `#C35E01` | 195, 94, 1 | Primary bottom edge cluster | Direction approved |
| Danger Coral | `#F25F35` | 242, 95, 53 | Danger face cluster | Direction approved |
| Danger Highlight | `#F98559` | 249, 133, 89 | Danger inner highlight | Direction approved |
| Danger Depth | `#B52A0B` | 181, 42, 11 | Danger bottom edge cluster | Direction approved |
| Parchment | `#F5DFB4` | 245, 223, 180 | Price face cluster | Direction approved |
| Parchment Highlight | `#FFFBE3` | 255, 251, 227 | Price inner highlight | Direction approved |
| Parchment Edge | `#C0934C` | 192, 147, 76 | Price boundary/depth cluster | Direction approved |
| Espresso | `#321F0E` | 50, 31, 14 | Dark live text | Direction approved |
| Danger Ink | `#370400` | 55, 4, 0 | Danger live text | Consider unifying with Espresso |
| Cream Text | `#F7E4BE` | 247, 228, 190 | Heading/status text | Direction approved |
| Guide Teal | `#39917C` | 57, 145, 124 | Reference annotations | Documentation only |
| Guide Rule | `#2E5D4B` | 46, 93, 75 | Reference rules/dots | Documentation only |
| Coin Gold | `#FFC92C` | 255, 201, 44 | Currency concept | Must map to authoritative resource art |

Dominant-color extraction also finds forest-context colors (`#8BC1AC`, `#47643A`, `#283516`,
`#94652C`, `#CDA963`). Those belong to the mock scene, not the button-system palette.

### 6.2 Sampled contrast checks

These calculations use the reference samples and WCAG relative luminance. They are analytical;
final sprites and TMP rendering still require Unity screenshot validation.

| Foreground / background | Ratio | Analytical result |
|---|---:|---|
| Espresso `#321F0E` / Primary `#FDA720` | 8.02:1 | Pass body and large text |
| Espresso `#321F0E` / Secondary `#31A78D` | 5.26:1 | Pass body and large text |
| Danger Ink `#370400` / Coral `#F25F35` | 5.48:1 | Pass body and large text |
| Espresso `#321F0E` / Parchment `#F5DFB4` | 12.03:1 | Strong pass |
| Cream `#F7E4BE` / Owned `#012F2A` | 11.65:1 | Strong pass |
| Cream `#F7E4BE` / Equipped `#014F43` | 7.63:1 | Strong pass |
| Cream `#F7E4BE` / Pine `#011B16` | 14.36:1 | Strong pass |

### 6.3 Palette usage rules

- Use Espresso rather than pure black for primary text and outlines.
- Keep face variation within approximately 8–12% luminance so texture does not compete with copy.
- Keep bottom depth darker but shallow; it must not become a second wooden chassis.
- Ownership states must differ by icon shape and copy, not only by teal shade.
- Disabled, loading, blocked, and reduced-motion colors remain token/presenter policy and must be
  tested against final rendered sprites.

## 7. Typography analysis

### 7.1 Observed hierarchy

| Use | Observed box | Approximate visual treatment | Production interpretation |
|---|---:|---|---|
| Board title | 422×77 | Tall condensed uppercase cream with small dark offset | Reference presentation only |
| Board subtitle | 362×29 | Condensed uppercase teal, high tracking | Reference presentation only |
| Section labels | 66–214×24–25 | Compact condensed uppercase teal | Documentation/gallery only |
| Primary label | Approx. 142×62 inside B-01 | Bold condensed uppercase, Espresso | Live TMP Action role |
| Secondary label | Approx. 199×55 | Bold condensed uppercase, near-black | Live TMP Action role |
| Danger label | Approx. 169×53 | Bold condensed uppercase, dark red-brown | Live TMP Action role |
| Price value | Approx. 166×57 | Bold condensed digits, Espresso | Live TMP Value role |
| Status copy | Approx. 170–237×43–48 | Cream condensed uppercase | Live TMP Action/Status role |

The board's lettering is generated raster content and cannot be used as font art. Production must
select a licensed project TMP font. Desired characteristics:

- condensed or semi-condensed;
- bold but not bulbous;
- open counters at phone scale;
- minimal or no outline;
- at most one subtle 1–2 px/dp shadow/offset;
- stable tabular numerals or width handling for prices;
- uppercase with deliberate tracking, not generated spacing.

Existing project fonts must be audited before choosing. If neither Lilita One nor the current body
font reproduces this narrower hierarchy, record typography as an approval dependency rather than
baking text or adding an unlicensed font.

## 8. Icon and illustration inventory

### 8.1 Functional UI icons

| IDs | Icon | Observed size | Construction | Production decision |
|---|---|---:|---|---|
| I-16 / P-01 | Gear | 33×33 inside 51×51 socket | Dark eight-tooth gear, cream hub | Functional Settings glyph; recreate/use licensed project icon as live layer |
| I-17 / P-02 | Currency coin | 31×31 inside 101×43 chip | Gold circular medallion | Use authoritative game resource icon, not generated coin pixels |
| I-18 / P-03 | Plus | 23×27 inside 40×51 socket | Cream thick plus | Functional add-currency glyph; live layer |
| I-19 / P-09 | Knife | 49×50 inside 67×68 socket | Diagonal cream blade, amber grip | Functional knife action; use authoritative project knife/icon art |
| I-10 / I-11 | Price medallion | 67×68; internal mark 31×38 | Gold coin with conifer/up-arrow-like mark | Concept only; internal meaning is ambiguous |
| I-12 / I-13 | Owned mark | 59×60; check 31×28 | Teal ring plus check | Approved semantic direction; must remain separate live/status layer |
| I-14 / I-15 | Equipped mark | 60×63; internal mark 30×37 | Shield plus conifer | Shield gives non-color distinction; consider a clearer star/knife symbol |

### 8.2 Decorative reference-only icons

| IDs | Icon | Decision |
|---|---|---|
| I-01, I-02 | Mirrored botanical header sprigs | Reference-board decoration; not part of button family |
| I-03–I-08 | Inward side arrows on Primary/Secondary/Danger | Reject. They imply navigation/focus without corresponding behavior |
| Gold title dots | Small separators | Documentation decoration only |
| Section rules and endpoint dots | Horizontal guide ornaments | Gallery/documentation only |
| Footer tree silhouettes and corner foliage | Environmental framing | Not reusable button elements |

### 8.3 Context illustrations

| ID | Illustration | Observed box | Decision |
|---|---|---:|---|
| P-04 / I-20 | Knife embedded in target | 74×174 | Main-menu contextual illustration; outside button scope |
| P-05 / I-21 | Round target and stand | 205×274 | Main-menu contextual illustration; outside button scope |
| I-22 | Mushrooms | Approx. 62×62 | Environmental decoration; outside button scope |
| — | Pine trees, rocks, bushes, clouds | Phone background | Environmental direction only |
| — | Phone hardware/notch/speaker | Mockup chrome | Never a game asset |

### 8.4 Icon-system rules inferred from the reference

- One glyph per socket.
- Icon silhouette should occupy roughly 55–70% of its visual socket.
- Do not nest an existing framed/composite button icon inside a new socket.
- Use two-tone construction at most: semantic foreground plus one accent.
- Keep icon background and hit root separate from the glyph.
- Use shape plus label/state copy for Owned and Equipped.
- Navigation glyph production remains P2-005 and is not authorized by this reference analysis.

## 9. Screen-layer decomposition

The embedded phone contains the following layers from back to front:

1. **Sky gradient / atmospheric background** — pale mint sky, clouds.
2. **Distant forest** — low-contrast conifers.
3. **Midground forest** — darker tree silhouettes framing the playfield.
4. **Ground dressing** — bushes, rocks, mushrooms, grass.
5. **Hero target assembly** — target disc and wooden stand.
6. **Hero knife** — separate foreground illustration.
7. **Top utility UI** — Settings, currency chip, Add Currency.
8. **Primary action stack** — Primary, Secondary, Danger.
9. **Bottom icon action** — centered icon-only socket.
10. **Safe-area/phone chrome** — mockup-only hardware around the viewport.

This hierarchy communicates useful composition, but it is not authorization to recompose the Main
Menu. Screen redesign belongs to P3. For the current button effort, only layers 7–9 define reusable
UI relationships.

## 10. Approved, conditional, and rejected elements

### Approved direction

- clipped-corner geometry;
- thin boundary and shallow shadow;
- amber Primary, teal Secondary, coral Danger, parchment Price;
- deep-teal status rows;
- Espresso/Cream typography pairing;
- square icon socket using the same geometry;
- simple ring/check for Owned;
- a separate non-circular Equipped shape;
- large quiet centers and live text/icon layers.

### Conditional; requires a clean candidate

- exact hue/saturation values;
- texture strength;
- precise corner-cut size;
- exact depth/shadow pixels;
- exact typography/font;
- exact Equipped interior symbol;
- final status-row heights;
- nine-slice borders and runtime source dimensions.

### Rejected

- old thick carved frames and asymmetric lower-right machinery;
- heavy black blob lettering;
- decorative arrows on ordinary actions;
- generated text as artwork;
- generated currency/knife pixels as authoritative icons;
- nested composite icons;
- oversized floating ownership badges;
- cropping this board into runtime assets;
- stretching the embedded narrow phone to 9:16.

## 11. Production element list

If the direction passes a clean candidate review, the eventual production package should contain:

1. Blank Primary surface.
2. Blank Secondary surface.
3. Blank Danger surface.
4. Blank Price surface.
5. Blank square icon socket.
6. Owned check mark.
7. Equipped shield/distinct mark.
8. Optional shared thin highlight/edge rules only if not baked into the surfaces.
9. Live TMP Action and Value styles.
10. Live authoritative icons supplied by their existing domain owners.

The concept sheet itself should demonstrate text and icons but the retained runtime surfaces must
remain blank.

## 12. Validation requirements for a future clean candidate

Before production replacement or live integration:

- compare every variant at 1080×1920 and 360×640;
- inspect grayscale and color-blind-independent Owned/Equipped recognition;
- verify live TMP labels and price values at their longest representative widths;
- remove decorative arrows and all generated text from source surfaces;
- confirm a single geometry and edge thickness across variants;
- test minimum, typical, and maximum nine-slice widths;
- validate the 48 dp minimum hit target independently of the visual face;
- record and inspect video for Pressed, Loading, Blocked, cancellation, Disabled, and reduced
  motion after final sprites exist;
- obtain explicit user visual approval before replacing the rejected runtime family or integrating
  production screens.

## 13. Provenance boundary

The reference was produced with OpenAI built-in ImageGen for visual exploration. Its composite
pixels are not production assets. The retained reference and measurements document direction and
relationships only. Any future production sprite must have its own prompt, generation/edit record,
source file, modification history, hash, import settings, and approval evidence.

## Appendix A. Exact reference-generation prompt

```text
Use case: ui-mockup
Asset type: high-fidelity visual direction reference for a portrait casual mobile knife-toss arcade game UI
Primary request: create one polished art-direction board showing a cohesive modern button family and a small in-context phone screen sample. This is a reference concept, not production sprites.
Subject: six clean button examples: primary, secondary, danger, icon-only, price, and owned/equipped. Use simple sample labels: PLAY, STAGES, RETRY, 1,250, OWNED, EQUIPPED. Include a compact portion of a forest arcade main menu to demonstrate scale.
Style/medium: premium contemporary casual mobile game UI; graphic, crisp, restrained, playful, slightly tactile; subtle hand-crafted forest personality without medieval fantasy ornament.
Color palette: deep pine background, warm amber primary, fresh teal secondary, coral danger, warm cream price surface, dark espresso typography; controlled highlights.
Materials/textures: mostly flat painted shapes with a very subtle paper/woodgrain texture; thin dark edging; small soft shadow; no thick carved frames.
Composition/framing: portrait 9:16 reference sheet, large readable controls at real phone scale, consistent rounded-octagonal geometry, generous negative space, clean hierarchy.
Lighting/mood: bright, friendly, energetic, polished.
Constraints: thin borders, shallow depth, consistent silhouettes, high legibility, restrained ownership badges, full-width modern controls. Avoid bulky frames, black blob lettering, faux-medieval wood, gold overload, bevel stacking, mismatched corners, excessive grain, tiny details, photorealism, characters, hands, watermarks, or logos.
```

The prompt requested a 9:16 reference sheet; the actual output is 941×1672 (aspect 0.5628), which
is effectively 9:16. The internal phone viewport is separately narrower and must not be confused
with the output aspect.
