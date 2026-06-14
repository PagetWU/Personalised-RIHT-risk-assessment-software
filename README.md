# RIHT Single-Patient Demo Toolkit

This repository contains a lightweight research demo for patient-level radiation-induced hypothyroidism (RIHT) risk reporting after head-and-neck radiotherapy.

The toolkit reads one patient case, extracts thyroid image/dose features, runs a Cox RIHT risk model, reproduces comparison NTCP-style outputs, checks whether thyroid dose sparing may be anatomically feasible, and writes a self-contained HTML report.

This is a research demonstration and counterfactual audit tool. It is not a treatment planning system, not a clinical decision system, and not intended for automatic plan modification.

## What Is Included

```text
riht_demo/
  Python package for case discovery, image/dose feature extraction,
  Cox-model prediction, NTCP-style comparison, optimizability audit,
  counterfactual dose audit, and HTML report generation.

model_assets_parameters/
  Lightweight Cox model parameters used by the demo.

autoseg/
  Optional helper files for thyroid auto-segmentation when a local
  nnU-Net environment and checkpoint are available.

autoseg_model/
  Local model folder layout for the optional thyroid segmentation model.

examples/
  Example command and a rendered HTML sample report.

RIHT_demo_launcher.exe
  Optional prebuilt Windows launcher.

RIHT_demo_launcher.cs
  Windows launcher source.

build_launcher.ps1
  Rebuilds the Windows launcher locally.
```


## Installation

Use Python 3.10+ in a local virtual environment or conda environment.

```powershell
cd F:\hypothyroidism_final_work\work_newline_2026.5\codings\codings\6_demo\github_update
python -m pip install -r requirements.txt
```

Required packages are listed in `requirements.txt`:

```text
numpy
pandas
scipy
SimpleITK
matplotlib
PyRadiomics
```

If you want to use automatic thyroid segmentation, install and configure nnU-Net separately, then download the model checkpoint as described below.

## Model Assets and Auto-Segmentation Checkpoint

The RIHT Cox prediction model parameters and the optional nnU-Net thyroid auto-segmentation checkpoint are distributed as a separate model-asset package because the segmentation checkpoint is too large for normal GitHub tracking.

Download the complete model-asset package from Google Drive:

```text
https://drive.google.com/file/d/11w32m51_bM2XIsvkfD604FJQ0GFpoJRB/view?usp=drive_link

After downloading, place `checkpoint_best.pth` in this folder layout:

```text
model_assets_parameters/
  full_qeh_model_coefficients.csv
  full_qeh_scaler_parameters.csv
  model_spec.json
  qeh_breslow_baseline_hazard.csv
  qeh_scaled_feature_means.csv

autoseg_model/
  Dataset1102_ThyroidSegmentation/
    nnUNetTrainer__nnUNetResEncUNetMPlans__3d_fullres/
      dataset.json
      plans.json
      fold_0/
        checkpoint_best.pth

If the repository package does not include `dataset.json` or `plans.json`, keep them beside the checkpoint in the same layout when distributing the model package.

The demo can run without this checkpoint if you provide a thyroid mask manually with `--thyroid-mask-path` or place `Thyroid_mask.mha` / `thyroid_mask.mha` inside the case folder.

## Input Case Folder

Each patient case folder should contain:

```text
CT.mha
RTdose.mha / DOSE.mha / dose.nii.gz
Thyroid_mask.mha or thyroid_mask.mha    optional but recommended
GTV/CTV/PTV target masks                optional
```

Supported image formats:

- `.mha`
- `.nii`
- `.nii.gz`

Target masks are optional. If target masks are present and their filenames contain `GTV`, `CTV`, or `PTV`, the demo will use them for target-adjacency and optimizability checks.

If no thyroid mask is found and automatic segmentation is not disabled, the CLI will try to call the configured nnU-Net thyroid segmentation pipeline.

## Quick Start

Run the demo from the repository root.

```powershell
python -m riht_demo.cli predict `
  --case-dir "D:\case001" `
  --age 58 `
  --gender Male `
  --n-stage 2 `
  --asset-dir ".\model_assets_parameters" `
  --ct-window-level 50 `
  --ct-window-width 400 `
  --hotspot-threshold-gy 40 `
  --out-dir "D:\riht_outputs\case001"
```

Important: pass `--asset-dir ".\model_assets_parameters"` unless you have renamed or copied the model assets to the package default path.

## Run With A Provided Thyroid Mask

If the thyroid mask is not in the case folder, provide it explicitly:

```powershell
python -m riht_demo.cli predict `
  --case-dir "D:\case001" `
  --thyroid-mask-path "D:\case001_masks\thyroid_mask.mha" `
  --age 58 `
  --gender Male `
  --n-stage 2 `
  --asset-dir ".\model_assets_parameters" `
  --out-dir "D:\riht_outputs\case001"
```

## Run Without Auto-Segmentation

Use this mode when you want the command to fail if no thyroid mask is available:

```powershell
python -m riht_demo.cli predict `
  --case-dir "D:\case001" `
  --age 58 `
  --n-stage 2 `
  --asset-dir ".\model_assets_parameters" `
  --no-auto-segment-thyroid `
  --out-dir "D:\riht_outputs\case001"
```

## Run With nnU-Net Auto-Segmentation

When the nnU-Net environment is configured locally and `checkpoint_best.pth` has been placed under `.\autoseg_model`, run:

```powershell
python -m riht_demo.cli predict `
  --case-dir "D:\case001" `
  --age 58 `
  --n-stage 2 `
  --asset-dir ".\model_assets_parameters" `
  --autoseg-model-folder ".\autoseg_model" `
  --out-dir "D:\riht_outputs\case001"
```

Use `--force-auto-segment-thyroid` to run auto-segmentation even when a thyroid mask already exists.

If your package includes a custom segmentation PowerShell script, pass it with `--autoseg-script`. If not, configure your local nnU-Net command wrapper before using auto-segmentation.

## Main CLI Arguments

| Argument | Required | Description |
|---|---:|---|
| `--case-dir` | yes | Folder containing CT, dose, optional thyroid mask, and optional target masks. |
| `--age` | yes | Patient age. |
| `--n-stage` | yes | N-stage encoded as the model input value, usually 0-3. |
| `--gender` | no | Display value in the report. Default: `Unknown`. |
| `--asset-dir` | recommended | Model asset directory. Use `.\model_assets_parameters` for this package. |
| `--thyroid-mask-path` | no | Explicit thyroid mask path. |
| `--no-auto-segment-thyroid` | no | Disable auto-segmentation if no thyroid mask exists. |
| `--force-auto-segment-thyroid` | no | Run auto-segmentation even when a mask exists. |
| `--autoseg-script` | no | Optional path to a local thyroid segmentation wrapper script. |
| `--autoseg-model-folder` | no | Path to the local Dataset1102 nnU-Net model folder. |
| `--dose-scale-to-cgy` | no | Dose scaling override. If omitted, Gy-like dose maps are multiplied by 100. |
| `--ct-window-level` | no | CT window level for the report image. Default: 50. |
| `--ct-window-width` | no | CT window width for the report image. Default: 400. |
| `--hotspot-threshold-gy` | no | Thyroid hotspot threshold for contouring and counterfactual audit. Default: 40 Gy. |
| `--out-dir` | yes | Output folder. |

## Outputs

The output folder contains:

| File | Description |
|---|---|
| `RIHT_demo_report.html` | Main self-contained HTML report. |
| `prediction_summary.json` | Summary of inputs, model outputs, risk estimates, and generated files. |
| `patient_features.csv` | Extracted model features and scaled feature values. |
| `dvh_metrics.csv` | Thyroid DVH and dose summary metrics. |
| `risk_curve.csv` | Cox-model risk curve over 1-9 years and all-period horizon. |
| `target_adjacency_metrics.csv` | Target availability, adjacency, and hotspot overlap metrics. |
| `dose_optimization_audit.csv` | Counterfactual thyroid-sparing audit table. |
| `other_model_reproduction.csv` | Comparison NTCP-style model outputs. |
| `thyroid_zoom.png` | CT/dose/thyroid visualization used by the HTML report. |

A public-safe rendered example is available at:

```text
examples/QEH_499_html_sample/RIHT_demo_report.html
```


## Optional Windows Launcher

The repository includes a small Windows launcher:

```text
RIHT_demo_launcher.exe
```

To rebuild it locally:

```powershell
.\build_launcher.ps1
```

The launcher is a convenience wrapper. The Python CLI is the canonical interface.

## Smoke-Tested Cases

The local smoke tests are summarized in `TEST_RESULTS.md`.

Completed checks include:

- `.mha` input with no target masks.
- `.nii.gz` input consistency check.
- A case with target masks and optimizability classification.

Near-threshold cases can show small classification differences when image-derived thyroid volume or Dmean differs from the original tabular DVH values.

## Notes For Public Distribution

Before publishing:

1. Confirm that no patient CT, dose, mask, or PHI files are included.
2. Keep the nnU-Net checkpoint outside normal Git history; provide the Baidu Cloud link and extraction code in the checkpoint section above.
3. Prefer relative example paths in documentation.
4. Keep `model_assets_parameters/` with the demo if the prediction command is expected to run out of the box.
5. Confirm the downloaded checkpoint lands at `autoseg_model/Dataset1102_ThyroidSegmentation/nnUNetTrainer__nnUNetResEncUNetMPlans__3d_fullres/fold_0/checkpoint_best.pth`.

## Disclaimer

This software is provided for research demonstration, reproducibility, and counterfactual audit. It is not a certified medical device, not a radiotherapy planning optimizer, and not a substitute for clinician review.
