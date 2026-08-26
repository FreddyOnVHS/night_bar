// DialogueData.cs
// All data structures for the dialogue system.
// No MonoBehaviour — pure C# data containers.

using System.Collections.Generic;

namespace NightAtTheBar.Dialogue
{
    // ── Friendship tiers (both directions) ───────────────────────────────────
    public enum FriendshipTier
    {
        Fight       = -6,   // -100
        Antagonist  = -5,   // -60 to -99
        Hostile     = -4,   // -30 to -59
        Cool        = -3,   // -1 to -29
        Stranger    =  0,   //  0 to  29
        Acquaintance=  1,   // 30 to 59
        Friend      =  2,   // 60+
    }

    public static class FriendshipTierHelper
    {
        public static FriendshipTier GetTier(int friendship) => friendship switch
        {
            >= 60               => FriendshipTier.Friend,
            >= 30               => FriendshipTier.Acquaintance,
            >= 5                => FriendshipTier.Stranger,
            >= -1               => FriendshipTier.Stranger,
            >= -29              => FriendshipTier.Cool,
            >= -59              => FriendshipTier.Hostile,
            >= -99              => FriendshipTier.Antagonist,
            _                   => FriendshipTier.Fight,
        };
    }

    // ── Conditions that gate beats and choices ────────────────────────────────
    public enum ConditionType
    {
        None,
        DrunkAbove,
        DrunkBelow,
        FriendshipAbove,
        FriendshipBelow,
        ArcCompleted,
        ArcNotCompleted,
        PatronPresent,
        PatronFriendshipAbove,
        TimeAfter,          // game-minute
        TimeBefore,
        DayIndex,           // 0=Mon…6=Sun
        InventoryHas,
        NightNumber,        // nights survived
        ConversationBeat,
        RandomChance,       // 0-100
    }

    public class DialogueCondition
    {
        public ConditionType Type;
        public float         FloatValue;
        public string        StringValue; // arc id, patron id, item id
        public int           IntValue;
    }

    // ── Consequences of choosing an option ───────────────────────────────────
    public enum ConsequenceType
    {
        FriendshipDelta,        // + or - friendship with this patron
        FriendshipDeltaOther,   // + or - friendship with another patron
        DrunkDelta,
        BoredomDelta,
        StyleDelta,
        GrantItem,
        RemoveItem,
        GrantTicket,
        SetFlag,                // generic bool flag on game state
        AdvanceTime,
        TriggerEvent,           // fire a GameEvent by name
        UnlockArc,
        LockArc,
        TriggerFight,
        TriggerEject,
        EndConversation,
        LogLine,                // push a string to the game log
    }

    public class DialogueConsequence
    {
        public ConsequenceType Type;
        public float           FloatValue;
        public string          StringValue;
        public int             IntValue;
    }

    // ── A single choice within a beat ────────────────────────────────────────
    public class DialogueChoice
    {
        public string                    Id;
        public string                    Label;          // shown to player
        public string                    DrunkLabel;     // shown if drunk > 60 (slurred)
        public List<DialogueCondition>   Conditions  = new(); // must all pass to show
        public List<DialogueConsequence> Consequences = new();
        public string                    NextBeatId;     // null = end conversation
        public bool                      IsDeflect;      // triggers deflect penalty if patron notices
    }

    // ── A single beat (one back-and-forth exchange) ───────────────────────────
    public class DialogueBeat
    {
        public string                  Id;
        public string                  PatronLine;       // what the patron says
        public string                  PatronLineDrunk;  // variant if player is very drunk
        public List<DialogueCondition> EntryConditions = new(); // must pass to reach this beat
        public List<DialogueChoice>    Choices         = new();
        public bool                    AutoAdvance;      // no player choice, fires first valid choice
        public float                   AutoAdvanceDelay; // game-minutes before auto-firing
    }

    // ── An arc = a full conversation (multiple beats) ─────────────────────────
    public class DialogueArc
    {
        public string                  Id;
        public PatronId                PatronId;
        public string                  DisplayName;      // e.g. "Used to be mine"
        public int                     ArcIndex;         // order within patron arcs
        public FriendshipTier          MinTier;          // minimum tier to access
        public FriendshipTier          MaxTier;          // maximum tier (negative arcs cap here)
        public List<DialogueCondition> UnlockConditions = new();
        public List<DialogueBeat>      Beats            = new();
        public string                  FirstBeatId;

        // Friendship effects applied when arc STARTS (e.g. patron is pleased you came back)
        public int OnStartFriendshipDelta;

        // Time cost of the full arc in game-minutes
        public int TimeMin = 5;
        public int TimeMax = 10;

        // Whether this arc can be replayed or is one-shot
        public bool OneShot = true;

        // Tags for special handling
        public bool IsNegativeArc;
        public bool IsFightArc;
        public bool IsRecoveryArc;
    }

    // ── Patron's full dialogue definition ────────────────────────────────────
    public class PatronDialogue
    {
        public PatronId          PatronId;
        public int               StartingFriendship;
        public List<DialogueArc> Arcs = new();

        // Passive log lines when patron is at various tiers (fires on zone entry)
        public Dictionary<FriendshipTier, List<string>> PassiveLines = new();

        // Lines when player is too drunk to talk to this patron
        public string DrunkRejectionLine;

        // Lines when patron is at Cool/Hostile/Antagonist and player approaches
        public string CoolGreeting;
        public string HostileGreeting;
        public string AntagonistGreeting;

        // Fight lines
        public string FightTriggerLine;     // what they say right before it kicks off
        public string FightDeEscalateLine;  // what they say if you back down
        public string PostFightLine;        // next night, first thing they say
    }

    // ── Runtime state per patron per night ───────────────────────────────────
    public class PatronConversationState
    {
        public PatronId       PatronId;
        public int            Friendship;       // -100 to +100
        public FriendshipTier CurrentTier => FriendshipTierHelper.GetTier(Friendship);
        public HashSet<string> CompletedArcIds  = new();
        public HashSet<string> CompletedBeatIds = new();
        public string         CurrentArcId;
        public string         CurrentBeatId;
        public bool           InConversation;
        public bool           FightTriggered;
        public bool           EjectedTonight;
        public int            ConversationsCompletedTonight;
        public bool           ArmWrestleOfferedTonight;
        public bool           GaveGift;

        // Flags set by dialogue consequences
        public Dictionary<string, bool>   Flags   = new();
        public Dictionary<string, int>    IntData = new();
    }
}
