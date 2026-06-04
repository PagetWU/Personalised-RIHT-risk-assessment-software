param(
    [Parameter(Mandatory = $true)][string]$PatientDir,
    [Parameter(Mandatory = $true)][string]$OutputDir,
    [string]$CaseId,
    [string]$ModelFolder = "E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\outputs\valid_model_1102_from_root_checkpoint\nnUNet_results\Dataset1102_ThyroidSegmentation\nnUNetTrainer__nnUNetResEncUNetMPlans__3d_fullres",
    [string]$Checkpoint = "checkpoint_best.pth",
    [string]$DatasetId = "Dataset1102_ThyroidSegmentation",
    [string]$Plans = "nnUNetResEncUNetMPlans",
    [string]$Trainer = "nnUNetTrainer",
    [string]$Configuration = "3d_fullres",
    [string[]]$Folds = @("0"),
    [string]$Device = "cuda",
    [string]$NnUNetRawRoot = "E:\totalsegtry2\nnUNet\nnUNet_raw",
    [string]$NnUNetPreprocessedRoot = "E:\totalsegtry2\nnUNet\nnUNet_preprocessed",
    [string]$NnUNetResultsRoot = "E:\totalsegtry2\nnUNet\nnUNet_results"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$skillRoot = Split-Path -Parent $scriptRoot
$pythonExe = "C:\Users\Jiaming\.conda\envs\totalsegtry2\python.exe"
$predictExe = "C:\Users\Jiaming\.conda\envs\totalsegtry2\Scripts\nnUNetv2_predict.exe"
$overlayDir = Join-Path $skillRoot "runtime\nnunetv2_clean_2_6_0"
if (-not (Test-Path $overlayDir)) {
    $overlayDir = "E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\runtime\nnunetv2_clean_2_6_0"
}
if (-not (Test-Path $overlayDir)) {
    throw "nnUNet overlay runtime not found. Checked: $overlayDir"
}

if (-not $CaseId) {
    $CaseId = Split-Path -Leaf $PatientDir
}

$resolvedModelFolder = $null
if ($ModelFolder -and (Test-Path $ModelFolder)) {
    $resolvedModelFolder = (Resolve-Path $ModelFolder).Path
    $datasetDirFromModel = Split-Path -Parent $resolvedModelFolder
    if ($datasetDirFromModel) {
        $DatasetId = Split-Path -Leaf $datasetDirFromModel
        $resultsRootFromModel = Split-Path -Parent $datasetDirFromModel
        if ($resultsRootFromModel) {
            $NnUNetResultsRoot = $resultsRootFromModel
        }
    }
    $modelLeaf = Split-Path -Leaf $resolvedModelFolder
    $parts = $modelLeaf -split "__"
    if ($parts.Length -ge 3) {
        $Trainer = $parts[0]
        $Plans = $parts[1]
        $Configuration = $parts[2]
    }
}

$patientDirResolved = (Resolve-Path $PatientDir).Path
$outputDirResolved = (Resolve-Path (New-Item -ItemType Directory -Force $OutputDir)).Path

$ctMha = Join-Path $patientDirResolved "CT.mha"
$refMask = $null
foreach ($candidateMask in @("thyroid_mask.mha", "Thyroid_mask.mha")) {
    $candidatePath = Join-Path $patientDirResolved $candidateMask
    if (Test-Path $candidatePath) {
        $refMask = $candidatePath
        break
    }
}
if (-not (Test-Path $ctMha)) {
    throw "CT.mha not found under $patientDirResolved"
}

$stageDir = Join-Path $outputDirResolved $CaseId
$inputNiiDir = Join-Path $stageDir "stage\input_nii"
$predNiiDir = Join-Path $stageDir "stage\pred_nii"
$recoveredDir = Join-Path $stageDir "recovered_mha"
$reportDir = Join-Path $stageDir "reports"
New-Item -ItemType Directory -Force $inputNiiDir,$predNiiDir,$recoveredDir,$reportDir | Out-Null

& $pythonExe (Join-Path $scriptRoot "mha_to_nii_case.py") `
    --input-mha $ctMha `
    --output-dir $inputNiiDir `
    --case-id $CaseId

$env:PYTHONPATH = $overlayDir
$env:PYTHONUTF8 = "1"
$env:PYTHONIOENCODING = "utf-8"
$env:nnUNet_raw = (Resolve-Path (New-Item -ItemType Directory -Force $NnUNetRawRoot)).Path
$env:nnUNet_preprocessed = (Resolve-Path (New-Item -ItemType Directory -Force $NnUNetPreprocessedRoot)).Path
$env:nnUNet_results = (Resolve-Path (New-Item -ItemType Directory -Force $NnUNetResultsRoot)).Path

$foldArgs = @()
foreach ($fold in $Folds) {
    $foldArgs += $fold
}

& $predictExe `
    -i $inputNiiDir `
    -o $predNiiDir `
    -d $DatasetId `
    -p $Plans `
    -tr $Trainer `
    -c $Configuration `
    -f $foldArgs `
    -chk $Checkpoint `
    -device $Device `
    -npp 1 `
    -nps 1

$predNii = Join-Path $predNiiDir "$CaseId.nii.gz"
if (-not (Test-Path $predNii)) {
    throw "Prediction output not found: $predNii"
}

$predMaskMha = Join-Path $recoveredDir "thyroid_mask_autoseg.mha"
& $pythonExe (Join-Path $scriptRoot "nii_to_mha_mask.py") `
    --input-nii $predNii `
    --reference-ct $ctMha `
    --output-mha $predMaskMha

$checkArgs = @(
    (Join-Path $scriptRoot "check_segmentation_geometry.py"),
    "--ct", $ctMha,
    "--pred-mask", $predMaskMha,
    "--summary-json", (Join-Path $reportDir "geometry_summary.json"),
    "--summary-md", (Join-Path $reportDir "geometry_summary.md")
)
if ($refMask -and (Test-Path $refMask)) {
    $checkArgs += @("--reference-mask", $refMask)
}
& $pythonExe @checkArgs

Write-Output $stageDir

