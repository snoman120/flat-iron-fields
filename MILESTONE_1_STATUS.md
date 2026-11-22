# Milestone 1 Status — 2026-11-21 (Unity Rebaseline)

**Owner:** Director Agent  
**Directive:** `DIRECTIVE.md` (binding)  
**Contract:** `ARCHITECTURE.md`  
**Model:** `llama/qwen2.5-7b` (local Jetson only; no cloud substitution)  
**Target:** Unity 6 LTS + 2D URP, C#, Windows/Linux/macOS desktop  
**Status:** Reopened for Unity implementation

> **Director authority:** The Director Agent owns the go/no-go call. Task statuses flip to **Done** only after Unity acceptance evidence is recorded here. The previous HTML5 Canvas GO decision is historical and does not apply to the Unity target.

## Current Gate

**Decision: NO-GO — Unity Milestone 1 is reopened and not yet implemented.**

All 8 tasks are **Not Started** under Unity acceptance criteria. No Unity project, scenes, C# scripts, ScriptableObjects, EditMode tests, or PlayMode tests have been verified yet.

This rebaseline is a documentation-only change. The HTML5 source and browser tests remain in the repository as a **reference prototype** but are excluded from the Unity gate.

## Historical Evidence (Superseded)

The browser prototype previously passed:

- `tests/smoke.html` + `tests/smoke.js`: 18/18, zero console errors
- Integrated browser gameplay script: 17/17, zero console errors
- Director historical decision: GO for the HTML5 vertical slice

That evidence is retained only as design proof. It must not be used to mark any Unity task Done.

## Task Board

| # | Task | Owner | Status | Unity proof required |
| --- | --- | --- | --- | --- |
| 1 | Basic Crop System | Core Systems | Not Started | 26 CropDefinition assets, growth stages, seasonality, yield/quality math, EditMode tests |
| 2 | Core Farming Mechanics | Core Systems | Not Started | Tilemap farm, plot state machine, tools, water/day growth, PlayMode manual loop test |
| 3 | Farm Layout and Expansion | Core Systems | Not Started | 5/10/20/30/50 plot stages, tile footprints 10x20→50x20, purchase path, EditMode shape tests |
| 4 | Tractor System Basics | Systems | Not Started | TractorController with Input System, 4 types, speed/capacity/fuel, PlayMode movement test |
| 5 | Basic UI and Menus | Client | Not Started | Boot/MainMenu/Farm scenes, HUD, Seeds/Sell/Land/Tractor/Skills panels, help overlay |
| 6 | Economy System Basics | Systems | Not Started | Coins, quality price, sell/buy/craft, inventory updates, EditMode economy tests |
| 7 | Basic Farm Progression | Systems | Not Started | XP, 6 levels, 6 skills, unlocks, EditMode progression tests |
| 8 | Initial Tractor Attachments | Systems | Not Started | 6 attachment implementations, T cycle, fuel/cargo effects, PlayMode attachment test |

## Acceptance Checklist

All boxes must be checked before a Unity **GO**.

### Binding constraints

- [ ] All work generated on local `llama/qwen2.5-7b`; no cloud model substitution
- [ ] Unity 6 LTS + 2D URP project opens with no compile or missing-package errors
- [ ] C# source only for Unity implementation; no browser APIs or `window.FIF`
- [ ] Target platforms Windows/Linux/macOS configured in Build Settings
- [ ] Modules isolated per `ARCHITECTURE.md`; no cross-owner edits
- [ ] Game loop runs at stable 60 FPS target
- [ ] No Unity Console errors during load, menu use, farming, driving, buying/selling, or day advance

### Task 1 — Crop system

- [ ] 26 crop ScriptableObjects with all required fields
- [ ] Growth stages Seed → Sprout → Grow → Bloom → Harvest
- [ ] Seasonality, multi-harvest, yield range, and Poor/Normal/Good/Perfect quality
- [ ] Crop selection works in Unity UI

### Task 2 — Core farming mechanics

- [ ] Regular, raised, and fenced plot types represented
- [ ] Plot states Wild/Tilled/Cultivated/Watered/Planted/Ready/Harvested functional and visible
- [ ] Watering, weeding, pests, fertilizer, seasons, and day advance affect growth
- [ ] Out-of-season planting rejected

### Task 3 — Farm layout and expansion

- [ ] Starting farm = 5 plots, 10x20 tile footprint
- [ ] Expansion stages = 10, 20, 30, 50 plots with correct footprints
- [ ] Buying land visibly adds plots in Unity

### Task 4 — Tractor basics

- [ ] Basic, Crop, Heavy Duty, and Specialized tractor definitions
- [ ] WASD/arrow driving, reverse, and speed differences work
- [ ] Fuel, cargo, position, and attachment shown in HUD
- [ ] PlayMode movement test passes

### Task 5 — UI and menus

- [ ] Boot, Main Menu, and Farm scenes exist and load in order
- [ ] HUD, crop selection, inventory, tractor management, and purchase panels work
- [ ] Crop status indicators and seasonal color indicators visible
- [ ] Controls readable and player can return to play from menus

### Task 6 — Economy basics

- [ ] Farming coins work across selling and purchases
- [ ] Quality-adjusted selling, seed/land/tractor purchases, and crafting work
- [ ] Income/expenses update immediately in state and UI
- [ ] Negative currency prevented

### Task 7 — Player progression

- [ ] XP awarded by farming activities and sales
- [ ] Levels Noob → Master work
- [ ] Skill points investable across all six skills
- [ ] Unlocks trigger and are visible

### Task 8 — Core tractor attachments

- [ ] Tillage, Planter, Harvester, and Transporter work in play
- [ ] Watering and Fertilizer represented and functional
- [ ] Attachment switching and fuel/cargo updates work
- [ ] PlayMode attachment test passes

### Integrated playable slice

- [ ] One Unity session: start run → till → water → plant → advance day → harvest → sell → buy seeds/land → XP/level → skill invest → switch tools/crops → drive tractor → use ≥4 attachments
- [ ] Save/load/reset preserve expected state without console errors
- [ ] At least one desktop build smoke test passes
- [ ] Evidence recorded here with scene/script/test paths, console output, and Director verification

## Integration Requests

| ID | Request | Owner | Needed for | Status |
| --- | --- | --- | --- | --- |
| IR-1 | Create Unity 6 LTS + 2D URP project skeleton and package manifest. | Coding Agent | All tasks | Open |
| IR-2 | Confirm exact Unity 6 LTS minor version installed on the build machine. | QA / Director | Build reproducibility | Open |
| IR-3 | Create reference screenshot set from the HTML5 prototype for visual parity. | Client / QA | Art/UI direction | Open |
| IR-4 | Define Git ignore rules for Unity generated folders. | Coding Agent / Style | Source control hygiene | Open |

## Blockers

**Active gate blocker:** Unity implementation has not started. All 8 tasks are Not Started and no Unity acceptance evidence exists.

| Blocker | Owner | Impact | Escalation | Status |
| --- | --- | --- | --- | --- |
| B-1: No Unity project skeleton exists yet. | Coding Agent | No scenes, scripts, tests, or builds can be verified. | Create project per `ARCHITECTURE.md`; Director re-gates. | Open — Major |
| B-2: Unity version not pinned beyond “6 LTS”. | QA / Director | Build/test reproducibility risk. | Record exact version in this file once confirmed. | Open — Minor |
| B-3: HTML5 GO decision is superseded. | Director | Stakeholders may mistake browser evidence for Unity completion. | Historical evidence section above is authoritative for rebaseline. | Mitigated |

## Go / No-Go

**Current decision:** **NO-GO**.

GO decision rule for the Unity milestone:

1. All 8 Unity task rows marked **Done** with Unity evidence.
2. Every Acceptance Checklist box checked.
3. Integrated Unity playable slice passes in one session with zero console errors.
4. No Critical or Major blockers open; Minor items have owner and workaround.
5. Binding constraints pass: local model, Unity 6 LTS + 2D URP, architecture ownership, cross-platform desktop configuration.

**Director owns the final GO/NO-GO call.** The previous browser-slice GO is archived as historical evidence only.
