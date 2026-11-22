# QA Test Plan — Unity Milestone 1

**Status:** Blocked by Unity Editor license (see `MILESTONE_1_STATUS.md`, B-01)  
**Target:** Unity 6000.3.9f1 + 2D URP, Windows primary

## Environment Preconditions

1. Unity Editor 6000.3.9f1 installed.
2. A valid Unity Editor license active on the machine.
3. Packages resolve from `Packages/manifest.json`:
   - `com.unity.render-pipelines.universal`
   - `com.unity.inputsystem`
   - `com.unity.textmeshpro`
   - `com.unity.test-framework`
   - `com.unity.ide.visualstudio`
4. Open project root `C:\dev\flat-iron-fields`.

## Unity Editor Setup

1. Allow initial import to finish.
2. Run `Flat Iron Fields > Generate Crop Assets`.
3. Run `Flat Iron Fields > Generate System Assets`.
4. Run `Flat Iron Fields > Generate Scenes`.
5. Confirm assets exist:
   - `Assets/ScriptableObjects/Crops/*.asset` — 26 files.
   - `Assets/ScriptableObjects/Tractors/*.asset` — 4 files.
   - `Assets/ScriptableObjects/Tractors/Attachments/*.asset` — 6 files.
   - `Assets/ScriptableObjects/Skills/*.asset` — 6 files.
   - `Assets/ScriptableObjects/Recipes/*.asset` — 2 files.
   - `Assets/Scenes/Boot.unity`, `MainMenu.unity`, `Farm.unity`.

## EditMode Tests

Run via Test Runner `EditMode` tab. Expected: 20 tests pass.

Coverage:
- crop growth stages, season validation, water requirement, quality, yield
- farm shapes 5/10/20/30/50, plot creation and full manual loop
- economy quality pricing, sell, buy seed, buy land, negative-coin prevention, crafting
- progression thresholds, level-up skill points, skill investment, unlocks

## PlayMode Tests

Run via Test Runner `PlayMode` tab. Expected: 4 tests pass.

Coverage:
- manual till → water → plant → grow → harvest → sell
- tractor movement, fuel use, and all six attachments
- save/load/reset
- UI menu open/close and message rendering

## Manual Integrated Slice

1. Open `Assets/Scenes/MainMenu.unity` and press Play.
2. Click `Start Farming`; confirm `Farm.unity` loads.
3. Use `1` Shovel, `2` Watering Can, `3` Rake/Plant on a highlighted plot.
4. Press `N` until the crop is ready; water again if needed.
5. Use `5` Scythe to harvest.
6. Open management panels with `B`, `U`, `M`, `V`, `K`; close with `Escape` not required (toggle buttons in generated UI).
7. Drive with WASD; cycle crop with Q/E and attachment with T.
8. Use attachments: tillage, planter, harvester, transporter, watering, fertilizer.
9. Save/load/reset through `GameController` API or UI wiring.
10. Confirm no Unity Console errors.

## Build Smoke Test

1. Build Settings: add Boot, MainMenu, Farm in that order.
2. Build Windows x64 development build to `Builds/Windows/FlatIronFields.exe`.
3. Launch executable; confirm Main Menu appears.
4. Start run; confirm Farm loads and HUD renders.
5. Capture build log and screenshot.

## Evidence Required

- Test Runner pass/fail counts and test names.
- Unity Console output with no errors.
- Screenshot of Main Menu and Farm scene.
- Windows build log and executable path.
