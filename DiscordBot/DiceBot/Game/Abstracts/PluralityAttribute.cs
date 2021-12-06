using System;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public sealed class PluralityAttribute : Attribute
    {
        public string Plural { get; set; }
        public string Singular { get; set; }

        public PluralityAttribute(string singular, string plural = null)
        {
            if (plural == null)
            {
                plural = singular + "s";
            }
            Plural = plural;
            Singular = singular;
        }
    }
}
