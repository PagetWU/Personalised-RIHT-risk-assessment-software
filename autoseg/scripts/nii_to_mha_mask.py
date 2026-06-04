import argparse
from pathlib import Path

import SimpleITK as sitk


def main() -> None:
    parser = argparse.ArgumentParser(description="Convert predicted thyroid_seg.nii.gz to thyroid_mask_autoseg.mha using CT.mha as reference")
    parser.add_argument("--input-nii", required=True)
    parser.add_argument("--reference-ct", required=True)
    parser.add_argument("--output-mha", required=True)
    args = parser.parse_args()

    pred = sitk.ReadImage(str(Path(args.input_nii)))
    mask = sitk.Cast(pred > 0, sitk.sitkUInt8)
    ct = sitk.ReadImage(str(Path(args.reference_ct)))

    if tuple(mask.GetSize()) == tuple(ct.GetSize()):
        mask.CopyInformation(ct)
    else:
        mask = sitk.Resample(
            mask,
            ct,
            sitk.Transform(),
            sitk.sitkNearestNeighbor,
            0,
            sitk.sitkUInt8,
        )

    output_mha = Path(args.output_mha)
    output_mha.parent.mkdir(parents=True, exist_ok=True)
    sitk.WriteImage(mask, str(output_mha))
    print(output_mha)


if __name__ == "__main__":
    main()
