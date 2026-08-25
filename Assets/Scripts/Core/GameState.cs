// GameState.cs
// Pure data container for one night's runtime state.
// No Unity dependencies. Serialise this to save/load.

using System.Collections.Generic;

namespace NightAtTheBar
{
    public class PatronState
    {
        public PatronId Id;
        public string   DisplayName;
        public int      Friendship;
        public int      FriendTier;       // 0=Stranger, 1=Acquaintance, 2=Friend
        public int      ConversationBeat; // progress within current conversation (0-3)
        public int      ConversationsCompleted;
        public bool     GaveGift;         // received stuffed bear
        public int      LeaveMinute;
    }

    public class GameState
    {
        // ── Meters ───────────────────────────────────────────────────────────
        public float Drunk    = 15f;
        public float Boredom  = 30f;

        // fractional accumulators (not visible to UI — internal only)
        public float DrunkDecayFrac   = 0f;
        public float BoredomRiseFrac  = 0f;

        // ── Time ─────────────────────────────────────────────────────────────
        public int   GameMinute = Tuning.NightStartMinute;  // 540 = 9:00 PM

        // ── Location ─────────────────────────────────────────────────────────
        public BarZone CurrentZone = BarZone.Entrance;

        // ── Inventory ────────────────────────────────────────────────────────
        public List<ItemDefinition> Inventory = new(Tuning.MaxInventorySlots);
        public int JerkyCharges   = 0;
        public bool GoldenTicketUsed = false;
        public bool MustacheUsed  = false;

        // ── Currency ─────────────────────────────────────────────────────────
        public int DrinkTickets = 0;

        // ── Patrons ──────────────────────────────────────────────────────────
        public List<PatronState> Patrons = new();

        // ── Patron-triggered flags ────────────────────────────────────────────
        public bool BuyerActive        = false;
        public int  BuyerDrinkTimer    = 0;      // game-minutes until next forced drink
        public bool OffDutyPerkActive  = false;  // drink cost -4
        public bool ConspiracyFollowing = false; // +2 boredom/min passively

        // ── Bathroom ─────────────────────────────────────────────────────────
        public int            BathroomCooldown = 0;  // game-minutes remaining
        public BathroomEvent  LastBathroomEvent = BathroomEvent.None;
        public HashSet<BathroomEvent> BathroomEventsFound = new();
        public int SnowblowerUsedCount = 0; // max 2 per night

        // ── Random event tracking ─────────────────────────────────────────────
        public NightPhase LastPhase       = NightPhase.EarlyBird;
        public int        EventsThisPhase = 0;
        public bool       CardDeclined    = false;
        public bool       ArmWrestleOffered = false;

        // ── Scoring accumulators ──────────────────────────────────────────────
        public float SweetSpotMinutes   = 0f;
        public float BestSweetStreak    = 0f;
        public float CurrentSweetStreak = 0f;
        public int   ActivitiesDone     = 0;
        public int   ClawWins           = 0;
        public bool  SlotJackpot        = false;
        public int   StylePoints        = 0;
        public int   ConversationsTotal = 0;
        public int   PatronsBefriended  = 0;

        // ── Driving ──────────────────────────────────────────────────────────
        public int DriveSegment     = 0;
        public int DriveHP          = 3;
        public int PoliceAttention  = 0;
        public int DriveScore       = 100;

        // ── Night outcome ─────────────────────────────────────────────────────
        public bool       NightEnded = false;
        public EndingType Ending;
    }

    // Persists across nights (campaign save)
    public class CampaignState
    {
        public int  CurrentDayIndex = 0;  // 0=Monday … 6=Sunday
        public int  NightsCompleted = 0;

        // Persisted between nights
        public List<ItemDefinition>             SavedInventory    = new();
        public Dictionary<PatronId, PatronState> SavedFriendships = new();

        // All-time stats
        public int  TotalClawWins     = 0;
        public int  TotalNightsSurvived = 0;
    }
}
