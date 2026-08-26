// PatronDialogues_Veteran.cs
using NightAtTheBar.Dialogue;

namespace NightAtTheBar
{
    public static class PatronDialogues_Veteran
    {
        public static PatronDialogue Build() => PatronDialogueFactory.Create(
            id:                  PatronId.Veteran,
            startingFriendship:  -10,
            drunkRejection:      "He looks at you for a moment. Then looks away.",
            coolGreeting:        "*He turns slightly away. Conversation over for tonight.*",
            hostileGreeting:     "I don't talk about it.",
            antagonistGreeting:  "*He is very still. That's not calm. That's discipline.*",
            fightLine:           "He stands up slowly. \"I'm going to ask you one time to walk away.\"",
            deEscalateLine:      "You walk away. He sits back down. You're at -90. Long road back.",
            postFightLine:       "*He nods when you come in. Not forgiveness. Just acknowledgment.*",

            // ── Arc 1: Watching the door ─────────────────────────────────────
            ArcBuilder.Create(PatronId.Veteran, "vet_arc1", "Watching the door", 1, FriendshipTier.Stranger)
                .Beat("v1_b1", "*He nods when you sit nearby. That's it. Waits to see if you make it weird.*")
                    .Choice("v1_b1_a", "*nod back*")
                        .Friendship(15)
                        .Log("Best response.")
                        .GoTo("v1_b2")
                    .Choice("v1_b1_b", "Good spot.")
                        .Friendship(12)
                        .Log("He looks at you. \"Yeah.\"")
                        .GoTo("v1_b2")
                    .Choice("v1_b1_c", "Are you waiting for someone?")
                        .Friendship(10)
                        .Log("\"No.\" Said without elaboration. But not coldly.")
                        .GoTo("v1_b2")
                .Beat("v1_b2", "*Later. He's on his second drink. Still nursing it.*")
                    .Choice("v1_b2_a", "What are you drinking?")
                        .Friendship(12)
                        .Log("He tells you. Unpretentious choice.")
                        .GoTo("v1_b3")
                    .Choice("v1_b2_b", "Good night to be out.")
                        .Friendship(10)
                        .Log("He considers this. \"Decent one.\"")
                        .GoTo("v1_b3")
                    .Choice("v1_b2_c", "*say nothing*")
                        .Friendship(15)
                        .Log("He seems comfortable with the silence.")
                        .GoTo("v1_b3")
                .Beat("v1_b3", "*At some point he says: \"You're not going to ask, are you.\" Not a question.*")
                    .Choice("v1_b3_a", "About what?")
                        .Friendship(18)
                        .Log("\"Right.\" He almost smiles.")
                    .Choice("v1_b3_b", "Ask what?")
                        .Friendship(15)
                        .Log("\"Nothing.\" But he seems to relax.")
                    .Choice("v1_b3_c", "Not my business.")
                        .Friendship(20)
                        .Log("\"Good.\"")
                .Build(),

            // ── Arc 2: The joke ──────────────────────────────────────────────
            ArcBuilder.Create(PatronId.Veteran, "vet_arc2", "The joke", 2, FriendshipTier.Acquaintance)
                .RequiresArc("vet_arc1")
                .Beat("v2_b1", "You want to hear something funny?")
                    .Choice("v2_b1_a", "Sure.")
                        .Friendship(12)
                        .GoTo("v2_b2")
                    .Choice("v2_b1_b", "Is it actually funny?")
                        .Friendship(15)
                        .Log("He thinks. \"Depends.\"")
                        .GoTo("v2_b2")
                .Beat("v2_b2",
                    "*He tells a joke. It's old. Military. Dry. The setup takes a while. The punchline is short. It's funnier than it has any right to be.*")
                    .Choice("v2_b2_a", "*laugh*")
                        .Friendship(20)
                        .Log("He nods. \"Yeah.\"")
                        .GoTo("v2_b3")
                    .Choice("v2_b2_b", "That's terrible.")
                        .Friendship(18)
                        .Log("\"Yeah.\" He's pleased.")
                        .GoTo("v2_b3")
                    .Choice("v2_b2_c", "Where did you hear that?")
                        .Friendship(22)
                        .UnlockArc("vet_arc3")
                        .Log("\"Made it up. 2007. Lot of downtime.\"")
                        .GoTo("v2_b3")
                .Beat("v2_b3", "*He goes back to his drink. Doesn't push the conversation.*")
                    .Choice("v2_b3_a", "You got more of those?")
                        .Friendship(15)
                        .Log("\"Few.\"")
                    .Choice("v2_b3_b", "2007.")
                        .Friendship(18)
                        .Log("He looks at his drink. \"Long deployment.\"")
                    .Choice("v2_b3_c", "*say nothing*")
                        .Friendship(12)
                        .Log("He seems fine with it.")
                .Build(),

            // ── Arc 3: Downtime ──────────────────────────────────────────────
            ArcBuilder.Create(PatronId.Veteran, "vet_arc3", "Downtime", 3, FriendshipTier.Acquaintance)
                .RequiresArc("vet_arc2")
                .Beat("v3_b1", "You asked about 2007.")
                    .Choice("v3_b1_a", "You don't have to—")
                        .Friendship(15)
                        .Log("\"I know.\"")
                        .GoTo("v3_b2")
                    .Choice("v3_b1_b", "If you want to.")
                        .Friendship(12)
                        .Log("\"Thought about it. Yeah.\"")
                        .GoTo("v3_b2")
                .Beat("v3_b2",
                    "Fourteen months. Helmand. The downtime was the part nobody tells you about. You make things during the downtime. Jokes. Routines. Ways of being.")
                    .Choice("v3_b2_a", "Ways of being.")
                        .Friendship(20)
                        .Log("\"You get used to certain things. Takes a while to get unused to them.\"")
                        .GoTo("v3_b3")
                    .Choice("v3_b2_b", "Do you miss it?")
                        .Friendship(22)
                        .Log("Long pause. \"The people. Not the rest.\"")
                        .GoTo("v3_b3")
                    .Choice("v3_b2_c", "How many deployments?")
                        .Friendship(18)
                        .Log("\"Three. That was the last.\"")
                        .GoTo("v3_b3")
                .Beat("v3_b3", "The door thing. You noticed.")
                    .Choice("v3_b3_a", "Hard not to.")
                        .Friendship(18)
                        .Log("\"Most people don't.\"")
                    .Choice("v3_b3_b", "Does it bother you?")
                        .Friendship(15)
                        .Log("\"Not anymore. Just is.\"")
                    .Choice("v3_b3_c", "Seems useful.")
                        .Friendship(22)
                        .Style(10)
                        .Log("He looks at you. Then: \"Yeah. Sometimes.\"")
                .Build(),

            // ── Arc 4: Water switch (passive — fired by GameManager) ─────────
            // This arc is triggered automatically when player drunk > 80 near Veteran at Friend tier
            ArcBuilder.Create(PatronId.Veteran, "vet_arc4_passive", "The water switch", 4, FriendshipTier.Friend)
                .RequiresArc("vet_arc3")
                .RequiresDrunkAbove(80)
                .Beat("v4_b1",
                    "*You didn't see it happen. Your drink is water. He's looking somewhere else.*")
                    .Choice("v4_b1_a", "Thank you.")
                        .Friendship(20)
                        .Log("He nods once.")
                    .Choice("v4_b1_b", "I had it handled.")
                        .Friendship(15)
                        .Log("\"I know.\" He doesn't make it a thing.")
                    .Choice("v4_b1_c", "*say nothing*")
                        .Friendship(22)
                        .Log("He expects nothing.")
                .Build(),

            // ── Arc 5: Good people ───────────────────────────────────────────
            ArcBuilder.Create(PatronId.Veteran, "vet_arc5", "Good people", 5, FriendshipTier.Friend)
                .RequiresArc("vet_arc3")
                .Beat("v5_b1",
                    "*He sits near you instead of the end. First time.*")
                    .Choice("v5_b1_a", "Different spot tonight.")
                        .Friendship(15)
                        .Log("\"Yeah.\"")
                        .GoTo("v5_b2")
                    .Choice("v5_b1_b", "Good view from here?")
                        .Friendship(12)
                        .Log("He looks around. \"Good enough.\"")
                        .GoTo("v5_b2")
                    .Choice("v5_b1_c", "*say nothing*")
                        .Friendship(18)
                        .Log("He orders. Asks if you need anything. First time.")
                        .GoTo("v5_b2")
                .Beat("v5_b2", "I want to say something. Don't make it weird.")
                    .Choice("v5_b2_a", "Okay.")
                        .Friendship(12)
                        .GoTo("v5_b3")
                    .Choice("v5_b2_b", "Wouldn't dream of it.")
                        .Friendship(15)
                        .Log("He almost smiles.")
                        .GoTo("v5_b3")
                // Auto-advance to final beat
                .Beat("v5_b3", "You're good people. That's all.")
                    .Choice("v5_b3_a", "*receive it*")
                        .Friendship(30)
                        .Style(25)
                        .Log("From him, that's a lot. You both know it.")
                .Build(),

            // ── Negative arcs ────────────────────────────────────────────────
            ArcBuilder.Create(PatronId.Veteran, "vet_neg_cool", "Cool", 10, FriendshipTier.Cool, FriendshipTier.Cool)
                .Negative().Repeatable()
                .Beat("vn_cool", "*He turns slightly away when you approach. Conversation is over for tonight.*")
                    .Choice("vn_cool_a", "*back off*")
                        .Log("Understood.")
                        .End()
                    .Choice("vn_cool_b", "*push it*")
                        .Friendship(-10)
                        .Log("He moves his drink. You're not getting anywhere tonight.")
                        .End()
                .Build(),

            ArcBuilder.Create(PatronId.Veteran, "vet_neg_hostile", "Hostile", 11, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("vnh_b1", "I don't talk about it.")
                    .Choice("vnh_a", "I understand.")
                        .Friendship(-15)
                        .Log("\"No, you don't. But I appreciate you saying so.\"")
                        .End()
                    .Choice("vnh_b", "I was just curious.")
                        .Friendship(-40)
                        .Log("\"I know.\" He finishes his drink.")
                        .End()
                    .Choice("vnh_c", "*push further*")
                        .Friendship(-20)
                        .Log("He goes very quiet.")
                        .End()
                .Build(),

            ArcBuilder.Create(PatronId.Veteran, "vet_neg_antagonist", "Very still", 12, FriendshipTier.Antagonist, FriendshipTier.Antagonist)
                .Negative().Repeatable()
                .Beat("vna_b1",
                    "*He is very still. That's not calm. That's discipline. He's watching the door. He's watching you too.*")
                    .Choice("vna_a", "*stay away*")
                        .Log("Smart.")
                        .End()
                    .Choice("vna_b", "*approach anyway*")
                        .Friendship(-20)
                        .Log("He stands. Not rushed. \"I'm going to ask you one time.\"")
                        .GoTo("vna_b2")
                .Beat("vna_b2", "Walk away.")
                    .Choice("vna_b2_walk", "*walk away*")
                        .Log("He sits back down. You're at -90. Long road.")
                        .End()
                    .Choice("vna_b2_stay", "*stay*")
                        .Friendship(-30)
                        .TriggerFight()
                        .End()
                .Build(),

            // ── Fight arc ────────────────────────────────────────────────────
            ArcBuilder.Create(PatronId.Veteran, "vet_fight", "The Fight", 13, FriendshipTier.Fight, FriendshipTier.Fight)
                .Fight()
                .Beat("vf_b1",
                    "One specific trigger: mocking his service. He stands up slowly. \"I'm going to ask you one time to walk away.\"")
                    .Choice("vf_b1_walk", "*walk away*")
                        .Friendship(10)
                        .Log("He sits back down. You're at -90 but no fight. Recovery possible next night.")
                        .End()
                    .Choice("vf_b1_stay", "*don't walk away*")
                        .Friendship(-30)
                        .TriggerEvent("bar_fight_veteran")
                        .TriggerEject()
                        .Log("Bar goes quiet. Both ejected. The most serious fight in the game.")
                        .End()
                .Build(),

            // ── Recovery (next night after fight) ────────────────────────────
            ArcBuilder.Create(PatronId.Veteran, "vet_recovery", "Recovery", 14, FriendshipTier.Antagonist, FriendshipTier.Cool)
                .Recovery().RequiresNight(1)
                .Beat("vr_b1",
                    "*He nods when you come in. Not forgiveness. Just acknowledgment.*")
                    .Choice("vr_b1_a", "*nod back*")
                        .Friendship(10)
                        .Log("You're at -50. Long road. But open.")
                        .End()
                    .Choice("vr_b1_b", "*sit near him without speaking first*")
                        .Friendship(5)
                        .Log("He allows it.")
                        .End()
                .Build()
        );
    }
}
