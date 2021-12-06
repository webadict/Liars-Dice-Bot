using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.TexasHoldem
{
    public class TexasHoldemPlayer : GamePlayer, IPlayer
    {
        public bool Active => Chips + Bet > 0;
        public bool AllIn => Chips == 0 && Bet > 0;

        public bool Folded { get; set; }

        public int Chips { get; set; }
        public int Bet { get; set; }
        public int Pot { get; set; }

        public List<Card> Cards { get; }

        public TexasHoldemPlayer(SocketUser user) : base(user)
        {
            Cards = new List<Card>();
        }

        public void GiveCard(Card card)
        {
            Cards.Add(card);
        }

        public string GetStatus(bool reveal = false)
        {
            var message = $"{User.Username} - ";
            if (reveal)
            {
                message += $"{string.Join(" ", Cards.Select(x => x.ToString()))}";
            }
            message += $"Chips: {Chips}; Bet: {Bet}";
            return message;
        }

        public int RaiseBetTo(int newBet)
        {
            if (newBet <= Bet)
            {
                return 0;
            }
            int increase = newBet - Bet;
            if (increase > Chips)
            {
                increase = Chips;
            }
            Bet += increase;
            Chips -= increase;
            return increase;
        }
    }
}
