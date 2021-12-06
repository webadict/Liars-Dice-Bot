using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public class NumberCardBook
    {
        private readonly Random _random;

        protected List<NumberCard> Cards { get; }

        public int Count => Cards.Count;

        public NumberCardBook(Random random, int minNumber, int maxNumber)
        {
            _random = random;
            Cards = new List<NumberCard>();
            for(int i = minNumber; i <= maxNumber; i++)
            {
                Cards.Add(new NumberCard(i));
            }
        }

        public void ReduceTo(int number)
        {
            while(Cards.Count > number)
            {
                Cards.RemoveAt(_random.Next(Cards.Count));
            }
        }

        public NumberCard DrawRandomCard()
        {
            if (Count == 0)
            {
                return null;
            }
            int index = _random.Next(Cards.Count);
            NumberCard card = Cards[index];
            Cards.RemoveAt(index);
            return card;
        }
    }
}
