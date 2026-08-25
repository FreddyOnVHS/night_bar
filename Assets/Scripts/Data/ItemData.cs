// ItemData.cs
// Definitions for all inventory items and patron data.
// No Unity dependencies — pure C# data.

using System.Collections.Generic;

namespace NightAtTheBar
{
    public class ItemDefinition
    {
        public string       Id;
        public string       DisplayName;
        public string       Description;
        public ItemMechanic Mechanic;
        public float        Value;      // mechanic-specific (boredomMult, charges, etc.)
        public bool         Consumable; // destroyed on use
    }

    public class PatronDefinition
    {
        public PatronId Id;
        public string   DisplayName;
        public string   Description;
        public string   Perk;
        public string   Risk;
        public bool     CoreRegular;   // always in pool
        public int      LeaveMinute;   // 999 = stays all night
    }

    public static class ItemDatabase
    {
        public static readonly List<ItemDefinition> All = new()
        {
            // ── Common ───────────────────────────────────────────────────────
            new() { Id="ipod_2gb",   DisplayName="Crappy iPod (2GB)",          Description="Only has Smash Mouth and one Enya album.",             Mechanic=ItemMechanic.BoredomSlow, Value=0.80f },
            new() { Id="bear",       DisplayName="Tiny stuffed bear",           Description="It's damp for some reason.",                           Mechanic=ItemMechanic.Bear,        Value=1f    },
            new() { Id="sunglasses", DisplayName="Knockoff sunglasses",         Description="One lens is scratched.",                               Mechanic=ItemMechanic.Sunglasses,  Value=1f    },
            new() { Id="monstor",    DisplayName="Monstor energy drink",        Description="It's called 'Monstor'.",                               Mechanic=ItemMechanic.Monstor,     Value=1f,   Consumable=true },
            // ── Uncommon ─────────────────────────────────────────────────────
            new() { Id="ipod_4gb",   DisplayName="Crappy iPod (4GB)",           Description="All of Smash Mouth plus three Nickelback albums.",      Mechanic=ItemMechanic.BoredomSlow, Value=0.65f },
            new() { Id="foamfinger", DisplayName="Foam finger",                 Description="Says '#1 DRINKER'.",                                   Mechanic=ItemMechanic.FoamFinger,  Value=1.30f },
            new() { Id="jerky",      DisplayName="Mystery meat jerky",          Description="Expired in 2019.",                                     Mechanic=ItemMechanic.Jerky,       Value=2f    },
            new() { Id="flashlight", DisplayName="Mini flashlight",             Description="Barely works, flickers.",                              Mechanic=ItemMechanic.Flashlight,  Value=1f    },
            // ── Rare ─────────────────────────────────────────────────────────
            new() { Id="ipod_8gb",   DisplayName="Crappy iPod (8GB)",           Description="Has one good song on it somehow.",                     Mechanic=ItemMechanic.BoredomSlow, Value=0.50f },
            new() { Id="goldticket", DisplayName="Golden drink ticket",         Description="Bartender looks confused but honors it.",              Mechanic=ItemMechanic.GoldenTicket,Value=1f,   Consumable=true },
            new() { Id="mustache",   DisplayName="Fake mustache",               Description="Falls off after one use.",                             Mechanic=ItemMechanic.Mustache,    Value=1f,   Consumable=true },
        };

        // Weighted random pick (Common 60%, Uncommon 30%, Rare 10%)
        public static ItemDefinition RandomPrize(System.Random rng)
        {
            var common   = All.GetRange(0, 4);
            var uncommon = All.GetRange(4, 4);
            var rare     = All.GetRange(8, 3);
            int roll = rng.Next(100);
            var pool = roll < 60 ? common : roll < 90 ? uncommon : rare;
            return pool[rng.Next(pool.Count)];
        }
    }

    public static class PatronDatabase
    {
        public static readonly List<PatronDefinition> All = new()
        {
            new() { Id=PatronId.Regular,      DisplayName="The Regular",           Description="Mellow, always here.",                  Perk="Passive boredom -2/min",                          Risk="None",                              CoreRegular=true,  LeaveMinute=999 },
            new() { Id=PatronId.Storyteller,  DisplayName="The Storyteller",       Description="Talks forever, huge boredom drop.",     Perk="Bathroom rare events +10% spawn rate",            Risk="Burns lots of time",                CoreRegular=false, LeaveMinute=999 },
            new() { Id=PatronId.Buyer,        DisplayName="The Buyer",             Description="Keeps ordering rounds.",                Perk="Periodically delivers free drinks",               Risk="Drunk creeps up passively",          CoreRegular=false, LeaveMinute=999 },
            new() { Id=PatronId.Crier,        DisplayName="The Crier",             Description="Sad drunk, needs support.",             Perk="Designated driver — skip driving minigame",       Risk="Requires 4 full conversations",     CoreRegular=true,  LeaveMinute=999 },
            new() { Id=PatronId.Instigator,   DisplayName="The Instigator",        Description="Loud, wants to arm wrestle.",           Perk="-18 boredom on arm wrestle win",                  Risk="+15 drunk on loss",                 CoreRegular=false, LeaveMinute=810 },
            new() { Id=PatronId.OffDuty,      DisplayName="Off-Duty Bartender",    Description="Quiet, knows tricks.",                  Perk="Drink base drunk hit -4 points",                  Risk="Only present 9 PM–12:30 AM",        CoreRegular=true,  LeaveMinute=750 },
            new() { Id=PatronId.ConspiracyGuy,DisplayName="Conspiracy Guy",        Description="Won't stop about aliens.",              Perk="-8 boredom per conversation",                     Risk="+2 boredom/min after friendship 30",CoreRegular=false, LeaveMinute=999 },
        };

        public static PatronDefinition Get(PatronId id) => All.Find(p => p.Id == id);
    }
}
