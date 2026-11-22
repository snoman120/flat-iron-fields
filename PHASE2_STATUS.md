# Phase 2 Status — Systems Depth and Polish

**Date:** 2026-11-22  
**Directive:** `DIRECTIVE.md`  
**Roadmap:** GDD §10.2 Phase 2  
**Model:** `llama/qwen2.5-7b` (local Jetson only)  
**Target:** Unity 6000.3.9f1 + 2D URP, Windows primary  
**Status:** Increment complete and verified on Windows

## Scope Delivered

This increment covers Phase 2 systems depth plus the procedural audio and seasonal visual palette passes. Cross-platform build QA, screenshot evidence, and quest-claim UI buttons remain follow-ups.

| Feature | Status | Implementation |
| --- | --- | --- |
| Weather modifiers | Done | `Assets/Scripts/Farming/WeatherSystem.cs` |
| Tractor upgrades | Done | `Assets/Scripts/Tractors/TractorUpgradeDefinition.cs`, `TractorUpgradeDatabase.cs`, `TractorUpgradeSystem.cs` |
| Quests and rewards | Done | `Assets/Scripts/Progression/QuestDefinition.cs`, `QuestDatabase.cs`, `QuestSystem.cs` |
| Quest/upgrades UI | Done | `Assets/Scripts/UI/UIManager.cs`, `Assets/Scripts/Input/InputRouter.cs`, `GameController.HandleInput` |
| Save/load hardening | Done | `Assets/Scripts/Game/GameController.cs` |
| Procedural audio | Done | `Assets/Scripts/Game/AudioService.cs`, `AudioCue.cs`, `Assets/Audio/FarmAudio.mixer` |
| Seasonal palette | Done | `Assets/Scripts/Game/SeasonalPalette.cs`, `SeasonalPaletteSystem.cs`, `Assets/Audio/SeasonalVolumeProfile.asset` |

## Feature Details

### Weather modifiers

- Rainy: +2 water per day to planted plots.
- Cloudy: no water change.
- Sunny: -1 water per day.
- Winter: deterministic dormancy — crops grow only on even days.
- Out-of-season crops never advance.
- Deterministic and keyed only to explicit day advance (`GameController.NewDay`), never frame rate.

### Tractor upgrades

Seven upgrade assets generated under `Assets/ScriptableObjects/Tractors/Upgrades/`:

- Speed Tune I/II (+20% / +35% speed)
- Fuel Economy I/II (-20% / -35% fuel burn)
- Cargo Rack I/II (+8 / +16 capacity)
- Auto Steer (automation level 1)

Purchases validate level unlocks and affordability through `EconomySystem.TrySpend`. Modifiers flow into `TractorController` movement, fuel timer, and cargo capacity.

### Quests

Four quest assets generated under `Assets/ScriptableObjects/Quests/`:

- First Harvest — harvest 3 crops, reward 40 coins / 20 XP
- Market Day — sell 5 crops, reward 60 coins / 30 XP
- Homestead — expand once, reward 100 coins / 40 XP
- Mechanised — 6 attachment actions, reward 120 coins / 50 XP

Progress is recorded from harvest, sell, land purchase, and tractor attachment events. Claiming pays coins via `EconomySystem.AddReward` and XP via `ProgressionSystem.AddXp`.

### Save/load hardening

Previously lost state is now persisted and restored:

- `FarmState.ExpansionStage`
- `PlayerState.UnlockedIds`
- `PlayerState.TractorUpgrades`
- `PlayerState.QuestProgress` and `PlayerState.ClaimedQuests`
- `TractorState.Cargo` and `CargoCount`

### Procedural audio

- All clips are generated in memory at runtime; the build ships no external audio assets.
- Loops: farm music, ambient wind/birds, tractor engine, and rain.
- Engine volume/pitch track tractor movement; rain loop activates only in rainy weather.
- Twelve SFX cues cover till, plant, water, harvest, coin, build, upgrade, reward, day, attachment, rain, and sun.
- Audio routes through the generated `FarmAudio.mixer` Music/Sfx groups, with exposed `MusicVolume` and `SfxVolume` parameters.
- `GameController` fires cues for tools, day advance, weather, sales, land purchase, upgrades, attachments, and quest claims.

### Seasonal palette

- Four distinct schemes for Spring, Summer, Autumn, and Winter.
- Applies camera background color, Tilemap tint, HUD season text tint, and URP `ColorAdjustments` (post exposure, contrast, saturation, color filter).
- A global `Volume` with `SeasonalVolumeProfile.asset` is wired into the Farm and MainMenu scenes.
- Palette changes are driven by explicit start/day/load/reset transitions, not frame rate.

## Generated Asset Counts

| Path | Count |
| --- | --- |
| `Assets/ScriptableObjects/Tractors/Upgrades/*.asset` | 7 |
| `Assets/ScriptableObjects/Quests/*.asset` | 4 |
| `Assets/Audio/FarmAudio.mixer` | 1 |
| `Assets/Audio/SeasonalVolumeProfile.asset` | 1 |

Audio clips themselves are generated at runtime by `AudioService`; no clip files are required.

Generators: `Assets/Scripts/Crops/Editor/CropAssetGenerator.cs`, `Assets/Scripts/Tractors/Editor/SystemAssetGenerator.cs`, `Assets/Scripts/Game/Editor/PhaseTwoAssetGenerator.cs`, and `Assets/Scripts/Game/Editor/SceneGenerator.cs`, all invoked by `Flat Iron Fields > Generate All Milestone 1 Assets`.

## Verification Evidence

### Offline compile

```text
Compiling 44 files with 404 references using DotNetSdkRoslyn...
csc.dll exit code: 0
```

### Unity import/generation

```text
Unity exit code: 0
Error lines: 0
```

Log: `C:\Users\yuppl\AppData\Local\Temp\opencode\unity-phase2-polish-generate-final2.log`

Generated output:

```text
Flat Iron Fields: generated 26 crop assets.
Flat Iron Fields: generated tractor, attachment, upgrade, skill, quest, and recipe assets.
Flat Iron Fields: generated audio mixer and seasonal volume profile.
Flat Iron Fields: generated Boot, MainMenu, and Farm scenes.
```

Generators: `CropAssetGenerator`, `SystemAssetGenerator`, `PhaseTwoAssetGenerator`, and `SceneGenerator`, all invoked by `Flat Iron Fields > Generate All Milestone 1 Assets`.

### Tests

EditMode:

```text
result=Passed total=29 passed=29 failed=0 skipped=0
```

PlayMode:

```text
result=Passed total=6 passed=6 failed=0 skipped=0
```

New tests:

- `Phase2Tests.Weather_DailyWaterDeltaAndWinterDormancy`
- `Phase2Tests.Weather_RainWatersAndGrowsPlantedPlot`
- `Phase2Tests.Weather_DryingSunStopsGrowth`
- `Phase2Tests.TractorUpgrades_PurchaseAppliesStatModifiers`
- `Phase2Tests.TractorUpgrades_RejectDuplicateAndUnaffordable`
- `Phase2Tests.Quests_ProgressAndClaimAwardsRewards`
- `Phase2Tests.Quests_IncompleteCannotBeClaimed`
- `Phase2PolishTests.SeasonalPalette_AllSeasonsHaveDistinctReadableSchemes`
- `Phase2PolishTests.SeasonalPalette_UnknownSeasonFallsBackToSpring`
- `IntegratedSliceTests.Phase2_WeatherUpgradesQuestsAndSaveRoundTrip`
- `IntegratedSliceTests.AudioAndPalette_InitializeApplySeasonAndCuePlayback`

Results XML: `editmode-phase2polish-final.xml`, `playmode-phase2polish-final.xml` in `C:\Users\yuppl\AppData\Local\Temp\opencode`.

### Console

`unity-phase2-polish-generate-final2.log`, `unity-editmode-phase2polish-final.log`, `unity-playmode-phase2polish-final.log`, and `unity-build-phase2polish-final.log` contain 0 error patterns (`error CS####`, `NullReferenceException`, `ArgumentException`, `InvalidOperationException`, `MissingReferenceException`, `UnassignedReferenceException`, `Script Compilation Error`, `[Error]`).

### Build and startup smoke

```text
Flat Iron Fields Windows build: Succeeded, size=95615184 bytes, time=00:00:03.3755061, errors=0, warnings=0
```

Rebuilt executable stayed alive for 10 seconds; `Player.log` contained 0 error lines.

## Defects Found and Fixed

First Phase 2 run was 25/27 EditMode. Both were test expectation errors, not gameplay defects:

- `Weather_RainWatersAndGrowsPlantedPlot`: rain adds 2 water but growth consumes 1, so final water is 1 — expectation corrected.
- `TractorUpgrades_PurchaseAppliesStatModifiers`: Cargo Rack I requires level 2 — test now grants XP before purchase.

Compile integration errors were also fixed: missing `using` directives for `CropSystem`, `EconomySystem`, and `ProgressionSystem`.

Polish-pass defects fixed before final verification:

- `com.unity.modules.audio` was missing from `Packages/manifest.json`; runtime audio types did not resolve in Unity even though offline compile succeeded.
- `Object` was ambiguous between `UnityEngine.Object` and `System.Object` in PlayMode tests and the mixer generator; references are now fully qualified.
- `AudioMixerController` is not a `ScriptableObject`; the generator now casts the reflected instance to `UnityEngine.Object` and resolves Music/Sfx groups via `AssetDatabase.LoadAllAssetsAtPath`.
- `AudioClip.Create(..., stream: true)` cannot be followed by `SetData`; loops now use non-streamed clips.
- Regenerated ScriptableObjects were losing `m_Name`; crop and system generators now set `asset.name` from the asset id.

## Follow-ups

- Quest claim and upgrade purchase buttons in the UI panels.
- Linux/macOS builds — tracked in issue #13.
- Screenshot evidence and manual GUI walkthrough — tracked in issue #14.
- Reference prototype visual parity screenshots — tracked in issue #12.

## Final Call

**Verified on Windows.** Phase 2 systems depth and polish pass Unity import, generation, 29/29 EditMode, 6/6 PlayMode, 0 Console errors, Windows build, and startup smoke. Audio (#16) and seasonal palette (#17) are complete.
