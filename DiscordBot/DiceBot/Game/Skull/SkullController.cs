using Discord;
using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.Skull
{
    public class SkullController : BaseGameController<SkullPlayer>, IGameController
    {
        private Random _random { get; }

        public SkullBid CurrentBid { get; private set; }
        public SkullPlayer CurrentPlayer { get; private set; } = null;

        public override List<SkullPlayer> Players { get; } = new List<SkullPlayer>();

        public SkullController(Random random)
        {
            _random = random;
            CurrentBid = new SkullBid(this);
            ResetGame();
        }

        #region Rules

        public bool Bid(SkullPlayer player, int quantity)
        {
            if (CurrentPlayer != player)
            {
                return false;
            }
            if (CurrentBid.Bid(bidder: player, quantity: quantity))
            {
                AdvancePlayer();
                ActiveChannel.SendMessageAsync($"{CurrentBid.CurrentBidder.User.Mention} has bid {quantity}. It is now {CurrentPlayer.User.Mention}'s bid.").Wait();
                return true;
            }
            return false;
        }

        public void Challenge(SkullPlayer challenger)
        {
            if (CurrentBid.CurrentBidder == null || CurrentBid.CurrentBidder == challenger)
            {
                return;
            }
            if (CurrentPlayer != challenger)
            {
                return;
            }
            ActiveChannel.SendMessageAsync($"{challenger.User.Mention} has challenged {CurrentBid.CurrentBidder.User.Mention}'s bid of {CurrentBid.Quantity}.").Wait();
            EndRound();
        }

        public void Pass(SkullPlayer passer)
        {
            if (CurrentBid.CurrentBidder == null || CurrentBid.CurrentBidder == passer)
            {
                return;
            }
            if (CurrentPlayer != passer)
            {
                return;
            }
            ActiveChannel.SendMessageAsync($"{passer.User.Mention} has passed.").Wait();
            EndRound();
        }

        public int CountDice(int rank)
        {
            int count = 0;
            RevealDice();
            ActiveChannel.SendMessageAsync($"There are {count} {rank}s.").Wait();
            return count;
        }

        private void RemoveToken(SkullPlayer punished, int number = 1)
        {
            punished.RemoveToken(number);
            string message = $"{punished.User.Mention} has lost ";
            if (number > 1)
            {
                message += $"{number} dice.";
            } else
            {
                message += "a die.";
            }
            ActiveChannel.SendMessageAsync(message).Wait();
            if (punished.Tokens.Count == 0)
            {
                ActiveChannel.SendMessageAsync($"{punished.User.Mention} has been eliminated.").Wait();
            }
        }

        #endregion

        #region Game

        public override void ResetGame()
        {
            Players.Clear();
        }

        protected override void StartGame()
        {
            foreach (SocketUser user in Users)
            {
                Players.Add(new SkullPlayer(user));
            }
            foreach (SkullPlayer player in Players)
            {
                player.ActivatePlayer();
                player.SetTokens(1, 3);
            }
            StartRound();
        }

        protected override void EndGame()
        {
            StopGame();
        }

        protected override string GetActivePlayerList()
        {
            return "```Players:\n" + string.Join("\n", ActivePlayers.Select(x => string.Format("{0} - {1} {2}", x.User.Username, x.Tokens.Count, x.Tokens.Count > 1 ? "dice" : "die"))) + "```";
        }

        public void SendDice(SkullPlayer player)
        {
            if (ActivePlayers.Contains(player))
            {
                string message = "Your tokens are: " + player.GetDiceString();
                player.User.SendMessageAsync(message).Wait();
            }
        }

        public void SetCurrentPlayer(SkullPlayer player = null)
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

        public void AdvancePlayer()
        {
            if (CurrentPlayer == null)
            {
                CurrentPlayer = GetRandomPlayer();
            } else
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

        public void StartRound()
        {
            foreach (SkullPlayer player in ActivePlayers)
            {
                player.RandomizeTokens(_random);
                SendDice(player);
            }
            if (CurrentPlayer == null || !ActivePlayers.Contains(CurrentPlayer))
            {
                AdvancePlayer();
            }
            CurrentBid.ResetBid();
            ActiveChannel.SendMessageAsync($"{CurrentPlayer.User.Mention} starts the bidding.").Wait();
        }

        public void StartPhase()
        {

        }

        public void EndPhase()
        {

        }

        public void EndRound()
        {
            if (ActivePlayers.Count <= 1)
            {
                var winner = ActivePlayers.FirstOrDefault();
                if (winner != null)
                {
                    ActiveChannel.SendMessageAsync($"{winner.User.Mention} has won!").Wait();
                }
                EndGame();
            } else
            {
                StartRound();
            }
        }

        public void RevealDice()
        {
            string message = "```";
            bool next = false;
            foreach (SkullPlayer player in ActivePlayers)
            {
                if (next)
                {
                    message += "\n";
                }
                message += player.User.Username + " - " + player.GetDiceString();
                next = true;
            }
            message += "```";
            ActiveChannel.SendMessageAsync(message).Wait();
        }

        #endregion Game

        #region Players

        public SkullPlayer GetRandomPlayer()
        {
            if (ActivePlayers.Count == 0)
            {
                return null;
            }
            return ActivePlayers[_random.Next(ActivePlayers.Count)];
        }

        #endregion Players
    }
}
