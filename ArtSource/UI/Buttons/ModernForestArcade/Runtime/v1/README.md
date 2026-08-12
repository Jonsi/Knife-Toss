# Modern Forest Arcade normalized runtime sprites v1

Date: 2026-08-12
Status: imported into Unity and validated

These PNGs are deterministic runtime derivatives of the retained `Masters/v1` sources. Rebuild
them from the repository root with:

```text
python3 ArtSource/UI/Buttons/ModernForestArcade/build_runtime_sprites.py \
  --master-dir ArtSource/UI/Buttons/ModernForestArcade/Masters/v1 \
  --runtime-dir ArtSource/UI/Buttons/ModernForestArcade/Runtime/v1
```

The builder crops and normalizes the approved masters, uses the Primary alpha as the exact shared
action silhouette for Primary/Secondary/Danger, adapts only RGB for semantic variants, and composes
the two separate ownership marks into one multiple-sprite sheet. It never bakes labels, prices,
resource values, or ownership truth.

| Output | Size | Unity border L/B/R/T | SHA-256 |
|---|---:|---:|---|
| `ButtonPrimary.png` | 768×204 | 48/30/48/30 | `afadbb65cf592e844111b344f887cae5740f909908c535cb0f3c524fcc5103d4` |
| `ButtonSecondary.png` | 768×204 | 48/30/48/30 | `0c5ade491ea71b791f88abfe6c306732b31d7576003744ffb5658c69b38def03` |
| `ButtonDanger.png` | 768×204 | 48/30/48/30 | `c559e446fe4d4317c585dc095b96a6ca0463d934902ecb3e3af981ccc63845be` |
| `ButtonPrice.png` | 768×200 | 48/28/48/28 | `626977ebca1a566f874fb986adc76697e319660110966cec3625a607f2b5d744` |
| `ButtonStatus.png` | 768×172 | 42/24/42/24 | `5f0efb0bd737a0e49e2e67ea4ff85561d38cf9a51590254693939b92f71284dc` |
| `ButtonIconSocket.png` | 256×256 | 40/40/40/40 | `3a91ca965a0f31f6d4b789dd664357bfefc617ef38be2364294fcb356ec682a9` |
| `ButtonOwnershipMarks.png` | 512×256 | none | `3c28c0f5128640ed7aae243a74e49d54ba1a927e1f003ce064faad9ec299fdff` |

Automated Unity coverage proves transparent and opaque pixels, fully opaque nine-slice centers,
exact action-family alpha identity, importer/platform settings, ownership rects, and exact V2 atlas
membership. Generated output identity depends on the retained input bytes, the checked-in builder,
and compatible Pillow resampling behavior.
