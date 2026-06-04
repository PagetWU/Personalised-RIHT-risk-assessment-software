import argparse
from pathlib import Path

import SimpleITK as sitk


def main() -> None:
    parser = argparse.ArgumentParser(description="Convert one CT.mha to nnUNet-style <case>_0000.nii.gz")
    parser.add_argument("--input-mha", required=True)
    parser.add_argument("--output-dir", required=True)
    parser.add_argument("--case-id", required=True)
    args = parser.parse_args()

    input_mha = Path(args.input_mha)
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    output_nii = output_dir / f"{args.case_id}_0000.nii.gz"

    image = sitk.ReadImage(str(input_mha))
    sitk.WriteImage(image, str(output_nii))
    print(output_nii)


if __name__ == "__main__":
    main()
