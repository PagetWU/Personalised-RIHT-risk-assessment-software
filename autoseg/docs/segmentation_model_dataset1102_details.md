# Dataset1102 Thyroid Auto-Segmentation Model Details

## 1. Model Identity

| Item | Value |
|---|---|
| Model purpose | Thyroid gland auto-segmentation on CT |
| Model type | Locally trained nnU-Net v2 model |
| External/public pretrained model | No |
| Dataset name | `Dataset1102_ThyroidSegmentation` |
| Foreground label | `thyroid = 1` |
| Background label | `background = 0` |
| Input modality | CT |
| Inference checkpoint | `checkpoint_best.pth` |
| Inference fold | `fold_0` |

## 2. Main Model Files

| File type | Path |
|---|---|
| Recovered model root | `E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\outputs\valid_model_1102_from_root_checkpoint\nnUNet_results\Dataset1102_ThyroidSegmentation\nnUNetTrainer__nnUNetResEncUNetMPlans__3d_fullres` |
| Best checkpoint | `E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\outputs\valid_model_1102_from_root_checkpoint\nnUNet_results\Dataset1102_ThyroidSegmentation\nnUNetTrainer__nnUNetResEncUNetMPlans__3d_fullres\fold_0\checkpoint_best.pth` |
| Plans file | `E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\outputs\valid_model_1102_from_root_checkpoint\nnUNet_results\Dataset1102_ThyroidSegmentation\nnUNetTrainer__nnUNetResEncUNetMPlans__3d_fullres\plans.json` |
| Dataset file | `E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\outputs\valid_model_1102_from_root_checkpoint\nnUNet_results\Dataset1102_ThyroidSegmentation\nnUNetTrainer__nnUNetResEncUNetMPlans__3d_fullres\dataset.json` |
| Checkpoint size | 815,367,910 bytes, approximately 777.6 MB |

## 3. Training Data Records

| Item | Value |
|---|---|
| Training dataset declared in `dataset.json` | `Dataset1102_ThyroidSegmentation` |
| Number of training cases declared | 446 |
| Number of test cases declared | 0 |
| Tensor image size | 3D |
| File ending | `.nii.gz` |
| Channel names | `0: CT` |
| Label names | `background`, `thyroid` |

## 4. Preprocessed Data Located

| Item | Value |
|---|---|
| Preprocessed data directory | `E:\totalsegtry2\nnUNet\nnUNet_preprocessed\Dataset1102_ThyroidSegmentation` |
| `gt_segmentations` count | 446 |
| Low-resolution image `.b2nd` count | 446 |
| Low-resolution segmentation `_seg.b2nd` count | 446 |
| `splits_final.json` present | Yes |
| Standard raw dataset directory found | No |
| Missing standard raw path | `E:\totalsegtry2\nnUNet\nnUNet_raw\Dataset1102_ThyroidSegmentation` |

## 5. Training Result Records Located

| Item | Value |
|---|---|
| Training result root | `E:\totalsegtry2\nnUNet\nnUNet_results\Dataset1102_ThyroidSegmentation` |
| Result directory 1 | `nnUNetResEncUNetMPlans__nnUNetPlans__3d_fullres` |
| Result directory 2 | `nnUNetTrainer__nnUNetResEncUNetMPlans__3d_fullres` |
| Best checkpoint present | Yes |
| Final checkpoint present | Yes |
| Training log present | Yes |
| Progress plot present | Yes |
| Validation summary present | Yes |
| Validation mask count | 90 |

## 6. nnU-Net Configuration

| Item | Value |
|---|---|
| Framework | nnU-Net v2 |
| Bundled local runtime | `nnunetv2_clean_2_6_0` |
| Trainer | `nnUNetTrainer` |
| Plans | `nnUNetResEncUNetMPlans` |
| Configuration | `3d_fullres` |
| Network class | `dynamic_network_architectures.architectures.unet.ResidualEncoderUNet` |
| Preprocessor | `DefaultPreprocessor` |
| Image reader/writer | `SimpleITKIO` |
| Normalization | `CTNormalization` |
| Use mask for normalization | `false` |
| Batch size | 2 |
| Patch size | `[48, 192, 192]` |
| Target spacing | `[3.0, 1.015625, 1.015625]` mm |
| Median image size in voxels | `[129.0, 512.0, 512.0]` |
| Transpose forward | `[0, 1, 2]` |
| Transpose backward | `[0, 1, 2]` |

## 7. Architecture Details

| Item | Value |
|---|---|
| Architecture | Residual Encoder U-Net |
| Dimensionality | 3D |
| Number of stages | 6 |
| Features per stage | `[32, 64, 128, 256, 320, 320]` |
| Convolution operation | `torch.nn.modules.conv.Conv3d` |
| Normalization operation | `torch.nn.modules.instancenorm.InstanceNorm3d` |
| Normalization epsilon | `1e-05` |
| Normalization affine | `true` |
| Dropout | `null` |
| Nonlinearity | `torch.nn.LeakyReLU` |
| Nonlinearity inplace | `true` |

### Kernel Sizes

```text
[
  [1, 3, 3],
  [3, 3, 3],
  [3, 3, 3],
  [3, 3, 3],
  [3, 3, 3],
  [3, 3, 3]
]
```

### Strides

```text
[
  [1, 1, 1],
  [1, 2, 2],
  [2, 2, 2],
  [2, 2, 2],
  [2, 2, 2],
  [1, 2, 2]
]
```

### Residual Blocks

| Item | Value |
|---|---|
| Encoder blocks per stage | `[1, 3, 4, 6, 6, 6]` |
| Decoder convolutions per stage | `[1, 1, 1, 1, 1]` |
| Convolution bias | `true` |

## 8. Resampling

| Item | Value |
|---|---|
| Data resampling function | `resample_data_or_seg_to_shape` |
| Segmentation resampling function | `resample_data_or_seg_to_shape` |
| Probability resampling function | `resample_data_or_seg_to_shape` |
| Data interpolation order | 3 |
| Data `order_z` | 0 |
| Segmentation interpolation order | 1 |
| Segmentation `order_z` | 0 |
| Probability interpolation order | 1 |
| Probability `order_z` | 0 |
| Force separate z | `null` |

## 9. Loss Function

| Item | Value |
|---|---|
| Loss function | `DC_and_CE_loss` |
| Components | Dice loss + Cross-Entropy loss |
| Dice implementation | `MemoryEfficientSoftDiceLoss` |
| Cross-entropy implementation | `RobustCrossEntropyLoss` |
| Dice weight | 1 |
| Cross-entropy weight | 1 |
| `batch_dice` | `true` |
| `do_bg` | `false` |
| Smoothing | `1e-5` |
| Ignore label | `None` |
| Deep supervision | Enabled |
| Deep supervision weights | nnU-Net default exponentially decreasing weights; lowest output weight set to 0 and weights normalized |

## 10. Optimizer And Scheduler

| Item | Value |
|---|---|
| Optimizer | SGD |
| Initial learning rate | `1e-2` |
| Momentum | `0.99` |
| Nesterov | `true` |
| Weight decay | `3e-5` |
| LR scheduler | `PolyLRScheduler` |
| Planned epochs | 1000 |
| Deep supervision enabled during training | `true` |

## 11. Validation Metrics

| Item | Value |
|---|---|
| Validation fold | `fold_0` |
| Validation cases | 90 |
| Mean Dice | 0.8924235537337857 |
| Mean IoU | 0.8076643191986493 |
| Mean FN | 521.0444444444445 |
| Mean FP | 638.5777777777778 |
| Mean TN | 33778226.51111111 |
| Mean TP | 5150.044444444445 |
| Mean predicted voxels | 5788.622222222222 |
| Mean reference voxels | 5671.0888888888885 |

## 12. Training Log Values Found

The recovered training log has signs of file contamination or concatenation, so epoch 0 loss should not be reported as reliable.

Readable values found in the log:

| Log item | Value |
|---|---|
| Early readable train loss | approximately `-0.8351` |
| Early readable validation loss | approximately `-0.8564` |
| Early readable pseudo Dice | approximately `0.8723` |
| Later readable train loss | approximately `-0.8876` |
| Later readable validation loss | approximately `-0.8957` |
| Later readable pseudo Dice | approximately `0.9050` |
| Best EMA pseudo Dice found in readable log | approximately `0.8970` |

Note: negative loss values are expected because nnU-Net reports Dice loss in a negative soft-Dice form as part of the compound loss.

## 13. Inference/Test Output Records

| Item | Path |
|---|---|
| QMH test output | `E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\outputs\qm010_pipeline_test_20260409_valid1102` |
| PWH test output | `E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\outputs\pwh10_pipeline_test_20260409_valid1102` |
| Pipeline script | `E:\QEH_NPC_THYROID\python\phd_thesis\skills\thyroid-auto-segmentation-skills-old\scripts\run_thyroid_segmentation_pipeline.ps1` |

## 14. Important Caveats

| Item | Status |
|---|---|
| Clean recovered model bundle | Available |
| Preprocessed training data | Available |
| Training result records | Available |
| Validation summary | Available |
| Original standard raw `Dataset1102` folder | Not found |
| Some files under `E:\totalsegtry2\nnUNet` | Show signs of corruption or wrong binary content when read as text |
| Recommended paper wording | Use "locally trained/recovered nnU-Net v2 model" |
| Wording to avoid | Do not claim that the complete raw training set is currently available for exact retraining |

