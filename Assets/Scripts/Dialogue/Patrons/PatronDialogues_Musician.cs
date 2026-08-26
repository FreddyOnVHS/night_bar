// PatronDialogues_Musician.cs
using NightAtTheBar.Dialogue;

namespace NightAtTheBar
{
    public static class PatronDialogues_Musician
    {
        public static PatronDialogue Build() => PatronDialogueFactory.Create(
            id:                  PatronId.Musician,
            startingFriendship:  -5,
            drunkRejection:      "She doesn't look up. \"I'm working.\"",
            coolGreeting:        "*She doesn't look up when you approach.*",
            hostileGreeting:     "\"I'm working.\"",
            antagonistGreeting:  "*She picks up her notebook and moves to a different seat.*",
            fightLine:           null,
            deEscalateLine:      null,
            postFightLine:       "*She's at a corner table. She glances over once.*",

            // ── Arc 1: The notebook ──────────────────────────────────────────
            ArcBuilder.Create(PatronId.Musician, "mus_arc1", "The notebook", 1, FriendshipTier.Stranger)
                .RequiresDrunkBelow(76) // drunk gate
                .Beat("m1_b1",
                    "*She finishes a line, then looks up. \"Sorry. Almost lost it.\"*")
                    .Choice("m1_b1_a", "Lost what?")
                        .Friendship(15)
                        .Log("\"The line. When it's there you have to—\" She gestures. You understand or you don't.")
                        .GoTo("m1_b2")
                    .Choice("m1_b1_b", "Don't stop on my account.")
                        .Friendship(12)
                        .Log("\"It's fine. It's gone now anyway.\"")
                        .GoTo("m1_b2")
                    .Choice("m1_b1_c", "What are you writing?")
                        .Friendship(15)
                        .Log("\"Lyrics. Sort of. More like notes toward lyrics.\"")
                        .GoTo("m1_b2")
                .Beat("m1_b2",
                    "I play here on Thursdays. Open mic. You should come sometime.",
                    "*She says it like she doesn't care if you come or not. She does, a little.*")
                    .Choice("m1_b2_a", "What kind of music?")
                        .Friendship(15)
                        .Log("\"Depends on the Thursday.\"")
                        .GoTo("m1_b3")
                    .Choice("m1_b2_b", "I'll try to make it.")
                        .Friendship(18)
                        .Log("\"Don't make it for me. Make it if you want to hear something good.\"")
                        .GoTo("m1_b3")
                    .Choice("m1_b2_c", "Are you any good?")
                        .Friendship(20)
                        .Log("She looks at you for a beat. \"I think so. The notebook would say yes.\"")
                        .GoTo("m1_b3")
                // Thursday variant
                    .Choice("m1_b2_d", "I play here tonight actually. I hope you stay.")
                        .ShowIfDay(3) // Thursday = index 3
                        .Friendship(18)
                        .GoTo("m1_b3")
                .Beat("m1_b3", "*She goes back to writing. Then, without looking up:* \"Get me a water? I keep forgetting.\"")
                    .Choice("m1_b3_a", "*do it*")
                        .Friendship(15)
                        .Log("She looks up when you put it down. \"Thank you.\" Like she means it.")
                    .Choice("m1_b3_b", "What do I look like, a waiter?")
                        .Friendship(10)
                        .Log("She smiles at the notebook. \"Fair.\"")
                    .Choice("m1_b3_c", "*flag down the bartender for her*")
                        .Friendship(18)
                        .Log("\"Oh. You didn't have to—\" \"I know.\" Good moment.")
                .Build(),

            // ── Arc 2: The songs ─────────────────────────────────────────────
            ArcBuilder.Create(PatronId.Musician, "mus_arc2", "The songs", 2, FriendshipTier.Acquaintance)
                .RequiresArc("mus_arc1")
                .RequiresDrunkBelow(76)
                .Beat("m2_b1",
                    "I've been working on the same song for three months. Same song. Different versions of it every night.")
                    .Choice("m2_b1_a", "What's it about?")
                        .Friendship(15)
                        .Log("\"I don't know yet. That's probably why it's taking three months.\"")
                        .GoTo("m2_b2")
                    .Choice("m2_b1_b", "Is it getting better?")
                        .Friendship(18)
                        .Log("\"The song is getting more honest. I'm not sure that's the same as better.\"")
                        .GoTo("m2_b2")
                    .Choice("m2_b1_c", "Play me a version.")
                        .Friendship(12)
                        .Log("Long pause. \"Not yet.\"")
                        .GoTo("m2_b2")
                .Beat("m2_b2",
                    "The problem is I keep protecting the person it's about. Softening it. And every time I soften it, it stops being true.")
                    .Choice("m2_b2_a", "Stop protecting them.")
                        .Friendship(15)
                        .Log("She looks at you. \"It's not that simple.\"")
                        .GoTo("m2_b3")
                    .Choice("m2_b2_b", "Maybe the protection is part of the song.")
                        .Friendship(22)
                        .Log("She writes something down. Doesn't explain what.")
                        .GoTo("m2_b3")
                    .Choice("m2_b2_c", "Who is it about?")
                        .Friendship(18)
                        .Log("\"Nobody yet. Everybody eventually. That's how it works.\"")
                        .GoTo("m2_b3")
                .Beat("m2_b3", "*She shows you one line. Just one. Covers the rest.*")
                    .Choice("m2_b3_a", "That's really good.")
                        .Friendship(18)
                        .Log("\"Yeah. That one's staying.\"")
                    .Choice("m2_b3_b", "*say nothing*")
                        .Friendship(20)
                        .Log("She watches your face. \"Okay. Good.\" Your expression was enough.")
                    .Choice("m2_b3_c", "What's the rest?")
                        .Friendship(15)
                        .Log("\"Not yet.\" Warmer than before.")
                .Build(),

            // ── Arc 3: Thursday show ─────────────────────────────────────────
            ArcBuilder.Create(PatronId.Musician, "mus_arc3", "The Thursday show", 3, FriendshipTier.Acquaintance)
                .RequiresArc("mus_arc2")
                .RequiresDay(3) // Thursday only
                .RequiresDrunkBelow(76)
                .Beat("m3_b1",
                    "*She's at the small stage area. Tuning. She sees you.* \"You came.\"")
                    .Choice("m3_b1_a", "I said I would.")
                        .Friendship(15)
                        .Log("\"People say things.\"")
                        .GoTo("m3_b2")
                    .Choice("m3_b1_b", "Wouldn't miss it.")
                        .Friendship(12)
                        .Log("She doesn't quite believe you yet. But she's glad.")
                        .GoTo("m3_b2")
                    .Choice("m3_b1_c", "*say nothing, just be there*")
                        .Friendship(18)
                        .GoTo("m3_b2")
                .Beat("m3_b2",
                    "*She plays four songs. The third one is the one she's been working on. She hesitates before it, and doesn't look at anyone during it.*")
                    .Choice("m3_b2_a", "The third one.")
                        .Friendship(20)
                        .Log("She looks at you sharply. \"What about it?\"")
                        .GoTo("m3_b3")
                    .Choice("m3_b2_b", "That was great.")
                        .Friendship(12)
                        .Log("\"Thank you.\" Careful.")
                        .GoTo("m3_b3")
                    .Choice("m3_b2_c", "*say nothing until she comes back*")
                        .Friendship(15)
                        .Log("She sits back down. \"Well?\"")
                        .GoTo("m3_b3")
                .Beat("m3_b3",
                    "The third one isn't done. But I had to play it. Sometimes you have to play the unfinished thing to know what's missing.")
                    .Choice("m3_b3_a", "What's missing?")
                        .Friendship(20)
                        .Log("She opens her notebook. \"I think I know now.\"")
                    .Choice("m3_b3_b", "I liked it unfinished.")
                        .Friendship(22)
                        .Log("She looks at you. Something shifts.")
                    .Choice("m3_b3_c", "Will you play it again when it's done?")
                        .Friendship(18)
                        .Log("\"If you're here on Thursday.\"")
                .Build(),

            // ── Arc 4: The notebook page ─────────────────────────────────────
            ArcBuilder.Create(PatronId.Musician, "mus_arc4", "The notebook page", 4, FriendshipTier.Friend)
                .RequiresArc("mus_arc3")
                .RequiresDrunkBelow(76)
                .Beat("m4_b1",
                    "*She puts the notebook on the table. Face up. Open.* \"I don't do this.\"")
                    .Choice("m4_b1_a", "Then don't.")
                        .Friendship(18)
                        .Log("\"I want to.\"")
                        .GoTo("m4_b2")
                    .Choice("m4_b1_b", "Okay.")
                        .Friendship(15)
                        .GoTo("m4_b2")
                    .Choice("m4_b1_c", "*say nothing*")
                        .Friendship(15)
                        .GoTo("m4_b2")
                .Beat("m4_b2",
                    "*The page is about someone. The details sound like you. Or close enough that it matters.* \"I wrote that before I knew you. Which is weird.\"")
                    .Choice("m4_b2_a", "That's very weird.")
                        .Friendship(20)
                        .Log("\"Yes.\"")
                        .GoTo("m4_b3")
                    .Choice("m4_b2_b", "Maybe you write what you're looking for.")
                        .Friendship(25)
                        .Log("She closes the notebook. Opens it again. \"Maybe.\"")
                        .GoTo("m4_b3")
                    .Choice("m4_b2_c", "Who did you think it was about?")
                        .Friendship(20)
                        .Log("\"I thought it was fiction.\"")
                        .GoTo("m4_b3")
                .Beat("m4_b3",
                    "I'm telling you this because I think you should know. Not because I know what to do with it.")
                    .Choice("m4_b3_a", "I don't know what to do with it either.")
                        .Friendship(25)
                        .Log("\"Good. Let's not do anything with it then.\" Something decided.")
                    .Choice("m4_b3_b", "Play me the finished song.")
                        .Friendship(20)
                        .Log("\"Come Thursday.\"")
                    .Choice("m4_b3_c", "*say nothing*")
                        .Friendship(22)
                        .Log("She closes the notebook. \"Okay.\"")
                .Build(),

            // ── Arc 5: The dedication ────────────────────────────────────────
            ArcBuilder.Create(PatronId.Musician, "mus_arc5", "The dedication", 5, FriendshipTier.Friend)
                .RequiresArc("mus_arc4")
                .RequiresDay(3) // Thursday only
                .RequiresDrunkBelow(76)
                .Beat("m5_b1",
                    "*Before her last song:* \"This one's for someone who doesn't know what to do with things. Which I think is the right person.\"")
                    .Choice("m5_b1_listen", "*listen*")
                        .Friendship(5)
                        .GoTo("m5_b2")
                .Beat("m5_b2",
                    "*She plays it. The finished version. The best thing you've heard in this bar. She doesn't look at anyone during it. Then at the end she does. It's you.*")
                    .Choice("m5_b2_after", "*wait for her to come back*")
                        .GoTo("m5_b3")
                .Beat("m5_b3", "Well?")
                    .Choice("m5_b3_a", "It's done.")
                        .Friendship(30)
                        .Style(25)
                        .Log("\"Yeah.\"")
                    .Choice("m5_b3_b", "*say nothing*")
                        .Friendship(35)
                        .Style(30)
                        .Log("Best response. She seems satisfied.")
                    .Choice("m5_b3_c", "That was the one.")
                        .Friendship(30)
                        .Style(25)
                        .Log("\"That was the one.\"")
                .Build(),

            // ── Negative arcs ────────────────────────────────────────────────
            ArcBuilder.Create(PatronId.Musician, "mus_neg_cool", "I'm working", 10, FriendshipTier.Cool, FriendshipTier.Cool)
                .Negative().Repeatable()
                .Beat("mn_cool", "*She doesn't look up.*")
                    .Choice("mn_cool_a", "*back off*")
                        .Log("Stays at -30. Can recover next approach.")
                        .End()
                    .Choice("mn_cool_b", "*hover*")
                        .Friendship(-20)
                        .Log("\"I'm working.\"")
                        .End()
                .Build(),

            ArcBuilder.Create(PatronId.Musician, "mus_neg_hostile", "I'm working - hostile", 11, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .RequiresDrunkAbove(74) // triggered by approaching drunk
                .Beat("mnh_b1", "\"I'm working.\"")
                    .Choice("mnh_a", "*back off*")
                        .Log("Stays at -30.")
                        .End()
                    .Choice("mnh_b", "Come on.")
                        .Friendship(-20)
                        .Log("\"I said I'm working.\"")
                        .End()
                .Build(),

            ArcBuilder.Create(PatronId.Musician, "mus_neg_antagonist", "She moves", 12, FriendshipTier.Antagonist, FriendshipTier.Antagonist)
                .Negative()
                .Beat("mna_b1", "*She picks up her notebook and moves to a different seat.*")
                    .Choice("mna_follow", "*follow her*")
                        .Friendship(-20)
                        .Log("\"Please don't.\" Final. Arc 2 locked permanently this night.")
                        .LockArc("mus_arc2")
                        .TriggerEvent("bartender_watching")
                        .End()
                    .Choice("mna_stay", "*respect it*")
                        .Log("The empty seat is pointed. But you don't make it worse.")
                        .End()
                .Build(),

            // ── Recovery (Thursday show, stay for whole set) ─────────────────
            ArcBuilder.Create(PatronId.Musician, "mus_recovery", "Stay for the set", 13, FriendshipTier.Hostile, FriendshipTier.Cool)
                .Recovery().RequiresDay(3)
                .Beat("mr_b1", "*You go to her Thursday show. You stay for the whole set. You don't approach after.*")
                    .Choice("mr_b1_a", "*stay, don't approach*")
                        .Friendship(20)
                        .Log("She notices. Next night: back to -5. Slow rebuild possible.")
                        .End()
                .Build()
        );
    }
}
