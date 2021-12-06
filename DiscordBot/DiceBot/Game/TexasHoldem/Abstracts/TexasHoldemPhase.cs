using System.ComponentModel;

namespace DiscordBot.DiceBot.Game.TexasHoldem.Abstracts
{
    public enum TexasHoldemPhase
    {
        PREFLOP,
        [Description("Flop")]
        FLOP,
        [Description("Turn")]
        TURN,
        [Description("River")]
        RIVER,
    }
}
