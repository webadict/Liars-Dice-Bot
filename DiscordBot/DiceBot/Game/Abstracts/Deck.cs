using System;
using System.Collections.Generic;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public class Deck
    {
        private readonly Random _random;

        List<Card> Cards { get; }

        public int Count => Cards.Count;

        public Deck(Random random)
        {
            _random = random;
            Cards = new List<Card>();
        }
        
        public void Create(int numberOfDecks = 1)
        {
            Cards.Clear();
            for (int i=0; i<numberOfDecks; i++)
            {
                foreach (Suit suit in GetSuits())
                {
                    foreach (Rank rank in GetRanks())
                    {
                        Cards.Add(new Card(rank, suit));
                    }
                }
            }
        }

        public Card DrawRandomCard()
        {
            if (Count == 0)
            {
                return null;
            }
            int index = _random.Next(Cards.Count);
            Card card = Cards[index];
            Cards.RemoveAt(index);
            return card;
        }

        public List<Suit> GetSuits()
        {
            return new List<Suit>
            {
                Suit.CLUBS,
                Suit.DIAMONDS,
                Suit.HEARTS,
                Suit.SPADES,
            };
        }

        public List<Rank> GetRanks()
        {
            return new List<Rank>
            {
                Rank.TWO,
                Rank.THREE,
                Rank.FOUR,
                Rank.FIVE,
                Rank.SIX,
                Rank.SEVEN,
                Rank.EIGHT,
                Rank.NINE,
                Rank.TEN,
                Rank.JACK,
                Rank.QUEEN,
                Rank.KING,
                Rank.ACE_HIGH,
            };
        }
    }
}
