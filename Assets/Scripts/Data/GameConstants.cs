// GameConstants.cs
// Night at the Bar — static data, enums, and tuning values.
// Edit tuning values here; nothing else needs to change.

namespace NightAtTheBar
{
    // ── Meters ───────────────────────────────────────────────────────────────
    public enum DrunkZone { Bored, SweetSpot, BarfRisk }

    // ── Zones ────────────────────────────────────────────────────────────────
    public enum BarZone
    {
        Entrance, BarCounter, KaraokeStage, PoolTable,
        Darts, Jukebox, Bathroom, SlotMachine, ClawMachine,
        BoothSeating, ParkingLot
    }

    // ── Night phases ─────────────────────────────────────────────────────────
    public enum NightPhase { EarlyBird, WarmingUp, PeakHours, LastCall, ClosingTime }

    // ── Patron IDs ───────────────────────────────────────────────────────────
    public enum PatronId { Regular, Storyteller, Buyer, Crier, Instigator, OffDuty, ConspiracyGuy }

    // ── Endings ──────────────────────────────────────────────────────────────
    public enum EndingType
    {
        MadeItHome, RanHome, CrierDrove,
        BarfedOut, LeftEarly,
        Crashed, ArrestedPullOver, ArrestedChase
    }

    // ── Inventory item mechanics ─────────────────────────────────────────────
    public enum ItemMechanic
    {
        BoredomSlow, Bear, Sunglasses, Monstor,
        FoamFinger, Jerky, Flashlight,
        GoldenTicket, Mustache
    }

    // ── Bathroom events ──────────────────────────────────────────────────────
    public enum BathroomEvent { None, Ladder, Snowblower, BrokenMirror, PassedOutPatron }

    // ── Driving obstacles ────────────────────────────────────────────────────
    public enum DriveObstacle
    {
        RedLight, ParkedCar, HeadlightGlare,
        HighwayMerge, Pothole,
        TrashCan, Deer, Pedestrian, ParkedCop
    }

    // ── Driving choices ──────────────────────────────────────────────────────
    public enum DriveChoice { A, B }   // generic two-option for all obstacles

    // ── Random mid-night events ──────────────────────────────────────────────
    public enum RandomEventType { Bump, MysteryShot, CardDeclined, LightsFlicker }

    // ── Karaoke songs ────────────────────────────────────────────────────────
    public enum KaraokeDifficulty { Easy, Medium, Hard }

    // =========================================================================
    // TUNING — change numbers here, never in game logic scripts
    // =========================================================================
    public static class Tuning
    {
        // Time
        public const int NightStartMinute  = 540;   // 9:00 PM
        public const int NightEndMinute    = 840;   // 2:00 AM
        public const float RealSecsPerGameMin = 2f; // 1 game-minute = 2 real seconds

        // Drunk meter
        public const int DrunkBoreZoneMax   = 24;
        public const int DrunkSweetSpotMax  = 74;
        public const int DrunkBarfThreshold = 100;
        public const int DrunkResetValue    = 15;
        public const float DrunkDecayPerMin = 0.5f; // fractional, per game-minute

        // Boredom meter
        public const int BoredomResetValue  = 30;
        public const int BoredomMaxValue    = 100;

        // Inventory
        public const int MaxInventorySlots = 3;

        // Bathroom cooldown range (game-minutes)
        public const int BathCooldownMin = 8;
        public const int BathCooldownMax = 15;

        // Claw machine
        public const float ClawGrabRate  = 0.30f;
        public const float ClawHoldRate  = 0.60f;

        // Patron friendship tiers
        public const int FriendshipAcquaintance = 30;
        public const int FriendshipFriend        = 60;

        // Random event chance per zone transition
        public const float RandomEventChance = 0.15f;

        // Day difficulty table
        // Index 0 = Monday … 6 = Sunday
        public static readonly DayConfig[] Days = new DayConfig[]
        {
            new DayConfig("Monday",    drinkBase:15, boredomTick:1.5f, wildEvents:false),
            new DayConfig("Tuesday",   drinkBase:16, boredomTick:1.7f, wildEvents:false),
            new DayConfig("Wednesday", drinkBase:17, boredomTick:2.0f, wildEvents:false),
            new DayConfig("Thursday",  drinkBase:18, boredomTick:2.3f, wildEvents:true),
            new DayConfig("Friday",    drinkBase:20, boredomTick:2.5f, wildEvents:true),
            new DayConfig("Saturday",  drinkBase:22, boredomTick:3.0f, wildEvents:true),
            new DayConfig("Sunday",    drinkBase:18, boredomTick:1.2f, wildEvents:false),
        };

        // Phase time boundaries (game-minutes)
        public static readonly PhaseConfig[] Phases = new PhaseConfig[]
        {
            new PhaseConfig(NightPhase.EarlyBird,   540, 630),
            new PhaseConfig(NightPhase.WarmingUp,   630, 720),
            new PhaseConfig(NightPhase.PeakHours,   720, 780),
            new PhaseConfig(NightPhase.LastCall,    780, 825),
            new PhaseConfig(NightPhase.ClosingTime, 825, 840),
        };

        // Max random events per phase
        public static int MaxEventsForPhase(NightPhase p) => p switch
        {
            NightPhase.EarlyBird   => 1,
            NightPhase.WarmingUp   => 2,
            NightPhase.PeakHours   => 4,
            NightPhase.LastCall    => 2,
            _                      => 0,
        };
    }

    // ── Simple data structs (no MonoBehaviour) ───────────────────────────────
    public struct DayConfig
    {
        public string Name;
        public int    DrinkBase;
        public float  BoredomTick;
        public bool   WildEvents;
        public DayConfig(string name, int drinkBase, float boredomTick, bool wildEvents)
        { Name = name; DrinkBase = drinkBase; BoredomTick = boredomTick; WildEvents = wildEvents; }
    }

    public struct PhaseConfig
    {
        public NightPhase Phase;
        public int StartMinute;
        public int EndMinute;
        public PhaseConfig(NightPhase phase, int start, int end)
        { Phase = phase; StartMinute = start; EndMinute = end; }
    }
}
