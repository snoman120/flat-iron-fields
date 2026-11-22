# Active Directive — 2026-11-21 (Unity Rebaseline)

This is the standing order for every agent in this repository. The project has pivoted from the temporary HTML5 Canvas prototype to the production target: **Unity 6 LTS + 2D URP**.

The Director Agent owns the go/no-go call and keeps `MILESTONE_1_STATUS.md` current.

## Non-negotiables

1. All work runs on the local Jetson model `llama/qwen2.5-7b`. No cloud model substitution.
2. Target is a **Unity 6 LTS + 2D URP** C# project, not a browser app.
3. Target platforms are **Windows, Linux, and macOS desktop**. Windows is the primary validation target.
4. All 8 Milestone 1 tasks must be represented in working Unity form:
   - crop system
   - core farming mechanics
   - farm layout and expansion
   - tractor basics
   - UI and menus
   - economy basics
   - player progression
   - core tractor attachments
5. Keep modules isolated per `ARCHITECTURE.md`. Do not edit files owned by another agent.
6. The existing HTML5 Canvas slice is a **reference prototype only**. Its browser test results are historical evidence, not Unity acceptance evidence.
7. Record blockers immediately in `MILESTONE_1_STATUS.md` with owner and impact.
8. Every completed task needs Unity evidence: scene/prefab/script paths, EditMode/PlayMode results, console output, and Director verification.

## Definition of done

- Unity project opens in Unity 6 LTS with no missing-package or compile errors.
- Boot → Main Menu → Farm scenes load and the game loop runs at a stable frame rate.
- Player can till, water, plant, harvest, sell, buy seeds/land, gain XP, level up, switch tools/crops, and drive a tractor with at least four attachments in Unity play mode.
- EditMode and PlayMode tests pass for the implemented systems.
- Save/load/reset work without console errors.
- At least one desktop build smoke test passes; remaining cross-platform builds are tracked by QA.
- `MILESTONE_1_STATUS.md` marks each Unity task Done / In Progress / Blocked with evidence, and the Director records the final GO/NO-GO.

## Immediate actions

1. Rebaseline all project markdown to Unity (complete with this directive set).
2. Create the Unity 6 LTS + 2D URP project skeleton and package manifest.
3. Reopen Milestone 1 status with Unity acceptance criteria.
4. Assign agents to their Unity-owned directories in `ARCHITECTURE.md`.
5. Preserve the HTML5 files unchanged as reference material.

## Schedule

The previous “tonight” browser-slice deadline is superseded by this Unity rebaseline. The Director records the new target date and milestone schedule in `MILESTONE_1_STATUS.md`.

No follow-up approval is required to execute against this directive.
