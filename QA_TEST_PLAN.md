# QA Test Plan — Unity Milestone 1

**Status:** Executed on Windows — EditMode 27/27, PlayMode 5/5, build PASS  
**Target:** Unity 6000.3.9f1 + 2D URP, Windows primary

Phase 1 baseline: EditMode 20/20, PlayMode 4/4. Phase 2 increment added 7 EditMode and 1 PlayMode test; all pass.

## Environment Preconditions

1. Unity Editor 6000.3.9f1 installed.
2. A valid Unity Editor license active on the machine — **verified** `Unity Personal / Assigned / Unlimited`.
3. Packages resolved from `Packages/manifest.json`:
   - `com.unity.2d.sprite` 1.0.0
   - `com.unity.2d.tilemap` 1.0.0
   - `com.unity.inputsystem` 1.18.0
   - `com.unity.render-pipelines.universal` 17.3.0
   - `com.unity.test-framework` 1.6.0
   - `com.unity.textmeshpro` 5.0.0
   - `com.unity.ugui` 2.0.0
4. Open project root `C:\dev\flat-iron-fields`.

## Unity Editor Setup

1. Allow initial import to finish — done, exit code 0.
2. Run `Flat Iron Fields > Generate All Milestone 1 Assets` (single command runs crop assets, system assets, and scenes).
3. Confirm assets exist — verified:

| Path | Expected | Actual |
| --- | --- | --- |
| `Assets/ScriptableObjects/Crops/*.asset` | 26 | 26 |
| `Assets/ScriptableObjects/Tractors/*.asset` | 4 | 4 |
| `Assets/ScriptableObjects/Tractors/Attachments/*.asset` | 6 | 6 |
| `Assets/ScriptableObjects/Skills/*.asset` | 6 | 6 |
| `Assets/ScriptableObjects/Recipes/*.asset` | 2 | 2 |
| `Assets/Scenes/Boot.unity`, `MainMenu.unity`, `Farm.unity` | 3 | 3 |

## EditMode Tests — RESULT: 27/27 PASSED

```text
result=Passed total=27 passed=27 failed=0 skipped=0
```

Results XML (Phase 2): `C:\Users\yuppl\AppData\Local\Temp\opencode\editmode-phase2final.xml`  
Log (Phase 2): `C:\Users\yuppl\AppData\Local\Temp\opencode\unity-editmode-phase2final.log`

Results XML: `C:\Users\yuppl\AppData\Local\Temp\opencode\editmode-results2.xml`  
Log: `C:\Users\yuppl\AppData\Local\Temp\opencode\unity-editmode2.log`

Passing tests:

- `CropSystemTests.GrowthStages_HasFiveExpectedStages`
- `CropSystemTests.IsInSeason_AcceptsValidAndRejectsInvalid`
- `CropSystemTests.Advance_RequiresWaterAndReachesReady`
- `CropSystemTests.QualityFor_UsesBonusAndWeeds`
- `CropSystemTests.YieldFor_UsesCropDefinitionAndQuality`
- `FarmGridTests.GetShape_ReturnsCanonicalShapes`
- `FarmGridTests.CreateFarm_StageZeroHasFiveWildPlots`
- `FarmGridTests.ManualLoop_TillWaterPlantGrowHarvest`
- `FarmGridTests.Plant_RejectsUnknownCropAndWrongState`
- `FarmGridTests.Expand_AcceptsValidStagesAndClampsInvalid`
- `EconomyTests.Price_ScalesWithQuality`
- `EconomyTests.Sell_RemovesInventoryAndAddsCoins`
- `EconomyTests.BuySeed_SpendsCoinsAndGrantsSeed`
- `EconomyTests.BuyPlot_AdvancesExpansion`
- `EconomyTests.Purchases_NeverAllowNegativeCoins`
- `EconomyTests.Craft_ConsumesInputsAndProducesOutput`
- `ProgressionTests.LevelForXp_UsesSixThresholds`
- `ProgressionTests.AddXp_GrantsSkillPointOnLevelUp`
- `ProgressionTests.InvestSkill_ConsumesPointAndAddsSkill`
- `ProgressionTests.CheckUnlocks_RegistersCropsAndTractors`
- `Phase2Tests.Weather_DailyWaterDeltaAndWinterDormancy`
- `Phase2Tests.Weather_RainWatersAndGrowsPlantedPlot`
- `Phase2Tests.Weather_DryingSunStopsGrowth`
- `Phase2Tests.TractorUpgrades_PurchaseAppliesStatModifiers`
- `Phase2Tests.TractorUpgrades_RejectDuplicateAndUnaffordable`
- `Phase2Tests.Quests_ProgressAndClaimAwardsRewards`
- `Phase2Tests.Quests_IncompleteCannotBeClaimed`

Coverage confirmed: crop stages/season/water/quality/yield; farm shapes 5/10/20/30/50 and full manual loop; economy pricing, sell, buy seed, buy land, negative-coin prevention, crafting; progression thresholds, skill points, skill investment, unlocks.

### Defects found and fixed on first real run

First run was 17/20. Three test-setup defects were corrected and the economy land-purchase path was hardened:

- `BuyPlot_AdvancesExpansion`: test started with insufficient coins; now funds the purchase and asserts the correct remaining balance. `EconomySystem.BuyPlot` now validates the target expansion stage before spending coins.
- `Expand_AcceptsValidStagesAndClampsInvalid`: expectation corrected to match `Mathf.Clamp` semantics — `GetShape(-1)` yields 5 plots, `GetShape(99)` yields 50 plots.
- `ManualLoop_TillWaterPlantGrowHarvest`: test now tops up water after planting so four growth days can elapse.

## PlayMode Tests — RESULT: 5/5 PASSED

```text
result=Passed total=5 passed=5 failed=0 skipped=0
```

Results XML (Phase 2): `C:\Users\yuppl\AppData\Local\Temp\opencode\playmode-phase2final.xml`  
Log (Phase 2): `C:\Users\yuppl\AppData\Local\Temp\opencode\unity-playmode-phase2final.log`

Results XML: `C:\Users\yuppl\AppData\Local\Temp\opencode\playmode-results2.xml`  
Log: `C:\Users\yuppl\AppData\Local\Temp\opencode\unity-playmode2.log`

Passing tests:

- `IntegratedSliceTests.ManualLoop_TillWaterPlantGrowHarvestSell`
- `IntegratedSliceTests.Tractor_MovementAndAllSixAttachments`
- `IntegratedSliceTests.SaveLoadReset_RoundTripsState`
- `IntegratedSliceTests.UIManager_OpenCloseMenusWithoutErrors`
- `IntegratedSliceTests.Phase2_WeatherUpgradesQuestsAndSaveRoundTrip`

Coverage confirmed: manual till → water → plant → grow → harvest → sell; tractor movement and fuel use; all six attachments with cargo → inventory transfer; save/load/reset; UI menu open/close and message rendering.

### Defect found and fixed on first real run

First run was 3/4. `ManualLoop_TillWaterPlantGrowHarvestSell` indexed specific quality keys (`bucket[Quality.Normal]`) that need not exist; changed to sum the inventory bucket safely via `InventoryCount`.

## Console Verification — RESULT: 0 ERRORS

Scanned `unity-import2.log`, `unity-generate3.log`, `unity-editmode2.log`, `unity-playmode2.log`, and `unity-build.log` for `error CS####`, `Exception:`, `NullReferenceException`, `ArgumentException`, `InvalidOperationException`, `MissingReferenceException`, `UnassignedReferenceException`, and `Script Compilation Error`. Result: **0 error lines**.

## Build Smoke Test — RESULT: PASS

See `BUILD_SMOKE_REPORT.md`. Windows x64 build succeeded with 0 errors; built executable booted and stayed alive with 0 runtime errors in `Player.log`.

## Evidence Required

| Evidence | Status |
| --- | --- |
| Test Runner pass/fail counts and test names | Done — see above |
| Unity Console output with no errors | Done — 0 error lines |
| Windows build log and executable path | Done — `BUILD_SMOKE_REPORT.md` |
| Editor/player screenshot | QA follow-up |
| Linux/macOS build smoke | QA follow-up |

## Rejected Evidence

An untrusted QA agent output claimed all three platform builds succeeded without any logs or artifacts. It was rejected and recorded in `MILESTONE_1_STATUS.md`. Only the Unity-generated results above count as acceptance evidence.
