// DriveManager.cs
// Driving minigame logic. Attach to a persistent GameObject.
// UI subscribes to events; calls MakeChoice() for player input.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightAtTheBar
{
    public class DriveManager : MonoBehaviour
    {
        public static DriveManager Instance { get; private set; }

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<DriveObstacle, string>  OnObstaclePresented; // obstacle, description
        public event Action<string>                 OnObstacleResolved;
        public event Action<int, int, int>          OnDriveStatsChanged; // hp, police, score
        public event Action<bool>                   OnPoliceEncounter;   // true = pulled over
        public event Action<EndingType>             OnDriveEnded;
        public event Action<string>                 OnLogLine;

        private GameState   _state;
        private System.Random _rng = new();
        private int _segment = 0; // 1–3

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void BeginDrivingMinigame(GameState state)
        {
            _state   = state;
            _segment = 1;
            _state.DriveHP         = 3;
            _state.PoliceAttention = 0;
            _state.DriveScore      = 100;
            Log("--- Driving home ---");
            PresentNextSegment();
        }

        private void PresentNextSegment()
        {
            if (_segment > 3) { Arrive(); return; }

            var obstacle = PickObstacle(_segment);
            string desc = DescribeObstacle(obstacle);
            Log($"--- Segment {_segment}: {SegmentName(_segment)} ---");
            Log(desc);
            OnObstaclePresented?.Invoke(obstacle, desc);
        }

        private DriveObstacle PickObstacle(int seg)
        {
            bool hasSunglasses = _state.Inventory.Exists(i => i.Mechanic == ItemMechanic.Sunglasses);
            List<DriveObstacle> pool = seg switch
            {
                1 => hasSunglasses
                    ? new() { DriveObstacle.RedLight, DriveObstacle.ParkedCar }
                    : new() { DriveObstacle.RedLight, DriveObstacle.ParkedCar, DriveObstacle.HeadlightGlare },
                2 => new() { DriveObstacle.HighwayMerge, DriveObstacle.Pothole },
                _ => new() { DriveObstacle.TrashCan, DriveObstacle.Deer, DriveObstacle.Pedestrian, DriveObstacle.ParkedCop },
            };
            return pool[_rng.Next(pool.Count)];
        }

        // UI calls this with the player's choice (true=A, false=B)
        public void MakeChoice(DriveObstacle obstacle, bool choiceA)
        {
            int dd = Mathf.Max(0, (int)_state.Drunk - 25);
            bool hasIpod = _state.Inventory.Exists(i => i.Mechanic == ItemMechanic.BoredomSlow);
            int roll = _rng.Next(1, 101) + Mathf.RoundToInt(dd * 0.8f) + (hasIpod ? -10 : 0);

            switch (obstacle)
            {
                case DriveObstacle.RedLight:
                    if (choiceA) { _state.DriveScore += 5; Log("You stop at the red. Right call."); }
                    else { _state.PoliceAttention += 20; _state.DriveScore -= 20; Log("You blow through. (police +20)"); }
                    break;

                case DriveObstacle.ParkedCar:
                case DriveObstacle.HeadlightGlare:
                    if (roll > 60) { TakeHit(obstacle == DriveObstacle.ParkedCar ? "clip a parked car" : "swerve from headlights", 1, 30, 30); }
                    else { _state.DriveScore += 10; Log("You thread through carefully."); }
                    break;

                case DriveObstacle.HighwayMerge:
                    if (choiceA) // gun it
                    {
                        int r2 = _rng.Next(1, 101) + Mathf.RoundToInt(dd * 0.6f);
                        if (r2 > 60) TakeHit("scrape the guardrail", 1, 15, 20);
                        else { _state.DriveScore += 15; Log("Merge perfectly. Smooth."); }
                    }
                    else { _state.PoliceAttention += 5; _state.DriveScore += 5; Log("Wait for a gap. Cars honk. Safe."); }
                    break;

                case DriveObstacle.Pothole:
                    if (roll > 55) { _state.DriveScore -= 15; _state.PoliceAttention += 5; Log("POTHOLE! Car jerks hard. (-15 score)"); }
                    else Log("Dodge the pothole cleanly.");
                    break;

                case DriveObstacle.TrashCan:
                    if (roll > 50) TakeHit("hit a trash can", 1, 0, 15);
                    else Log("Swerve around the trash can.");
                    break;

                case DriveObstacle.Deer:
                    if (choiceA) // brake
                    {
                        int r2 = _rng.Next(1, 101) + Mathf.RoundToInt(dd * 0.5f);
                        if (r2 > 65) { _state.DriveScore -= 10; Log("Stop just in time. Rude deer."); }
                        else { _state.DriveScore += 10; Log("Brake smoothly. Deer moseys off."); }
                    }
                    else // swerve
                    {
                        int r2 = _rng.Next(1, 101) + Mathf.RoundToInt(dd * 0.7f);
                        if (r2 > 55) TakeHit("swerve into a mailbox", 1, 0, 25);
                        else { _state.DriveScore += 15; Log("Nice reflexes. Swerve clean."); }
                    }
                    break;

                case DriveObstacle.Pedestrian:
                    if (roll > 40) { _state.DriveScore -= 20; _state.PoliceAttention += 15; Log("Swerve hard to avoid. Close call."); }
                    else { _state.DriveScore += 15; Log("Slow down. They wave. Civilized."); }
                    break;

                case DriveObstacle.ParkedCop:
                    if (_state.PoliceAttention > 30 || roll > 65) _state.PoliceAttention += 25;
                    Log(_state.PoliceAttention >= 70 ? "You swerve passing the cop. They notice." : "Drive past clean.");
                    if (_state.PoliceAttention >= 70)
                    {
                        Log("🚨 Lights and sirens behind you!");
                        OnPoliceEncounter?.Invoke(true);
                        NotifyStats();
                        return; // wait for ResolvePolicePullover
                    }
                    break;
            }

            if (_state.DriveHP <= 0) { Crash(); return; }
            NotifyStats();
            _segment++;
            PresentNextSegment();
        }

        public void ResolvePolicePullover(bool pullOver)
        {
            // Check mustache
            var mustache = _state.Inventory.Find(i => i.Mechanic == ItemMechanic.Mustache);
            if (pullOver && mustache != null && !_state.MustacheUsed)
            {
                _state.MustacheUsed = true;
                _state.Inventory.Remove(mustache);
                _state.PoliceAttention = 0;
                _state.DriveScore += 20;
                _state.StylePoints += 20;
                Log("Cop doesn't recognise you. Mustache falls off. (+20 style)");
                _segment++;
                PresentNextSegment();
                return;
            }

            if (pullOver)
            {
                int dd = Mathf.Max(0, (int)_state.Drunk - 25);
                int testRoll = _rng.Next(1, 101) - Mathf.RoundToInt(dd * 1.2f);
                if (testRoll > 30)
                {
                    _state.DriveScore += 30; _state.PoliceAttention = 0;
                    Log("You PASS the sobriety test! Warning given.");
                    _segment++;
                    PresentNextSegment();
                }
                else
                {
                    Log("You fail the test. Handcuffs click.");
                    End(EndingType.ArrestedPullOver);
                }
            }
            else // flee
            {
                int dd = Mathf.Max(0, (int)_state.Drunk - 25);
                int cr = _rng.Next(1, 101) + Mathf.RoundToInt(dd * 0.8f);
                Log("You FLOOR IT. Sirens wail.");
                if (cr > 55)
                {
                    _state.DriveHP--;
                    if (_state.DriveHP <= 0) End(EndingType.ArrestedChase);
                    else { _state.DriveScore -= 40; Log("You lose them in back streets."); End(EndingType.RanHome); }
                }
                else { _state.DriveScore -= 30; Log("Lost them. Driveway, lights off."); End(EndingType.RanHome); }
            }
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        private void TakeHit(string what, int hpLoss, int policeLoss, int scoreLoss)
        {
            _state.DriveHP -= hpLoss;
            _state.PoliceAttention += policeLoss;
            _state.DriveScore -= scoreLoss;
            Log($"You {what}! (HP: {_state.DriveHP})");
        }

        private void Arrive()
        {
            Log("You see your house. So close...");
            Log("Driveway. Engine off. Key out. Silence.");
            Log("...crickets. You made it.");
            End(EndingType.MadeItHome);
        }

        private void Crash()
        {
            Log("Your car is done. Steam from the hood. A neighbour calls 911.");
            End(EndingType.Crashed);
        }

        private void End(EndingType ending)
        {
            _state.Ending    = ending;
            _state.NightEnded = true;
            NotifyStats();
            OnDriveEnded?.Invoke(ending);
            // Notify GameManager to handle campaign state
            GameManager.Instance?.PauseTimer();
        }

        private void NotifyStats() =>
            OnDriveStatsChanged?.Invoke(_state.DriveHP, _state.PoliceAttention, _state.DriveScore);

        private void Log(string msg) => OnLogLine?.Invoke(msg);

        private string SegmentName(int seg) => seg switch
        {
            1 => "Downtown streets",
            2 => "Highway merge",
            _ => "Residential streets",
        };

        private string DescribeObstacle(DriveObstacle o) => o switch
        {
            DriveObstacle.RedLight      => "Red light ahead.",
            DriveObstacle.ParkedCar     => "Parked cars tight on both sides.",
            DriveObstacle.HeadlightGlare=> "Oncoming headlights blinding you.",
            DriveObstacle.HighwayMerge  => "Highway on-ramp. Merge into traffic.",
            DriveObstacle.Pothole       => "POTHOLE in the road!",
            DriveObstacle.TrashCan      => "Trash can in the road.",
            DriveObstacle.Deer          => "A DEER in your headlights!",
            DriveObstacle.Pedestrian    => "Pedestrian crossing ahead!",
            DriveObstacle.ParkedCop     => "Parked cop car on the shoulder. Heart rate spikes.",
            _                           => "",
        };
    }
}
