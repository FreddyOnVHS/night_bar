// GameManager.cs
// Central game logic controller. Attach to a persistent GameObject in Unity.
// All game rules live here. UI subscribes to events; it never writes state directly.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NightAtTheBar
{
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── State ─────────────────────────────────────────────────────────────
        public GameState    State    { get; private set; }
        public CampaignState Campaign { get; private set; }

        private System.Random _rng = new();
        private float _gameTimerAccum = 0f;   // real seconds since last game-minute tick
        private bool  _timerPaused   = false;

        // ── Events (UI subscribes to these) ───────────────────────────────────
        public event Action<GameState>          OnStateChanged;
        public event Action<string>             OnLogLine;
        public event Action<NightPhase>         OnPhaseChanged;
        public event Action<EndingType, int>    OnNightEnded;       // ending, score
        public event Action<BathroomEvent>      OnBathroomEvent;
        public event Action<RandomEventType>    OnRandomEvent;
        public event Action<ItemDefinition, List<ItemDefinition>> OnInventoryFull; // new item, current inv
        public event Action<PatronState>        OnPatronFriendshipChanged;
        public event Action<DriveObstacle>      OnDriveObstacle;
        public event Action                     OnMorningAfter;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Campaign = new CampaignState();
            StartNewNight();
        }

        private void Update()
        {
            if (_timerPaused || State == null || State.NightEnded) return;

            _gameTimerAccum += Time.deltaTime;
            while (_gameTimerAccum >= Tuning.RealSecsPerGameMin)
            {
                _gameTimerAccum -= Tuning.RealSecsPerGameMin;
                TickOneGameMinute();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // NIGHT SETUP
        // ═════════════════════════════════════════════════════════════════════

        public void StartNewNight()
        {
            var cfg = Tuning.Days[Campaign.CurrentDayIndex % 7];

            State = new GameState();
            State.Drunk   = Tuning.DrunkResetValue;
            State.Boredom = Tuning.BoredomResetValue;

            // Restore persistent inventory
            State.Inventory = new List<ItemDefinition>(Campaign.SavedInventory);

            // Build patron roster
            State.Patrons = BuildPatronRoster();

            // Restore persistent friendships
            foreach (var ps in State.Patrons)
            {
                if (Campaign.SavedFriendships.TryGetValue(ps.Id, out var saved))
                {
                    ps.Friendship            = saved.Friendship;
                    ps.FriendTier            = saved.FriendTier;
                    ps.ConversationBeat      = saved.ConversationBeat;
                    ps.ConversationsCompleted= saved.ConversationsCompleted;
                    ps.GaveGift              = saved.GaveGift;
                }
                // Re-apply perks from persisted friendships
                if (ps.FriendTier >= 2) ApplyPatronPerk(ps);
            }

            _timerPaused = false;
            Log($"--- {cfg.Name} night. You push open the door. ---");
            if (Campaign.NightsCompleted == 0)
                Log("Walk up to something and press E to interact.");
            else
                Log("The familiar smell hits you. Another night begins.");

            NotifyStateChanged();
        }

        private List<PatronState> BuildPatronRoster()
        {
            var cores  = PatronDatabase.All.Where(p => p.CoreRegular).ToList();
            var guests = PatronDatabase.All.Where(p => !p.CoreRegular)
                                           .OrderBy(_ => _rng.Next()).Take(2).ToList();
            var night  = cores.Concat(guests).OrderBy(_ => _rng.Next()).Take(5);

            return night.Select(def => new PatronState
            {
                Id          = def.Id,
                DisplayName = def.DisplayName,
                LeaveMinute = def.LeaveMinute,
            }).ToList();
        }

        // ═════════════════════════════════════════════════════════════════════
        // TIMER TICK
        // ═════════════════════════════════════════════════════════════════════

        private void TickOneGameMinute()
        {
            AdvanceTime(1);
        }

        public void AdvanceTime(int gameMinutes)
        {
            var cfg = Tuning.Days[Campaign.CurrentDayIndex % 7];

            for (int i = 0; i < gameMinutes; i++)
            {
                State.GameMinute++;

                // ── Sweet spot tracking ───────────────────────────────────────
                if (State.Drunk >= Tuning.DrunkBoreZoneMax + 1 &&
                    State.Drunk <= Tuning.DrunkSweetSpotMax)
                {
                    State.SweetSpotMinutes++;
                    State.CurrentSweetStreak++;
                    State.BestSweetStreak = Math.Max(State.BestSweetStreak, State.CurrentSweetStreak);
                }
                else State.CurrentSweetStreak = 0;

                // ── Drunk decay ───────────────────────────────────────────────
                State.DrunkDecayFrac += Tuning.DrunkDecayPerMin;
                int drunkDecay = (int)State.DrunkDecayFrac;
                State.DrunkDecayFrac -= drunkDecay;
                State.Drunk = Mathf.Clamp(State.Drunk - drunkDecay, 0, 120);

                // ── Boredom rise ──────────────────────────────────────────────
                float boredomMult = GetBoredomMultiplier();
                State.BoredomRiseFrac += cfg.BoredomTick * boredomMult;
                int boredomAdd = (int)State.BoredomRiseFrac;
                State.BoredomRiseFrac -= boredomAdd;
                State.Boredom = Mathf.Clamp(State.Boredom + boredomAdd, 0, 100);

                // ── Conspiracy follower penalty ───────────────────────────────
                if (State.ConspiracyFollowing && State.CurrentZone != BarZone.Bathroom)
                    State.Boredom = Mathf.Clamp(State.Boredom + 2, 0, 100);

                // ── Regular friend passive ────────────────────────────────────
                var regular = State.Patrons.FirstOrDefault(p => p.Id == PatronId.Regular && p.FriendTier >= 2);
                if (regular != null)
                    State.Boredom = Mathf.Clamp(State.Boredom - 2, 0, 100);

                // ── Bathroom cooldown ─────────────────────────────────────────
                if (State.BathroomCooldown > 0) State.BathroomCooldown--;

                // ── Buyer passive drinks ──────────────────────────────────────
                if (State.BuyerActive)
                {
                    State.BuyerDrinkTimer--;
                    if (State.BuyerDrinkTimer <= 0)
                    {
                        State.BuyerDrinkTimer = Rand(15, 25);
                        int hit = DrinkDrunkHit();
                        State.Drunk = Mathf.Clamp(State.Drunk + hit, 0, 120);
                        Log($"The Buyer slides you another drink. (+{hit} drunk)");
                    }
                }

                // ── Phase change ──────────────────────────────────────────────
                var phase = GetPhase();
                if (phase != State.LastPhase)
                {
                    State.LastPhase = phase;
                    State.EventsThisPhase = 0;
                    OnPhaseChanged?.Invoke(phase);
                    LogPhaseChange(phase);
                }
            }

            CheckEndConditions();
            NotifyStateChanged();
        }

        private float GetBoredomMultiplier()
        {
            // iPod reduces boredom tick
            var ipod = State.Inventory.FirstOrDefault(i => i.Mechanic == ItemMechanic.BoredomSlow);
            return ipod != null ? ipod.Value : 1f;
        }

        private void LogPhaseChange(NightPhase phase)
        {
            var msgs = new Dictionary<NightPhase, string>
            {
                { NightPhase.WarmingUp,   "--- The bar starts filling up. ---" },
                { NightPhase.PeakHours,   "--- Peak hours. It's packed in here. ---" },
                { NightPhase.LastCall,    "--- LAST CALL! The bartender shouts it. ---" },
                { NightPhase.ClosingTime, "--- Lights on. 2:00 AM. Time to go. ---" },
            };
            if (msgs.TryGetValue(phase, out var msg)) Log(msg);
        }

        // ═════════════════════════════════════════════════════════════════════
        // END CONDITIONS
        // ═════════════════════════════════════════════════════════════════════

        private void CheckEndConditions()
        {
            if (State.NightEnded) return;

            // Barf out
            if (State.Drunk >= Tuning.DrunkBarfThreshold)
            {
                // Mustache save
                var mustache = State.Inventory.FirstOrDefault(i => i.Mechanic == ItemMechanic.Mustache);
                if (mustache != null && !State.MustacheUsed)
                {
                    State.MustacheUsed = true;
                    State.Inventory.Remove(mustache);
                    State.Drunk = 92;
                    Log("About to barf... the fake mustache saves you! 'First offense.' It falls off.");
                    return;
                }
                TriggerEnding(EndingType.BarfedOut);
                return;
            }

            // Bored out
            if (State.Boredom >= Tuning.BoredomMaxValue)
            {
                TriggerEnding(EndingType.LeftEarly);
                return;
            }

            // 2:00 AM
            if (State.GameMinute >= Tuning.NightEndMinute)
            {
                var crier = State.Patrons.FirstOrDefault(p => p.Id == PatronId.Crier && p.FriendTier >= 2);
                if (crier != null)
                {
                    Log("2:00 AM. The Crier walks up. \"I got you. Give me your keys.\"");
                    Log("You fall asleep in the passenger seat. You wake up at home. Safe.");
                    TriggerEnding(EndingType.CrierDrove);
                }
                else
                {
                    Log($"2:00 AM. Time to drive home. Drunk level: {(int)State.Drunk}");
                    _timerPaused = true;
                    DriveManager.Instance?.BeginDrivingMinigame(State);
                }
            }
        }

        private void TriggerEnding(EndingType ending)
        {
            if (State.NightEnded) return;
            State.NightEnded = true;
            State.Ending = ending;
            _timerPaused = true;

            int score = ScoreCalculator.Calculate(State, Campaign);
            OnNightEnded?.Invoke(ending, score);

            bool survived = ending == EndingType.MadeItHome ||
                            ending == EndingType.CrierDrove ||
                            ending == EndingType.RanHome;
            if (survived) HandleSurvivedNight();
        }

        private void HandleSurvivedNight()
        {
            // Save state for next night
            Campaign.SavedInventory = new List<ItemDefinition>(State.Inventory);
            Campaign.SavedFriendships.Clear();
            foreach (var p in State.Patrons)
                Campaign.SavedFriendships[p.Id] = p;
            Campaign.NightsCompleted++;
            Campaign.TotalNightsSurvived++;
            Campaign.TotalClawWins += State.ClawWins;

            OnMorningAfter?.Invoke();
        }

        public void AdvanceToNextNight()
        {
            Campaign.CurrentDayIndex++;
            StartNewNight();
        }

        public void RestartCampaign()
        {
            Campaign = new CampaignState();
            StartNewNight();
        }

        // ═════════════════════════════════════════════════════════════════════
        // ZONE ACTIONS
        // ═════════════════════════════════════════════════════════════════════

        public void EnterZone(BarZone zone)
        {
            State.CurrentZone = zone;
            AdvanceTime(Rand(2, 4));
            Log($"You walk over to the {zone}.");
            MaybeFireRandomEvent();
            NotifyStateChanged();
        }

        // ── Bar counter ───────────────────────────────────────────────────────
        public void OrderDrink()
        {
            int hit = DrinkDrunkHit();
            if (State.JerkyCharges > 0) { hit = Mathf.RoundToInt(hit * 0.6f); State.JerkyCharges--; }
            State.Drunk   = Mathf.Clamp(State.Drunk + hit, 0, 120);
            State.Boredom = Mathf.Clamp(State.Boredom - 8, 0, 100);
            var phase = GetPhase();
            AdvanceTime(phase == NightPhase.PeakHours ? Rand(6, 10) : Rand(3, 5));
            Log($"You order a drink. (+{hit} drunk, -8 boredom)");
        }

        public void OrderFood()
        {
            State.Drunk = Mathf.Clamp(State.Drunk - 8, 0, 120);
            State.JerkyCharges += 2;
            AdvanceTime(Rand(4, 6));
            Log("Bar snacks. Greasy, salty, perfect. (-8 drunk, slows next 2 drinks)");
        }

        public void OrderWater()
        {
            State.Drunk = Mathf.Clamp(State.Drunk - 10, 0, 120);
            AdvanceTime(3);
            Log("Water. Responsible. Boring. (-10 drunk)");
        }

        public void UseTicket()
        {
            if (State.DrinkTickets <= 0) return;
            State.DrinkTickets--;
            int hit = DrinkDrunkHit();
            State.Drunk   = Mathf.Clamp(State.Drunk + hit, 0, 120);
            State.Boredom = Mathf.Clamp(State.Boredom - 8, 0, 100);
            AdvanceTime(2);
            Log($"Drink ticket redeemed. (+{hit} drunk, -8 boredom)");
        }

        public void UseGoldenTicket()
        {
            if (State.GoldenTicketUsed) return;
            State.GoldenTicketUsed = true;
            var gt = State.Inventory.FirstOrDefault(i => i.Mechanic == ItemMechanic.GoldenTicket);
            if (gt != null) State.Inventory.Remove(gt);
            State.Drunk   = Mathf.Clamp(State.Drunk + 10, 0, 120);
            State.Boredom = Mathf.Clamp(State.Boredom - 15, 0, 100);
            AdvanceTime(3);
            Log("Golden ticket redeemed. Weird cocktail. (+10 drunk, -15 boredom)");
        }

        // ── Pool ──────────────────────────────────────────────────────────────
        public void PlayPool()
        {
            float foamMult = FoamFingerMult();
            int drunkDrop = Rand(5, 8);
            State.Drunk = Mathf.Clamp(State.Drunk - drunkDrop, 0, 120);
            int roll = Rand(1, 100) - Mathf.Max(0, (int)State.Drunk - 50);
            if (roll > 40)
            {
                int drop = Mathf.RoundToInt(4 * foamMult);
                State.Boredom = Mathf.Clamp(State.Boredom - drop, 0, 100);
                Log($"Nice game of pool. (-{drunkDrop} drunk, -{drop} boredom)");
            }
            else Log($"Rough game. Scratched twice. (-{drunkDrop} drunk)");
            State.ActivitiesDone++;
            AdvanceTime(Rand(10, 15));
        }

        // ── Darts ─────────────────────────────────────────────────────────────
        public void PlayDarts()
        {
            float foamMult = FoamFingerMult();
            int drunkDrop  = 5;
            int boredomDrop = Mathf.RoundToInt(4 * foamMult);
            State.Drunk   = Mathf.Clamp(State.Drunk - drunkDrop, 0, 120);
            State.Boredom = Mathf.Clamp(State.Boredom - boredomDrop, 0, 100);
            bool hit = Rand(1, 100) - Mathf.Max(0, (int)State.Drunk - 50) > 30;
            Log(hit ? $"Solid throw! (-{drunkDrop} drunk, -{boredomDrop} boredom)"
                    : $"You hit the wall. (-{drunkDrop} drunk, -{boredomDrop} boredom anyway)");
            State.ActivitiesDone++;
            AdvanceTime(Rand(5, 8));
        }

        // ── Jukebox ───────────────────────────────────────────────────────────
        public void PlayJukebox()
        {
            int drop = Mathf.RoundToInt(6 * FoamFingerMult());
            State.Boredom = Mathf.Clamp(State.Boredom - drop, 0, 100);
            Log($"You pick a song. Energy shifts. (-{drop} boredom)");
            State.ActivitiesDone++;
            AdvanceTime(Rand(3, 5));
        }

        // ── Bathroom ──────────────────────────────────────────────────────────
        public void EnterBathroom()
        {
            // Base: splash water
            State.Drunk = Mathf.Clamp(State.Drunk - 5, 0, 120);
            State.BathroomCooldown = Rand(Tuning.BathCooldownMin, Tuning.BathCooldownMax);
            Log("You splash cold water on your face. (-5 drunk)");
            AdvanceTime(Rand(3, 5));

            // Roll for event
            BathroomEvent evt = RollBathroomEvent();
            if (evt != BathroomEvent.None)
            {
                State.LastBathroomEvent = evt;
                OnBathroomEvent?.Invoke(evt);
                // Resolution is handled by UI calling ResolveBathroomEvent()
            }
        }

        private BathroomEvent RollBathroomEvent()
        {
            float rareMod = 0;
            if (State.Inventory.Any(i => i.Mechanic == ItemMechanic.Flashlight)) rareMod += 15;
            var storyteller = State.Patrons.FirstOrDefault(p => p.Id == PatronId.Storyteller && p.FriendTier >= 2);
            if (storyteller != null) rareMod += 10;

            int r = Rand(1, 100);
            if (r <= 25 + rareMod && State.LastBathroomEvent != BathroomEvent.Ladder)
                return BathroomEvent.Ladder;
            if (r <= 40 + rareMod && State.LastBathroomEvent != BathroomEvent.Snowblower
                                  && State.SnowblowerUsedCount < 2)
                return BathroomEvent.Snowblower;
            if (r <= 50) return BathroomEvent.BrokenMirror;
            if (r <= 60) return BathroomEvent.PassedOutPatron;
            return BathroomEvent.None;
        }

        // Called by UI after player makes a bathroom event choice
        public void ResolveBathroomEvent(BathroomEvent evt, bool choiceA)
        {
            State.BathroomEventsFound.Add(evt);
            switch (evt)
            {
                case BathroomEvent.Ladder:
                    float failChance = Mathf.Max(0, State.Drunk - 40) * 1.5f;
                    bool fell = Rand(1, 100) < failChance;
                    if (!fell)
                    {
                        State.Drunk   = Mathf.Clamp(State.Drunk - 12, 0, 120);
                        State.Boredom = Mathf.Clamp(State.Boredom - 8, 0, 100);
                        Log("You climb the ladder! View: ceiling tile. (-12 drunk, -8 boredom)");
                        if (State.Drunk > 70) { State.StylePoints += 30; Log("Style! Bathroom ladder while hammered. (+30)"); }
                    }
                    else
                    {
                        State.Drunk = Mathf.Clamp(State.Drunk + 3, 0, 120);
                        Log("You fall off the ladder. Floor groaning. (+3 drunk)");
                        AdvanceTime(Rand(8, 12));
                    }
                    AdvanceTime(Rand(4, 6));
                    break;

                case BathroomEvent.Snowblower:
                    if (choiceA) // pee in gas tank
                    {
                        State.SnowblowerUsedCount++;
                        State.Drunk   = Mathf.Clamp(State.Drunk - 10, 0, 120);
                        State.Boredom = Mathf.Clamp(State.Boredom - 12, 0, 100);
                        State.StylePoints += 25;
                        Log("You pop the gas cap and commit. Deeply satisfying. (-10 drunk, -12 boredom, +25 style)");
                        AdvanceTime(Rand(4, 6));
                    }
                    else Log("You decide not to risk it.");
                    break;

                case BathroomEvent.BrokenMirror:
                    State.Drunk   = Mathf.Clamp(State.Drunk - 3, 0, 120);
                    State.Boredom = Mathf.Clamp(State.Boredom - 4, 0, 100);
                    Log("Broken mirror. You stare at yourself. (-3 drunk, -4 boredom)");
                    break;

                case BathroomEvent.PassedOutPatron:
                    if (choiceA) // steal ticket
                    {
                        State.DrinkTickets++;
                        Log("You pocket the ticket. (+1 drink ticket)");
                        AdvanceTime(2);
                    }
                    else // help them up
                    {
                        State.Boredom = Mathf.Clamp(State.Boredom - 5, 0, 100);
                        Log("You help them up. Feel decent about yourself. (-5 boredom)");
                        AdvanceTime(Rand(5, 8));
                    }
                    break;
            }
            CheckEndConditions();
            NotifyStateChanged();
        }

        // ── Slot machine ──────────────────────────────────────────────────────
        public void PullSlots()
        {
            AdvanceTime(3);
            State.Boredom = Mathf.Clamp(State.Boredom - 3, 0, 100);
            State.ActivitiesDone++;
            int r = Rand(1, 100);
            if (r <= 45)      Log("No match. Near-miss. (-3 boredom)");
            else if (r <= 70) { State.DrinkTickets += 1; Log("Two beers! +1 drink ticket."); }
            else if (r <= 85) { State.DrinkTickets += 2; Log("Two shots! +2 drink tickets."); }
            else if (r <= 93) { State.DrinkTickets += 3; State.JerkyCharges += 2; Log("THREE BEERS! 3 tickets + snack voucher!"); }
            else if (r <= 97) { State.DrinkTickets += 5; Log("THREE SHOTS! 5 drink tickets!"); }
            else if (r <= 99)
            {
                State.SlotJackpot = true;
                int hit = DrinkDrunkHit() * 2;
                State.Drunk   = Mathf.Clamp(State.Drunk + hit, 0, 120);
                State.Boredom = Mathf.Clamp(State.Boredom - 15, 0, 100);
                State.StylePoints += 40;
                Log($"JACKPOT! THREE SEVENS! Free tab! (+{hit} drunk, -15 boredom, +40 style)");
            }
            else { State.BathroomCooldown = 0; Log("Three barfs! Sympathy prize: bathroom cooldown reset."); }

            CheckEndConditions();
            NotifyStateChanged();
        }

        // ── Claw machine ──────────────────────────────────────────────────────
        public void TryClaw()
        {
            AdvanceTime(5);
            State.ActivitiesDone++;

            bool grabbed = (float)_rng.NextDouble() < Tuning.ClawGrabRate;
            if (!grabbed) { Log("The claw closes on nothing. Classic."); NotifyStateChanged(); return; }
            bool held    = (float)_rng.NextDouble() < Tuning.ClawHoldRate;
            if (!held)    { Log("It grabbed something! Lifting... and drops it. Devastating."); NotifyStateChanged(); return; }

            var prize = ItemDatabase.RandomPrize(_rng);
            State.ClawWins++;
            Log($"YOU WON: {prize.DisplayName}! {prize.Description}");
            Log($"Effect: {prize.Mechanic}");

            if (State.Inventory.Count >= Tuning.MaxInventorySlots)
                OnInventoryFull?.Invoke(prize, new List<ItemDefinition>(State.Inventory));
            else
                GrantItem(prize);

            NotifyStateChanged();
        }

        // Called by UI after player chooses which item to drop
        public void ResolveInventoryFull(ItemDefinition newItem, int dropIndex)
        {
            if (dropIndex >= 0 && dropIndex < State.Inventory.Count)
            {
                Log($"You drop the {State.Inventory[dropIndex].DisplayName} and pocket the {newItem.DisplayName}.");
                State.Inventory.RemoveAt(dropIndex);
                GrantItem(newItem);
            }
            else Log($"You leave the {newItem.DisplayName} on the sticky bar floor.");
            NotifyStateChanged();
        }

        private void GrantItem(ItemDefinition item)
        {
            // Only one iPod at a time — replace if better
            if (item.Mechanic == ItemMechanic.BoredomSlow)
            {
                var existing = State.Inventory.FirstOrDefault(i => i.Mechanic == ItemMechanic.BoredomSlow);
                if (existing != null)
                {
                    if (item.Value < existing.Value) // lower value = slower boredom = better
                        State.Inventory.Remove(existing);
                    else { Log("You already have a better iPod."); return; }
                }
            }
            State.Inventory.Add(item);
            if (item.Mechanic == ItemMechanic.Jerky) State.JerkyCharges += (int)item.Value;
        }

        // ── Use Monstor ───────────────────────────────────────────────────────
        public void UseMonstor()
        {
            var m = State.Inventory.FirstOrDefault(i => i.Mechanic == ItemMechanic.Monstor);
            if (m == null) return;
            State.Inventory.Remove(m);
            State.Drunk = Mathf.Clamp(State.Drunk - 8, 0, 120);
            Log("Monstor cracked open. Tastes like battery acid and ambition. (-8 drunk)");
            NotifyStateChanged();
        }

        // ═════════════════════════════════════════════════════════════════════
        // PATRON CONVERSATIONS
        // ═════════════════════════════════════════════════════════════════════

        public void TalkToPatron(PatronId patronId)
        {
            var p = State.Patrons.FirstOrDefault(x => x.Id == patronId);
            if (p == null || p.FriendTier >= 3) return;

            float bearBoost = (State.Inventory.Any(i => i.Mechanic == ItemMechanic.Bear) && !p.GaveGift) ? 1.5f : 1f;
            int baseGain;

            AdvanceTime(Rand(5, 10));
            State.ConversationsTotal++;

            switch (p.Id)
            {
                case PatronId.Regular:
                    baseGain = 15;
                    State.Boredom = Mathf.Clamp(State.Boredom - 6, 0, 100);
                    Log("Easy chat with the Regular. They mention song B7 on the jukebox. (-6 boredom)");
                    break;
                case PatronId.Storyteller:
                    baseGain = 12;
                    State.Boredom = Mathf.Clamp(State.Boredom - 10, 0, 100);
                    Log("The Storyteller launches into an incredible ghost story. (-10 boredom)");
                    AdvanceTime(Rand(5, 8));
                    break;
                case PatronId.Buyer:
                    baseGain = 15;
                    int bHit = DrinkDrunkHit();
                    State.Drunk   = Mathf.Clamp(State.Drunk + bHit, 0, 120);
                    State.Boredom = Mathf.Clamp(State.Boredom - 6, 0, 100);
                    Log($"The Buyer orders before you sit. (+{bHit} drunk, -6 boredom)");
                    break;
                case PatronId.Crier:
                    baseGain = 18;
                    State.Boredom = Mathf.Clamp(State.Boredom - 4, 0, 100);
                    p.ConversationBeat++;
                    string[] crierLines =
                    {
                        "The Crier talks about their ex. It's heavy. You listen.",
                        "They open up more. Appreciate you sticking around.",
                        "They tell you about their childhood dog. You're both tearing up.",
                        "\"You're a good person. Let me drive you home tonight.\""
                    };
                    Log(crierLines[Mathf.Min(p.ConversationBeat - 1, 3)] + " (-4 boredom)");
                    break;
                case PatronId.OffDuty:
                    baseGain = 20;
                    State.Boredom = Mathf.Clamp(State.Boredom - 4, 0, 100);
                    Log("The Off-Duty Bartender shows you a drink trick. (-4 boredom)");
                    break;
                case PatronId.ConspiracyGuy:
                    baseGain = 10;
                    State.Boredom = Mathf.Clamp(State.Boredom - 8, 0, 100);
                    Log("Moon projection theory. Weirdly entertaining. (-8 boredom)");
                    if (p.Friendship >= 30 && !State.ConspiracyFollowing)
                    {
                        State.ConspiracyFollowing = true;
                        Log("He's following you now. +2 boredom/min to everything.");
                    }
                    break;
                case PatronId.Instigator:
                    baseGain = 12;
                    State.Boredom = Mathf.Clamp(State.Boredom - 6, 0, 100);
                    Log("Instigator's bear-fight story. Entertaining. (-6 boredom)");
                    break;
                default: baseGain = 10; break;
            }

            p.Friendship += Mathf.RoundToInt(baseGain * bearBoost);
            p.ConversationsCompleted++;
            UpdateFriendshipTier(p);
            CheckEndConditions();
            NotifyStateChanged();
        }

        public void GiveBearToPatron(PatronId patronId)
        {
            var p   = State.Patrons.FirstOrDefault(x => x.Id == patronId);
            var bear = State.Inventory.FirstOrDefault(i => i.Mechanic == ItemMechanic.Bear);
            if (p == null || bear == null || p.GaveGift) return;
            p.GaveGift = true;
            p.Friendship += 25;
            State.Inventory.Remove(bear);
            Log($"You give {p.DisplayName} the stuffed bear. They smile. (+25 friendship)");
            UpdateFriendshipTier(p);
            NotifyStateChanged();
        }

        public void ResolveArmWrestle(bool accepted, bool buyIn)
        {
            var p = State.Patrons.FirstOrDefault(x => x.Id == PatronId.Instigator);
            if (!accepted)
            {
                State.Boredom = Mathf.Clamp(State.Boredom + 5, 0, 100);
                Log("\"LAME.\" Awkward. (+5 boredom)");
                NotifyStateChanged(); return;
            }
            AdvanceTime(5);
            int drunkPenalty = Mathf.Max(0, (int)State.Drunk - 40);
            bool win = Rand(1, 100) - drunkPenalty > 40;
            if (win)
            {
                int drop = Mathf.RoundToInt(18 * FoamFingerMult());
                State.Boredom = Mathf.Clamp(State.Boredom - drop, 0, 100);
                if (p != null) p.Friendship += 20;
                Log($"You SLAM their arm down! (-{drop} boredom)");
                if (State.Drunk > 60) { State.StylePoints += 20; Log("Style! Arm wrestle while drunk. (+20)"); }
            }
            else
            {
                State.Drunk = Mathf.Clamp(State.Drunk + 15, 0, 120);
                if (p != null) p.Friendship += 10;
                Log("They crush you. Consolation shot. (+15 drunk)");
            }
            if (buyIn) { State.DrinkTickets++; if (p != null) p.Friendship += 5; Log("They honor the bet. (+1 ticket)"); }
            if (p != null) UpdateFriendshipTier(p);
            CheckEndConditions();
            NotifyStateChanged();
        }

        private void UpdateFriendshipTier(PatronState p)
        {
            int old = p.FriendTier;
            if (p.Friendship >= Tuning.FriendshipFriend && p.FriendTier < 2)
            {
                p.FriendTier = 2;
                State.PatronsBefriended++;
                var def = PatronDatabase.Get(p.Id);
                Log($"{p.DisplayName} is your friend! Perk: {def?.Perk}");
                ApplyPatronPerk(p);
            }
            else if (p.Friendship >= Tuning.FriendshipAcquaintance && p.FriendTier < 1)
            {
                p.FriendTier = 1;
                Log($"{p.DisplayName} warms up to you. Acquaintances now.");
            }
            if (p.FriendTier != old) OnPatronFriendshipChanged?.Invoke(p);
        }

        private void ApplyPatronPerk(PatronState p)
        {
            switch (p.Id)
            {
                case PatronId.OffDuty:   State.OffDutyPerkActive = true; break;
                case PatronId.Buyer:     State.BuyerActive = true; State.BuyerDrinkTimer = Rand(15, 25); break;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // RANDOM MID-NIGHT EVENTS
        // ═════════════════════════════════════════════════════════════════════

        private void MaybeFireRandomEvent()
        {
            var phase = GetPhase();
            if (phase == NightPhase.ClosingTime) return;
            if (phase != State.LastPhase)   { State.EventsThisPhase = 0; State.LastPhase = phase; }
            if (State.EventsThisPhase >= Tuning.MaxEventsForPhase(phase)) return;
            if ((float)_rng.NextDouble() > Tuning.RandomEventChance) return;

            State.EventsThisPhase++;
            var available = new List<RandomEventType>();
            if (phase == NightPhase.PeakHours || phase == NightPhase.WarmingUp) available.Add(RandomEventType.Bump);
            if (phase != NightPhase.EarlyBird) available.Add(RandomEventType.MysteryShot);
            if (phase == NightPhase.LastCall && !State.CardDeclined) available.Add(RandomEventType.CardDeclined);
            if (phase == NightPhase.PeakHours || phase == NightPhase.LastCall) available.Add(RandomEventType.LightsFlicker);
            if (available.Count == 0) return;

            var evt = available[Rand(0, available.Count - 1)];
            OnRandomEvent?.Invoke(evt);
            // Auto-resolve flicker; others are resolved by UI calling ResolveRandomEvent()
            if (evt == RandomEventType.LightsFlicker) ResolveRandomEvent(evt, true);
        }

        public void ResolveRandomEvent(RandomEventType evt, bool choiceA)
        {
            switch (evt)
            {
                case RandomEventType.Bump:
                    int bumpDrunk = Rand(3, 5);
                    State.Drunk = Mathf.Clamp(State.Drunk + bumpDrunk, 0, 120);
                    Log($"Someone spills their drink on you. (+{bumpDrunk} drunk)");
                    if (!choiceA) // Watch it
                    {
                        if (Rand(1, 2) == 1) { State.Drunk = Mathf.Clamp(State.Drunk + DrinkDrunkHit(), 0, 120); Log("\"Sorry, let me buy you one.\""); }
                        else { Log("They get aggressive. Bouncer intervenes. 5 min wasted."); AdvanceTime(5); }
                    }
                    break;
                case RandomEventType.MysteryShot:
                    if (choiceA)
                    {
                        int r = Rand(1, 3);
                        if (r == 1) { State.Drunk = Mathf.Clamp(State.Drunk + 20, 0, 120); State.Boredom = Mathf.Clamp(State.Boredom - 5, 0, 100); Log("STRONG. Burns. Tastes amazing. (+20 drunk, -5 boredom)"); }
                        else if (r == 2) { State.Drunk = Mathf.Clamp(State.Drunk + 8, 0, 120); State.Boredom = Mathf.Clamp(State.Boredom + 5, 0, 100); Log("Disgusting. You gag. (+8 drunk, +5 boredom)"); }
                        else { State.Drunk = Mathf.Clamp(State.Drunk + 12, 0, 120); Log("Not bad. Bartender nods. (+12 drunk)"); }
                        AdvanceTime(2);
                    }
                    else Log("You decline. The bartender remembers.");
                    break;
                case RandomEventType.CardDeclined:
                    State.CardDeclined = true;
                    if (choiceA) // ask someone
                    {
                        var buyer = State.Patrons.FirstOrDefault(p => p.Id == PatronId.Buyer && p.FriendTier >= 1);
                        if (buyer != null) { buyer.Friendship += 10; Log("The Buyer covers you happily."); }
                        else { var friend = State.Patrons.FirstOrDefault(p => p.FriendTier >= 1); if (friend != null) { friend.Friendship -= 10; Log($"{friend.DisplayName} covers you reluctantly."); } else { State.Drunk = Mathf.Clamp(State.Drunk - 10, 0, 120); AdvanceTime(15); Log("Nobody. You wash dishes. (-10 drunk)"); } }
                    }
                    else // check pocket
                    {
                        if (State.DrinkTickets > 0) { State.DrinkTickets--; Log("Found a ticket. Crisis averted."); }
                        else { State.Drunk = Mathf.Clamp(State.Drunk - 10, 0, 120); AdvanceTime(15); Log("Nothing. Dishes it is. (-10 drunk)"); }
                    }
                    break;
                case RandomEventType.LightsFlicker:
                    int shift = Rand(-5, 5);
                    State.Drunk = Mathf.Clamp(State.Drunk + shift, 0, 120);
                    Log($"Lights flicker. Two seconds of dark. Something feels {(shift > 0 ? "wobblier" : shift < 0 ? "clearer" : "the same")}. ({(shift >= 0 ? "+" : "")}{shift} drunk)");
                    break;
            }
            CheckEndConditions();
            NotifyStateChanged();
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════════════════

        public NightPhase GetPhase()
        {
            foreach (var pc in Tuning.Phases)
                if (State.GameMinute >= pc.StartMinute && State.GameMinute < pc.EndMinute)
                    return pc.Phase;
            return NightPhase.ClosingTime;
        }

        public DayConfig GetDayConfig() => Tuning.Days[Campaign.CurrentDayIndex % 7];

        private int DrinkDrunkHit()
        {
            int base_ = GetDayConfig().DrinkBase;
            return State.OffDutyPerkActive ? Mathf.Max(10, base_ - 4) : base_;
        }

        private float FoamFingerMult() =>
            State.Inventory.Any(i => i.Mechanic == ItemMechanic.FoamFinger) ? 1.3f : 1f;

        private int Rand(int min, int max) => _rng.Next(min, max + 1);

        public void PauseTimer()  => _timerPaused = true;
        public void ResumeTimer() => _timerPaused = false;

        private void NotifyStateChanged() => OnStateChanged?.Invoke(State);
        private void Log(string msg)      => OnLogLine?.Invoke(msg);
    }
}
