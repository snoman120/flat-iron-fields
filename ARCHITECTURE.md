# Milestone 1 Architecture Contract — Unity

Read-only for all agents. Follow this exactly so parallel work integrates cleanly.

## Stack

- **Unity:** 6 LTS
- **Renderer:** 2D URP
- **Language:** C#
- **Input:** Unity Input System package
- **Tiles:** Unity Grid + Tilemap
- **Data:** ScriptableObject assets for static definitions
- **UI:** uGUI + TextMeshPro
- **Save:** JSON in `Application.persistentDataPath`
- **Tests:** Unity Test Framework (`Assets/Tests/EditMode`, `Assets/Tests/PlayMode`)

No JavaScript, `window.FIF`, browser APIs, npm, web server, or external runtime assets are used by the Unity implementation. The existing `index.html` / `src/*.js` files are reference material only.

## Scene and Bootstrap Order

1. `Assets/Scenes/Boot.unity`
2. `Assets/Scenes/MainMenu.unity`
3. `Assets/Scenes/Farm.unity`

Bootstrap sequence inside `Boot.unity`:

1. `ServiceLocator` / `GameBootstrap`
2. `SaveService`
3. `CropDatabase` (loads ScriptableObject assets)
4. `FarmGrid` (builds plots from current expansion stage)
5. `EconomySystem`
6. `ProgressionSystem`
7. `TractorService`
8. `InputRouter`
9. `UIManager` / `HudController`
10. `GameController`
11. Load `MainMenu.unity`; player starts run → `Farm.unity`

Systems must tolerate being initialized in isolation for EditMode tests.

## Shared Namespaces

```csharp
namespace FlatIronFields.Core
namespace FlatIronFields.Crops
namespace FlatIronFields.Farming
namespace FlatIronFields.Economy
namespace FlatIronFields.Progression
namespace FlatIronFields.Tractors
namespace FlatIronFields.Input
namespace FlatIronFields.UI
namespace FlatIronFields.Game
```

Use constructor or interface injection through `GameBootstrap`; avoid singletons except `GameBootstrap` itself.

## Project Layout and Ownership

| Agent | Owns |
| --- | --- |
| Core Systems | `Assets/Scripts/Core/`, `Assets/Scripts/Crops/`, `Assets/Scripts/Farming/`, `Assets/ScriptableObjects/Crops/`, `Assets/ScriptableObjects/Farm/` |
| Systems | `Assets/Scripts/Economy/`, `Assets/Scripts/Progression/`, `Assets/Scripts/Tractors/`, `Assets/ScriptableObjects/Economy/`, `Assets/ScriptableObjects/Tractors/`, `Assets/ScriptableObjects/Skills/` |
| Client | `Assets/Scenes/`, `Assets/Prefabs/`, `Assets/Scripts/Input/`, `Assets/Scripts/UI/`, `Assets/Scripts/Game/`, `Assets/Tests/PlayMode/`, `README.md` |
| QA | `Assets/Tests/EditMode/`, test plans, build smoke reports |
| Style / Authenticity | Guidelines and review reports only |
| Director | `MILESTONE_1_STATUS.md`, GO/NO-GO |

Do not edit another agent's directories. Write integration requests in `MILESTONE_1_STATUS.md`.

## Required Data Assets

### CropDefinition (ScriptableObject)

Fields:

```csharp
string Id;
string DisplayName;
Season[] Seasons;
int GrowthDays;
int YieldMin;
int YieldMax;
int SeedCost;
int SalePrice;
bool MultipleHarvest;
int UnlockedAtLevel;
```

### TractorDefinition (ScriptableObject)

```csharp
string Id;
string DisplayName;
float Speed;
int CargoCapacity;
int Cost;
int UnlockedAtLevel;
```

### AttachmentDefinition / SkillDefinition / RecipeDefinition

All static gameplay tuning lives in ScriptableObjects, not hardcoded dictionaries.

## Required Runtime Shape

```csharp
public sealed class GameState
{
    public int Day;
    public Season Season;
    public Weather Weather;
    public int Coins;
    public ToolId SelectedTool;
    public string SelectedCropId;
    public Inventory Inventory;
    public FarmState Farm;
    public TractorState Tractor;
    public PlayerState Player;
    public List<string> Messages;
}
```

Inventory:

```csharp
Dictionary<string, int> Seeds;             // cropId -> count
Dictionary<string, Dictionary<Quality, int>> Crops; // cropId -> quality -> count
List<ToolId> Tools;
```

Plot:

```csharp
public sealed class Plot
{
    public int Id;
    public Vector2Int GridPosition;
    public PlotType Type;
    public PlotState State;
    public string CropId;
    public int StageIndex;
    public int Water;
    public int QualityBonus;
    public bool Ready;
    public int Weeds;
}
```

## Required APIs

### `FlatIronFields.Crops.CropSystem`

```csharp
bool IsInSeason(string cropId, Season season);
IReadOnlyList<CropGrowthStage> GrowthStages(); // Seed, Sprout, Grow, Bloom, Harvest
bool Advance(Plot plot, int days);
Quality QualityFor(Plot plot);
CropYield YieldFor(Plot plot);
```

### `FlatIronFields.Farming.FarmGrid`

```csharp
FarmShape GetShape(int expansionStage);
List<Plot> CreateFarm(int expansionStage);
bool Expand(int expansionStage);
bool Till(Plot plot);
bool Water(Plot plot);
bool Plant(Plot plot, string cropId);
CropYield Harvest(Plot plot);
```

`FarmShape`: plots, cols, rows, tileCols, tileRows.

Stages: 5/10/20/30/50 plots with tile footprints 10x20, 10x20, 20x20, 30x20, 50x20.

### `FlatIronFields.Economy.EconomySystem`

```csharp
int Price(string cropId, Quality quality);
int Sell(string cropId, int quantity, Quality quality); // returns coins earned
bool BuySeed(string cropId, int quantity);
bool BuyPlot();
bool BuyTractor(string tractorId);
bool Craft(string recipeId);
```

Selling must remove nested inventory and never allow negative coins.

### `FlatIronFields.Progression.ProgressionSystem`

```csharp
int LevelForXp(int xp);
void AddXp(int amount, string reason);
bool InvestSkill(string skillId);
void CheckUnlocks();
```

Six levels: Noob, Beginner, Intermediate, Advanced, Expert, Master.  
Six skills: planting, harvesting, cropSelection, resourceManagement, tractorOperation, farmExpansion.

### `FlatIronFields.Tractors.TractorController`

```csharp
void Initialize(TractorDefinition definition);
void Attach(string attachmentId);
void UpdateMovement(float deltaTime, InputState input);
bool ApplyAttachment(Plot plot, string attachmentId);
int CargoCount();
```

Attachment IDs: `tillage`, `planter`, `harvester`, `transporter`, `watering`, `fertilizer`.

### `FlatIronFields.Input.InputRouter`

```csharp
InputState Current { get; }
bool WasPressed(ActionId action);
```

Use the Unity Input System action map. Do not read legacy `Input.GetKey` in new code.

### `FlatIronFields.UI.UIManager`

```csharp
void Initialize(GameState state);
void RenderHud(GameState state);
void OpenMenu(MenuId menu);
void CloseMenu();
void Message(string text);
```

### `FlatIronFields.Game.GameController`

```csharp
void StartRun();
void NewDay();
void Update(float deltaTime);
void UseTool(Plot plot);
void SaveGame();
void LoadGame();
void ResetGame();
```

## Behavior Notes

- Prefer short, testable methods over deep abstraction.
- Every public method must tolerate null state and return safely.
- Use `var` only when the type is obvious; prefer explicit types in public APIs.
- Keep each source file under ~400 lines.
- No blocking I/O on the main thread; save/load is synchronous but small JSON only.
- Use `Time.deltaTime` for movement; day advance is explicit and deterministic.
- All plot state changes go through `FarmGrid` methods, not direct field writes from UI.
- All currency movement goes through `EconomySystem`.
- All XP and unlocks go through `ProgressionSystem`.
- Manual and tractor actions must update the same `GameState` instance.

## Testing Contract

- EditMode: crop math, farm shapes, economy, progression, save schema
- PlayMode: manual farming loop, tractor driving, attachments, UI menu flow, full integrated slice
- Evidence: test names, pass/fail counts, Unity Console, screenshots or build logs

## Reference Prototype

Do not port browser JavaScript directly. Use the HTML5 slice only to confirm intended behavior and data values.
