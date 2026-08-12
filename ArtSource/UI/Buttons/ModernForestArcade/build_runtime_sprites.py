#!/usr/bin/env python3
"""Build Unity-ready Modern Forest Arcade button sprites from retained RGBA masters.

The action family deliberately shares one exact alpha silhouette. Secondary and Danger keep
their generated paint treatment, but Primary owns the geometry so ImageGen edge drift cannot
leak into production. All crops, sizes, pivots, gutters, and ownership-sheet cells are fixed.
"""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image


ALPHA_THRESHOLD = 3
GUTTER = 2

ACTION_SIZE = (768, 204)
PRICE_SIZE = (768, 200)
STATUS_SIZE = (768, 172)
ICON_SIZE = (256, 256)
MARK_CELL_SIZE = (256, 256)
MARK_SHEET_SIZE = (512, 256)


def alpha_bbox(image: Image.Image) -> tuple[int, int, int, int]:
    bbox = image.getchannel("A").point(
        lambda value: 255 if value >= ALPHA_THRESHOLD else 0
    ).getbbox()
    if bbox is None:
        raise ValueError("Source has no visible pixels")
    return bbox


def resize_to_canvas(
    image: Image.Image,
    source_bbox: tuple[int, int, int, int],
    size: tuple[int, int],
) -> Image.Image:
    cropped = image.crop(source_bbox)
    content_size = (size[0] - GUTTER * 2, size[1] - GUTTER * 2)
    resized = cropped.resize(content_size, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    canvas.alpha_composite(resized, (GUTTER, GUTTER))
    return canvas


def normalize_action_family(master_dir: Path, output_dir: Path) -> None:
    names = ("Primary", "Secondary", "Danger")
    sources = {
        name: Image.open(master_dir / f"Button{name}_Master_RGBA_v1.png").convert("RGBA")
        for name in names
    }
    geometry_bbox = alpha_bbox(sources["Primary"])
    normalized = {
        name: resize_to_canvas(source, geometry_bbox, ACTION_SIZE)
        for name, source in sources.items()
    }

    # Primary is the sole geometry owner. Retain each variant's generated RGB where it exists,
    # but apply Primary's exact alpha. Any tiny edit-created edge gap receives a semantic dark
    # boundary color instead of transparent-black RGB.
    geometry_alpha = normalized["Primary"].getchannel("A")
    edge_fallback = {
        "Secondary": (1, 47, 42),
        "Danger": (55, 4, 0),
    }
    for name in names:
        image = normalized[name]
        if name != "Primary":
            pixels = image.load()
            alpha = image.getchannel("A")
            geometry_pixels = geometry_alpha.load()
            alpha_pixels = alpha.load()
            fallback = edge_fallback[name]
            for y in range(ACTION_SIZE[1]):
                for x in range(ACTION_SIZE[0]):
                    target_alpha = geometry_pixels[x, y]
                    if target_alpha == 0:
                        pixels[x, y] = (0, 0, 0, 0)
                        continue
                    r, g, b, source_alpha = pixels[x, y]
                    if source_alpha < 8:
                        r, g, b = fallback
                    elif source_alpha < target_alpha:
                        blend = source_alpha / target_alpha
                        r = round(r * blend + fallback[0] * (1.0 - blend))
                        g = round(g * blend + fallback[1] * (1.0 - blend))
                        b = round(b * blend + fallback[2] * (1.0 - blend))
                    pixels[x, y] = (r, g, b, target_alpha)
        save_png(image, output_dir / f"Button{name}.png")


def normalize_surface(master: Path, destination: Path, size: tuple[int, int]) -> None:
    image = Image.open(master).convert("RGBA")
    save_png(resize_to_canvas(image, alpha_bbox(image), size), destination)


def normalize_mark(master: Path) -> Image.Image:
    image = Image.open(master).convert("RGBA")
    cropped = image.crop(alpha_bbox(image))
    available = MARK_CELL_SIZE[0] - 24, MARK_CELL_SIZE[1] - 24
    scale = min(available[0] / cropped.width, available[1] / cropped.height)
    resized_size = max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale))
    resized = cropped.resize(resized_size, Image.Resampling.LANCZOS)
    cell = Image.new("RGBA", MARK_CELL_SIZE, (0, 0, 0, 0))
    origin = ((MARK_CELL_SIZE[0] - resized.width) // 2, (MARK_CELL_SIZE[1] - resized.height) // 2)
    cell.alpha_composite(resized, origin)
    return cell


def build_mark_sheet(master_dir: Path, destination: Path) -> None:
    sheet = Image.new("RGBA", MARK_SHEET_SIZE, (0, 0, 0, 0))
    owned = normalize_mark(master_dir / "ButtonOwnedMark_Master_RGBA_v1.png")
    equipped = normalize_mark(master_dir / "ButtonEquippedMark_Master_RGBA_v1.png")
    sheet.alpha_composite(owned, (0, 0))
    sheet.alpha_composite(equipped, (MARK_CELL_SIZE[0], 0))
    save_png(sheet, destination)


def save_png(image: Image.Image, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    image.save(destination, optimize=True)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--master-dir", type=Path, required=True)
    parser.add_argument("--runtime-dir", type=Path, required=True)
    args = parser.parse_args()

    normalize_action_family(args.master_dir, args.runtime_dir)
    normalize_surface(
        args.master_dir / "ButtonPrice_Master_RGBA_v1.png",
        args.runtime_dir / "ButtonPrice.png",
        PRICE_SIZE,
    )
    normalize_surface(
        args.master_dir / "ButtonStatus_Master_RGBA_v1.png",
        args.runtime_dir / "ButtonStatus.png",
        STATUS_SIZE,
    )
    normalize_surface(
        args.master_dir / "ButtonIconSocket_Master_RGBA_v1.png",
        args.runtime_dir / "ButtonIconSocket.png",
        ICON_SIZE,
    )
    build_mark_sheet(args.master_dir, args.runtime_dir / "ButtonOwnershipMarks.png")


if __name__ == "__main__":
    main()
