using Discord;
using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.NoThanks
{
    public class NoThanksController : BaseGameController<NoThanksPlayer>, IGameController
    {
        private readonly Random _random;

        private const int MIN_CARD_NUMBER = 3;
        private const int MAX_CARD_NUMBER = 35;
        private const int REDUCE_TO_NUMBER = 24;
        private const int TOKENS = 11;
        protected override int MIN_PLAYERS => 3;
        protected override int MAX_PLAYERS => 6;

        public override List<NoThanksPlayer> Players { get; }
        
        public NumberCardBook CardBook { get; protected set; }

        public NoThanksPlayer CurrentPlayer { get; private set; }
        public NumberCard CurrentCard { get; private set; }
        public int CurrentTokens { get; private set; }

        public NoThanksController(Random random) : base()
        {
            _random = random;
            Players = new List<NoThanksPlayer>();
        }

        public override void ResetGame()
        {
            CardBook = new NumberCardBook(_random, MIN_CARD_NUMBER, MAX_CARD_NUMBER);
            CurrentPlayer = null;
        }

        protected override void EndGame()
        {
            List<NoThanksPlayer> players = ActivePlayers.OrderBy(x => x.GetScore(true)).ToList();
            int topScore = players.First().GetScore(true);
            ActiveChannel.SendMessageAsync("```Players:\n" + string.Join("\n", players.Select(x => x.GetStatus(true))) + "```").Wait();
            List<NoThanksPlayer> winners = players.Where(x => x.GetScore(true) == topScore).ToList();
            int count = winners.Count;
            var message = "";
            if (count == 1)
            {
                message = $"{winners.First().User.Mention} has won the game!";
            } else if (count == 2)
            {
                message = $"{string.Join(" and ", winners.Select(x => x.User.Mention))} have won the game!";
            } else
            {
                message = $"{string.Join(", ", winners.Select(x => x.User.Mention))} have won the game!";
            }
            ActiveChannel.SendMessageAsync(message).Wait();
            StopGame();
        }

        protected override string GetActivePlayerList()
        {
            return $"```Cards left: {CardBook.Count}\nPlayers:\n" + string.Join("\n", ActivePlayers.Select(x => x.GetStatus())) + "```";
        }

        protected override void StartGame()
        {
            // Count out 24 cards.
            CardBook.ReduceTo(REDUCE_TO_NUMBER);

            foreach (SocketUser user in Users)
            {
                NoThanksPlayer player = new NoThanksPlayer(user)
                {
                    Tokens = TOKENS,
                };
                Players.Add(player);
            }
            DrawNewCard();
            AdvancePlayer();
            ActiveChannel.SendMessageAsync($"`{CurrentCard.Value}` is now up for bidding. It is {CurrentPlayer.User.Mention}'s turn.").Wait();
        }

        public void DrawNewCard()
        {
            CurrentCard = CardBook.DrawRandomCard();
        }

        public void TakeCard(NoThanksPlayer player)
        {
            if (CurrentPlayer != player)
            {
                return;
            }
            var message = $"{player.User.Mention} takes the `{CurrentCard.Value}`";
            if (CurrentTokens > 0)
            {
                message += $" and {CurrentTokens} tokens";
            }
            message += ".";
            ActiveChannel.SendMessageAsync(message).Wait();
            player.Tokens += CurrentTokens;
            CurrentTokens = 0;
            player.GiveCard(CurrentCard);
            AdvancePlayer();
            DrawNewCard();
            if (CurrentCard == null)
            {
                EndGame();
            } else
            {
                ActiveChannel.SendMessageAsync($"`{CurrentCard.Value}` is now up for bidding. It is {CurrentPlayer.User.Mention}'s turn.").Wait();
            }
        }

        public void Pass(NoThanksPlayer player)
        {
            if (CurrentPlayer != player)
            {
                return;
            }
            if (player.Tokens <= 0)
            {
                return;
            }
            player.Tokens -= 1;
            CurrentTokens += 1;
            AdvancePlayer();
            var message = $"{ player.User.Mention} passes. ";
            if (CurrentTokens == 1)
            {
                message += $"There is now {CurrentTokens} token.";
            } else
            {
                message += $"There are now {CurrentTokens} tokens.";
            }
            message += $" It is now {CurrentPlayer.User.Mention}'s turn.";
            ActiveChannel.SendMessageAsync(message).Wait();
        }

        public void SendStatus(NoThanksPlayer player)
        {
            player.User.SendMessageAsync(player.GetStatus(true));
        }

        public void AdvancePlayer()
        {
            if (CurrentPlayer == null)
            {
                CurrentPlayer = GetRandomPlayer();
            }
            else
            {
                int index = ActivePlayers.IndexOf(CurrentPlayer);
                index++;
                if (index >= ActivePlayers.Count)
                {
                    index = 0;
                }
                SetCurrentPlayer(ActivePlayers[index]);
            }
        }

        public NoThanksPlayer GetRandomPlayer()
        {
            if (ActivePlayers.Count == 0)
            {
                return null;
            }
            return ActivePlayers[_random.Next(ActivePlayers.Count)];
        }

        public void SetCurrentPlayer(NoThanksPlayer player = null)
        {
            if (ActivePlayers.Contains(player))
            {
                CurrentPlayer = player;
            }
            else if (CurrentPlayer == null)
            {
                CurrentPlayer = GetRandomPlayer();
            }
        }
    }
}
