# Modern Forest Arcade game-content art

This folder records the separately generated production sources and deterministic runtime build
for the default knife and MainMenu throwing stump. The default knife is intentionally shared by
the MainMenu preview, normal gameplay knife, pre-stuck knife, and magazine silhouette. The twelve
unlockable knife-skin sprites remain unchanged.

## Provenance

- Author/commissioner: project owner through Codex.
- Generation mode: OpenAI built-in ImageGen via the `imagegen` skill.
- Default knife output ID: `exec-797c1b35-6f7a-497f-86b0-3ad4f00af6ee`.
- MainMenu target output ID: `exec-80278036-3666-494c-b853-c03d44c312f5`.
- Both sources were generated on a flat magenta background, converted to straight-alpha RGBA with
  the skill's chroma-removal helper, then normalized by `build_runtime_art.py`.
- No legacy target, knife, Layer Lab, reference-board, or third-party pixels were copied into the
  new images.

## Prompt summary

The default-knife prompt requested one front-facing upright casual forest-arcade knife with an
ivory/mint blade, amber wrapped grip, deep-teal fittings, espresso outline, generous padding, and
no shadow, aura, text, or additional object. The target prompt requested one shallow three-quarter
round stump with amber end-grain, four burned scoring rings, dark bark, restrained cracks, no knife,
no glow, no props, and no text. Both used a flat `#ff00ff` removal background.

## Runtime contract

| Runtime file | Canvas | Subject box (top-left pixels) | Existing reference preserved |
|---|---:|---:|---|
| `Assets/Art/gameplay/knife.png` | 256×384 | `74,5,108,375` | texture GUID, `Knife_0` internal ID, 3× PPU preserving its 0.36×1.25 world size |
| `Assets/Art/knife_pick_target.png` | 1536×1024 | `340,375,856,400` | texture GUID and MainMenu scene reference |

Rebuild from the repository root with:

```bash
python3 ArtSource/GameContent/ModernForestArcade/build_runtime_art.py
```

## SHA-256

| File | SHA-256 |
|---|---|
| `DefaultKnife_Chroma_v1.png` | `71bf2dbecd29249ecdb778c5a54222bac180a5d196fc9f338b0ca75cf5c86c6b` |
| `DefaultKnife_RGBA_v1.png` | `19b5c3b9f9491ed968e24a7f715e53b6f0e3422bd5486bd9f8a441e1561a8e24` |
| `MainMenuTarget_Chroma_v1.png` | `d976516ec1462a6a61a847124e2e11225f275a42ed9ba1de133c5bb270663953` |
| `MainMenuTarget_RGBA_v1.png` | `3b35bc3cc69b6c8d8e03cd06db0cfc1d32c710011f992b7c83a2acc596ea249b` |
| `Runtime/v1/DefaultKnife.png` | `59729620fd316faedf1fa427f65c9f389f664e97329a8a747653858ffd6b8be5` |
| `Runtime/v1/MainMenuTarget.png` | `09ceef719da7579da4667eaa0b0a7be37fa64ba8ad2e5264cf687b34c13345c2` |
