import argparse
import json
from pathlib import Path

import numpy as np
import SimpleITK as sitk


def get_stats(image: sitk.Image) -> dict:
    arr = sitk.GetArrayFromImage(image)
    coords = np.argwhere(arr > 0)
    voxels = int(coords.shape[0])
    if voxels == 0:
        bbox_zyx = None
        bbox_xyz = None
    else:
        mins = coords.min(axis=0).tolist()
        maxs = coords.max(axis=0).tolist()
        bbox_zyx = {"min": mins, "max": maxs}
        bbox_xyz = {
            "min": [int(mins[2]), int(mins[1]), int(mins[0])],
            "max": [int(maxs[2]), int(maxs[1]), int(maxs[0])],
        }
    return {
        "size_xyz": list(image.GetSize()),
        "spacing_xyz": list(image.GetSpacing()),
        "origin_xyz": list(image.GetOrigin()),
        "direction": list(image.GetDirection()),
        "foreground_voxels": voxels,
        "bbox_zyx": bbox_zyx,
        "bbox_xyz": bbox_xyz,
    }


def same_grid(a: sitk.Image, b: sitk.Image) -> bool:
    return (
        tuple(a.GetSize()) == tuple(b.GetSize())
        and np.allclose(a.GetSpacing(), b.GetSpacing())
        and np.allclose(a.GetOrigin(), b.GetOrigin())
        and np.allclose(a.GetDirection(), b.GetDirection())
    )


def dice(a: sitk.Image, b: sitk.Image) -> float:
    a_arr = sitk.GetArrayFromImage(a) > 0
    b_arr = sitk.GetArrayFromImage(b) > 0
    inter = np.logical_and(a_arr, b_arr).sum()
    denom = a_arr.sum() + b_arr.sum()
    if denom == 0:
        return 1.0
    return float((2.0 * inter) / denom)


def main() -> None:
    parser = argparse.ArgumentParser(description="Check CT and thyroid mask alignment")
    parser.add_argument("--ct", required=True)
    parser.add_argument("--pred-mask", required=True)
    parser.add_argument("--reference-mask")
    parser.add_argument("--summary-json", required=True)
    parser.add_argument("--summary-md", required=True)
    args = parser.parse_args()

    ct = sitk.ReadImage(str(Path(args.ct)))
    pred_mask = sitk.ReadImage(str(Path(args.pred_mask)))

    summary = {
        "ct": get_stats(ct),
        "pred_mask": get_stats(pred_mask),
        "pred_matches_ct_grid": same_grid(ct, pred_mask),
    }

    if args.reference_mask:
        ref_path = Path(args.reference_mask)
        if ref_path.exists():
            ref_mask = sitk.ReadImage(str(ref_path))
            summary["reference_mask"] = get_stats(ref_mask)
            summary["reference_matches_ct_grid"] = same_grid(ct, ref_mask)
            if not same_grid(pred_mask, ref_mask):
                ref_mask = sitk.Resample(
                    ref_mask,
                    pred_mask,
                    sitk.Transform(),
                    sitk.sitkNearestNeighbor,
                    0,
                    sitk.sitkUInt8,
                )
            summary["dice_vs_reference"] = dice(pred_mask, ref_mask)

    summary_json = Path(args.summary_json)
    summary_md = Path(args.summary_md)
    summary_json.parent.mkdir(parents=True, exist_ok=True)
    summary_md.parent.mkdir(parents=True, exist_ok=True)

    summary_json.write_text(json.dumps(summary, indent=2), encoding="utf-8")

    lines = [
        "# Segmentation Geometry Check",
        f"- CT: `{args.ct}`",
        f"- Pred mask: `{args.pred_mask}`",
        f"- Pred matches CT grid: `{summary['pred_matches_ct_grid']}`",
        f"- Pred foreground voxels: `{summary['pred_mask']['foreground_voxels']}`",
        f"- Pred bbox XYZ: `{summary['pred_mask']['bbox_xyz']}`",
    ]
    if "reference_mask" in summary:
        lines.extend(
            [
                f"- Reference mask: `{args.reference_mask}`",
                f"- Reference matches CT grid: `{summary['reference_matches_ct_grid']}`",
                f"- Dice vs reference: `{summary['dice_vs_reference']:.6f}`",
                f"- Reference bbox XYZ: `{summary['reference_mask']['bbox_xyz']}`",
            ]
        )
    summary_md.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(summary_json)


if __name__ == "__main__":
    main()
