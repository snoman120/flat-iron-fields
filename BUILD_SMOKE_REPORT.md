# Build Smoke Report — Unity Milestone 1

**Date:** 2026-11-22  
**Unity:** 6000.3.9f1  
**Pipeline:** 2D URP  
**Target:** Windows x64 desktop (primary validation target)  
**Result:** PASS

**Phase 2 rebuild:** PASS — 2026-11-22, `Flat Iron Fields Windows build: Succeeded, size=95602896 bytes, time=00:00:09.0914420, errors=0, warnings=3`. Rebuilt executable booted for 10 seconds with 0 runtime errors in `Player.log`. Log: `C:\Users\yuppl\AppData\Local\Temp\opencode\unity-build-phase2.log`.

## Build

Built with `Assets/Scripts/Game/Editor/BuildSmoke.cs` via:

```text
Unity.exe -batchmode -quit -projectPath C:\dev\flat-iron-fields \
  -executeMethod FlatIronFields.Game.Editor.BuildSmoke.BuildWindows \
  -logFile C:\Users\yuppl\AppData\Local\Temp\opencode\unity-build.log
```

```text
Flat Iron Fields Windows build: Succeeded, size=95590756 bytes, time=00:00:42.2013571, errors=0, warnings=3
Unity exit code: 0
```

Scenes included, in bootstrap order:

1. `Assets/Scenes/Boot.unity`
2. `Assets/Scenes/MainMenu.unity`
3. `Assets/Scenes/Farm.unity`

### Artifacts

Location: `Builds/Windows/`

| File | Size |
| --- | --- |
| `FlatIronFields.exe` | 667,648 bytes |
| `UnityPlayer.dll` | 35,796,904 bytes |
| `UnityCrashHandler64.exe` | 1,609,640 bytes |
| `FlatIronFields_Data/` | managed assemblies, resources, scenes |
| `MonoBleedingEdge/` | Mono runtime |
| `D3D12/` | Direct3D 12 shader cache |

Console: 0 errors. 3 non-fatal build warnings; no C# compiler warnings found in the log.

## Startup Smoke

Launched `Builds/Windows/FlatIronFields.exe` and observed the running process for 12 seconds.

- Process remained alive: yes.
- Assemblies loaded in 1.270 seconds.
- Graphics: Direct3D 12, AMD Radeon RX 6700 XT, feature level 12.2.
- Input System initialized.
- `Player.log` error lines: **0**.
- Process stopped intentionally after the observation window.

`Player.log` location:
`C:\Users\yuppl\AppData\LocalLow\DefaultCompany\flat-iron-fields\Player.log`

## Checklist

- [x] Activate Unity Editor license.
- [x] Import project and resolve package/compile errors.
- [x] Generate ScriptableObjects and scenes.
- [x] Run EditMode tests (27/27 passed).
- [x] Run PlayMode tests (5/5 passed).
- [x] Build Windows x64 player.
- [x] Launch build and confirm it boots without runtime errors.
- [x] Capture console and build log evidence.
- [ ] Capture screenshot of Main Menu and Farm scene (QA follow-up).
- [ ] Linux x64 build smoke (QA follow-up).
- [ ] macOS build smoke (QA follow-up).

## Final Call

**PASS** for the Windows x64 desktop smoke test. Linux and macOS builds remain QA follow-ups and do not block the Milestone 1 Windows GO.
