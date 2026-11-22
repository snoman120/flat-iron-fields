# GitHub Issue Backlog — Unity Milestone 1

Repository: https://github.com/snoman120/flat-iron-fields

These issues are ready to create once GitHub authentication is available. Labels referenced below should be created in the repository if they do not exist.

## Labels to create

- `Feature Request`
- `Bug`
- `Improvement`
- `Design Compliance`
- `Security`
- `Licensing`
- `Unity Migration`
- `Phase 1 - Core Farming`
- `Priority: Critical`
- `Priority: Major`
- `Priority: Minor`

---

## Issue 1 — [Unity M1] Implement basic crop system

**Labels:** `Feature Request`, `Phase 1 - Core Farming`, `Priority: Major`

**Body:**

Create the Unity crop data model and growth logic for all 26 canonical crops.

### Scope
- Create `CropDefinition` ScriptableObject in `Assets/Scripts/Crops/`
- Create 26 crop assets in `Assets/ScriptableObjects/Crops/`
- Implement growth stages: Seed → Sprout → Grow → Bloom → Harvest
- Implement seasonality, multi-harvest regrowth, yield range, and quality math
- Integrate crop selection with UI

### Acceptance
- 26 crop assets exist with all required fields
- Out-of-season planting is rejected
- Quality affects sale price
- EditMode tests cover growth, seasonality, yield, and quality

### References
- `GDD.md` §2.1
- `MILESTONE_1_TASKS.md` Task 1
- `ARCHITECTURE.md` → `FlatIronFields.Crops.CropSystem`

---

## Issue 2 — [Unity M1] Create core farming mechanics

**Labels:** `Feature Request`, `Phase 1 - Core Farming`, `Priority: Major`

**Body:**

Implement plot states, tools, day/time progression, and growth modifiers in Unity.

### Scope
- Tilemap farm grid in `Assets/Scenes/Farm.unity`
- Plot states: Wild, Tilled, Cultivated, Watered, Planted, Ready, Harvested
- Plot types: regular, raised, fenced
- Manual tools: shovel, watering can, rake, hoe, scythe, cart
- Water/day/season/weather/weed/fertilizer growth inputs

### Acceptance
- Player can till, water, plant, grow, and harvest in Play Mode
- Crop stage and plot state are visible in UI
- PlayMode test completes a full manual farming loop

### References
- `GDD.md` §2.2–2.3
- `MILESTONE_1_TASKS.md` Task 2
- `ARCHITECTURE.md` → `FlatIronFields.Farming.FarmGrid`

---

## Issue 3 — [Unity M1] Build farm layout and expansion

**Labels:** `Feature Request`, `Phase 1 - Core Farming`, `Priority: Minor`

**Body:**

Implement the Tilemap farm layout and expansion stages.

### Scope
- Starting farm: 5 plots, 10x20 tile footprint
- Stages: 10, 20, 30, 50 plots with correct tile footprints
- Plot shape metadata and purchase path through economy/UI

### Acceptance
- `GetShape(stage)` returns correct plot count and footprint for all stages
- Buying land visibly adds plots in Unity
- EditMode tests validate all shapes

### References
- `GDD.md` §3.2
- `MILESTONE_1_TASKS.md` Task 3

---

## Issue 4 — [Unity M1] Implement tractor system basics

**Labels:** `Feature Request`, `Phase 1 - Core Farming`, `Priority: Major`

**Body:**

Implement the Unity tractor controller and tractor definitions.

### Scope
- `TractorController` using Input System
- Basic, Crop, Heavy Duty, Specialized definitions
- WASD/arrow driving, reverse, speed differences
- Fuel, cargo, position, attachment status
- HUD status display

### Acceptance
- Tractor drives in Play Mode
- Different types have distinct speed/capacity
- Fuel decreases while driving
- PlayMode movement test passes

### References
- `GDD.md` §5
- `MILESTONE_1_TASKS.md` Task 4
- `ARCHITECTURE.md` → `FlatIronFields.Tractors.TractorController`

---

## Issue 5 — [Unity M1] Create basic UI and menus

**Labels:** `Feature Request`, `Phase 1 - Core Farming`, `Priority: Minor`

**Body:**

Build uGUI + TextMeshPro HUD and management menus.

### Scope
- Boot, Main Menu, Farm scenes
- HUD: day, season, weather, coins, XP, level, skill points
- Seeds, Sell, Land, Tractor, Skills panels
- Crop status indicators, tractor status, help overlay, seasonal indicators

### Acceptance
- Player can start from Main Menu and return to play
- All panels open/close without console errors
- Status indicators update live

### References
- `GDD.md` §6.1
- `MILESTONE_1_TASKS.md` Task 5

---

## Issue 6 — [Unity M1] Implement economy basics

**Labels:** `Feature Request`, `Phase 1 - Core Farming`, `Priority: Minor`

**Body:**

Implement farming coins, selling, purchases, crafting, and tracking.

### Scope
- `EconomySystem` with `Price`, `Sell`, `BuySeed`, `BuyPlot`, `BuyTractor`, `Craft`
- Quality-adjusted pricing
- Nested inventory: `cropId -> quality -> count`
- Income/expense stats

### Acceptance
- Selling increases coins and removes inventory
- Purchases decrease coins and grant items
- Negative coins are prevented
- EditMode tests cover all economy paths

### References
- `GDD.md` §3.3
- `MILESTONE_1_TASKS.md` Task 6

---

## Issue 7 — [Unity M1] Create basic farm progression

**Labels:** `Feature Request`, `Phase 1 - Core Farming`, `Priority: Minor`

**Body:**

Implement XP, levels, skill points, skills, and unlocks.

### Scope
- Six levels: Noob → Master
- Six skills: planting, harvesting, cropSelection, resourceManagement, tractorOperation, farmExpansion
- XP awards from farming, sales, expansion, tractor work
- Unlock checks for crops, tractors, decorations

### Acceptance
- XP thresholds work
- Level-up grants a skill point
- Skill investment persists
- Unlocks trigger and are visible

### References
- `GDD.md` §4
- `MILESTONE_1_TASKS.md` Task 7

---

## Issue 8 — [Unity M1] Implement core tractor attachments

**Labels:** `Feature Request`, `Phase 1 - Core Farming`, `Priority: Minor`

**Body:**

Implement the six tractor attachments and plot application effects.

### Scope
- Tillage, Planter, Harvester, Transporter, Watering, Fertilizer
- Attachment switching UI and `T` key cycle
- Fuel cost per application
- Cargo → inventory transport path

### Acceptance
- Four core attachments work in Play Mode
- Watering and Fertilizer are functional
- Cargo is populated by Harvester and emptied by Transporter
- PlayMode attachment test passes

### References
- `GDD.md` §5.2
- `MILESTONE_1_TASKS.md` Task 8

---

## Issue 9 — Create Unity 6 LTS + 2D URP project skeleton

**Labels:** `Unity Migration`, `Priority: Major`

**Body:**

Create the production Unity project skeleton per `ARCHITECTURE.md`.

### Scope
- Unity 6 LTS project with 2D URP template
- `Assets/`, `ProjectSettings/`, `Packages/`
- Scenes: Boot, MainMenu, Farm
- Package manifest with Input System, 2D URP, TextMeshPro, Test Framework
- Namespace/folder structure from `ARCHITECTURE.md`

### Acceptance
- Project opens with no compile or missing-package errors
- Boot → MainMenu → Farm load chain verified
- Directory ownership matches architecture contract

---

## Issue 10 — Pin exact Unity 6 LTS version and build targets

**Labels:** `Improvement`, `Unity Migration`, `Priority: Minor`

**Body:**

Record the exact Unity 6 LTS minor version and configure desktop build targets.

### Acceptance
- Exact version recorded in `MILESTONE_1_STATUS.md`
- Build Settings include Windows, Linux, macOS desktop
- QA can reproduce builds from the documented version

---

## Issue 11 — Add Unity .gitignore hygiene

**Labels:** `Improvement`, `Unity Migration`, `Priority: Minor`

**Body:**

Add and maintain ignore rules for Unity generated folders.

### Status
Completed locally in `.gitignore` on commit `c11f290`; verify after the Unity project is generated.

### Acceptance
- `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, build outputs ignored
- `Assets/`, `ProjectSettings/`, `Packages/` tracked

---

## Issue 12 — Reference prototype visual parity screenshots

**Labels:** `Improvement`, `Phase 1 - Core Farming`, `Priority: Minor`

**Body:**

Capture screenshots of the HTML5 reference prototype to guide Unity UI/art parity.

### Acceptance
- Farm view, HUD, Seeds, Sell, Land, Tractor, Skills panels captured
- Screenshots stored in documentation or QA evidence folder
- Unity UI reviewed against captured reference
