# Night at the Bar — Unity Scripts

## Architecture

All game **logic** lives in pure C# classes with no Unity rendering calls.
Unity UI scripts subscribe to **events** and call **public methods** — they never write to state directly.

```
Scripts/
├── Data/
│   ├── GameConstants.cs   — enums, tuning values, day/phase configs
│   └── ItemData.cs        — item and patron definitions (databases)
├── Core/
│   ├── GameState.cs       — runtime data containers (GameState, CampaignState, PatronState)
│   ├── GameManager.cs     — main logic controller (MonoBehaviour, singleton)
│   └── ScoreCalculator.cs — pure static scoring, no Unity deps
└── Minigames/
    ├── DriveManager.cs    — driving minigame logic (MonoBehaviour, singleton)
    └── KaraokeManager.cs  — karaoke scoring bridge (MonoBehaviour, singleton)
```

---

## Setup in Unity

1. Create an empty **persistent GameObject** in your first scene (e.g. `_GameManagers`).
2. Add `GameManager`, `DriveManager`, and `KaraokeManager` components to it.
3. Mark it `DontDestroyOnLoad` — `GameManager.Awake()` already does this.

---

## Wiring Your UI

Subscribe to events in your UI MonoBehaviours (usually in `Start()`):

```csharp
void Start()
{
    GameManager.Instance.OnStateChanged      += RefreshHUD;
    GameManager.Instance.OnLogLine           += AppendToLog;
    GameManager.Instance.OnPhaseChanged      += HandlePhaseChange;
    GameManager.Instance.OnNightEnded        += ShowEndingScreen;
    GameManager.Instance.OnBathroomEvent     += ShowBathroomEventPanel;
    GameManager.Instance.OnRandomEvent       += ShowRandomEventPanel;
    GameManager.Instance.OnInventoryFull     += ShowDropItemPanel;
    GameManager.Instance.OnPatronFriendshipChanged += RefreshPatronUI;
    GameManager.Instance.OnMorningAfter      += ShowMorningAfterScreen;

    DriveManager.Instance.OnObstaclePresented += ShowDriveObstacle;
    DriveManager.Instance.OnPoliceEncounter   += ShowPolicePanel;
    DriveManager.Instance.OnDriveEnded        += HandleDriveEnded;
    DriveManager.Instance.OnDriveStatsChanged += RefreshDriveHUD;

    KaraokeManager.Instance.OnKaraokeStart += StartRhythmGame;
    KaraokeManager.Instance.OnKaraokeEnd   += ShowKaraokeResult;
}
```

---

## Common Call Patterns

### Player walks to a zone
```csharp
GameManager.Instance.EnterZone(BarZone.PoolTable);
```

### Player orders a drink
```csharp
GameManager.Instance.OrderDrink();
```

### Player talks to a patron
```csharp
GameManager.Instance.TalkToPatron(PatronId.Crier);
```

### Bathroom event resolved by player
```csharp
// choiceA = true means first option (climb ladder, pee in snowblower, steal ticket)
GameManager.Instance.ResolveBathroomEvent(BathroomEvent.Ladder, choiceA: true);
```

### Random event resolved
```csharp
GameManager.Instance.ResolveRandomEvent(RandomEventType.Bump, choiceA: true); // "No worries"
```

### Inventory full — player drops item at index 1
```csharp
GameManager.Instance.ResolveInventoryFull(newItem, dropIndex: 1);
// dropIndex = -1 means drop the new item instead
GameManager.Instance.ResolveInventoryFull(newItem, dropIndex: -1);
```

### Arm wrestle
```csharp
GameManager.Instance.ResolveArmWrestle(accepted: true, buyIn: false);
```

### Give stuffed bear to patron
```csharp
GameManager.Instance.GiveBearToPatron(PatronId.Regular);
```

### Karaoke — rhythm component submits final score
```csharp
// Your rhythm component calls this when the note track finishes
KaraokeManager.Instance.SubmitScore(rhythmScore: 72); // 0-100
```

### Driving minigame choices
```csharp
// choiceA = true/false for each obstacle's two options (see GDD for mapping)
DriveManager.Instance.MakeChoice(DriveObstacle.Deer, choiceA: true); // brake
DriveManager.Instance.ResolvePolicePullover(pullOver: true);
```

### Morning after — player presses "Head back to the bar"
```csharp
GameManager.Instance.AdvanceToNextNight();
```

### Full restart
```csharp
GameManager.Instance.RestartCampaign();
```

---

## Tuning

All numbers are in `GameConstants.cs` under `static class Tuning`.
Change values there; nothing else needs to change.

Key values to tweak during playtesting:
- `DrunkDecayPerMin` — how fast you sober up passively
- `DayConfig.BoredomTick` — per-day boredom rise rate
- `DayConfig.DrinkBase` — drunk hit per drink per day
- `ClawGrabRate` / `ClawHoldRate` — claw machine win rates
- `RandomEventChance` — how often mid-night events fire

---

## What's NOT in these scripts

The following need Unity-specific implementations that depend on your art/scene setup:

- **Isometric map, player movement, collision** — your existing system
- **Patron NPC visuals and positions** — place them at booth spots in the scene
- **Rhythm game** — any Unity rhythm framework; call `KaraokeManager.Instance.SubmitScore(score)` when done
- **Drunk visual effects** — screen wobble, double vision, blackout blinks; read `GameState.Drunk` and apply in your camera/shader
- **Audio** — all sounds, music, phase-based jukebox changes
- **Driving visuals** — top-down road scroller; `DriveManager` handles all logic, you handle visuals
- **UI panels** — HUD, log, inventory display, ending screens
