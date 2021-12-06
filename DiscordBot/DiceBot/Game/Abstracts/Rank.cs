using System.ComponentModel;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public enum Rank
    {
        NONE,
        [Description("A")]
        [Plurality("Ace")]
        ACE_LOW,
        [Description("2")]
        [Plurality("Two")]
        TWO,
        [Description("3")]
        [Plurality("Three")]
        THREE,
        [Description("4")]
        [Plurality("Four")]
        FOUR,
        [Description("5")]
        [Plurality("Five")]
        FIVE,
        [Description("6")]
        [Plurality("Six", "Sixes")]
        SIX,
        [Description("7")]
        [Plurality("Seven")]
        SEVEN,
        [Description("8")]
        [Plurality("Eight")]
        EIGHT,
        [Description("9")]
        [Plurality("Nine")]
        NINE,
        [Description("T")]
        [Plurality("Ten")]
        TEN,
        [Description("J")]
        [Plurality("Jack")]
        JACK,
        [Description("Q")]
        [Plurality("Queen")]
        QUEEN,
        [Description("K")]
        [Plurality("King")]
        KING,
        [Description("A")]
        [Plurality("Ace")]
        ACE_HIGH,
        [Description("*")]
        [Plurality("Joker")]
        JOKER,
    }
}
