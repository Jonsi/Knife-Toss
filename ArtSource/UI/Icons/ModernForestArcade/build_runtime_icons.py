#!/usr/bin/env python3
"""Build normalized transparent MainMenu icon sprites from approved versioned sources."""

from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parent
BASE_SOURCE = ROOT / "Masters/v1/ModernForestArcade_IconFamily_RGBA_v1.png"
REVISION_SOURCE = ROOT / "Masters/v2/ModernForestArcade_IconFamily_RGBA_v2.png"
REMOVE_ADS_SOURCE = ROOT / "Masters/v5/IconRemoveAds_RGBA_v5.png"
SOURCE_OUTPUT = ROOT / "Runtime/v5"
UNITY_OUTPUT = ROOT.parents[3] / "Assets/Art/UI/Icons"
CANVAS_SIZE = 256
SUBJECT_SIZE = 220

# The generated source is a 4x2 layout. Cells use source-pixel bounds so the build is
# deterministic even though the original image is not an exact multiple of four.
CELLS = {
    "IconSettings": (0, 0, 418, 470),
    "IconStore": (418, 0, 836, 470),
    "IconMissions": (836, 0, 1254, 470),
    "IconRemoveAds": (1254, 0, 1672, 470),
    "IconPrevious": (0, 470, 418, 941),
    "IconNext": (418, 470, 836, 941),
    "IconCurrency": (836, 470, 1254, 941),
}

REVISED_ICONS = {"IconCurrency"}


def normalize(cell: Image.Image) -> Image.Image:
    alpha = cell.getchannel("A")
    bounds = alpha.getbbox()
    if bounds is None:
        raise RuntimeError("Source cell has no visible pixels")

    subject = cell.crop(bounds)
    scale = min(SUBJECT_SIZE / subject.width, SUBJECT_SIZE / subject.height)
    resized = subject.resize(
        (max(1, round(subject.width * scale)), max(1, round(subject.height * scale))),
        Image.Resampling.LANCZOS,
    )

    canvas = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    x = (CANVAS_SIZE - resized.width) // 2
    y = (CANVAS_SIZE - resized.height) // 2
    canvas.alpha_composite(resized, (x, y))
    return canvas


def main() -> None:
    base_source = Image.open(BASE_SOURCE).convert("RGBA")
    revision_source = Image.open(REVISION_SOURCE).convert("RGBA")
    remove_ads_source = Image.open(REMOVE_ADS_SOURCE).convert("RGBA")
    SOURCE_OUTPUT.mkdir(parents=True, exist_ok=True)
    UNITY_OUTPUT.mkdir(parents=True, exist_ok=True)

    for name, bounds in CELLS.items():
        if name == "IconRemoveAds":
            output = normalize(remove_ads_source)
        else:
            source = revision_source if name in REVISED_ICONS else base_source
            output = normalize(source.crop(bounds))
        output.save(SOURCE_OUTPUT / f"{name}.png", optimize=True)
        output.save(UNITY_OUTPUT / f"{name}.png", optimize=True)


if __name__ == "__main__":
    main()
