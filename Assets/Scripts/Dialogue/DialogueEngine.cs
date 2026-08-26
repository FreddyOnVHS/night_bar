// DialogueEngine.cs
// Drives conversation flow. Evaluates conditions, applies consequences,
// advances beats. UI subscribes to events and calls MakeChoice().

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NightAtTheBar.Dialogue
{
    public class DialogueEngine : MonoBehaviour
    {
        public static DialogueEngine Instance { get; private set; }

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<PatronId, DialogueBeat, List<DialogueChoice>> OnBeatPresented;
        public event Action<PatronId, DialogueChoice>                     OnChoiceMade;
        public event Action<PatronId, string>                             OnConversationEnded;
        public event Action<PatronId, int>                                OnFriendshipChanged;
        public event Action<PatronId>                                     OnFightTriggered;
        public event Action<PatronId>                                     OnPatronEjected;
        public event Action<string>                                       OnLogLine;
        public event Action<PatronId, FriendshipTier>                    OnTierChanged;
        public event Action<PatronId, string>                             OnPassiveLine;

        // ── State ─────────────────────────────────────────────────────────────
        private Dictionary<PatronId, PatronDialogue>         _dialogues = new();
        private Dictionary<PatronId, PatronConversationState> _states   = new();
        private GameState _gameState;
        private CampaignState _campaignState;
        private System.Random _rng = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Initialization ────────────────────────────────────────────────────

        public void Initialize(GameState gameState, CampaignState campaignState,
                               List<PatronConversationState> nightStates)
        {
            _gameState    = gameState;
            _campaignState = campaignState;
            _states.Clear();
            foreach (var s in nightStates) _states[s.PatronId] = s;

            // Register all patron dialogues
            RegisterAllDialogues();
        }

        public void RegisterDialogue(PatronDialogue d) => _dialogues[d.PatronId] = d;

        public PatronConversationState GetState(PatronId id) =>
            _states.TryGetValue(id, out var s) ? s : null;

        // ── Starting a conversation ───────────────────────────────────────────

        public bool CanTalk(PatronId id)
        {
            var state = GetState(id);
            if (state == null || state.InConversation || state.EjectedTonight) return false;

            var patronState = _gameState.Patrons.FirstOrDefault(p => p.Id == id);
            if (patronState == null || patronState.LeaveMinute <= _gameState.GameMinute) return false;

            // Drunk gate for specific patrons
            if (IsPatronDrunkSensitive(id) && _gameState.Drunk > 75)
            {
                var d = GetDialogue(id);
                if (d?.DrunkRejectionLine != null) OnLogLine?.Invoke(d.DrunkRejectionLine);
                return false;
            }
            return true;
        }

        public void StartConversation(PatronId id)
        {
            if (!CanTalk(id)) return;
            var state = GetState(id);
            var d     = GetDialogue(id);
            if (d == null) return;

            // Apply passive friendship drift when approached
            ApplyPassiveDrift(id, state);

            // Find best available arc
            var arc = FindBestArc(id, state, d);
            if (arc == null)
            {
                // No arc available — play tier-appropriate greeting
                PlayTierGreeting(id, state, d);
                return;
            }

            state.InConversation = true;
            state.CurrentArcId  = arc.Id;
            arc.OnStartFriendshipDelta.Let(delta => ApplyFriendship(id, delta));

            GameManager.Instance?.PauseTimer();
            GameManager.Instance?.AdvanceTime(0); // flush any pending ticks

            var beat = arc.Beats.FirstOrDefault(b => b.Id == arc.FirstBeatId);
            if (beat != null) PresentBeat(id, arc, beat, state);
        }

        // ── Beat presentation ─────────────────────────────────────────────────

        private void PresentBeat(PatronId id, DialogueArc arc, DialogueBeat beat,
                                  PatronConversationState state)
        {
            state.CurrentBeatId = beat.Id;

            // Patron line variant
            string line = (_gameState.Drunk > 60 && beat.PatronLineDrunk != null)
                ? beat.PatronLineDrunk : beat.PatronLine;
            if (line != null) OnLogLine?.Invoke(line);

            // Filter choices by conditions
            var valid = beat.Choices.Where(c => EvaluateAll(c.Conditions, id)).ToList();

            if (beat.AutoAdvance || valid.Count == 0)
            {
                // Auto-fire first valid choice or wait
                var auto = valid.FirstOrDefault();
                if (auto != null) ApplyChoice(id, arc, beat, auto, state);
                else EndConversation(id, "No valid choices.");
                return;
            }

            // Show drunk labels if applicable
            var shown = valid.Select(c => _gameState.Drunk > 60 && c.DrunkLabel != null
                ? new DialogueChoice { Id=c.Id, Label=c.DrunkLabel, Consequences=c.Consequences,
                    NextBeatId=c.NextBeatId, IsDeflect=c.IsDeflect, Conditions=c.Conditions }
                : c).ToList();

            OnBeatPresented?.Invoke(id, beat, shown);
        }

        // ── Player makes a choice ─────────────────────────────────────────────

        public void MakeChoice(PatronId id, string choiceId)
        {
            var state = GetState(id);
            if (state == null || !state.InConversation) return;
            var d   = GetDialogue(id);
            var arc = d?.Arcs.FirstOrDefault(a => a.Id == state.CurrentArcId);
            var beat = arc?.Beats.FirstOrDefault(b => b.Id == state.CurrentBeatId);
            var choice = beat?.Choices.FirstOrDefault(c => c.Id == choiceId);
            if (choice == null) return;

            OnChoiceMade?.Invoke(id, choice);
            ApplyChoice(id, arc, beat, choice, state);
        }

        private void ApplyChoice(PatronId id, DialogueArc arc, DialogueBeat beat,
                                  DialogueChoice choice, PatronConversationState state)
        {
            state.CompletedBeatIds.Add(beat.Id);

            // Deflect penalty — some patrons penalise evasion
            if (choice.IsDeflect) ApplyDeflectPenalty(id);

            // Apply consequences
            foreach (var c in choice.Consequences) ApplyConsequence(id, c, state);

            // Advance to next beat or end arc
            if (choice.NextBeatId != null)
            {
                var arc2 = GetDialogue(id)?.Arcs.FirstOrDefault(a => a.Id == state.CurrentArcId);
                var next = arc2?.Beats.FirstOrDefault(b => b.Id == choice.NextBeatId);
                if (next != null && EvaluateAll(next.EntryConditions, id))
                    PresentBeat(id, arc2, next, state);
                else
                    EndArc(id, arc, state);
            }
            else EndArc(id, arc, state);
        }

        private void EndArc(PatronId id, DialogueArc arc, PatronConversationState state)
        {
            state.CompletedArcIds.Add(arc.Id);
            state.ConversationsCompletedTonight++;
            EndConversation(id, arc.DisplayName + " arc complete.");
        }

        private void EndConversation(PatronId id, string reason)
        {
            var state = GetState(id);
            if (state != null) state.InConversation = false;

            GameManager.Instance?.ResumeTimer();
            GameManager.Instance?.AdvanceTime(UnityEngine.Random.Range(5, 11));

            OnConversationEnded?.Invoke(id, reason);
        }

        // ── Consequence application ───────────────────────────────────────────

        private void ApplyConsequence(PatronId id, DialogueConsequence c,
                                       PatronConversationState state)
        {
            var gm = GameManager.Instance;
            switch (c.Type)
            {
                case ConsequenceType.FriendshipDelta:
                    ApplyFriendship(id, c.IntValue);
                    break;

                case ConsequenceType.FriendshipDeltaOther:
                    if (Enum.TryParse<PatronId>(c.StringValue, out var otherId))
                        ApplyFriendship(otherId, c.IntValue);
                    break;

                case ConsequenceType.DrunkDelta:
                    if (gm != null)
                    {
                        gm.State.Drunk = Mathf.Clamp(gm.State.Drunk + c.FloatValue, 0, 120);
                        NotifyGameState();
                    }
                    break;

                case ConsequenceType.BoredomDelta:
                    if (gm != null)
                    {
                        gm.State.Boredom = Mathf.Clamp(gm.State.Boredom + c.FloatValue, 0, 100);
                        NotifyGameState();
                    }
                    break;

                case ConsequenceType.StyleDelta:
                    if (gm != null) gm.State.StylePoints += c.IntValue;
                    break;

                case ConsequenceType.GrantItem:
                    // GameManager handles item granting
                    gm?.GrantItemById(c.StringValue);
                    break;

                case ConsequenceType.RemoveItem:
                    gm?.RemoveItemById(c.StringValue);
                    break;

                case ConsequenceType.GrantTicket:
                    if (gm != null) gm.State.DrinkTickets += c.IntValue;
                    break;

                case ConsequenceType.SetFlag:
                    state.Flags[c.StringValue] = c.IntValue == 1;
                    break;

                case ConsequenceType.AdvanceTime:
                    gm?.AdvanceTime(c.IntValue);
                    break;

                case ConsequenceType.LogLine:
                    OnLogLine?.Invoke(c.StringValue);
                    break;

                case ConsequenceType.UnlockArc:
                    state.Flags["arc_unlocked_" + c.StringValue] = true;
                    break;

                case ConsequenceType.LockArc:
                    state.Flags["arc_locked_" + c.StringValue] = true;
                    break;

                case ConsequenceType.TriggerFight:
                    TriggerFight(id, state);
                    break;

                case ConsequenceType.TriggerEject:
                    TriggerEject(id, state);
                    break;

                case ConsequenceType.TriggerEvent:
                    GameEvents.Fire(c.StringValue);
                    break;

                case ConsequenceType.EndConversation:
                    EndConversation(id, "Patron ended conversation.");
                    break;
            }
        }

        // ── Friendship management ─────────────────────────────────────────────

        public void ApplyFriendship(PatronId id, int delta)
        {
            var state = GetState(id);
            if (state == null) return;

            var oldTier = state.CurrentTier;
            state.Friendship = Mathf.Clamp(state.Friendship + delta, -100, 100);
            OnFriendshipChanged?.Invoke(id, state.Friendship);

            var newTier = state.CurrentTier;
            if (newTier != oldTier)
            {
                OnTierChanged?.Invoke(id, newTier);
                HandleTierChange(id, state, oldTier, newTier);
            }

            // Propagate to GameState patron
            var gsp = _gameState.Patrons.FirstOrDefault(p => p.Id == id);
            if (gsp != null) gsp.Friendship = state.Friendship;

            // Check fight threshold
            if (state.Friendship <= -100 && !state.FightTriggered)
                TriggerFight(id, state);
        }

        private void HandleTierChange(PatronId id, PatronConversationState state,
                                       FriendshipTier oldTier, FriendshipTier newTier)
        {
            // Positive tier upgrades
            if (newTier == FriendshipTier.Friend && oldTier < FriendshipTier.Friend)
            {
                var def = PatronDatabase.Get(id);
                OnLogLine?.Invoke($"{def?.DisplayName} is your friend now! Perk: {def?.Perk}");
                GameManager.Instance?.ApplyPatronPerkById(id);
            }
            else if (newTier == FriendshipTier.Acquaintance && oldTier < FriendshipTier.Acquaintance)
            {
                var def = PatronDatabase.Get(id);
                OnLogLine?.Invoke($"{def?.DisplayName} warms up to you. Acquaintances now.");
            }

            // Negative tier drops — log them
            else if (newTier == FriendshipTier.Cool && oldTier > FriendshipTier.Cool)
                OnLogLine?.Invoke($"{PatronDatabase.Get(id)?.DisplayName} seems cooler toward you tonight.");
            else if (newTier == FriendshipTier.Hostile && oldTier > FriendshipTier.Hostile)
                OnLogLine?.Invoke($"{PatronDatabase.Get(id)?.DisplayName} is done with you for now.");
            else if (newTier == FriendshipTier.Antagonist && oldTier > FriendshipTier.Antagonist)
                OnLogLine?.Invoke($"Things with {PatronDatabase.Get(id)?.DisplayName} are getting tense.");
        }

        private void ApplyPassiveDrift(PatronId id, PatronConversationState state)
        {
            // Drunk penalty for sensitive patrons
            if (_gameState.Drunk > 75)
            {
                if (id == PatronId.Musician)  ApplyFriendship(id, -10);
                if (id == PatronId.Veteran)   ApplyFriendship(id, -15);
                if (id == PatronId.Nurse)     ApplyFriendship(id, -10);
            }
            if (_gameState.Drunk > 80 && id == PatronId.Nurse)
            {
                // Nurse notices and tells bartender
                OnLogLine?.Invoke("The Nurse gives you a look. The bartender is now watching.");
                GameEvents.Fire("bartender_watching");
            }
        }

        private void ApplyDeflectPenalty(PatronId id)
        {
            // Patrons who penalise deflection
            if (id == PatronId.RetiredDetective) ApplyFriendship(id, -15);
            else if (id == PatronId.Veteran)     ApplyFriendship(id, -10);
            else if (id == PatronId.Nurse)       ApplyFriendship(id, -5);
        }

        // ── Fight system ──────────────────────────────────────────────────────

        private void TriggerFight(PatronId id, PatronConversationState state)
        {
            if (state.FightTriggered) return;
            state.FightTriggered = true;
            state.InConversation  = false;

            var d = GetDialogue(id);
            if (d?.FightTriggerLine != null) OnLogLine?.Invoke(d.FightTriggerLine);

            OnFightTriggered?.Invoke(id);
            GameManager.Instance?.PauseTimer();

            // Check de-escalation helpers
            bool veteranPresent    = IsPatronPresentAndFriendly(PatronId.Veteran);
            bool youthPastorPresent= IsPatronPresentAndFriendly(PatronId.YouthPastor);
            bool biscuitPresent    = IsPatronPresentAndFriendly(PatronId.Dog);

            if (veteranPresent)
            {
                OnLogLine?.Invoke("The Veteran stands up slowly. Nobody swings. The moment passes.");
                ResolveFight(id, state, FightOutcome.DeEscalatedByVeteran);
            }
            else if (youthPastorPresent && _rng.Next(100) < 80)
            {
                OnLogLine?.Invoke("Dave steps between you. 'Hey. Hey. Let's take a breath.' Somehow it works.");
                ResolveFight(id, state, FightOutcome.DeEscalatedByPastor);
            }
            else if (biscuitPresent && _rng.Next(100) < 30)
            {
                OnLogLine?.Invoke("Biscuit barks once. The fight pauses. Nobody can explain why that was enough.");
                ResolveFight(id, state, FightOutcome.DeEscalatedByDog);
            }
            // else: UI handles player choices for fight resolution
        }

        public void ResolveFight(PatronId id, PatronConversationState state, FightOutcome outcome)
        {
            switch (outcome)
            {
                case FightOutcome.PlayerDeEscalated:
                    ApplyFriendship(id, 20); // back toward -80
                    OnLogLine?.Invoke("You back down. The moment passes. Barely.");
                    GameManager.Instance?.State.Let(s => s.StylePoints += 15);
                    GameManager.Instance?.ResumeTimer();
                    break;

                case FightOutcome.DeEscalatedByVeteran:
                case FightOutcome.DeEscalatedByPastor:
                case FightOutcome.DeEscalatedByDog:
                    ApplyFriendship(id, 10);
                    if (outcome == FightOutcome.DeEscalatedByDog)
                        GameManager.Instance?.State.Let(s => s.StylePoints += 25);
                    GameManager.Instance?.ResumeTimer();
                    break;

                case FightOutcome.MutualEject:
                    OnLogLine?.Invoke("The bouncer grabs both of you. Night over.");
                    TriggerEject(id, state);
                    GameManager.Instance?.State.Let(s => s.StylePoints -= 20);
                    GameManager.Instance?.TriggerEarlyEnd();
                    break;

                case FightOutcome.PlayerEjected:
                    OnLogLine?.Invoke("The bouncer points at you. You're done for tonight.");
                    TriggerEject(PatronId.Regular, null); // dummy — just ejects player
                    GameManager.Instance?.TriggerEarlyEnd();
                    break;

                case FightOutcome.PatronEjected:
                    OnLogLine?.Invoke("The bouncer ejects them. You stay. The bar watches.");
                    ApplyFriendship(id, -30);
                    var gm = GameManager.Instance;
                    if (gm != null)
                    {
                        gm.State.Boredom = Mathf.Clamp(gm.State.Boredom - 20, 0, 100);
                        gm.State.StylePoints += 10;
                        // Boost from witnesses
                        foreach (var p in _states.Values)
                            if (p.PatronId != id && p.Friendship > 0)
                                ApplyFriendship(p.PatronId, 5);
                    }
                    GameManager.Instance?.ResumeTimer();
                    break;
            }
        }

        private void TriggerEject(PatronId id, PatronConversationState state)
        {
            if (state != null) state.EjectedTonight = true;
            OnPatronEjected?.Invoke(id);
        }

        // ── Arc selection ─────────────────────────────────────────────────────

        private DialogueArc FindBestArc(PatronId id, PatronConversationState state,
                                          PatronDialogue d)
        {
            var tier = state.CurrentTier;

            // Negative arcs take priority when tier is negative
            if ((int)tier < 0)
            {
                var negArc = d.Arcs
                    .Where(a => a.IsNegativeArc
                             && (int)a.MinTier <= (int)tier
                             && (int)a.MaxTier >= (int)tier
                             && (!a.OneShot || !state.CompletedArcIds.Contains(a.Id))
                             && !state.Flags.ContainsKey("arc_locked_" + a.Id)
                             && EvaluateAll(a.UnlockConditions, id))
                    .OrderByDescending(a => a.ArcIndex)
                    .FirstOrDefault();
                if (negArc != null) return negArc;
            }

            // Positive arcs
            return d.Arcs
                .Where(a => !a.IsNegativeArc
                         && !a.IsFightArc
                         && (int)a.MinTier <= (int)tier
                         && (!a.OneShot || !state.CompletedArcIds.Contains(a.Id))
                         && !state.Flags.ContainsKey("arc_locked_" + a.Id)
                         && EvaluateAll(a.UnlockConditions, id))
                .OrderBy(a => a.ArcIndex)
                .FirstOrDefault();
        }

        // ── Condition evaluation ──────────────────────────────────────────────

        private bool EvaluateAll(List<DialogueCondition> conditions, PatronId id)
        {
            if (conditions == null || conditions.Count == 0) return true;
            return conditions.All(c => Evaluate(c, id));
        }

        private bool Evaluate(DialogueCondition c, PatronId id)
        {
            var state = GetState(id);
            var gm    = GameManager.Instance;
            if (gm == null) return true;

            return c.Type switch
            {
                ConditionType.None              => true,
                ConditionType.DrunkAbove        => gm.State.Drunk > c.FloatValue,
                ConditionType.DrunkBelow        => gm.State.Drunk < c.FloatValue,
                ConditionType.FriendshipAbove   => (state?.Friendship ?? 0) > c.IntValue,
                ConditionType.FriendshipBelow   => (state?.Friendship ?? 0) < c.IntValue,
                ConditionType.ArcCompleted      => state?.CompletedArcIds.Contains(c.StringValue) ?? false,
                ConditionType.ArcNotCompleted   => !(state?.CompletedArcIds.Contains(c.StringValue) ?? false),
                ConditionType.PatronPresent     => IsPatronPresent(c.StringValue),
                ConditionType.PatronFriendshipAbove =>
                    Enum.TryParse<PatronId>(c.StringValue, out var pid) &&
                    (GetState(pid)?.Friendship ?? 0) > c.IntValue,
                ConditionType.TimeAfter         => gm.State.GameMinute >= c.IntValue,
                ConditionType.TimeBefore        => gm.State.GameMinute < c.IntValue,
                ConditionType.DayIndex          => gm.Campaign.CurrentDayIndex % 7 == c.IntValue,
                ConditionType.InventoryHas      => gm.State.Inventory.Exists(i => i.Id == c.StringValue),
                ConditionType.NightNumber       => gm.Campaign.NightsCompleted >= c.IntValue,
                ConditionType.ConversationBeat  =>
                    state?.CompletedBeatIds.Contains(c.StringValue) ?? false,
                ConditionType.RandomChance      => _rng.Next(100) < c.IntValue,
                _                               => true,
            };
        }

        // ── Tier greeting (no arc available) ─────────────────────────────────

        private void PlayTierGreeting(PatronId id, PatronConversationState state,
                                       PatronDialogue d)
        {
            string line = state.CurrentTier switch
            {
                FriendshipTier.Cool        => d.CoolGreeting,
                FriendshipTier.Hostile     => d.HostileGreeting,
                FriendshipTier.Antagonist  => d.AntagonistGreeting,
                _                          => null,
            };
            if (line != null) OnLogLine?.Invoke(line);

            // Check passive lines
            if (d.PassiveLines.TryGetValue(state.CurrentTier, out var passives) && passives.Count > 0)
                OnPassiveLine?.Invoke(id, passives[_rng.Next(passives.Count)]);

            GameManager.Instance?.AdvanceTime(2);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private PatronDialogue GetDialogue(PatronId id) =>
            _dialogues.TryGetValue(id, out var d) ? d : null;

        private bool IsPatronDrunkSensitive(PatronId id) =>
            id == PatronId.Musician || id == PatronId.Veteran || id == PatronId.Nurse;

        private bool IsPatronPresent(string idStr) =>
            Enum.TryParse<PatronId>(idStr, out var pid) && IsPatronPresent(pid);

        private bool IsPatronPresent(PatronId id) =>
            _gameState.Patrons.Any(p => p.Id == id && p.LeaveMinute > _gameState.GameMinute);

        private bool IsPatronPresentAndFriendly(PatronId id) =>
            IsPatronPresent(id) && (GetState(id)?.Friendship ?? 0) >= 0;

        private void NotifyGameState() =>
            GameManager.Instance?.ForceStateNotify();

        // ── Register all dialogues ────────────────────────────────────────────
        // Each patron's dialogue is in its own file and registered here.

        private void RegisterAllDialogues()
        {
            RegisterDialogue(PatronDialogues_Regular.Build());
            RegisterDialogue(PatronDialogues_Crier.Build());
            RegisterDialogue(PatronDialogues_OffDuty.Build());
            RegisterDialogue(PatronDialogues_Buyer.Build());
            RegisterDialogue(PatronDialogues_Instigator.Build());
            RegisterDialogue(PatronDialogues_Storyteller.Build());
            RegisterDialogue(PatronDialogues_ConspiracyGuy.Build());
            RegisterDialogue(PatronDialogues_Musician.Build());
            RegisterDialogue(PatronDialogues_Divorce.Build());
            RegisterDialogue(PatronDialogues_Nurse.Build());
            RegisterDialogue(PatronDialogues_RecentlySingle.Build());
            RegisterDialogue(PatronDialogues_YouthPastor.Build());
            RegisterDialogue(PatronDialogues_Politician.Build());
            RegisterDialogue(PatronDialogues_Dog.Build());
            RegisterDialogue(PatronDialogues_Twins.Build());
            RegisterDialogue(PatronDialogues_FormerChef.Build());
            RegisterDialogue(PatronDialogues_Veteran.Build());
            RegisterDialogue(PatronDialogues_Insomniac.Build());
            RegisterDialogue(PatronDialogues_Widower.Build());
            RegisterDialogue(PatronDialogues_Kid.Build());
            RegisterDialogue(PatronDialogues_Detective.Build());
        }
    }

    // ── Fight outcomes ────────────────────────────────────────────────────────
    public enum FightOutcome
    {
        PlayerDeEscalated,
        DeEscalatedByVeteran,
        DeEscalatedByPastor,
        DeEscalatedByDog,
        MutualEject,
        PlayerEjected,
        PatronEjected,
    }

    // ── Simple game event bus ─────────────────────────────────────────────────
    public static class GameEvents
    {
        public static event Action<string> OnEvent;
        public static void Fire(string eventName) => OnEvent?.Invoke(eventName);
    }

    // ── Extension helpers ─────────────────────────────────────────────────────
    public static class Extensions
    {
        public static void Let<T>(this T obj, Action<T> action) where T : class
        { if (obj != null) action(obj); }
    }
}
