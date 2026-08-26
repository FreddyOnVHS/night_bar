// PatronDialogues_Stubs.cs
// Stub implementations for all remaining patrons.
// Each is a minimal valid PatronDialogue so the project compiles.
// Replace each Build() with full arc content as production work continues.

using NightAtTheBar.Dialogue;
using System.Collections.Generic;

namespace NightAtTheBar
{
    // ── Helper for building stub patrons quickly ──────────────────────────────
    internal static class StubHelper
    {
        public static PatronDialogue Stub(PatronId id, int startFriendship,
            string coolGreeting, string hostileGreeting, string antagonistGreeting,
            string fightLine = null, string postFightLine = null,
            params DialogueArc[] arcs)
        {
            return PatronDialogueFactory.Create(
                id, startFriendship,
                drunkRejection:      null,
                coolGreeting:        coolGreeting,
                hostileGreeting:     hostileGreeting,
                antagonistGreeting:  antagonistGreeting,
                fightLine:           fightLine,
                deEscalateLine:      null,
                postFightLine:       postFightLine,
                arcs);
        }
    }

    public static class PatronDialogues_Crier
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Crier, 10,
            "They look at their drink.",
            "I trusted you with something.",
            "I think I'm going to head out.",
            arcs: ArcBuilder.Create(PatronId.Crier, "cry_arc1", "We just grew apart", 1, FriendshipTier.Stranger)
                .Beat("c1_b1", "Sorry if I seem — I'm fine. I'm fine. How are you?")
                    .Choice("c1_a", "You don't seem fine.").Friendship(15).Log("\"Ha. No. I'm really not.\" They seem relieved.").GoTo("c1_b2")
                    .Choice("c1_b", "Rough night?").Friendship(12).Log("\"Rough month.\"").GoTo("c1_b2")
                .Beat("c1_b2", "We just... grew apart. That's what people say, right?")
                    .Choice("c1_b2_a", "Is that what happened?").Friendship(15).Log("\"I think so. I think that's the easy version.\"")
                    .Choice("c1_b2_b", "What's the hard version?").Friendship(18).UnlockArc("cry_arc2").Log("\"I don't know yet.\"")
                .Build(),
            ArcBuilder.Create(PatronId.Crier, "cry_neg_hostile", "Hostile", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("cnh", "I trusted you with something and you made it a joke.")
                    .Choice("cnh_a", "You're right. I'm sorry.").Friendship(-20).Log("\"Okay.\" Not forgiven. But acknowledged.")
                    .Choice("cnh_b", "It wasn't a big deal.").Friendship(-50).Log("\"It was to me.\" They turn away.")
                .Build()
        );
    }

    public static class PatronDialogues_OffDuty
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.OffDuty, 0,
            "*Shorter answers. Stops volunteering information.*",
            "I'm off the clock.",
            "*She doesn't look up when the time hits 12:30.*",
            arcs: ArcBuilder.Create(PatronId.OffDuty, "od_arc1", "Drink tips", 1, FriendshipTier.Stranger)
                .Beat("od1_b1", "You ordered that wrong, by the way.")
                    .Choice("od_a", "How do you know how I ordered?").Friendship(15)
                        .Log("\"I could hear you from here. Order the same thing light on ice. Better pour.\"")
                    .Choice("od_b", "How would you know?").Friendship(15)
                        .Log("\"I'm a bartender. Different bar, but still.\"")
                .Build(),
            ArcBuilder.Create(PatronId.OffDuty, "od_arc5_window", "The number", 5, FriendshipTier.Friend)
                .RequiresArc("od_arc1").RequiresTime(740).RequiresBefore(750)
                .Beat("od5_b1", "Hey. *She reaches for her jacket, then stops.*")
                    .Choice("od5_a", "Leaving already?").Friendship(30)
                        .Log("She slides a napkin over. A phone number. No name. One free drink per night from here on.")
                        .TriggerEvent("offduty_number_given")
                .Build(),
            ArcBuilder.Create(PatronId.OffDuty, "od_neg_hostile", "Off the clock", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("odnh", "I'm off the clock.")
                    .Choice("odnh_a", "I'm sorry.").Friendship(-15).Log("\"Okay.\" Back to -15.")
                    .Choice("odnh_b", "You're a bartender though.").Friendship(-50).Log("\"I'm not tonight.\"")
                .Build()
        );
    }

    public static class PatronDialogues_Buyer
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Buyer, 5,
            "*They stop offering rounds. Just drink alone.*",
            "I was trying to be friendly.",
            "You know what? Forget it.",
            arcs: ArcBuilder.Create(PatronId.Buyer, "buy_arc1", "What are you having?", 1, FriendshipTier.Stranger)
                .Beat("b1_b1", "*They wave down the bartender before you've sat down.* \"Whatever they're having. And another for me.\"")
                    .Choice("b1_a", "You didn't have to—").Friendship(10).Log("\"I wanted to. Sit down.\"")
                    .Choice("b1_b", "Thanks.").Friendship(12).Drunk(18)
                    .Choice("b1_c", "I was going to get water.").Friendship(15).Log("\"Get water too. Live a little.\"")
                .Build(),
            ArcBuilder.Create(PatronId.Buyer, "buy_arc5", "I took the offer", 5, FriendshipTier.Friend)
                .RequiresArc("buy_arc1").RequiresNight(2)
                .Beat("b5_b1", "*Different seat tonight. \"I called her.\"*")
                    .Choice("b5_a", "Your sister?").Friendship(15).Log("\"Yeah.\" Something lighter about them.")
                    .Choice("b5_b", "And?").Friendship(15)
                .Build(),
            ArcBuilder.Create(PatronId.Buyer, "buy_neg_hostile", "Hostile", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("bnh", "I was trying to be friendly. Apparently that's a problem.")
                    .Choice("bnh_a", "It's not.").Friendship(-20).Log("\"Then act like it.\"")
                    .Choice("bnh_b", "You buy drinks for everyone.").Friendship(-50).Log("\"Yeah. And?\"")
                .Build()
        );
    }

    public static class PatronDialogues_Storyteller
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Storyteller, 5,
            "*They stop mid-story. Finish it shorter.*",
            "I get it. The stories aren't your thing.",
            "*They tell a story about you. To the whole bar. Observational. Everyone hears it.*",
            arcs: ArcBuilder.Create(PatronId.Storyteller, "st_arc1", "The ghost in stall three", 1, FriendshipTier.Stranger)
                .Beat("st1_b1", "You been in that bathroom yet?")
                    .Choice("st1_a", "Yeah.").Friendship(12).Log("\"Notice anything... off?\"")
                    .Choice("st1_b", "Not yet.").Friendship(15).Log("\"When you go — look in the third stall.\"")
                .Build(),
            ArcBuilder.Create(PatronId.Storyteller, "st_neg_hostile", "Stories not your thing", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("stnh", "I get it. The stories aren't your thing.")
                    .Choice("stnh_a", "They're fine.").Friendship(-25).Log("\"High praise.\"")
                    .Choice("stnh_b", "Some of them must be true.").Friendship(15).Log("\"That's the most interesting question you've asked.\"")
                .Build(),
            ArcBuilder.Create(PatronId.Storyteller, "st_recovery", "Laugh at yourself", 11, FriendshipTier.Hostile, FriendshipTier.Cool)
                .Recovery()
                .Beat("str_b1", "*They told a story about you. You can laugh at it.*")
                    .Choice("str_a", "*laugh genuinely*").Friendship(20).Log("\"Okay. I like you now.\"")
                    .Choice("str_b", "*walk away*").Log("They carry on.")
                .Build()
        );
    }

    public static class PatronDialogues_ConspiracyGuy
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.ConspiracyGuy, 10,
            "\"Not everyone's ready for it.\"",
            "\"You're like everyone else. I thought you were different.\"",
            "*He's following you anyway. It's surveillance energy now.*",
            arcs: ArcBuilder.Create(PatronId.ConspiracyGuy, "cg_arc1", "The moon", 1, FriendshipTier.Stranger)
                .Beat("cg1_b1", "Can I ask you something? You seem like someone who thinks about things.")
                    .Choice("cg1_a", "Sometimes.").Friendship(12).Log("\"The moon.\" He says it like a topic sentence.")
                    .Choice("cg1_b", "Sure.").Friendship(12)
                .Build(),
            ArcBuilder.Create(PatronId.ConspiracyGuy, "cg_arc6", "I think I found it", 6, FriendshipTier.Friend)
                .RequiresArc("cg_arc1").RequiresNight(4)
                .Beat("cg6_b1", "I'm going to stop coming here.")
                    .Choice("cg6_a", "Why?").Friendship(15).Log("\"I think I found what I was looking for.\"")
                    .Choice("cg6_b", "The moon?").Friendship(25).Style(15).Log("He laughs. First genuine one. \"No. Not the moon.\"")
                    .Choice("cg6_c", "*say nothing*").Friendship(12)
                .Build(),
            ArcBuilder.Create(PatronId.ConspiracyGuy, "cg_neg_hostile", "Like everyone else", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("cgnh", "You're like everyone else. I thought you were different.")
                    .Choice("cgnh_a", "I'm not like everyone else.").Friendship(20).Log("\"Prove it.\" Recovery path.")
                    .Choice("cgnh_b", "*say nothing*").Friendship(-50)
                .Build()
        );
    }

    public static class PatronDialogues_Divorce
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Divorce, 5,
            "*He goes quiet.*",
            "I didn't come here to be the sad divorced guy.",
            "*He doesn't fight. He gives you the lawyer's card. Wrong side.*",
            arcs: ArcBuilder.Create(PatronId.Divorce, "div_arc1", "The ring", 1, FriendshipTier.Stranger)
                .Beat("div1_b1", "*He's looking at his left hand. Catches himself.* \"Sorry. Habit.\"")
                    .Choice("div1_a", "The ring?").Friendship(15).Log("\"You noticed that.\"")
                    .Choice("div1_b", "Don't worry about it.").Friendship(12)
                    .Choice("div1_c", "Is it new?").Friendship(18).Log("\"The forgetting? Or the ring?\"")
                .Build(),
            ArcBuilder.Create(PatronId.Divorce, "div_neg_hostile", "Hostile", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("divnh", "I didn't come here to be the sad divorced guy.")
                    .Choice("divnh_a", "That's not—").Friendship(-30).Log("\"It kind of is though.\"")
                    .Choice("divnh_b", "Have you been through it?").ShowIf(ConditionType.RandomChance, iv:50)
                        .Friendship(10).Log("Opens a door if you answer honestly.")
                .Build()
        );
    }

    public static class PatronDialogues_Nurse
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Nurse, -5,
            "*Book comes back out. Face down again.*",
            "I'm sorry. I thought I was clear that I'm off tonight.",
            "*She assesses you clinically and tells the bartender your approximate intoxication level.*",
            arcs: ArcBuilder.Create(PatronId.Nurse, "nur_arc1", "The book", 1, FriendshipTier.Stranger)
                .RequiresDrunkBelow(76)
                .Beat("nur1_b1", "*She doesn't look up.* \"Don't.\" *About the book.*")
                    .Choice("nur1_a", "I wasn't going to say anything.").Friendship(12).Log("\"Everyone says something.\"")
                    .Choice("nur1_b", "Is it good?").Friendship(15).Log("\"Embarrassing and wonderful and none of your business.\"")
                    .Choice("nur1_c", "I didn't see the cover.").Friendship(18).Log("\"Good.\" She closes it. Gives you her attention.")
                .Build(),
            ArcBuilder.Create(PatronId.Nurse, "nur_arc5", "Better read than me", 5, FriendshipTier.Friend)
                .RequiresArc("nur_arc1")
                .Beat("nur5_b1", "*She puts the book down.* \"I'm going to give this to you.\"")
                    .Choice("nur5_a", "I don't read romance novels.").Friendship(15).Log("\"You will.\"")
                    .Choice("nur5_b", "Why?").Friendship(20)
                        .Log("\"Because the third act is good and you seem like someone who needs to see how that works.\"")
                        .TriggerEvent("nurse_perk_active")
                .Build(),
            ArcBuilder.Create(PatronId.Nurse, "nur_neg_hostile", "Off the clock", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("nurnh", "I'm sorry. I thought I was clear that I'm off tonight.")
                    .Choice("nurnh_a", "You were. I'm sorry.").Friendship(-15).Log("\"Okay.\" Back to -15.")
                    .Choice("nurnh_b", "I was just—").Friendship(-50).Log("\"You keep starting sentences with that.\"")
                .Build()
        );
    }

    public static class PatronDialogues_RecentlySingle
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.RecentlySingle, 15,
            "*They're quieter. Not making eye contact.*",
            "I thought this was going to be a good night.",
            "*They move booths. Performing fine more aggressively over there.*",
            arcs: ArcBuilder.Create(PatronId.RecentlySingle, "rs_arc1", "First night out", 1, FriendshipTier.Stranger)
                .Beat("rs1_b1", "*Singing along. Stops mid-word.* \"Sorry. I'm — it's fine. I'm great. How are you?\"")
                    .Choice("rs1_a", "You seem great.").Friendship(12)
                        .Log("\"I AM great. I've been saying it for two weeks and it's starting to work.\"")
                    .Choice("rs1_b", "Are you okay?").Friendship(15)
                    .Choice("rs1_c", "Can I sit here?").Friendship(15)
                        .Log("\"Please. Yes. I've been talking to the bartender.\"")
                .Build(),
            ArcBuilder.Create(PatronId.RecentlySingle, "rs_neg_hostile", "Hostile", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("rsnh", "I thought this was going to be a good night.")
                    .Choice("rsnh_a", "It still can be.").Friendship(-15).Log("\"Maybe.\"")
                    .Choice("rsnh_b", "You'll be okay.").Friendship(-40)
                        .Log("\"I KNOW I'll be okay. I just—\" They stop. \"I know.\"")
                .Build()
        );
    }

    public static class PatronDialogues_YouthPastor
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.YouthPastor, 10,
            "*Still kind. But less chatty.*",
            "I'm going to be honest with you. You've been unkind tonight.",
            "\"I'm going to pray for you.\"",
            arcs: ArcBuilder.Create(PatronId.YouthPastor, "yp_arc1", "Not judging", 1, FriendshipTier.Stranger)
                .Beat("yp1_b1", "\"Hey! This seat taken? I'm not judging anything by the way. I just wanted to say that upfront.\"")
                    .Choice("yp1_a", "...okay.").Friendship(12)
                    .Choice("yp1_b", "Why would you be judging?").Friendship(15)
                    .Choice("yp1_c", "You can relax.").Friendship(18).Log("He does. Visibly. \"Thank you. I've been sitting very straight.\"")
                .Build(),
            ArcBuilder.Create(PatronId.YouthPastor, "yp_neg_hostile", "You've been unkind", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("ypnh", "I'm going to be honest with you. You've been unkind tonight.")
                    .Choice("ypnh_a", "You're right.").Friendship(20).Log("\"Thank you.\" Back to -10. He means the thanks.")
                    .Choice("ypnh_b", "I was just joking.").Friendship(-40).Log("\"I know. That's sometimes the problem.\"")
                    .Choice("ypnh_c", "So what.").Friendship(-55).Log("He looks at you. Not angry. Genuinely sad. Worse than anger.")
                .Build(),
            ArcBuilder.Create(PatronId.YouthPastor, "yp_neg_antagonist", "Prayer", 11, FriendshipTier.Antagonist, FriendshipTier.Antagonist)
                .Negative()
                .Beat("ypna", "\"I'm going to pray for you. Not because I think you're bad. Because I think you're stuck.\"")
                    .Choice("ypna_a", "*receive it*")
                        .Log("Somehow that's the worst thing anyone has said to you in this bar.")
                        .End()
                .Build()
        );
    }

    public static class PatronDialogues_Politician
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Politician, 5,
            "*He stops with the cards. Still friendly. Just less.*",
            "The park is real. The work is real.",
            "*He calls the bartender. Professionally.*",
            fightLine: "*He calls the bouncer over. Uses his political voice.*",
            postFightLine: "*He waves from across the bar. Offers a card again.*",
            arcs: ArcBuilder.Create(PatronId.Politician, "pol_arc1", "Vote Hendricks", 1, FriendshipTier.Stranger)
                .Beat("pol1_b1", "\"Hi! Jim Hendricks, city council. Great to meet you. Can I count on your support?\"")
                    .Choice("pol1_a", "I don't know who you are.").Friendship(10)
                    .Choice("pol1_b", "Sure.").Friendship(12)
                    .Choice("pol1_c", "I'm not interested in networking right now.").Friendship(18)
                        .Log("He looks startled. Then: \"Fair. Yeah. Fair.\" He puts the cards away.")
                .Build(),
            ArcBuilder.Create(PatronId.Politician, "pol_arc4", "The park passed", 4, FriendshipTier.Friend)
                .RequiresArc("pol_arc1")
                .Beat("pol4_b1", "*He comes in. He's got a folder. Trying not to smile.* \"It passed.\"")
                    .Choice("pol4_a", "The park?").Friendship(20).Log("\"The park.\" He puts the folder down.")
                    .Choice("pol4_b", "Third committee?").Friendship(22).Log("\"Unanimously.\"")
                .Build(),
            ArcBuilder.Create(PatronId.Politician, "pol_neg_hostile", "The park is real", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("polnh", "The park is real. The work is real.")
                    .Choice("polnh_a", "Fair point.").Friendship(20).Log("\"Thank you.\" Back to -10.")
                    .Choice("polnh_b", "All politicians say that.").Friendship(-40)
                .Build()
        );
    }

    public static class PatronDialogues_Dog
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Dog, 20,
            "*Biscuit moves to the other end of the booth.*",
            "*Biscuit saw that. Biscuit remembers.*",
            "*Biscuit sits with someone else.*",
            arcs: ArcBuilder.Create(PatronId.Dog, "dog_arc1", "The drink ticket", 1, FriendshipTier.Stranger)
                .Beat("dog1_b1", "*There's a large dog tied to a barstool. He has a drink ticket in his mouth. He looks at you.*")
                    .Choice("dog1_a", "Good boy.").Friendship(12).Log("He wags. He is a good boy. He's aware.")
                    .Choice("dog1_b", "Where did you get that ticket?").Friendship(18).Log("He doesn't say. He will never say.")
                    .Choice("dog1_c", "*take the ticket*").Friendship(15).GrantTicket(1)
                        .Log("He seems satisfied. This was his plan.")
                .Build(),
            ArcBuilder.Create(PatronId.Dog, "dog_neg_hostile", "Biscuit remembers", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative()
                .Beat("dognh", "*Biscuit saw you take the jerky. He moves away.*")
                    .Choice("dognh_a", "*give him a bar snack*").Friendship(25)
                        .Log("Biscuit thinks about it. Accepts. But his eyes say: I haven't forgotten.")
                    .Choice("dognh_b", "*ignore it*").Log("He settles elsewhere.")
                .Build()
        );
    }

    public static class PatronDialogues_Twins
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Twins, 0,
            "*\"Not a talker.\" / \"We noticed.\" / \"That's fine.\" / \"We'll stop.\"*",
            "*\"You don't like us.\" / \"It's okay.\" / \"Not everyone does.\"*",
            "*They know something about you. They tell someone else.*",
            arcs: ArcBuilder.Create(PatronId.Twins, "tw_arc1", "We finish", 1, FriendshipTier.Stranger)
                .Beat("tw1_b1", "\"You look like—\" / \"—someone who hasn't been here before.\"")
                    .Choice("tw1_a", "That obvious?").Friendship(12).Log("\"A little.\" / \"Kind of a lot.\"")
                    .Choice("tw1_b", "How did you—").Friendship(15).Log("\"We just—\" / \"—do that.\" / \"Yes.\"")
                .Build(),
            ArcBuilder.Create(PatronId.Twins, "tw_arc3", "The hint", 3, FriendshipTier.Acquaintance)
                .RequiresArc("tw_arc1").Repeatable()
                .Beat("tw3_b1", "\"Who do you want to know about?\"")
                    .Choice("tw3_a", "*choose a patron*")
                        .Friendship(20)
                        .TriggerEvent("twins_patron_hint")
                        .Log("They give you one piece of information about that patron's next arc.")
                .Build(),
            ArcBuilder.Create(PatronId.Twins, "tw_neg_hostile", "You don't like us", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("twnh", "\"You don't like us.\" / \"It's okay.\" / \"Not everyone does.\" / \"It's a lot.\" / \"We know.\"")
                    .Choice("twnh_a", "It's not that.").Friendship(10).Log("\"Then what is it?\" / \"And what is it?\"")
                    .Choice("twnh_b", "It is a little much.").Friendship(15).Log("\"Fair.\" / \"Very fair.\" / \"We respect honesty.\"")
                    .Choice("twnh_c", "*say nothing*").Friendship(-50)
                .Build()
        );
    }

    public static class PatronDialogues_FormerChef
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.FormerChef, -5,
            "*\"Right. Of course.\" She picks up her drink.*",
            "I worked in that kitchen for three years. It mattered.",
            "*She critiques everything you eat or drink.*",
            arcs: ArcBuilder.Create(PatronId.FormerChef, "fc_arc1", "They stopped serving food", 1, FriendshipTier.Stranger)
                .Beat("fc1_b1", "*She's looking at the menu. Making a face.* \"This used to say something different.\"")
                    .Choice("fc1_a", "The menu?").Friendship(15).Log("\"I wrote that menu. The old one.\"")
                    .Choice("fc1_b", "Were you here before?").Friendship(12).Log("\"I ran the kitchen. Three years.\"")
                .Build(),
            ArcBuilder.Create(PatronId.FormerChef, "fc_arc3_flask", "The flask", 3, FriendshipTier.Acquaintance)
                .RequiresArc("fc_arc1")
                .Beat("fc3_b1", "\"I brought something. You didn't hear this from me.\"")
                    .Choice("fc3_a", "*drink it*").Friendship(20).Drunk(-15).Boredom(-10)
                        .Log("It's incredible. She nods. \"Right.\"")
                        .TriggerEvent("flask_given")
                .Build(),
            ArcBuilder.Create(PatronId.FormerChef, "fc_neg_hostile", "It mattered", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("fcnh", "I worked in that kitchen for three years. It mattered.")
                    .Choice("fcnh_a", "I know. I wasn't—").Friendship(-25)
                    .Choice("fcnh_b", "Tell me about it.").Friendship(15).Log("She looks at you. \"Now you want to know?\"")
                    .Choice("fcnh_c", "Three years isn't that long.").Friendship(-55).Log("Very cold look.")
                .Build()
        );
    }

    public static class PatronDialogues_Insomniac
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Insomniac, 0,
            "\"Right. Cool. Good for you.\"",
            "I've had this conversation with people who slept last night.",
            "*She logs you on her phone.*",
            arcs: ArcBuilder.Create(PatronId.Insomniac, "ins_arc1", "Six weeks", 1, FriendshipTier.Stranger)
                .RequiresTime(660) // 11pm+
                .Beat("ins1_b1", "\"What time is it?\"")
                    .Choice("ins1_a", "*tell her*").Friendship(12).Log("\"I've been awake since—\" She does math. \"Yesterday.\"")
                    .Choice("ins1_b", "Late.").Friendship(15).Log("\"That's relative.\"")
                    .Choice("ins1_c", "You look tired.").Friendship(18).Log("\"Six weeks.\" Just that.")
                .Build(),
            ArcBuilder.Create(PatronId.Insomniac, "ins_arc3_pill", "The pill", 3, FriendshipTier.Acquaintance)
                .RequiresArc("ins_arc1").RequiresTime(660)
                .Beat("ins3_b1", "\"I want to give you something. Not weird.\"")
                    .Choice("ins3_a", "Mostly melatonin.").Friendship(18).Log("\"The rest is herbal. I think.\"")
                    .Choice("ins3_b", "*take it*").Friendship(20).Boredom(-20).Drunk(5)
                        .TriggerEvent("insomniac_pill_taken")
                .Build(),
            ArcBuilder.Create(PatronId.Insomniac, "ins_neg_hostile", "Hostile", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("insnh", "I've had this conversation with people who slept last night and it goes the same way.")
                    .Choice("insnh_a", "I didn't mean—").Friendship(-25)
                    .Choice("insnh_b", "Have you tried melatonin?").Friendship(-55).Log("\"I'm going to stop you there.\"")
                .Build()
        );
    }

    public static class PatronDialogues_Widower
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Widower, 10,
            "*He smiles. Smaller than before. Goes back to his drink.*",
            "\"I'm sorry. I've been talking too much tonight.\"",
            "*He doesn't get angry. He gets quiet. The specific quiet of someone who has learned to carry things alone.*",
            arcs: ArcBuilder.Create(PatronId.Widower, "wid_arc1", "He brings her up first", 1, FriendshipTier.Stranger)
                .Beat("wid1_b1",
                    "\"She used to love nights like this. Something about the light in here in winter. Couldn't explain it.\"")
                    .Choice("wid1_a", "She?").Friendship(15).Log("\"My wife. Margaret. Ten years gone.\"")
                    .Choice("wid1_b", "Sounds like she had good taste.").Friendship(18)
                        .Log("He smiles. \"In bars, certainly. In husbands, debatable.\"")
                .Build(),
            ArcBuilder.Create(PatronId.Widower, "wid_arc5", "Thirty years", 5, FriendshipTier.Friend)
                .RequiresArc("wid_arc1").RequiresNight(3)
                .Beat("wid5_b1", "Thirty-one years. I'd do them all again. The good parts twice.")
                    .Choice("wid5_a", "*receive it*").Friendship(30).Style(25)
                        .Log("You believe him. That's the remarkable part. You completely believe him.")
                .Build(),
            ArcBuilder.Create(PatronId.Widower, "wid_neg_hostile", "Talking too much", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("widnh", "\"I'm sorry. I've been talking too much tonight.\"")
                    .Choice("widnh_a", "You haven't.").Friendship(-20).Log("\"I have. It's alright.\"")
                    .Choice("widnh_b", "Please don't stop.").Friendship(-30).Log("\"I think I will.\"")
                    .Choice("widnh_c", "Did I say something?").Friendship(-25)
                        .Log("\"No. Some nights the talking makes it bigger.\"")
                .Build()
        );
    }

    public static class PatronDialogues_Kid
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.Kid, 5,
            "*He's quieter. Trying to look like he was never excited.*",
            "\"I know I'm new at this.\"",
            "*He's performing Very Much Fine across the bar.*",
            arcs: ArcBuilder.Create(PatronId.Kid, "kid_arc1", "He's so nervous", 1, FriendshipTier.Stranger)
                .Beat("kid1_b1", "*He spots you and immediately tries to look casual.* \"Hey. Hey. Cool bar.\"")
                    .Choice("kid1_a", "First time in a bar?").Friendship(12).Log("\"What? No. No. Maybe.\"")
                    .Choice("kid1_b", "You okay?").Friendship(15)
                    .Choice("kid1_c", "Cool bar.").Friendship(10).Log("\"Right? I found it. On my phone.\"")
                .Build(),
            ArcBuilder.Create(PatronId.Kid, "kid_arc3_drunk", "Too drunk", 3, FriendshipTier.Acquaintance)
                .Beat("kid3_b1", "*The Kid is at the bar. He's had too many.*")
                    .Choice("kid3_a", "*help him out*").Friendship(20)
                        .Log("He says thank you four times. The last one sticks.")
                        .TriggerEvent("kid_helped")
                        .Time(5)
                    .Choice("kid3_b", "*let the bartender handle it*").Friendship(5).Style(-10)
                    .Choice("kid3_c", "*do nothing*").Friendship(2).Style(-5)
                        .TriggerEvent("veteran_helps_kid")
                .Build(),
            ArcBuilder.Create(PatronId.Kid, "kid_neg_hostile", "I know I'm new", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("kidnh", "\"I know I'm new at this.\"")
                    .Choice("kidnh_a", "It's fine.").Friendship(-20).Log("\"Yeah.\" Wounded.")
                    .Choice("kidnh_b", "You'll figure it out.").Friendship(-30).Log("\"I'm fine.\" He's not fine.")
                .Build()
        );
    }

    public static class PatronDialogues_Detective
    {
        public static PatronDialogue Build() => StubHelper.Stub(
            PatronId.RetiredDetective, -5,
            "*She makes a note. Doesn't share it.*",
            "You performed answering that. I was asking for something real.",
            "*She tells you what she's concluded about you. Specific. Accurate. Uncomfortable.*",
            arcs: ArcBuilder.Create(PatronId.RetiredDetective, "det_arc1", "She already knows", 1, FriendshipTier.Stranger)
                .RequiresDay(6) // Sunday only
                .Beat("det1_b1", "*She looks at you when you sit nearby. Then looks away. Then back.* \"You've had a long week.\"")
                    .Choice("det1_a", "How did you—").Friendship(15)
                        .Log("\"Posture. How you scanned the room when you came in. The drink you ordered.\"")
                    .Choice("det1_b", "Retired detective.").Friendship(18)
                .Build(),
            ArcBuilder.Create(PatronId.RetiredDetective, "det_arc3_question", "The hard question", 3, FriendshipTier.Acquaintance)
                .RequiresArc("det_arc1").RequiresDay(6)
                .Beat("det3_b1", "\"Can I ask you something direct? I don't do the soft version of questions.\"")
                    .Choice("det3_a", "Go ahead.").Friendship(12).GoTo("det3_b2")
                    .Choice("det3_b", "How direct?").Friendship(15).Log("\"Very.\"").GoTo("det3_b2")
                .Beat("det3_b2", "\"Why do you keep coming back here?\"")
                    .Choice("det3_q1", "The drinks.").Friendship(10)
                    .Choice("det3_q2", "The people.").Friendship(18)
                    .Choice("det3_q3", "I don't know.").Friendship(20).Log("\"That's the honest answer.\"")
                    .Choice("det3_q4", "Something I'm looking for.").Friendship(22).Style(10)
                .Build(),
            ArcBuilder.Create(PatronId.RetiredDetective, "det_arc5", "One last case", 5, FriendshipTier.Friend)
                .RequiresArc("det_arc3_question").RequiresDay(6)
                .Beat("det5_b1", "*She has a file on the table. Closes it when you sit down.*")
                    .Choice("det5_a", "Case?").Friendship(15)
                    .Choice("det5_b", "Still working?").Friendship(18)
                        .Log("\"Never stopped. Not officially. Some things deserve more attention than they got.\"")
                .Build(),
            ArcBuilder.Create(PatronId.RetiredDetective, "det_neg_hostile", "Performed answering", 10, FriendshipTier.Hostile, FriendshipTier.Hostile)
                .Negative().Repeatable()
                .Beat("detnh", "You performed answering that. I was asking for something real.")
                    .Choice("detnh_a", "Fair.").Friendship(-10).Log("\"Thank you for that at least.\"")
                    .Choice("detnh_b", "I thought I answered.").Friendship(-25).Log("\"You answered something. Not what I asked.\"")
                    .Choice("detnh_c", "What do you want me to say?").Friendship(10)
                        .Log("\"Nothing. I was describing. Not accusing.\"")
                .Build()
        );
    }
}
