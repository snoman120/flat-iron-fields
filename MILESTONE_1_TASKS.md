# Milestone 1: Core Farming System — Unity Vertical Slice

**Target:** Unity 6 LTS + 2D URP, C#, cross-platform desktop  
**Status:** Reopened for Unity implementation  
**Prior browser slice:** Reference only; its HTML5 evidence does not satisfy Unity acceptance  
**Owner of final call:** Director Agent

---

## Goal

Deliver a playable Unity vertical slice that represents all eight Milestone 1 tasks in working form:

1. Crop system
2. Core farming mechanics
3. Farm layout and expansion
4. Tractor basics
5. UI and menus
6. Economy basics
7. Player progression
8. Core tractor attachments

---

## Task 1: Implement Basic Crop System

**Priority:** High  
**Assigned to:** Coding Agent / Core Systems  
**Status:** Not Started

**Description**

Create the Unity crop data model and growth logic for all 26 canonical crops with growth stages, seasonality, yield mechanics, multiple harvests, and quality levels.

**Deliverables**

- `Assets/ScriptableObjects/Crops/*.asset` for every crop
- Crop definition C# ScriptableObject class in `Assets/Scripts/Crops/`
- Growth stage logic: Seed → Sprout → Grow → Bloom → Harvest
- Season validation and multi-harvest regrowth rules
- Quality calculation affecting economy price
- Crop selection UI integration

**Acceptance**

- 26 crop assets exist with all required fields
- Growth stages progress under valid water/season conditions
- Out-of-season planting is rejected
- Quality affects sale price
- EditMode tests cover growth, seasonality, yield, and quality

---

## Task 2: Create Core Farming Mechanics

**Priority:** High  
**Assigned to:** Coding Agent / Core Systems  
**Status:** Not Started

**Description**

Implement plot management, plot states, day/time progression, weather/season effects, and manual farming tools in Unity.

**Deliverables**

- Tilemap-based farm grid in `Assets/Scenes/Farm.unity`
- Plot state machine: Wild, Tilled, Cultivated, Watered, Planted, Ready, Harvested
- Regular, raised, and fenced plot types
- Manual tool actions: shovel, watering can, rake, hoe, scythe, cart
- Day advance and season cycling
- Weed/pest/fertilizer modifiers

**Acceptance**

- Player can till, water, plant, grow, and harvest through Unity play mode
- Crop growth requires water and respects season
- Plot state and crop stage are visible in UI
- PlayMode test completes a full manual farming loop

---

## Task 3: Build Basic Farm Layout and Expansion

**Priority:** Medium  
**Assigned to:** Coding Agent / Core Systems  
**Status:** Not Started

**Description**

Create the starting farm and expansion stages using Unity Grid/Tilemap.

**Deliverables**

- Starting farm: 5 plots, 10x20 tile footprint
- Expansion stages: 10, 20, 30, 50 plots
- Plot shape metadata and Tilemap rendering
- Purchase path through economy and UI

**Acceptance**

- Stage 0 starts with 5 plots
- Each expansion stage produces the correct plot count and tile footprint
- Buying land visibly adds plots to the farm scene
- EditMode tests validate all plot shapes

---

## Task 4: Implement Tractor System Basics

**Priority:** High  
**Assigned to:** Coding Agent / Systems  
**Status:** Not Started

**Description**

Implement the Unity tractor controller, tractor data, movement, fuel, cargo, and status display.

**Deliverables**

- `TractorController` using the Unity Input System
- Tractor ScriptableObject definitions for Basic, Crop, Heavy Duty, Specialized
- WASD/arrow driving, reverse, speed differences
- Fuel consumption and cargo capacity
- Tractor status HUD: type, position, fuel, cargo, attachment

**Acceptance**

- Tractor drives in play mode with keyboard input
- Different tractor types have distinct speed/capacity values
- Fuel decreases while driving
- Tractor position maps to the plot under it
- PlayMode test verifies movement and status updates

---

## Task 5: Create Basic UI and Menus

**Priority:** Medium  
**Assigned to:** Coding Agent / Client  
**Status:** Not Started

**Description**

Build the Unity uGUI/TextMeshPro interface for gameplay and management.

**Deliverables**

- Boot, Main Menu, and Farm scenes
- HUD with day, season, weather, coins, XP, level, skill points
- Crop selection and seed purchase menu
- Sell, Land, Tractor, and Skills panels
- Inventory and crop status indicators
- Help/controls overlay
- Seasonal color indicators

**Acceptance**

- Player can start a run from the menu and return to play
- All management panels open and close without errors
- Crop and tractor status indicators update live
- No console errors during UI interaction

---

## Task 6: Implement Economy System Basics

**Priority:** Medium  
**Assigned to:** Coding Agent / Systems  
**Status:** Not Started

**Description**

Set up farming coins, selling, purchases, crafting, and income tracking in Unity.

**Deliverables**

- `EconomySystem` service
- Quality-adjusted crop pricing
- Buy seeds, buy land, buy tractor, craft item
- Inventory updates with nested quality buckets
- Income/expense tracking in player stats

**Acceptance**

- Selling a crop increases coins and removes inventory
- Buying seeds/land/tractors decreases coins and grants the item
- Quality changes price as designed
- Crafting consumes inputs and produces output
- EditMode tests cover all economy paths and negative-currency prevention

---

## Task 7: Create Basic Farm Progression

**Priority:** Medium  
**Assigned to:** Coding Agent / Systems  
**Status:** Not Started

**Description**

Implement XP, levels, skill points, skills, and unlocks.

**Deliverables**

- `ProgressionSystem` service
- Six levels: Noob → Master
- Six investable skills
- XP awards for farming, sales, expansion, and tractor work
- Unlock checks for crops, tractors, decorations

**Acceptance**

- XP crosses level thresholds correctly
- Level-up grants a skill point
- Skill investment persists and affects the intended system
- Unlocks trigger and are visible in UI
- EditMode tests cover thresholds and skill investment

---

## Task 8: Initial Tractor Attachments

**Priority:** Medium  
**Assigned to:** Coding Agent / Systems  
**Status:** Not Started

**Description**

Implement the six core tractor attachments and their application to plots.

**Deliverables**

- Attachment base class/interface
- Tillage, Planter, Harvester, Transporter, Watering, Fertilizer implementations
- Attachment switching UI and `T` key cycle
- Fuel cost per application
- Cargo and inventory transfer path

**Acceptance**

- At least four attachments work in play: Tillage, Planter, Harvester, Transporter
- Watering and Fertilizer are represented and functional
- Attachment changes apply the correct plot effect
- Cargo is populated by harvester and emptied by transporter
- PlayMode test verifies all six attachments

---

## Integrated Playable Slice

In one Unity play session:

1. Start from Main Menu
2. Till, water, and plant a plot
3. Advance a day and grow the crop to ready
4. Harvest and deposit into inventory
5. Sell at quality-adjusted price
6. Buy seeds and expand land
7. Gain XP and invest a skill point
8. Switch tools/crops and drive a tractor
9. Use at least four attachments
10. Save, load, and reset without errors

---

## Unity Evidence Standard

Every completed task must include:

- Scene or prefab path
- Script/asset paths with line references where useful
- EditMode/PlayMode test names and results
- Unity Console output with no errors
- Editor screenshot or build artifact where applicable
- Director verification before status flips to Done

---

## GitHub Issues Template

When creating GitHub Issues for bugs, feature requests, or problems discovered during development:

### Required Fields

- **Title**: Clear, concise description
- **Body**:
  - Clear description of the issue
  - Steps to reproduce
  - Expected vs actual behavior
  - Unity version, render pipeline, scene, and commit if applicable
  - Console log or screenshot if applicable
- **Labels**:
  - Bug, Feature Request, Improvement, Design Compliance, Security, Licensing, Unity Migration
  - Phase 1 - Core Farming
- **Priority**: Critical, Major, or Minor

### Categories

1. Bugs
2. Feature Requests
3. Improvements
4. Design Compliance
5. Security
6. Licensing
7. Unity Migration / Architecture

---

## Team Coordination

- **Director Agent**: Owns milestone status and GO/NO-GO
- **Coding Agent**: Implements Unity features and tests
- **QA Agent**: Runs EditMode/PlayMode tests, manual play sessions, and build smoke tests
- **Style Agent**: Enforces C# and Unity conventions
- **Authenticity Agent**: Checks GDD compliance and asset licensing

All bugs, issues, and problems must be created as GitHub Issues when a repository is available.

---

**Last Updated:** November 21, 2026
