// ScoreCalculator.cs
// Pure static scoring logic. No Unity dependencies.

using System;
using UnityEngine;

namespace NightAtTheBar
{
    public struct NightScore
    {
        public int Social;
        public int BarMastery;
        public int Drive;
        public int Style;
        public int Total;
        public string Rating;
        public string HangoverTitle;
        public string HangoverFlavor;
        public string BarfWakeup;
    }

    public static class ScoreCalculator
    {
        public static int Calculate(GameState s, CampaignState c)
        {
            return Full(s, c).Total;
        }

        public static NightScore Full(GameState s, CampaignState c)
        {
            var score = new NightScore();

            // ── Social (max 300) ──────────────────────────────────────────────
            score.Social = Math.Min(300,
                s.PatronsBefriended   * 50 +
                s.ConversationsTotal  * 10);
            // Crier arc bonus
            var crier = s.Patrons.Find(p => p.Id == PatronId.Crier);
            if (crier != null && crier.FriendTier >= 2) score.Social += 100;

            // ── Bar mastery (max 300) ─────────────────────────────────────────
            score.BarMastery = Math.Min(300,
                (int)s.BestSweetStreak * 2 +
                s.ActivitiesDone       * 5 +
                s.BathroomEventsFound.Count * 20 +
                s.ClawWins             * 15 +
                (s.SlotJackpot ? 40 : 0));

            // ── Drive (max 300) ───────────────────────────────────────────────
            score.Drive = s.Ending switch
            {
                EndingType.MadeItHome  => Math.Min(300, Math.Max(0, 100 + s.DriveScore)),
                EndingType.CrierDrove  => 50,
                EndingType.RanHome     => 30,
                _                      => 0,
            };

            // ── Style (max 100) ───────────────────────────────────────────────
            score.Style = Math.Min(100, s.StylePoints);

            score.Total = score.Social + score.BarMastery + score.Drive + score.Style;

            // ── Rating ────────────────────────────────────────────────────────
            score.Rating = s.Ending switch
            {
                EndingType.BarfedOut        => "Disaster",
                EndingType.LeftEarly        => "Why did you even go out?",
                EndingType.ArrestedPullOver => "Rock bottom",
                EndingType.ArrestedChase    => "Rock bottom",
                EndingType.Crashed          => "Disaster",
                EndingType.CrierDrove       => "You made a friend",
                EndingType.RanHome          => "Home safe (for now)",
                _ => score.Total switch
                {
                    >= 800 => "LEGEND",
                    >= 650 => "Great night",
                    >= 500 => "Decent night",
                    >= 350 => "Rough night",
                    >= 200 => "Disaster",
                    _      => "Rock bottom",
                }
            };

            // ── Hangover flavor ───────────────────────────────────────────────
            float drunkHome = s.Drunk;
            if (drunkHome >= 85)
            {
                score.HangoverTitle  = "Rough one.";
                score.HangoverFlavor = "You wake up fully clothed on top of the covers. There's a half-eaten granola bar on the nightstand that you don't remember buying. Your mouth tastes like regret and something citrus-adjacent.";
            }
            else if (drunkHome >= 65)
            {
                score.HangoverTitle  = "That was a night.";
                score.HangoverFlavor = "Head pounds a little but you're functional. A few texts you vaguely remember sending. Nothing catastrophic. You count that as a win.";
            }
            else if (drunkHome >= 40)
            {
                score.HangoverTitle  = "Pretty good night actually.";
                score.HangoverFlavor = "You slept fine. You even remember most of it. You're mildly proud of yourself, which is probably too generous.";
            }
            else
            {
                score.HangoverTitle  = "Completely responsible.";
                score.HangoverFlavor = "You woke up early. You drank water before bed. You feel suspiciously good for someone who spent the night in a bar.";
            }

            // ── Barf wakeup scenario ──────────────────────────────────────────
            if (s.Ending == EndingType.BarfedOut)
                score.BarfWakeup = PickBarfWakeup(s.GameMinute);

            return score;
        }

        private static string PickBarfWakeup(int barfMinute)
        {
            // Earlier barf = milder wakeup
            var rng = new System.Random();
            if (barfMinute < 630) // before 10:30 PM
                return new[]{ SIDEWALK, BUSH }[rng.Next(2)];
            if (barfMinute < 720) // before midnight
                return new[]{ SIDEWALK, BUSH, UGLY_GIRL, UGLY_MAN }[rng.Next(4)];
            if (barfMinute < 780) // before 1 AM
                return new[]{ UGLY_GIRL, UGLY_MAN, EX }[rng.Next(3)];
            return new[]{ EX, JAIL, JUNKYARD }[rng.Next(3)];
        }

        // ── Barf wake-up scenario text ────────────────────────────────────────
        private const string SIDEWALK =
            "THE SIDEWALK\nYou wake up on the concrete right outside the bar. The bouncer is standing over you. A street sweeper approaches. One shoe is missing. Your phone has 3% battery.";
        private const string BUSH =
            "THE BUSH\nYou're in a hedge outside someone's house. A sprinkler turns on and soaks you. A dog stares at you through a window. Your pockets are full of leaves.";
        private const string UGLY_GIRL =
            "UGLY GIRL'S BED\nYou wake up in a stranger's apartment. 'Live Laugh Love' in Comic Sans on the wall. A cat sits on your chest. Someone is humming aggressively off-key in the kitchen.";
        private const string UGLY_MAN =
            "UGLY MAN'S BED\nStudio apartment. Sword collection on the wall. A ferret is loose somewhere. He's asleep next to you in a sleep apnea mask that sounds like a broken vacuum. Your shirt is inside out.";
        private const string EX =
            "YOUR EX-GIRLFRIEND'S\nYou wake up on her couch. She's sitting across from you with coffee, fully dressed, looking at you with pity. Your phone shows a sent text at 1:47 AM: 'I miss youuuuuuiuuuu.' She drives you home in silence.";
        private const string JAIL =
            "JAIL\nYou're on a bench in a holding cell. Your pants are wet and it's ambiguous whose fault that is. A guy in the next cell sings the same line on loop. The officer eats a sandwich while making eye contact.";
        private const string JUNKYARD =
            "THE JUNKYARD\nYou're in the driver's seat of a car with no engine. In a junkyard. You're wearing a high-vis vest that isn't yours, holding a wrench. There's a half-eaten gas station burrito on the dashboard. Some questions don't have answers.";
    }
}
