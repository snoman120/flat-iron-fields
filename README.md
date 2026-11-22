# Flat Iron Fields

Flat Iron Fields is a 2D farming simulation with tractor-based farm management. The production target is **Unity 6 LTS + 2D URP** for **Windows, Linux, and macOS desktop**.

## Current State

The project has been rebaselined from a browser prototype to Unity.

- The Unity project skeleton and C# implementation are **not yet created**.
- The existing HTML5 Canvas files are preserved as a **reference design prototype only**.
- Milestone 1 has been reopened under Unity acceptance criteria in `MILESTONE_1_STATUS.md`.

## Documentation

| File | Purpose |
| --- | --- |
| `GDD.md` | Game design, systems, technical target, roadmap |
| `DIRECTIVE.md` | Standing orders for all agents |
| `ARCHITECTURE.md` | Unity project structure, ownership, APIs, bootstrap order |
| `MILESTONE_1_TASKS.md` | The eight Unity Milestone 1 tasks and acceptance criteria |
| `MILESTONE_1_STATUS.md` | Director-owned task board and GO/NO-GO gate |
| `.opencode/agents/*.md` | Director, Coding, QA, Style, and Authenticity roles |

## Planned Unity Project Layout

```text
Assets/
  Art/
  Audio/
  Prefabs/
  Scenes/
    Boot.unity
    MainMenu.unity
    Farm.unity
  ScriptableObjects/
    Crops/
    Tractors/
    Attachments/
    Skills/
    Recipes/
  Scripts/
    Core/
    Crops/
    Farming/
    Economy/
    Progression/
    Tractors/
    Input/
    UI/
    Game/
  Tests/
    EditMode/
    PlayMode/
ProjectSettings/
Packages/
```

## How to Run (once the Unity project exists)

1. Install Unity 6 LTS with the **2D URP** template.
2. Open this repository folder as the Unity project root, or copy the generated `Assets/`, `ProjectSettings/`, and `Packages/` into the root.
3. Open `Assets/Scenes/Boot.unity`.
4. Press Play in the Editor.
5. Start a run from the Main Menu to load the Farm scene.

## Planned Controls

| Input | Action |
| --- | --- |
| WASD / Arrows | Drive tractor |
| Space | Interact with plot under tractor |
| Mouse click | Use selected tool on a plot |
| 1–6 | Select tool |
| Q / E | Previous / next crop |
| T | Cycle tractor attachment |
| N | Advance day |
| B / U / M | Seeds / Upgrades / Management menu |

## Reference Prototype

The browser slice remains available for design reference:

- `index.html`
- `styles.css`
- `src/*.js`
- `tests/smoke.html`
- `tests/smoke.js`

It can be opened in a browser without a build step, but it is **not** the production implementation and its test results do not satisfy the Unity acceptance gate.

## Model / Agent Workflow

All project work is assigned to local agents running on the Jetson-served model `llama/qwen2.5-7b`. No cloud model substitution is permitted by `DIRECTIVE.md`.
