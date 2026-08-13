# Approved Stage Selection production elements

These isolated production assets recreate the third-phone Stage Selection direction from
`docs/GameWideUxArtReference/02_Meta_Progression_Target.png`; no composite board pixels ship.

- Built-in ImageGen output: `exec-46c715a4-8fbc-4a5b-a5ec-b04b09c24506`
- Provider: OpenAI built-in ImageGen, 2026-08-13.
- Prompt: create six isolated blank elements matching the reference—title plaque, open world frame,
  complete/current/locked stage tokens, and unlock requirement plaque—on flat magenta.
- Modification chain: system chroma helper with soft matte, despill, and one-pixel edge contraction;
  deterministic padded crops and normalization via `build_runtime_sprites.py`.

World names, stage numbers, status labels, artwork, rewards, callbacks, progression, and locks remain
separate runtime-owned layers.
