#!/usr/bin/env python3
"""Build isolated Stage Selection UI sprites from the approved generated sheet."""

from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "Masters/v1/StagesChrome_RGBA_v1.png"
RUNTIME = ROOT / "Runtime/v1"
UNITY_UI = ROOT.parents[2] / "Assets/Art/UI/StagesReference"

BOUNDS = {
    "StagesTitlePlaque": (186, 56, 1194, 237),
    "StagesWorldFrame": (56, 255, 1320, 780),
    "StagesTokenComplete": (62, 799, 325, 1059),
    "StagesTokenCurrent": (351, 799, 615, 1058),
    "StagesTokenLocked": (639, 799, 902, 1058),
    "StagesUnlockPlaque": (943, 860, 1330, 1008),
}

TARGETS = {
    "StagesTitlePlaque": (1024, 184),
    "StagesWorldFrame": (1024, 426),
    "StagesTokenComplete": (256, 256),
    "StagesTokenCurrent": (256, 256),
    "StagesTokenLocked": (256, 256),
    "StagesUnlockPlaque": (768, 294),
}


def resize_contain(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    scale = min(size[0] / image.width, size[1] / image.height)
    resized = image.resize(
        (max(1, round(image.width * scale)), max(1, round(image.height * scale))),
        Image.Resampling.LANCZOS,
    )
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    canvas.alpha_composite(
        resized,
        ((size[0] - resized.width) // 2, (size[1] - resized.height) // 2),
    )
    return canvas


def main() -> None:
    source = Image.open(SOURCE).convert("RGBA")
    RUNTIME.mkdir(parents=True, exist_ok=True)
    UNITY_UI.mkdir(parents=True, exist_ok=True)

    for name, bounds in BOUNDS.items():
        output = resize_contain(source.crop(bounds), TARGETS[name])
        output.save(RUNTIME / f"{name}.png", optimize=True)
        output.save(UNITY_UI / f"{name}.png", optimize=True)


if __name__ == "__main__":
    main()
