using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.RAF.Actions;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.RAF
{
    public class RAFController
    {
        public static int MAX_PLAYERS = 12;

        public SocketTextChannel ActiveChannel { get; set; }

        public GameState GameState { get; private set; }

        public List<RAFPlayer> Players { get; }
        public List<RAFPlayer> ActivePlayers => Players.Where(x => x.Active).ToList();
        public RAFActionBook ActionBook { get; }

        public RAFController()
        {
            Players = new List<RAFPlayer>();
            ActionBook = new RAFActionBook(this);
        }

        public bool AddPlayer(RAFPlayer player)
        {
            if (Players.Count >= MAX_PLAYERS)
            {
                return false;
            }
            Players.Add(player);
            return true;
        }

        public void GetPlayerList()
        {
            var message = "```";
            if (ActivePlayers.Count == 0)
            {
                message += "There are no active players.";
            } else
            {
                message += "Players:";
                var index = 0;
                foreach (RAFPlayer player in Players)
                {
                    index++;
                    if (player.Active)
                    {
                        message += $"\n{player.Nickname} ({index}) - {player.Health} Lives Left";
                    }
                }
            }
            message += "```";

            ActiveChannel.SendMessageAsync(message);
        }

        public void UseAction(RAFPlayer user, AbstractAction action)
        {
            ActionBook.AddAction(user, action);
            if (ActionBook.IsFull())
            {
                EndRound();
            }
        }

        public bool EnterSignup()
        {
            // Game is active or already in signups.
            if (GameState != GameState.Inactive)
            {
                return false;
            }
            ResetGame();
            GameState = GameState.InSignups;
            return true;
        }

        public void ResetGame()
        {

        }

        public void StartGame()
        {
            // Already active.
            if (GameState != GameState.InSignups)
            {
                return;
            }
            // Less than required number of players.
            if (Players.Count < 2)
            {
                return;
            }
            GameState = GameState.Active;
            StartRound();
            ActiveChannel.SendMessageAsync("Ready, Aim, Fire has started!");
        }

        public void StartRound()
        {
            ActionBook.Clear();
            GetPlayerList();
        }

        public void EndRound()
        {

            ActionBook.ResolveBook();
            var message = "Results:";
            foreach (RAFPlayer player in ActivePlayers)
            {
                int damage = player.EndTurn();
                if (damage > 0)
                {
                    message += $"\n{player.User.Mention} took {damage} damage!";
                    if (player.IsDead)
                    {
                        message += $"\n{player.User.Mention} died!";
                    }
                } else
                {
                    message += $"\n{player.User.Mention} was unscathed!";
                }
            }
            message += "";
            ActiveChannel.SendMessageAsync(message);
            if (ActivePlayers.Count <= 1)
            {
                EndGame();
            }
        }

        public void EndGame()
        {
            var message = "";
            if (ActivePlayers.Count == 1)
            {
                RAFPlayer winner = ActivePlayers.SingleOrDefault();
                message = $"{winner.User.Mention} wins the game!";
            } else if (ActivePlayers.Count == 0)
            {
                message = $"No one wins the game.";
            } else
            {
                return;
            }
            RemoveAllPlayers();
            GameState = GameState.Inactive;
            ActiveChannel.SendMessageAsync(message);
        }

        private void RemoveAllPlayers()
        {
            Players.Clear();
        }

        public RAFPlayer GetPlayer(SocketUser user)
        {
            return Players.FirstOrDefault(x => x.User == user);
        }

        public RAFPlayer GetPlayer(string playerName)
        {
            return Players.FirstOrDefault(x => x.Nickname.ToLower() == playerName);
        }

        public RAFPlayer GetPlayerByID(SocketUser user)
        {
            return Players.FirstOrDefault(x => x.User.Id == user.Id);
        }
    }
}
