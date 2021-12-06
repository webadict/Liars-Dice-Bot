using System.ComponentModel;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public class Card
    {
        public Suit Suit { get; private set; }
        public Rank Rank { get; private set; }

        public Card(Rank rank, Suit suit)
        {
            Suit = suit;
            Rank = rank;
        }

        public override string ToString()
        {
            return $"{Rank.GetAttributeOfType<DescriptionAttribute>().Description}{Suit.GetAttributeOfType<DescriptionAttribute>().Description}";
        }
    }
}
