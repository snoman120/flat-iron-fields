# Flat Iron Fields

Flat Iron Fields is a 2D farming simulation with tractor-based farm management. The production target is **Unity 6 LTS + 2D URP** with C# and desktop builds for Windows, Linux, and macOS.

The HTML5 Canvas files in the repository root are a preserved design reference only. They are not the production implementation and do not satisfy Milestone 1 acceptance.

## Repository State

- Unity version: `6000.3.9f1` (`UNITY_VERSION.txt`, `ProjectSettings/ProjectVersion.txt`)
- Local agent model: `llama/qwen2.5-7b` (`opencode.jsonc`; no cloud substitution)
- Current gate: **NO-GO** until Unity license activation and Unity Test Runner evidence are captured. See `MILESTONE_1_STATUS.md`.

## Unity Setup

1. Install Unity 6000.3.9f1 with the 2D URP template support.
2. Open this folder as a Unity project.
3. Allow package import to finish. Required packages are declared in `Packages/manifest.json`.
4. Run these menu commands:
   - `Flat Iron Fields > Generate Crop Assets`
   - `Flat Iron Fields > Generate System Assets`
   - `Flat Iron Fields > Generate Scenes`
5. Open `Assets/Scenes/MainMenu.unity` and press Play.

Generated assets:
- `Assets/ScriptableObjects/Crops/` — 26 crops
- `Assets/ScriptableObjects/Tractors/` — 4 tractors
- `Assets/ScriptableObjects/Tractors/Attachments/` — 6 attachments
- `Assets/ScriptableObjects/Skills/` — 6 skills
- `Assets/ScriptableObjects/Recipes/` — 2 recipes
- `Assets/Scenes/Boot.unity`, `MainMenu.unity`, `Farm.unity`

## Controls

| Input | Action |
| --- | --- |
| WASD / arrows | Drive tractor |
| Space | Use selected tool on plot under tractor |
| 1 | Shovel / till |
| 2 | Watering can |
| 3 | Rake / plant selected crop |
| 4 | Hoe / clear weeds |
| 5 | Scythe / harvest |
| 6 | Cart / transfer cargo |
| Q / E | Cycle crop |
| T | Cycle attachment |
| N | Advance day |
| B / U / M / V / K | Seeds / Sell / Land / Tractor / Skills menus |
| H | Help overlay |

## Systems

- Crop definitions, growth stages, seasonality, quality, and yield: `Assets/Scripts/Crops/`
- Plot states, expansion shapes, and manual tools: `Assets/Scripts/Farming/`
- Tractor definitions, Input System movement, fuel, cargo, and six attachments: `Assets/Scripts/Tractors/`
- Economy, recipes, quality-adjusted pricing, and purchase paths: `Assets/Scripts/Economy/`
- Six levels, six skills, XP, and unlocks: `Assets/Scripts/Progression/`
- UI, input routing, scene generation, save/load: `Assets/Scripts/UI/`, `Assets/Scripts/Input/`, `Assets/Scripts/Game/`

## Tests

- EditMode: `Assets/Tests/EditMode/`
- PlayMode: `Assets/Tests/PlayMode/`

Run both from Unity Test Runner. Expected counts and evidence requirements are in `QA_TEST_PLAN.md`.

## Reference Documents

- `GDD.md` — design document
- `ARCHITECTURE.md` — ownership and API contract
- `MILESTONE_1_TASKS.md` — acceptance criteria
- `MILESTONE_1_STATUS.md` — Director-owned gate and blockers
- `DIRECTIVE.md` — standing project directive
- `BUILD_SMOKE_REPORT.md` — build evidence status

---

**Last Updated:** November 22, 2026
