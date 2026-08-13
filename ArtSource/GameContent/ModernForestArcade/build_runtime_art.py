#!/usr/bin/env python3
"""Build the approved default knife and MainMenu target runtime textures."""

from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parent
MASTER = ROOT / "Masters" / "v1"
RUNTIME = ROOT / "Runtime" / "v1"
PROJECT = ROOT.parents[2]


def alpha_bbox(image: Image.Image, threshold: int = 8) -> tuple[int, int, int, int]:
    alpha = image.getchannel("A").point(lambda value: 255 if value > threshold else 0)
    bounds = alpha.getbbox()
    if bounds is None:
        raise ValueError("Source image contains no visible subject")
    return bounds


def normalize(
    source_path: Path,
    canvas_size: tuple[int, int],
    subject_box: tuple[int, int, int, int],
) -> Image.Image:
    source = Image.open(source_path).convert("RGBA")
    subject = source.crop(alpha_bbox(source))

    x, y, width, height = subject_box
    subject = subject.resize((width, height), Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
    canvas.alpha_composite(subject, (x, y))
    return canvas


def write(image: Image.Image, source_output: Path, project_output: Path) -> None:
    source_output.parent.mkdir(parents=True, exist_ok=True)
    project_output.parent.mkdir(parents=True, exist_ok=True)
    image.save(source_output, optimize=True)
    image.save(project_output, optimize=True)


def main() -> None:
    # Preserve the existing Knife_0 world dimensions at 3x source density.
    knife = normalize(
        MASTER / "DefaultKnife_RGBA_v1.png",
        canvas_size=(256, 384),
        subject_box=(74, 5, 108, 375),
    )
    write(
        knife,
        RUNTIME / "DefaultKnife.png",
        PROJECT / "Assets" / "Art" / "gameplay" / "knife.png",
    )

    # Preserve the MainMenu target texture canvas while making the stump shallow and readable.
    target = normalize(
        MASTER / "MainMenuTarget_RGBA_v1.png",
        canvas_size=(1536, 1024),
        subject_box=(340, 375, 856, 400),
    )
    write(
        target,
        RUNTIME / "MainMenuTarget.png",
        PROJECT / "Assets" / "Art" / "knife_pick_target.png",
    )


if __name__ == "__main__":
    main()
