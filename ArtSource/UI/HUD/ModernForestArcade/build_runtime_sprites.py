#!/usr/bin/env python3
"""Build normalized Gameplay HUD sprites from approved versioned RGBA masters."""

from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parent
MASTER = ROOT / "Masters" / "v1"
RUNTIME = ROOT / "Runtime" / "v1"
PROJECT = ROOT.parents[3]
UNITY_UI = PROJECT / "Assets" / "Art" / "UI" / "HUD"
GUTTER = 8

SOURCES = {
    "HudStageProgressShell": ("HudStageProgressShell_RGBA_v1.png", (1024, 384)),
    "HudKnifeMagazineShell": ("HudKnifeMagazineShell_RGBA_v1.png", (384, 1024)),
    "HudPowerUpBezel": ("HudPowerUpBezel_RGBA_v1.png", (512, 512)),
    "HudCountBadge": ("HudCountBadge_RGBA_v1.png", (256, 256)),
    "HudProgressMarker": ("HudProgressMarker_RGBA_v1.png", (128, 128)),
}


def alpha_bbox(image: Image.Image, threshold: int = 8) -> tuple[int, int, int, int]:
    alpha = image.getchannel("A").point(lambda value: 255 if value > threshold else 0)
    bounds = alpha.getbbox()
    if bounds is None:
        raise ValueError("Source image contains no visible subject")
    return bounds


def normalize(source_path: Path, canvas_size: tuple[int, int]) -> Image.Image:
    source = Image.open(source_path).convert("RGBA")
    subject = source.crop(alpha_bbox(source))

    available_width = canvas_size[0] - GUTTER * 2
    available_height = canvas_size[1] - GUTTER * 2
    scale = min(available_width / subject.width, available_height / subject.height)
    width = max(1, round(subject.width * scale))
    height = max(1, round(subject.height * scale))
    subject = subject.resize((width, height), Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
    origin = ((canvas_size[0] - width) // 2, (canvas_size[1] - height) // 2)
    canvas.alpha_composite(subject, origin)
    return canvas


def validate(name: str, image: Image.Image) -> None:
    width, height = image.size
    corners = (
        image.getpixel((0, 0))[3],
        image.getpixel((width - 1, 0))[3],
        image.getpixel((0, height - 1))[3],
        image.getpixel((width - 1, height - 1))[3],
    )
    if any(corners):
        raise ValueError(f"{name} has non-transparent canvas corners: {corners}")

    if name == "HudPowerUpBezel" and image.getpixel((width // 2, height // 2))[3] != 0:
        raise ValueError("HudPowerUpBezel center must remain transparent")


def write(image: Image.Image, source_output: Path, project_output: Path) -> None:
    source_output.parent.mkdir(parents=True, exist_ok=True)
    project_output.parent.mkdir(parents=True, exist_ok=True)
    image.save(source_output, optimize=True)
    image.save(project_output, optimize=True)


def main() -> None:
    for name, (source_name, canvas_size) in SOURCES.items():
        output = normalize(MASTER / source_name, canvas_size)
        validate(name, output)
        write(output, RUNTIME / f"{name}.png", UNITY_UI / f"{name}.png")


if __name__ == "__main__":
    main()
