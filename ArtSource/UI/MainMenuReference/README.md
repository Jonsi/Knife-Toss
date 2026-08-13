# Approved Core Loop Main Menu production elements

This package recreates clean production assets from the approved left-most Main Menu direction in
`docs/GameWideUxArtReference/01_Core_Loop_Target.png`. The reference pixels are not shipped.

- Built-in ImageGen background output: `exec-bcc65d77-4624-48e1-925e-97813c680d5f`
- Built-in ImageGen chrome output: `exec-d42147ee-72fd-4dec-8d50-2bdc91705751`
- Built-in ImageGen corrected button output: `exec-931ae3ce-11d4-45be-ba5c-0fec8f7af2b0`
- Provider: OpenAI built-in ImageGen, 2026-08-13.
- Background prompt: recreate only the bright 9:16 forest/waterfall environment, fully framed for
  portrait, with every UI, logo, knife, target, and selector element removed.
- Chrome prompt: recreate exactly seven isolated blank elements using the approved compact carved
  wood/gold/blue language: icon socket, side-nav plaque, currency bar, small icon socket, Play,
  Stages, and three-card selector.
- Button correction prompt: preserve the sheet while flattening only Play to about 3.15:1 and
  Stages to about 3.70:1, matching the approved reference proportions.
- Modification chain: generated on flat magenta, extracted with the system chroma helper using a
  soft matte, despill, and one-pixel edge contraction, component bounds recovered from alpha, then
  normalized deterministically by `build_runtime_sprites.py`.

Live labels, values, icons, callbacks, selection, navigation, currency, and purchase availability
remain separate runtime-owned layers.
