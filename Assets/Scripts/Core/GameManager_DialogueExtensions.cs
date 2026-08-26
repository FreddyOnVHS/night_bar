// GameManager_DialogueExtensions.cs
// Extends GameManager with methods the dialogue engine needs.
// Kept separate so GameManager.cs stays clean.

using UnityEngine;
using NightAtTheBar.Dialogue;

namespace NightAtTheBar
{
    public partial class GameManager : MonoBehaviour
    {
        // ── Called by DialogueEngine ──────────────────────────────────────────

        public void GrantItemById(string itemId)
        {
            var item = ItemDatabase.All.Find(i => i.Id == itemId);
            if (item == null) return;
            if (State.Inventory.Count >= Tuning.MaxInventorySlots)
                OnInventoryFull?.Invoke(item, new System.Collections.Generic.List<ItemDefinition>(State.Inventory));
            else
                GrantItem(item);
            ForceStateNotify();
        }

        public void RemoveItemById(string itemId)
        {
            var item = State.Inventory.Find(i => i.Id == itemId);
            if (item == null) return;
            State.Inventory.Remove(item);
            ForceStateNotify();
        }

        public void ApplyPatronPerkById(PatronId id)
        {
            var p = State.Patrons.Find(x => x.Id == id);
            if (p != null) ApplyPatronPerk(p);
        }

        public void ForceStateNotify() => OnStateChanged?.Invoke(State);

        // Triggered when fight leads to early night end
        public void TriggerEarlyEnd()
        {
            _timerPaused = true;
            var ending = State.Drunk >= Tuning.DrunkBarfThreshold ? EndingType.BarfedOut : EndingType.LeftEarly;
            TriggerEnding(ending);
        }

        // Initialize DialogueEngine for a new night
        private void InitDialogueEngine()
        {
            if (DialogueEngine.Instance == null) return;

            var nightStates = new System.Collections.Generic.List<Dialogue.PatronConversationState>();
            foreach (var p in State.Patrons)
            {
                // Restore persisted friendship if available
                int startFriendship = PatronDatabase.Get(p.Id) != null
                    ? GetStartingFriendship(p.Id) : 0;

                if (Campaign.SavedFriendships.TryGetValue(p.Id, out var saved))
                    startFriendship = saved.Friendship;

                nightStates.Add(new Dialogue.PatronConversationState
                {
                    PatronId   = p.Id,
                    Friendship = startFriendship,
                });
            }

            DialogueEngine.Instance.Initialize(State, Campaign, nightStates);

            // Wire dialogue events to GameManager log
            DialogueEngine.Instance.OnLogLine           += msg => OnLogLine?.Invoke(msg);
            DialogueEngine.Instance.OnFightTriggered    += id => OnLogLine?.Invoke($"--- Fight with {id}! ---");
            DialogueEngine.Instance.OnPatronEjected     += id => OnLogLine?.Invoke($"{id} has been ejected.");
        }

        private int GetStartingFriendship(PatronId id)
        {
            // From PatronDialogue definitions — centralized here
            return id switch
            {
                PatronId.Regular         =>   5,
                PatronId.Crier           =>  10,
                PatronId.OffDuty         =>   0,
                PatronId.Buyer           =>   5,
                PatronId.Instigator      =>   0,
                PatronId.Storyteller     =>   5,
                PatronId.ConspiracyGuy   =>  10,
                PatronId.Musician        =>  -5,
                PatronId.Divorce         =>   5,
                PatronId.Nurse           =>  -5,
                PatronId.RecentlySingle  =>  15,
                PatronId.YouthPastor     =>  10,
                PatronId.Politician      =>   5,
                PatronId.Dog             =>  20,
                PatronId.Twins           =>   0,
                PatronId.FormerChef      =>  -5,
                PatronId.Veteran         => -10,
                PatronId.Insomniac       =>   0,
                PatronId.Widower         =>  10,
                PatronId.Kid             =>   5,
                PatronId.RetiredDetective=> -5,
                _                        =>   0,
            };
        }
    }
}
