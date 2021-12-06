using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.NoThanks
{
    public class NoThanksPlayer : GamePlayer, IPlayer
    {
        public bool Active => true;

        public int Tokens { get; set; }

        public List<NumberCard> Cards { get; }

        public NoThanksPlayer(SocketUser user) : base(user)
        {
            Cards = new List<NumberCard>();
        }

        public void GiveCard(NumberCard card)
        {
            Cards.Add(card);
        }

        public int GetScore(bool withTokens = false)
        {
            NumberCard lastCard = null;
            int score = 0;
            foreach (NumberCard card in Cards.OrderBy(x => x.Value))
            {
                if (lastCard == null || lastCard.Value + 1 != card.Value)
                {
                    score += card.Value;
                }
                lastCard = card;
            }
            if (withTokens)
            {
                score -= Tokens;
            }
            return score;
        }

        public string GetStatus(bool withTokens = false)
        {
            NumberCard lastCard = null;
            string message = $"{Nickname} Cards:";
            foreach (NumberCard card in Cards.OrderBy(x => x.Value))
            {
                if (lastCard != null && lastCard.Value + 1 == card.Value)
                {
                    message += "-";
                } else
                {
                    message += " ";
                }
                message += $"{card.Value}";
                lastCard = card;
            }
            if (withTokens)
            {
                message += $" Tokens: {Tokens}";
            }
            message += $" (Score: {GetScore(withTokens)})";
            return message;
        }
    }
}
