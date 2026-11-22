# Phase 2 Status — Tractor and Systems Depth

**Date:** 2026-11-22  
**Directive:** `DIRECTIVE.md`  
**Roadmap:** GDD §10.2 Phase 2  
**Model:** `llama/qwen2.5-7b` (local Jetson only)  
**Target:** Unity 6000.3.9f1 + 2D URP, Windows primary  
**Status:** Increment complete and verified on Windows

## Scope Delivered

This increment covers the systems-depth portion of Phase 2. Audio pass, visual seasonal palette, and full quest/crafting content expansion remain follow-ups.

| Feature | Status | Implementation |
| --- | --- | --- |
| Weather modifiers | Done | `Assets/Scripts/Farming/WeatherSystem.cs` |
| Tractor upgrades | Done | `Assets/Scripts/Tractors/TractorUpgradeDefinition.cs`, `TractorUpgradeDatabase.cs`, `TractorUpgradeSystem.cs` |
| Quests and rewards | Done | `Assets/Scripts/Progression/QuestDefinition.cs`, `QuestDatabase.cs`, `QuestSystem.cs` |
| Quest/upgrades UI | Done | `Assets/Scripts/UI/UIManager.cs`, `Assets/Scripts/Input/InputRouter.cs`, `GameController.HandleInput` |
| Save/load hardening | Done | `Assets/Scripts/Game/GameController.cs` |

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

## Generated Asset Counts

| Path | Count |
| --- | --- |
| `Assets/ScriptableObjects/Tractors/Upgrades/*.asset` | 7 |
| `Assets/ScriptableObjects/Quests/*.asset` | 4 |

Generator: `Assets/Scripts/Tractors/Editor/SystemAssetGenerator.cs` via `Flat Iron Fields > Generate All Milestone 1 Assets`.

## Verification Evidence

### Offline compile

```text
Compiling 39 files with 404 references using DotNetSdkRoslyn...
csc.dll exit code: 0
```

### Unity import/generation

```text
Unity exit code: 0
Error lines: 0
```

Log: `C:\Users\yuppl\AppData\Local\Temp\opencode\unity-phase2-generate.log`

### Tests

EditMode:

```text
result=Passed total=27 passed=27 failed=0 skipped=0
```

PlayMode:

```text
result=Passed total=5 passed=5 failed=0 skipped=0
```

New tests:

- `Phase2Tests.Weather_DailyWaterDeltaAndWinterDormancy`
- `Phase2Tests.Weather_RainWatersAndGrowsPlantedPlot`
- `Phase2Tests.Weather_DryingSunStopsGrowth`
- `Phase2Tests.TractorUpgrades_PurchaseAppliesStatModifiers`
- `Phase2Tests.TractorUpgrades_RejectDuplicateAndUnaffordable`
- `Phase2Tests.Quests_ProgressAndClaimAwardsRewards`
- `Phase2Tests.Quests_IncompleteCannotBeClaimed`
- `IntegratedSliceTests.Phase2_WeatherUpgradesQuestsAndSaveRoundTrip`

Results XML: `editmode-phase2final.xml`, `playmode-phase2final.xml` in `C:\Users\yuppl\AppData\Local\Temp\opencode`.

### Console

`unity-phase2-generate.log`, `unity-editmode-phase2final.log`, `unity-playmode-phase2final.log`, and `unity-build-phase2.log` contain 0 error patterns (`error CS####`, `NullReferenceException`, `ArgumentException`, `InvalidOperationException`, `MissingReferenceException`, `UnassignedReferenceException`, `Script Compilation Error`).

### Build and startup smoke

```text
Flat Iron Fields Windows build: Succeeded, size=95602896 bytes, time=00:00:09.0914420, errors=0, warnings=3
```

Rebuilt executable stayed alive for 10 seconds; `Player.log` contained 0 error lines.

## Defects Found and Fixed

First Phase 2 run was 25/27 EditMode. Both were test expectation errors, not gameplay defects:

- `Weather_RainWatersAndGrowsPlantedPlot`: rain adds 2 water but growth consumes 1, so final water is 1 — expectation corrected.
- `TractorUpgrades_PurchaseAppliesStatModifiers`: Cargo Rack I requires level 2 — test now grants XP before purchase.

Compile integration errors were also fixed: missing `using` directives for `CropSystem`, `EconomySystem`, and `ProgressionSystem`.

## Follow-ups

- Audio pass: AudioMixer, seasonal music, engine loop, SFX (`GDD §6.2`) — issue #16.
- Seasonal palette pass using 2D URP color grading and tinted Tilemaps — issue #17.
- Quest content expansion and quest-claim UI buttons.
- Linux/macOS builds — tracked in issue #13.
- Screenshot evidence and manual GUI walkthrough — tracked in issue #14.

## Final Call

**Verified on Windows.** Phase 2 systems-depth increment passes Unity import, generation, 27/27 EditMode, 5/5 PlayMode, 0 Console errors, Windows build, and startup smoke.
