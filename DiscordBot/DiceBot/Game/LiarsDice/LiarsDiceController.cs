using Discord;
using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiscordBot.DiceBot.Game.LiarsDice
{
    public class LiarsDiceController : BaseGameController<LiarsDicePlayer>, IGameController
    {
        public LiarsDiceMods Mods { get; set; } = new LiarsDiceMods();

        private Random _random { get; }

        public LiarsDiceBid CurrentBid { get; private set; }
        public LiarsDicePlayer CurrentPlayer { get; private set; } = null;

        public DiceCollection PoolPlayer { get; set; } = new DiceCollection();

        public List<LiarsDiceUser> LiarsDiceUsers { get; } = new List<LiarsDiceUser>();
        public override List<SocketUser> Users => LiarsDiceUsers.Select(x => x.User).ToList();
        public override List<LiarsDicePlayer> Players { get; } = new List<LiarsDicePlayer>();
        public bool ShouldCountTotal
        {
            get
            {
                return Mods.CountPips && Players.Any(x => x.Dice.Count == 1);
            }
        }

        public LiarsDiceController(Random random)
        {
            _random = random;
            CurrentBid = new LiarsDiceBid(this);
            ResetGame();
        }

        #region Rules

        public bool Bid(LiarsDicePlayer player, int total)
        {
            if (CurrentPlayer != player)
            {
                return false;
            }
            if (!ShouldCountTotal)
            {
                return false;
            }
            if (CurrentBid.Bid(bidder: player, total: total))
            {
                AdvancePlayer();
                ActiveChannel.SendMessageAsync($"{CurrentBid.CurrentBidder.User.Mention} has bid a total of {total}. It is now {CurrentPlayer.User.Mention}'s bid.").Wait();
                return true;
            }
            return false;
        }

        public bool Bid(LiarsDicePlayer player, int quantity, int rank)
        {
            if (CurrentPlayer != player)
            {
                return false;
            }
            if (ShouldCountTotal)
            {
                return false;
            }
            if (rank < 1)
            {
                ActiveChannel.SendMessageAsync($">:( You *know* why that's bad.").Wait();
                return false;
            }
            if (rank > Mods.NumberOfSides)
            {
                ActiveChannel.SendMessageAsync($"{Mods.NumberOfSides}s are the highest you can bid.").Wait();
                return false;
            }
            if (Mods.SixesOnly && rank != 6)
            {
                ActiveChannel.SendMessageAsync($"Sixes for the six goddess.").Wait();
                return false;
            }
            if (Mods.Wilds && rank == 1)
            {
                ActiveChannel.SendMessageAsync("Cannot bid 1s while playing with wilds.").Wait();
                return false;
            }
            if (CurrentBid.Bid(bidder: player, quantity: quantity, rank: rank))
            {
                AdvancePlayer();
                ActiveChannel.SendMessageAsync($"{CurrentBid.CurrentBidder.User.Mention} has bid {quantity} {rank}s. It is now {CurrentPlayer.User.Mention}'s bid.").Wait();
                return true;
            }
            return false;
        }

        public void NotifyCurrentPlayer()
        {
            ActiveChannel.SendMessageAsync($"It is now {CurrentPlayer.User.Mention}'s bid. The previous bid was {CurrentBid.Quantity} {CurrentBid.Rank}.").Wait();
        }

        public void Challenge(LiarsDicePlayer challenger)
        {
            if (CurrentBid.CurrentBidder == null || CurrentBid.CurrentBidder == challenger)
            {
                return;
            }
            if (CurrentPlayer != challenger)
            {
                return;
            }
            string message;
            if (ShouldCountTotal)
            {
                ActiveChannel.SendMessageAsync($"{challenger.User.Mention} has challenged {CurrentBid.CurrentBidder.User.Mention}'s bid of {CurrentBid.Total}.").Wait();
                if (TotalDice() >= CurrentBid.Total)
                {
                    message = RemoveDie(challenger);
                }
                else
                {
                    message = RemoveDie(CurrentBid.CurrentBidder);
                }
            }
            else
            {
                ActiveChannel.SendMessageAsync($"{challenger.User.Mention} has challenged {CurrentBid.CurrentBidder.User.Mention}'s bid of {CurrentBid.Quantity} {CurrentBid.Rank}s.").Wait();
                if (CountDice(CurrentBid.Rank) >= CurrentBid.Quantity)
                {
                    message = RemoveDie(challenger);
                }
                else
                {
                    message = RemoveDie(CurrentBid.CurrentBidder);
                }
            }
            ActiveChannel.SendMessageAsync(message).Wait();
            EndRound();
        }

        public void DeadOn(LiarsDicePlayer challenger)
        {
            if (CurrentBid.CurrentBidder == null || CurrentBid.CurrentBidder == challenger)
            {
                return;
            }
            if (CurrentPlayer != challenger)
            {
                return;
            }
            string message;
            if (ShouldCountTotal)
            {
                ActiveChannel.SendMessageAsync($"{challenger.User.Mention} has claimed \"Dead-on\" on {CurrentBid.CurrentBidder.User.Mention}'s bid of {CurrentBid.Total}.").Wait();
                if (TotalDice() != CurrentBid.Quantity)
                {
                    message = RemoveDie(challenger);
                }
                else
                {
                    message = string.Join("\n", ActivePlayers.Where(p => p != challenger).Select(x => RemoveDie(x)));
                }
            }
            else
            {
                ActiveChannel.SendMessageAsync($"{challenger.User.Mention} has claimed \"Dead-on\" on {CurrentBid.CurrentBidder.User.Mention}'s bid of {CurrentBid.Quantity} {CurrentBid.Rank}s.").Wait();
                if (CountDice(CurrentBid.Rank) != CurrentBid.Quantity)
                {
                    message = RemoveDie(challenger);
                }
                else
                {
                    message = string.Join("\n", ActivePlayers.Where(p => p != challenger).Select(x => RemoveDie(x)));
                }
            }
            ActiveChannel.SendMessageAsync(message).Wait();
            EndRound();
        }

        public int TotalDice()
        {
            int total = 0;
            foreach (LiarsDicePlayer player in ActivePlayers)
            {
                total += player.GetTotal(Mods);
            }
            RevealDice();
            ActiveChannel.SendMessageAsync($"The dice total to {total}.").Wait();
            return total;
        }

        public int CountDice(int rank)
        {
            int count = 0;
            foreach (LiarsDicePlayer player in ActivePlayers)
            {
                count += player.GetRankCount(CurrentBid.Rank, Mods);
            }
            RevealDice();
            ActiveChannel.SendMessageAsync($"There are {count} {rank}s.").Wait();
            return count;
        }

        private string RemoveDie(LiarsDicePlayer punished, int number = 1)
        {
            punished.RemoveDie(number);
            string message = $"{punished.User.Mention} has lost ";
            if (number > 1)
            {
                message += $"{number} dice.";
            } else
            {
                message += "a die.";
            }
            if (punished.Dice.Count == 0)
            {
                message += $"\n{punished.User.Mention} has been eliminated.";
            }
            return message;
        }

        #endregion

        #region Game

        public override void ResetGame()
        {
            Mods.ResetGame();
            Players.Clear();
        }

        protected override void StartGame()
        {
            Shuffle(Users);
            foreach (SocketUser user in Users)
            {
                Players.Add(new LiarsDicePlayer(user));
            }
            foreach (LiarsDicePlayer player in Players)
            {
                player.ActivatePlayer();
                player.SetDice(Mods.NumberOfDice, Mods.NumberOfSides);
            }
            StartRound();
        }

        protected override void EndGame()
        {
            StopGame();
        }

        protected override string GetActivePlayerList()
        {
            return GetPlayerList();
        }

        public void SendDice(LiarsDicePlayer player)
        {
            if (ActivePlayers.Contains(player))
            {
                if (Mods.Blind)
                {
                    var dice = ActivePlayers.Where(x => x != player).SelectMany(x => x.GetDice()).OrderBy(x => x);
                    player.User.SendMessageAsync(string.Join(", ", dice.Select(x => x == 1 && Mods.Wilds ? "*" : x.ToString()))).Wait();
                } else
                {
                    string message = "Your dice are: " + player.GetDiceString(Mods);
                    player.User.SendMessageAsync(message).Wait();
                }
            }
        }

        public void SetCurrentPlayer(LiarsDicePlayer player = null)
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
            if (Mods.Chaos)
            {
                ActiveChannel.SendMessageAsync($"Performing Chaos...\n{Mods.PerformChaos(_random)}");
            }
            foreach (LiarsDicePlayer player in ActivePlayers)
            {
                player.SetDiceSides(Mods.NumberOfSides);
                player.ResetPlayer(_random, Mods);
            }
            foreach (LiarsDicePlayer player in ActivePlayers)
            {
                SendDice(player);
            }
            if (Mods.Pool)
            {
                int average = (int)ActivePlayers.Average(x => x.Dice.Count);
                PoolPlayer.SetDice(average, Mods.NumberOfSides);
                PoolPlayer.RollDice(_random, Mods);
            }
            if (CurrentPlayer == null || !ActivePlayers.Contains(CurrentPlayer))
            {
                AdvancePlayer();
            }
            CurrentBid.ResetBid();

            ActiveChannel.SendMessageAsync($"{GetPlayerList()}\n{CurrentPlayer.User.Mention} starts the bidding.").Wait();
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

        public string RevealDiceString()
        {
            string message = "```";
            bool next = false;
            foreach (LiarsDicePlayer player in ActivePlayers)
            {
                if (next)
                {
                    message += "\n";
                }
                message += player.Nickname + " - " + player.GetDiceString(Mods);
                next = true;
            }
            if (Mods.Pool)
            {
                message += "\nPool - " + PoolPlayer.GetDiceString(Mods);
            }
            message += "```";
            return message;
        }

        public void RevealDice()
        {
            string message = RevealDiceString();
            ActiveChannel.SendMessageAsync(message).Wait();
        }

        public string GetPlayerList()
        {
            if (ActivePlayers.Count == 0)
            {
                return "There are currently no players.";
            }
            var players = string.Join("\n", ActivePlayers.Select(x => string.Format("{0} - {1} {2}", x.Nickname, x.Dice.Count, x.Dice.Count > 1 ? "dice" : "die")));
            var total = ActivePlayers.Sum(x => x.Dice.Count);
            if (Mods.Pool)
            {
                players += string.Format("\nPool - {0} {1}", PoolPlayer.Count, PoolPlayer.Count > 1 ? "dice" : "die");
                total += PoolPlayer.Count;
            }
            var totalString = string.Format("Total: {0} {1}", total, total > 1 ? "dice" : "die");
            return $"```Players:\n{players}\n\n{totalString}```";
        }

        #endregion Game

        #region Players

        public LiarsDicePlayer GetRandomPlayer()
        {
            if (ActivePlayers.Count == 0)
            {
                return null;
            }
            return ActivePlayers[_random.Next(ActivePlayers.Count)];
        }

        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public override void RemovePlayer(SocketUser user)
        {
            if (GameState != GameState.Active)
            {
                base.RemovePlayer(user);
            } else
            {
                ActiveChannel.SendMessageAsync($"{user.Mention} has been murderified!").Wait();
            }
            if (!Users.Contains(user))
            {
                return;
            }
            LiarsDiceUsers.RemoveAll(x => x.User == user);
            EndRound();
        }

        public override void AddPlayer(SocketUser user)
        {
            if (GameState != GameState.InSignups)
            {
                return;
            }
            if (Users.Count >= MAX_PLAYERS)
            {
                return;
            }
            if (Users.Contains(user))
            {
                return;
            }
            var liarUser = new LiarsDiceUser(user);
            LiarsDiceUsers.Add(liarUser);
            ActiveChannel.SendMessageAsync($"{user.Mention} has joined the game!");
        }

        public void SaveGameeStats()
        {

        }

        #endregion Players

        #region Extra

        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        #endregion
    }
}
