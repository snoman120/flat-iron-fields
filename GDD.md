# Flat Iron Fields - Game Design Document (GDD)

**Version:** 2.0  
**Date:** November 21, 2026  
**Engine:** Unity 6 LTS with 2D URP  
**Platforms:** Windows, Linux, macOS (desktop-first)  
**Genre:** Farming Simulation / Strategy

---

## 0. Document Status

Version 2.0 rebaselines Flat Iron Fields from the temporary HTML5 Canvas prototype to the shipping target: **Unity 6 LTS + 2D URP**.

- The browser slice remains in the repository as a **reference design prototype** and evidence of the core loop.
- It is **not** the production implementation and is not part of the Unity acceptance gate.
- All future implementation, milestone, architecture, and QA documentation targets Unity C#.

---

## 1. Game Overview

### 1.1 High-Level Concept

Flat Iron Fields is a 2D farming simulation that combines relaxing crop cultivation with strategic tractor-based farm management. Players start as a beginner farmer and gradually build a farm into a thriving agricultural operation, learning to use tractors and attachments for increasingly efficient work.

### 1.2 Core Vision

Create an engaging farming experience that:

- Combines the relaxed farming mechanics of games like Stardew Valley and Harvest Moon
- Introduces tractor-based automation and efficiency
- Provides a clear progression path from beginner to expert farmer
- Sets up future expansion into base defense and zombie-wave mechanics

### 1.3 Game Pillars

1. **Relaxing Farming**: Enjoyable crop cultivation and harvesting mechanics
2. **Strategic Management**: Efficient farm planning and resource allocation
3. **Tractor System**: Versatile tractor mechanics that automate farming tasks
4. **Progression**: Clear advancement from noob to expert farmer
5. **Discovery**: Unlocking new tools, crops, and techniques as players progress

---

## 2. Core Gameplay

### 2.1 Crop System

**Canonical crop list (26 types, deduplicated from the original GDD):**

Grass, Cabbage, Wheat, Carrot, Sweet Potato, Potato, Corn, Pepper, Tomato, Watermelon, Pumpkin, Onion, Garlic, Strawberry, Apple, Peach, Plum, Grape, Cherry, Orange, Lemon, Blueberry, Blackberry, Raspberry, Golden Apple, Moonberry.

Notes:

- `Grape/Grapes`, `Cherry/Cherries`, and `Strawberry/Strawberries` are treated as single crop IDs.
- Golden Apple and Moonberry are discovery/secret crops tied to later progression levels.
- Each crop definition includes: id, display name, growing seasons, growth days, yield range, seed cost, sale price, multiple-harvest flag, and unlock level.

**Growth stages:**

`Seed → Sprout → Grow → Bloom → Harvest`

**Seasonality:**

- Four seasons: Spring, Summer, Autumn, Winter
- Each crop has one or more valid seasons
- Out-of-season planting is rejected unless an explicit unlock/override is implemented
- Perennial/multi-harvest crops can regrow within their valid season

**Quality levels:**

`Poor, Normal, Good, Perfect`

Quality affects sale price. Quality is influenced by watering consistency, Fertilizer attachment use, weeds/pests, and relevant progression skills.

### 2.2 Farm Management

- **Plot management**: Players own a fixed amount of land divided into plots
- **Plot types**: Regular plots, raised beds (dual crop support), fenced plots (crop protection)
- **Plot states**: Wild, Tilled, Cultivated, Watered, Planted, Ready, Harvested
- **Growth inputs**: Water, weeding, pest protection, fertilizer, season, weather
- **Time system**: Real-time day/season progression with a manual next-day action for testing and pacing

### 2.3 Farming Tools

- **Shovel**: Dig holes and till soil
- **Watering Can**: Irrigate crops
- **Rake**: Prepare soil for planting
- **Hoe**: Weed and till soil
- **Scythe**: Harvest crops quickly
- **Cart**: Transport harvested crops and supplies

### 2.4 Tractor System

**Tractor types:**

- **Basic Tractor**: Simple utility tractor; till and transport
- **Crop Tractor**: Specialized for planting and harvesting
- **Heavy Duty**: Large-scale field operations
- **Specialized**: Unique functions such as precision plowing or irrigation

**Tractor functions:**

- Automatic soil preparation
- Large-scale planting
- Efficient harvesting
- Crop transportation
- Fertilizer application
- Irrigation management

**Attachments:**

- Tillage
- Planter
- Harvester
- Transporter
- Watering
- Fertilizer

---

## 3. Game Structure

### 3.1 Main Game Loop

1. Prepare land: till, water, clear weeds
2. Plant crops: select season-appropriate crops and seeds
3. Manage growth: water, weed, fertilize, protect from pests
4. Harvest crops: collect mature crops manually or with a tractor
5. Sell resources: sell crops at the farm store or through the cart
6. Purchase upgrades: expand land, buy seeds/tools/tractors, invest skills
7. Repeat through seasons

### 3.2 Farm Expansion

- **Starting farm**: 5 plots (10x20 tile footprint)
- **Stage 1**: 10 plots (10x20 tiles)
- **Stage 2**: 20 plots (20x20 tiles)
- **Stage 3**: 30 plots (30x20 tiles)
- **Stage 4**: 50 plots (50x20 tiles)

Expansion is purchased with farming coins and applied to the Unity Tilemap farm grid.

### 3.3 Economy System

- **Currency**: Farming coins
- **Income sources**: Crop sales, crafting, quests/challenges, special events
- **Expenditures**: Land, tools, tractors, attachments, upgrades, research, event costs

---

## 4. Progression System

### 4.1 Skill Levels

1. Noob
2. Beginner
3. Intermediate
4. Advanced
5. Expert
6. Master

### 4.2 Skill Progression

- Farming activities award XP
- XP converts into level progression
- Level-ups grant skill points
- Skill points are invested in:
  - Planting efficiency
  - Harvesting speed
  - Crop selection
  - Resource management
  - Tractor operation
  - Farm expansion

### 4.3 Unlocks and Discoveries

- New crops
- New tools and tractor types
- Attachments and upgrades
- Farm decorations
- Special abilities
- Secret crops and achievements

---

## 5. Tractor Mechanics

### 5.1 Controls

- Drive with WASD or arrow keys via the Unity Input System
- Space interacts with the plot under the tractor
- Q/E cycles selected crop
- T cycles attachment
- N advances the day
- Number keys select tools
- B/U/M open management menus

### 5.2 Attachments

| Attachment | Function |
| --- | --- |
| Tillage | Prepares soil for planting |
| Planter | Plants the selected in-season seed |
| Harvester | Harvests mature crops into cargo |
| Transporter | Unloads cargo into inventory |
| Watering | Irrigates plots automatically |
| Fertilizer | Adds quality bonus to plots |

### 5.3 Tractor Upgrades

- Speed enhancement
- Fuel efficiency
- Cargo capacity
- Special abilities
- Automation level

---

## 6. Visual and Audio Design

### 6.1 Visual Style

- **2D pixel art** with retro aesthetics and modern readability
- Unity Tilemap for farm plots and terrain
- 2D URP lighting and color grading for seasonal mood
- Greens for crops, browns for soil, blues for water, earth tones for structures

**UI:**

- uGUI with TextMeshPro for HUD and menus
- Seasonal color indicators
- Clear crop status indicators: stage, water, readiness, quality bonus
- Persistent tractor status: type, attachment, fuel, cargo

### 6.2 Audio Design

- Relaxing farm ambience and seasonal music
- Tractor engine loops and attachment feedback
- Till/plant/water/harvest/weather/collect sound effects
- Unity AudioMixer for music/SFX balance

---

## 7. Technical Specification

### 7.1 Engine and Stack

- **Unity:** 6 LTS
- **Renderer:** 2D URP
- **Language:** C#
- **Input:** Unity Input System package
- **Tile system:** Unity Grid + Tilemap
- **Data:** ScriptableObject assets for crops, tractor types, attachments, skills, and recipes
- **UI:** uGUI + TextMeshPro
- **Save:** JSON to `Application.persistentDataPath`
- **Tests:** Unity Test Framework (EditMode + PlayMode)
- **Source control:** Git; generated Library/Temp/Logs ignored

No npm, web server, browser APIs, or external runtime assets are required by the Unity build.

### 7.2 Target Platforms

Desktop-first, cross-platform:

- Windows 10/11 (primary)
- macOS 12+
- Linux x64

Platform-specific input, save paths, and build settings are handled through Unity abstractions.

### 7.3 Proposed Project Layout

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

### 7.4 Performance Targets

- Stable 60 FPS on target desktop hardware
- Under 1 GB memory for the base game
- Under 5 seconds from Boot scene to playable farm on target hardware
- Deterministic day advance and save/load without frame-rate dependence

### 7.5 Reference Prototype

The previous dependency-free HTML5 Canvas slice is preserved as a **design reference only**:

- `index.html`, `styles.css`, `src/*.js`
- `tests/smoke.html`, `tests/smoke.js`

Its browser test results are historical evidence of the design loop, not Unity acceptance evidence.

---

## 8. Future Expansion (Phase 4)

### 8.1 Base Defense System

**Milestone:** Phase 4 - Base Defense & Zombies

Features:

- Weekly zombie waves with escalating difficulty
- Special zombie types and boss zombies
- Strategic base placement and defense upgrades
- Cured zombies become drones for patrol, combat, gathering, and farm assistance
- Shared economy and progression between farming and defense systems

---

## 9. Monetization (Optional)

- One-time purchase
- DLC with new crops, tractors, or modes
- Cosmetics
- Seasonal pass

Ethical constraints:

- No pay-to-win
- Fair pricing
- Transparent purchase descriptions
- Family-friendly content

---

## 10. Development Roadmap

### 10.1 Phase 1: Unity Core Farming

- Create Unity 6 LTS + 2D URP project
- Implement crop system and ScriptableObject data
- Implement plot states, Tilemap farm, day/season cycle
- Implement tools, manual farming, and tractor basics
- Implement UI/menus, economy, progression, and core attachments
- Deliver playable vertical slice with Unity Test Framework evidence

### 10.2 Phase 2: Tractor and Systems Depth

- All tractor types and attachment upgrades
- Weather modifiers and seasonal palette pass
- Complete crafting, quests, and unlocks
- Audio pass and optimization

### 10.3 Phase 3: Farm Expansion

- Full land expansion stages
- New crops, tools, decorations, and balance tuning
- Save/load hardening and performance optimization

### 10.4 Phase 4: Base Defense & Zombies

- Weekly wave system
- Base defense structures and drone mechanics
- Integration testing with farming systems

### 10.5 Phase 5: Polish and Release

- Bug fixing, localization, QA, certification, and release builds

---

## 11. Risk Assessment

### 11.1 Technical Risks

- Unity 6 LTS/2D URP package drift
- Input System migration errors
- Save data compatibility across builds
- Cross-platform build differences

### 11.2 Content Risks

- Balance between manual and tractor farming
- Crop variety without overwhelming players
- Replayability across seasons

### 11.3 Market Risks

- Competition in the farming genre
- Player expectations around “tractor automation”
- Timing and discoverability

---

## 12. Success Metrics

### 12.1 Quantitative

- Day 1, Day 7, Day 30 retention
- Session length and frequency
- Revenue and conversion where applicable
- Player satisfaction scores

### 12.2 Qualitative

- Player feedback and reviews
- Community activity
- Partnerships and press coverage

---

## 13. Conclusion

Flat Iron Fields combines a relaxing farming loop with tractor-based efficiency and a clear progression path. The Unity 6 LTS + 2D URP foundation provides the production target for the full 2D game, with the existing HTML5 slice retained only as a reference proof of the core design. The planned base-defense expansion will add long-term challenge while preserving the peaceful farming identity.

---

## Document Version History

| Version | Date | Author | Description |
| --- | --- | --- | --- |
| 1.0 | 2025-11-21 | Game Team | Initial GDD creation |
| 2.0 | 2026-11-21 | Project Team | Unity 6 LTS + 2D URP rebaseline; cross-platform desktop target; browser slice marked as reference prototype |

**End of Document**
