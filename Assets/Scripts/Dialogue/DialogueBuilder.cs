// DialogueBuilder.cs
// Fluent builder API so patron dialogue files stay readable.
// Usage: ArcBuilder.Create(...).Beat(...).Choice(...).Build()

using System.Collections.Generic;

namespace NightAtTheBar.Dialogue
{
    // ── Arc builder ───────────────────────────────────────────────────────────
    public class ArcBuilder
    {
        private DialogueArc _arc = new();
        private List<BeatBuilder> _beats = new();

        public static ArcBuilder Create(PatronId patron, string id, string displayName,
                                         int arcIndex, FriendshipTier minTier,
                                         FriendshipTier maxTier = FriendshipTier.Friend)
        {
            var b = new ArcBuilder();
            b._arc.PatronId    = patron;
            b._arc.Id          = id;
            b._arc.DisplayName = displayName;
            b._arc.ArcIndex    = arcIndex;
            b._arc.MinTier     = minTier;
            b._arc.MaxTier     = maxTier;
            return b;
        }

        public ArcBuilder Negative()    { _arc.IsNegativeArc = true; return this; }
        public ArcBuilder Fight()       { _arc.IsFightArc = true; return this; }
        public ArcBuilder Recovery()    { _arc.IsRecoveryArc = true; return this; }
        public ArcBuilder Repeatable()  { _arc.OneShot = false; return this; }
        public ArcBuilder Time(int min, int max) { _arc.TimeMin = min; _arc.TimeMax = max; return this; }
        public ArcBuilder OnStart(int friendshipDelta) { _arc.OnStartFriendshipDelta = friendshipDelta; return this; }

        public ArcBuilder Requires(ConditionType type, float fv = 0, string sv = null, int iv = 0)
        {
            _arc.UnlockConditions.Add(new DialogueCondition { Type=type, FloatValue=fv, StringValue=sv, IntValue=iv });
            return this;
        }

        public ArcBuilder RequiresArc(string arcId)    => Requires(ConditionType.ArcCompleted, sv: arcId);
        public ArcBuilder RequiresNoArc(string arcId)  => Requires(ConditionType.ArcNotCompleted, sv: arcId);
        public ArcBuilder RequiresTier(FriendshipTier t) => Requires(ConditionType.FriendshipAbove, iv: TierMinFriendship(t) - 1);
        public ArcBuilder RequiresDay(int dayIndex)    => Requires(ConditionType.DayIndex, iv: dayIndex);
        public ArcBuilder RequiresTime(int afterMinute)=> Requires(ConditionType.TimeAfter, iv: afterMinute);
        public ArcBuilder RequiresBefore(int minute)   => Requires(ConditionType.TimeBefore, iv: minute);
        public ArcBuilder RequiresNight(int n)         => Requires(ConditionType.NightNumber, iv: n);
        public ArcBuilder RequiresDrunkBelow(float d)  => Requires(ConditionType.DrunkBelow, fv: d);
        public ArcBuilder RequiresDrunkAbove(float d)  => Requires(ConditionType.DrunkAbove, fv: d);
        public ArcBuilder RequiresPatron(PatronId p)   => Requires(ConditionType.PatronPresent, sv: p.ToString());
        public ArcBuilder RequiresItem(string itemId)  => Requires(ConditionType.InventoryHas, sv: itemId);

        public BeatBuilder Beat(string id, string patronLine, string drunkVariant = null)
        {
            var bb = new BeatBuilder(this, id, patronLine, drunkVariant);
            _beats.Add(bb);
            return bb;
        }

        public PatronDialogue BuildPatronDialogue(int startingFriendship = 0,
                                                    string drunkRejection = null,
                                                    string coolGreeting = null,
                                                    string hostileGreeting = null,
                                                    string antagonistGreeting = null,
                                                    string fightLine = null,
                                                    string deEscalateLine = null,
                                                    string postFightLine = null)
        {
            // This overload isn't used here — see PatronDialogues_*.cs for full patron builds
            return null;
        }

        public DialogueArc Build()
        {
            if (_beats.Count > 0)
            {
                _arc.FirstBeatId = _beats[0]._beat.Id;
                foreach (var bb in _beats) _arc.Beats.Add(bb._beat);
            }
            return _arc;
        }

        private static int TierMinFriendship(FriendshipTier t) => t switch
        {
            FriendshipTier.Friend       => 60,
            FriendshipTier.Acquaintance => 30,
            FriendshipTier.Stranger     => 5,
            FriendshipTier.Cool         => -1,
            FriendshipTier.Hostile      => -30,
            FriendshipTier.Antagonist   => -60,
            _                           => -100,
        };
    }

    // ── Beat builder ──────────────────────────────────────────────────────────
    public class BeatBuilder
    {
        internal DialogueBeat _beat = new();
        private ArcBuilder    _arc;

        internal BeatBuilder(ArcBuilder arc, string id, string patronLine, string drunkVariant)
        {
            _arc = arc;
            _beat.Id              = id;
            _beat.PatronLine      = patronLine;
            _beat.PatronLineDrunk = drunkVariant;
        }

        public ChoiceBuilder Choice(string id, string label, string drunkLabel = null)
        {
            var c = new DialogueChoice { Id = id, Label = label, DrunkLabel = drunkLabel };
            _beat.Choices.Add(c);
            return new ChoiceBuilder(this, c);
        }

        public BeatBuilder Requires(ConditionType type, float fv = 0, string sv = null, int iv = 0)
        {
            _beat.EntryConditions.Add(new DialogueCondition { Type=type, FloatValue=fv, StringValue=sv, IntValue=iv });
            return this;
        }

        // Continue building more beats on the arc
        public BeatBuilder Beat(string id, string patronLine, string drunkVariant = null)
            => _arc.Beat(id, patronLine, drunkVariant);

        public DialogueArc Build() => _arc.Build();
    }

    // ── Choice builder ────────────────────────────────────────────────────────
    public class ChoiceBuilder
    {
        private BeatBuilder    _beat;
        private DialogueChoice _choice;

        internal ChoiceBuilder(BeatBuilder beat, DialogueChoice choice)
        { _beat = beat; _choice = choice; }

        public ChoiceBuilder GoTo(string beatId)
        { _choice.NextBeatId = beatId; return this; }

        public ChoiceBuilder Deflect()
        { _choice.IsDeflect = true; return this; }

        public ChoiceBuilder Friendship(int delta)
            => Consequence(ConsequenceType.FriendshipDelta, iv: delta);

        public ChoiceBuilder FriendshipWith(PatronId other, int delta)
            => Consequence(ConsequenceType.FriendshipDeltaOther, sv: other.ToString(), iv: delta);

        public ChoiceBuilder Drunk(float delta)
            => Consequence(ConsequenceType.DrunkDelta, fv: delta);

        public ChoiceBuilder Boredom(float delta)
            => Consequence(ConsequenceType.BoredomDelta, fv: delta);

        public ChoiceBuilder Style(int delta)
            => Consequence(ConsequenceType.StyleDelta, iv: delta);

        public ChoiceBuilder Time(int minutes)
            => Consequence(ConsequenceType.AdvanceTime, iv: minutes);

        public ChoiceBuilder Log(string line)
            => Consequence(ConsequenceType.LogLine, sv: line);

        public ChoiceBuilder SetFlag(string flag, bool val = true)
            => Consequence(ConsequenceType.SetFlag, sv: flag, iv: val ? 1 : 0);

        public ChoiceBuilder UnlockArc(string arcId)
            => Consequence(ConsequenceType.UnlockArc, sv: arcId);

        public ChoiceBuilder LockArc(string arcId)
            => Consequence(ConsequenceType.LockArc, sv: arcId);

        public ChoiceBuilder GrantTicket(int count = 1)
            => Consequence(ConsequenceType.GrantTicket, iv: count);

        public ChoiceBuilder GrantItem(string itemId)
            => Consequence(ConsequenceType.GrantItem, sv: itemId);

        public ChoiceBuilder TriggerFight()
            => Consequence(ConsequenceType.TriggerFight);

        public ChoiceBuilder TriggerEject()
            => Consequence(ConsequenceType.TriggerEject);

        public ChoiceBuilder TriggerEvent(string eventName)
            => Consequence(ConsequenceType.TriggerEvent, sv: eventName);

        public ChoiceBuilder End()
            => Consequence(ConsequenceType.EndConversation);

        public ChoiceBuilder ShowIf(ConditionType type, float fv = 0, string sv = null, int iv = 0)
        {
            _choice.Conditions.Add(new DialogueCondition { Type=type, FloatValue=fv, StringValue=sv, IntValue=iv });
            return this;
        }

        public ChoiceBuilder ShowIfDrunkAbove(float d) => ShowIf(ConditionType.DrunkAbove, fv: d);
        public ChoiceBuilder ShowIfDrunkBelow(float d) => ShowIf(ConditionType.DrunkBelow, fv: d);
        public ChoiceBuilder ShowIfArc(string id)      => ShowIf(ConditionType.ArcCompleted, sv: id);
        public ChoiceBuilder ShowIfNoArc(string id)    => ShowIf(ConditionType.ArcNotCompleted, sv: id);
        public ChoiceBuilder ShowIfFlag(string flag)   => ShowIf(ConditionType.SetFlag, sv: flag, iv: 1);
        public ChoiceBuilder ShowIfDay(int d)          => ShowIf(ConditionType.DayIndex, iv: d);
        public ChoiceBuilder ShowIfItem(string id)     => ShowIf(ConditionType.InventoryHas, sv: id);
        public ChoiceBuilder ShowIfTime(int after)     => ShowIf(ConditionType.TimeAfter, iv: after);
        public ChoiceBuilder ShowIfPatron(PatronId p)  => ShowIf(ConditionType.PatronPresent, sv: p.ToString());

        // Back to beat to add more choices
        public ChoiceBuilder Choice(string id, string label, string drunkLabel = null)
            => _beat.Choice(id, label, drunkLabel);

        // Back to arc for more beats
        public BeatBuilder Beat(string id, string line, string drunk = null)
            => _beat.Beat(id, line, drunk);

        public DialogueArc Build() => _beat.Build();

        private ChoiceBuilder Consequence(ConsequenceType type, float fv = 0,
                                           string sv = null, int iv = 0)
        {
            _choice.Consequences.Add(new DialogueConsequence { Type=type, FloatValue=fv, StringValue=sv, IntValue=iv });
            return this;
        }
    }

    // ── Patron dialogue factory helper ────────────────────────────────────────
    public static class PatronDialogueFactory
    {
        public static PatronDialogue Create(PatronId id, int startingFriendship,
            string drunkRejection, string coolGreeting, string hostileGreeting,
            string antagonistGreeting, string fightLine, string deEscalateLine,
            string postFightLine, params DialogueArc[] arcs)
        {
            var d = new PatronDialogue
            {
                PatronId            = id,
                StartingFriendship  = startingFriendship,
                DrunkRejectionLine  = drunkRejection,
                CoolGreeting        = coolGreeting,
                HostileGreeting     = hostileGreeting,
                AntagonistGreeting  = antagonistGreeting,
                FightTriggerLine    = fightLine,
                FightDeEscalateLine = deEscalateLine,
                PostFightLine       = postFightLine,
            };
            d.Arcs.AddRange(arcs);
            return d;
        }
    }
}
