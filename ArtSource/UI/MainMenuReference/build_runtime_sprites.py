#!/usr/bin/env python3
"""Build the approved Core Loop Main Menu chrome into isolated Unity sprites."""

from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "Masters/v1/MainMenuChrome_RGBA_v1.png"
BUTTON_SOURCE = ROOT / "Masters/v1/MainMenuButtons_RGBA_v2.png"
BACKGROUND_SOURCE = ROOT / "Masters/v1/MainMenuForestBackground_v1.png"
RUNTIME = ROOT / "Runtime/v1"
UNITY_UI = ROOT.parents[2] / "Assets/Art/UI/MainMenuReference"

# Bounds were recovered from connected components of alpha >= 16. They are padded by a few
# pixels so antialiasing and the shallow painted shadow remain intact.
BOUNDS = {
    "MainMenuWoodIconSocket": (73, 96, 326, 341),
    "MainMenuSideNavPlaque": (433, 77, 680, 398),
    "MainMenuCurrencyBar": (781, 124, 1407, 315),
    "MainMenuWoodIconSocketSmall": (1461, 114, 1683, 332),
    "MainMenuKnifeSelectorFrame": (916, 459, 1767, 829),
}

BUTTON_BOUNDS = {
    "MainMenuPlayButton": (14, 605, 476, 731),
    "MainMenuStagesButton": (499, 608, 903, 733),
}

TARGETS = {
    "MainMenuWoodIconSocket": (256, 256),
    "MainMenuSideNavPlaque": (256, 336),
    "MainMenuCurrencyBar": (768, 234),
    "MainMenuWoodIconSocketSmall": (256, 256),
    "MainMenuPlayButton": (768, 256),
    "MainMenuStagesButton": (768, 224),
    "MainMenuKnifeSelectorFrame": (1024, 445),
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
    button_source = Image.open(BUTTON_SOURCE).convert("RGBA")
    RUNTIME.mkdir(parents=True, exist_ok=True)
    UNITY_UI.mkdir(parents=True, exist_ok=True)

    for name, bounds in BOUNDS.items():
        output = resize_contain(source.crop(bounds), TARGETS[name])
        output.save(RUNTIME / f"{name}.png", optimize=True)
        output.save(UNITY_UI / f"{name}.png", optimize=True)

    for name, bounds in BUTTON_BOUNDS.items():
        output = resize_contain(button_source.crop(bounds), TARGETS[name])
        output.save(RUNTIME / f"{name}.png", optimize=True)
        output.save(UNITY_UI / f"{name}.png", optimize=True)

    background = Image.open(BACKGROUND_SOURCE).convert("RGB")
    background.save(RUNTIME / "MainMenuForestBackground.png", optimize=True)
    background.save(UNITY_UI / "MainMenuForestBackground.png", optimize=True)


if __name__ == "__main__":
    main()
