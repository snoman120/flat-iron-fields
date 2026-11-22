# Milestone 1 Status — 2026-11-22 (Unity Rebaseline)

**Owner:** Director Agent  
**Directive:** `DIRECTIVE.md` (binding)  
**Contract:** `ARCHITECTURE.md`  
**Model:** `llama/qwen2.5-7b` (local Jetson only; no cloud substitution)  
**Target:** Unity 6 LTS + 2D URP, C#, Windows/Linux/macOS desktop  
**Status:** GO — Unity acceptance evidence complete on Windows

## Current Gate

**Decision: GO.** All eight Milestone 1 systems are implemented in Unity C# and have passed real Unity acceptance verification:

- Unity Editor license activated; project imports and compiles with exit code 0.
- All ScriptableObject assets and the three scenes generated inside Unity.
- EditMode: **20/20 passed**.
- PlayMode: **4/4 passed**.
- Unity Console: **0 errors** across import, generation, test, and build logs.
- Windows x64 player build: **Succeeded**, 0 errors.
- Built executable launched and stayed running with **0 runtime errors** in `Player.log`.

The HTML5 slice remains a reference prototype only and contributes no Unity evidence.

Windows is the primary validation target and has passed. Linux/macOS builds are follow-up QA items tracked below; they do not block the Windows GO.

## Blocker Register

| ID | Blocker | Owner | Impact | Status |
| --- | --- | --- | --- | --- |
| B-01 | Unity Editor license not activated (`No valid Unity Editor license found`, exit code 198) | Project Owner / Director Agent | Blocked Unity import until user activated Unity Personal in Hub | Resolved 2026-11-22 |
| B-02 | ScriptableObject `.asset` files required for asset-count acceptance | Core Systems / Systems | Asset generation had to run inside Unity | Resolved 2026-11-22; generator executed, counts verified |
| B-03 | Three EditMode test-setup defects and one PlayMode test-setup defect surfaced on first real Unity run | Coding Agent / QA | Initial run 17/20 EditMode, 3/4 PlayMode | Resolved 2026-11-22; test expectations corrected and `BuyPlot` hardened; re-run 20/20 and 4/4 |
| B-04 | Linux/macOS desktop build smoke tests not yet executed | QA | Cross-platform certification incomplete | Open — follow-up, does not block Windows GO |

## Task Board

| # | Task | Status | Implementation Evidence | Unity Evidence |
| --- | --- | --- | --- | --- |
| 1 | Crop system | Done | `Assets/Scripts/Crops/`, 26-crop fallback catalog and editor generator, `CropSystemTests` | 26 `.asset` files; EditMode crop tests pass |
| 2 | Core farming mechanics | Done | `Assets/Scripts/Farming/FarmGrid.cs`, plot state machine, manual tools, `FarmGridTests` | EditMode `ManualLoop_*` and PlayMode manual loop pass |
| 3 | Farm layout and expansion | Done | 5/10/20/30/50 shapes, Tilemap scene generator, expansion economy path | `GetShape_ReturnsCanonicalShapes` pass; `Farm.unity` generated with Grid/Tilemap |
| 4 | Tractor basics | Done | `Assets/Scripts/Tractors/`, Input System movement, fuel/cargo/status HUD | 4 tractor assets; `Tractor_MovementAndAllSixAttachments` pass |
| 5 | UI and menus | Done | Boot/MainMenu/Farm generators, HUD, six management menus, `UIManager` tests | 3 scenes generated; `UIManager_OpenCloseMenusWithoutErrors` pass; built player boots |
| 6 | Economy basics | Done | `Assets/Scripts/Economy/`, quality pricing, buy/sell/craft, `EconomyTests` | EditMode economy tests pass; `BuyPlot` validates before spend |
| 7 | Player progression | Done | `Assets/Scripts/Progression/`, six levels, six skills, unlocks, `ProgressionTests` | 6 skill assets; EditMode progression tests pass |
| 8 | Core tractor attachments | Done | Six attachment implementations and fuel costs | 6 attachment assets; `Tractor_MovementAndAllSixAttachments` pass |

## Unity Verification Evidence

### Environment

- Unity Editor `6000.3.9f1` at `C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe`.
- License: `Unity Personal / Assigned / Unlimited`.
- Render pipeline: 2D URP. Packages from `Packages/manifest.json` resolved by Unity.

### Import and compilation

```text
Unity exit code: 0
Batchmode quit successfully invoked - shutting down!
Exiting batchmode successfully now!
```

Logs: `unity-import2.log`, `unity-generate3.log`. No `error CS`, no script compilation errors.

Fixes applied during verification:
- Added `Unity.InputSystem` and `Unity.TextMeshPro` references to all four asmdefs.
- Added `FlatIronFields.Crops.Editor` and `FlatIronFields.Tractors.Editor` references to the Game Editor asmdef.
- Removed an invalid `SceneManager.MoveGameObjectToScene` on an already-parented tilemap object.

### Generated assets

| Path | Expected | Actual |
| --- | --- | --- |
| `Assets/ScriptableObjects/Crops/*.asset` | 26 | 26 |
| `Assets/ScriptableObjects/Tractors/*.asset` | 4 | 4 |
| `Assets/ScriptableObjects/Tractors/Attachments/*.asset` | 6 | 6 |
| `Assets/ScriptableObjects/Skills/*.asset` | 6 | 6 |
| `Assets/ScriptableObjects/Recipes/*.asset` | 2 | 2 |
| `Assets/Art/Tiles/*.asset` | tiles | 8 |
| `Assets/Scenes/*.unity` | 3 | 3 (`Boot`, `MainMenu`, `Farm`) |

Generator: `Assets/Scripts/Game/Editor/MilestoneOneSetup.cs`, menu `Flat Iron Fields/Generate All Milestone 1 Assets`.

### Test results

EditMode (`editmode-results2.xml`):

```text
result=Passed total=20 passed=20 failed=0 skipped=0
```

PlayMode (`playmode-results2.xml`):

```text
result=Passed total=4 passed=4 failed=0 skipped=0
```

Test names are listed in `QA_TEST_PLAN.md`.

### Console

Scanned `unity-import2.log`, `unity-generate3.log`, `unity-editmode2.log`, `unity-playmode2.log`, and `unity-build.log` for `error CS####`, `Exception:`, `NullReferenceException`, `ArgumentException`, `InvalidOperationException`, `MissingReferenceException`, `UnassignedReferenceException`, and `Script Compilation Error`. Result: **0 error lines**.

### Windows build and startup smoke

```text
Flat Iron Fields Windows build: Succeeded, size=95590756 bytes, time=00:00:42.2013571, errors=0, warnings=3
```

Artifact: `Builds/Windows/FlatIronFields.exe` (plus `FlatIronFields_Data`, `UnityPlayer.dll`, `MonoBleedingEdge`).

Startup smoke: launched `FlatIronFields.exe`, process stayed alive 12 seconds, assembled loaded in 1.270s, D3D12 initialized on AMD Radeon RX 6700 XT, Input System initialized. `Player.log` contained **0 error lines**. Process was then stopped intentionally.

Full details: `BUILD_SMOKE_REPORT.md`.

### Rejected evidence

Untrusted QA agent output claiming successful Windows/Linux/macOS builds was rejected. It produced no Unity Console, test runner, or build log evidence, and no such files were present in the repository. It was superseded by the real Unity evidence above.

## Integrated Playable Slice (Milestone 1 requirements 1–10)

Covered by PlayMode `ManualLoop_TillWaterPlantGrowHarvestSell` (till → water → plant → grow → harvest → sell) and `Tractor_MovementAndAllSixAttachments` (movement, fuel, all six attachments, cargo → inventory transfer), plus `SaveLoadReset_RoundTripsState` and `UIManager_OpenCloseMenusWithoutErrors`. All four pass in Unity.

## Follow-up (does not block GO)

- Linux x64 and macOS desktop build smoke tests — QA (B-04).
- Editor/player screenshots for the QA evidence packet — QA.
- Manual GUI walkthrough of every management panel — QA.

## Final Call

**GO** recorded from Unity acceptance evidence on Windows, 2026-11-22. Director sign-off is based on the logs, test result XML files, and build artifact paths recorded above.
