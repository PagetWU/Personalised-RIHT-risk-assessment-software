# RIHT 6_demo Test Results

## Environment

- Python: `C:\Users\Jiaming\.conda\envs\python\python.exe`
- Checked imports: `SimpleITK`, `radiomics`, `pandas`, `scipy`
- CLI entry: `python -m riht_demo.cli --help`

## Smoke Tests

### PWH002, MHA input, no target masks

Command:

```powershell
python -m riht_demo.cli predict `
  --case-dir "F:\hypothyroidism_final_work\new_data\Thyroid NTCP validation\PWH_final_1102\PW002" `
  --age 74.13424657534246 `
  --n-stage 2 `
  --out-dir "C:\Users\Jiaming\Documents\hypothyroidism\six_demo_build\outputs\PWH002_no_target_smoke"
```

Result:

- With-volume original all-period risk: `0.4710926767`
- No-volume demo all-period risk should be interpreted against the no-volume model assets.
- Phenotype: `persistent_low`
- Optimizability: `indeterminate_missing_target`

### PWH002, NII.GZ input

The same PWH002 images were converted from `.mha` to `.nii.gz` and rerun.

Result:

- Demo all-period risk: `0.4707316674`
- Phenotype: `persistent_low`
- Optimizability: `indeterminate_missing_target`
- This confirms `.nii.gz` input works and is numerically consistent with `.mha`.

### QEH_022, MHA input, target masks available

Command:

```powershell
python -m riht_demo.cli predict `
  --case-dir "E:\QEH_NPC_THYROID\data\RIHT\image\MHA\QEH\mha_database\QEH_022" `
  --age 59.29 `
  --n-stage 3 `
  --out-dir "C:\Users\Jiaming\Documents\hypothyroidism\six_demo_build\outputs\QEH_022_smoke"
```

Result:

- Original all-period risk: `0.9213288165`
- Demo all-period risk: `0.9180934757`
- Original 3-year risk: `0.2710619785`
- Demo 3-year risk: `0.2673996141`
- Optimizability: `non_optimizable_target_adjacent`

Note: this case lies very close to the `3y >= 0.27` threshold. The small difference is driven mainly by direct image-derived thyroid volume / Dmean differences from the original tabular DVH values. The risk curve is close, but the threshold label can flip near the boundary.
