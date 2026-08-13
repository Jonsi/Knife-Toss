# Modern Forest Arcade MainMenu icons

This source package replaces the temporary procedural MainMenu glyphs with a bespoke illustrated
family matching the approved Modern Forest Arcade reference.

## Source and generation record

- Reference: `docs/UIProduction/References/ModernForestArcade_ButtonSystem_Reference.png`
- Built-in ImageGen output ID: `exec-6deb42c9-742b-4cc4-ab4f-556061074a7c`
- Chroma source SHA-256: `63c456496891fba3b34b5d460e3eaf5923ef4e626aa212d250551df0f0320580`
- Alpha source SHA-256: `ed9c15c73eccb8c295166eac91e24c2f9dada13da17ed51a49b2b5d7931f9c4c`
- Provider: OpenAI built-in ImageGen, generated 2026-08-13 for this project.
- Modification chain: generated on flat magenta; chroma removed with the system ImageGen helper;
  cells cropped and normalized deterministically by `build_runtime_icons.py`.

The production prompt requested exactly seven isolated hand-painted casual-game glyphs in a 4x2
sheet: Settings, Store, Missions, Remove Ads, Previous, Next, and Currency. It required the
reference's ivory/espresso/amber/teal palette, shallow bevels, subtle wood/enamel texture, strong
32-48 dp silhouettes, no text, no button/socket frames, and a uniform `#FF00FF` background.

Runtime PNGs are 256x256 RGBA with a normalized 220px maximum subject dimension. Live values,
routes, availability, and interaction remain owned by the existing Unity components; these files
are presentation only.

## v2 targeted revision

User review approved Settings, Store, Missions, Previous, and Next without changes. The v2 edit
therefore replaces only two cells and continues building the other five from the exact v1 source:

- Remove Ads: simplified video-ad card with no television knobs or fine control details.
- Currency: replaced the pine emblem with a raised uppercase `CG` monogram for CuzGames.

Built-in ImageGen edit output ID: `exec-8e4786d3-3210-4a40-bce8-13e12fe53c5c`.
The v2 chroma and RGBA masters have SHA-256 hashes
`abf0bbaf2518ca00d6dc5c29a232d1e6688089710f1cba1843faa31710fb0d3e` and
`90cea9b814400ac98e85750f629f0a40be859bcccb6feb5ac9ddcfdea3d3e147` respectively.

## v3 Remove Ads correction

Live review showed that the v2 prohibition ring collapsed into disconnected arc fragments at the
90 px glyph size. V3 replaces only `IconRemoveAds` with a separately generated horizontal video
card, ivory play mark, and one continuous coral diagonal slash. It deliberately has no circle,
ring, arc, lower bar, external triangle, knobs, antenna, or stand. The other six icons remain
byte-identical to their approved v1/v2 runtime inputs.

- Built-in ImageGen output ID: `exec-624a5662-b723-40e4-bd78-8ddf63258057`
- Editable sources: `Masters/v3/IconRemoveAds_Chroma_v3.png` and
  `Masters/v3/IconRemoveAds_RGBA_v3.png`
- Modification chain: generated on flat magenta, extracted with the system chroma helper using
  explicit balanced-magenta key `#ef09ef`, then normalized to the shared 256×256 / 220 px contract.

## v4 proportion correction

V3 removed the broken fragments but retained a second rectangular frame inside the existing button
socket, which made the glyph appear undersized. V4 replaces only `IconRemoveAds` with one frameless
large ivory/amber play triangle crossed by a continuous coral slash. The combined mark uses the
full square glyph silhouette without a screen, card, ring, arc, lower bar, or external fragment.

- Built-in ImageGen output ID: `exec-59ff2447-ac54-4f25-b214-0c30d53eb931`
- Editable sources: `Masters/v4/IconRemoveAds_Chroma_v4.png` and
  `Masters/v4/IconRemoveAds_RGBA_v4.png`
- Modification chain: generated on flat magenta, extracted using explicit balanced-magenta key
  `#ef09ef`, then normalized by the shared deterministic runtime builder.

## v5 approved ADS correction

V5 replaces only `IconRemoveAds` with the user-approved button concept: large optically centered
uppercase `ADS` in warm ivory with a dark-teal separation edge, crossed by one warm red-orange
slash rendered on top. There is no inner card, screen, play mark, prohibition circle, or secondary
frame. The other six icons remain byte-identical to their approved runtime inputs.

- Approved button mockup output ID: `exec-ef425905-554d-4345-9fed-095699df933a`
- Production cutout output ID: `exec-6f667938-3a00-45f7-ab72-6ed04bc83b29`
- Editable sources: `Masters/v5/IconRemoveAds_Chroma_v5.png` and
  `Masters/v5/IconRemoveAds_RGBA_v5.png`
- Modification chain: generated from the approved button mockup on flat magenta, extracted with
  the system ImageGen chroma helper using a soft matte, despill, and one-pixel edge contraction,
  then normalized by the shared deterministic runtime builder.
