using System.ComponentModel;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public enum Suit
    {
        [Description("♣")]
        CLUBS,
        [Description("♦")]
        DIAMONDS,
        [Description("♥")]
        HEARTS,
        [Description("♠")]
        SPADES,
        [Description("*")]
        WILDS,
    }
}
