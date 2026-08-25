// KaraokeManager.cs
// Karaoke minigame logic. The actual rhythm input (note timing)
// is handled by your Unity UI/rhythm component. Call SubmitScore()
// when the rhythm segment ends.

using System;
using UnityEngine;

namespace NightAtTheBar
{
    public enum KaraokeResult { StandingOvation, Good, Mediocre, Bombed }

    public class KaraokeManager : MonoBehaviour
    {
        public static KaraokeManager Instance { get; private set; }

        // ── Events ────────────────────────────────────────────────────────────
        // UI subscribes to start the rhythm segment
        public event Action<KaraokeDifficulty, float> OnKaraokeStart; // difficulty, drunkPenalty 0-1
        public event Action<KaraokeResult, int>       OnKaraokeEnd;   // result, boredom delta
        public event Action<string>                   OnLogLine;

        private KaraokeDifficulty _currentDiff;
        private System.Random _rng = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // Called when player steps up to the stage and picks a song
        public void BeginKaraoke(KaraokeDifficulty difficulty)
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            _currentDiff = difficulty;
            gm.PauseTimer();

            // Communicate drunk penalty to the rhythm component (0=clean, 1=almost unplayable)
            float drunkPenalty = Mathf.Clamp01((gm.State.Drunk - 40f) / 60f);
            OnKaraokeStart?.Invoke(difficulty, drunkPenalty);
        }

        // Called by your rhythm component with a raw score 0-100
        public void SubmitScore(int rhythmScore)
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            var state = gm.State;
            float foamMult = state.Inventory.Exists(i => i.Mechanic == ItemMechanic.FoamFinger) ? 1.3f : 1f;

            // Skill boost from 8GB iPod
            bool hasEightGig = state.Inventory.Exists(i => i.Mechanic == ItemMechanic.BoredomSlow && i.Value <= 0.5f);
            int adjustedScore = rhythmScore + (hasEightGig ? 10 : 0);

            // Difficulty modifier
            int diffMod = _currentDiff switch
            {
                KaraokeDifficulty.Easy   =>  20,
                KaraokeDifficulty.Hard   => -20,
                _                        =>   0,
            };
            adjustedScore += diffMod;

            int baseDrop = _currentDiff switch
            {
                KaraokeDifficulty.Easy   => 10,
                KaraokeDifficulty.Medium => 14,
                KaraokeDifficulty.Hard   => 20,
                _                        => 10,
            };

            KaraokeResult result;
            int boredomDelta;

            if (adjustedScore >= 80)
            {
                result      = KaraokeResult.StandingOvation;
                boredomDelta = -Mathf.RoundToInt(baseDrop * 1.5f * foamMult);
                Log($"STANDING OVATION! ({boredomDelta} boredom)");
                if (state.Drunk > 70) { state.StylePoints += 30; Log("Style! Hammered but talented. (+30)"); }
                // Buff all present patrons
                foreach (var p in state.Patrons) p.Friendship += 5;
            }
            else if (adjustedScore >= 50)
            {
                result       = KaraokeResult.Good;
                boredomDelta = -Mathf.RoundToInt(baseDrop * foamMult);
                Log($"Good performance! ({boredomDelta} boredom)");
            }
            else if (adjustedScore >= 25)
            {
                result       = KaraokeResult.Mediocre;
                boredomDelta = -2;
                Log("Mediocre. Polite clapping. (-2 boredom)");
            }
            else
            {
                result       = KaraokeResult.Bombed;
                boredomDelta = 10;
                Log("You bombed it. One person slow-claps. (+10 boredom)");
            }

            state.Boredom       = Mathf.Clamp(state.Boredom + boredomDelta, 0, 100);
            state.ActivitiesDone++;
            gm.AdvanceTime(Mathf.RoundToInt(UnityEngine.Random.Range(8, 13)));
            gm.ResumeTimer();

            OnKaraokeEnd?.Invoke(result, boredomDelta);
        }

        private void Log(string message)
        {
            OnLogLine?.Invoke(message);
        }
    }
}