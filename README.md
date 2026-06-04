# Personalised-RIHT-risk-assessment-software-
The personalised RIHT risk assessment software was designed to support clinically usable patient-level assessment from routine radiotherapy planning data.

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
  Optional nnU-Net wrapper scripts for thyroid auto-segmentation.

autoseg_model/
  Local model folder layout for the optional thyroid segmentation model.

autoseg_model_stub/
  Public-safe model layout stub. The large nnU-Net checkpoint is not included.

examples/
  Example command and a rendered HTML sample report.

RIHT_demo_launcher.exe
  Optional prebuilt Windows launcher.

RIHT_demo_launcher.cs
  Windows launcher source.

build_launcher.ps1
  Rebuilds the Windows launcher locally.
```

## What Is Not Included

- Patient CT images, dose maps, thyroid masks, or target masks.
- Protected health information.
- API keys or remote service credentials.
- The large nnU-Net thyroid segmentation checkpoint. The expected `checkpoint_best.pth` is approximately 815 MB and should be distributed through Git LFS, a release asset, Zenodo/OSF, or institutional storage.

The demo can still run without the nnU-Net checkpoint if a thyroid mask is provided in the case folder.

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

If you want to use automatic thyroid segmentation, install and configure nnU-Net separately. See:

- `autoseg/docs/segmentation_model_dataset1102_details.md`
- `autoseg/scripts/run_thyroid_segmentation_pipeline.ps1`
- `autoseg_model_stub/README.md`

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

When the nnU-Net model and environment are configured locally:

```powershell
python -m riht_demo.cli predict `
  --case-dir "D:\case001" `
  --age 58 `
  --n-stage 2 `
  --asset-dir ".\model_assets_parameters" `
  --autoseg-script ".\autoseg\scripts\run_thyroid_segmentation_pipeline.ps1" `
  --autoseg-model-folder ".\autoseg_model" `
  --out-dir "D:\riht_outputs\case001"
```

Use `--force-auto-segment-thyroid` to run auto-segmentation even when a thyroid mask already exists.

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
| `--autoseg-script` | no | Path to the thyroid segmentation PowerShell pipeline. |
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

## Model Defaults

Current packaged Cox model:

- Model id: `O3_EXTSMALL_0878_NOVOLUME`
- Model label: `R3 + D4 + C + Dmean only`
- Main clinical inputs: age and N-stage
- Main dose input: thyroid Dmean
- Radiomics/dosiomics features: loaded from `model_assets_parameters/model_spec.json`

Display defaults:

- CT window: WL 50 / WW 400
- Dose display range: 0-70 Gy in the HTML report
- Hotspot threshold: 40 Gy, configurable with `--hotspot-threshold-gy`

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
2. Keep the nnU-Net checkpoint outside normal Git history unless using Git LFS or release assets.
3. Prefer relative example paths in documentation.
4. Keep `model_assets_parameters/` with the demo if the prediction command is expected to run out of the box.
5. Document how external users should obtain the optional auto-segmentation checkpoint.

## Disclaimer

This software is provided for research demonstration, reproducibility, and counterfactual audit. It is not a certified medical device, not a radiotherapy planning optimizer, and not a substitute for clinician review.

