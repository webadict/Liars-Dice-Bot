using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public abstract class BaseGameController<T> : IGameController where T : IPlayer
    {
        protected virtual int MIN_PLAYERS => 2;
        protected virtual int MAX_PLAYERS => 12;

        public SocketTextChannel ActiveChannel { get; set; }

        public GameState GameState { get; protected set; }

        public virtual List<SocketUser> Users { get; }
        public abstract List<T> Players { get; }
        public List<T> ActivePlayers
        {
            get
            {
                return Players.Where(x => x.Active).ToList();
            }
        }

        protected BaseGameController()
        {
            Users = new List<SocketUser>();
        }

        public abstract void ResetGame();

        public void EnterSignups()
        {
            // Game is active or already in signups.
            if (GameState != GameState.Inactive)
            {
                return;
            }
            ResetGame();
            Users.Clear();
            Players.Clear();
            GameState = GameState.InSignups;
        }

        protected abstract void StartGame();

        public virtual void CreateGame()
        {
            // Already active.
            if (GameState != GameState.InSignups)
            {
                return;
            }
            // Less than required number of players.
            if (Users.Count < MIN_PLAYERS)
            {
                ActiveChannel.SendMessageAsync("Not enough players. Game disbanded.");
                GameState = GameState.Inactive;
                return;
            }
            GameState = GameState.Active;
            StartGame();

        }

        protected abstract void EndGame();

        public virtual void StopGame()
        {
            GameState = GameState.Inactive;
            Players.Clear();
        }

        public virtual void AddPlayer(SocketUser user)
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
            Users.Add(user);
            ActiveChannel.SendMessageAsync($"{user.Mention} has joined the game!");
        }

        public virtual void RemovePlayer(SocketUser user)
        {
            if (GameState != GameState.InSignups)
            {
                return;
            }
            if (!Users.Contains(user))
            {
                return;
            }
            Users.Remove(user);
            ActiveChannel.SendMessageAsync($"{user.Mention} has left the game!");
        }

        protected abstract string GetActivePlayerList();

        public void GetPlayers()
        {
            var message = "There are currently no players.";
            if (GameState == GameState.InSignups)
            {
                if (Users.Count > 0)
                {
                    message = "```Players:\n" + string.Join("\n", Users.Select(x => x is SocketGuildUser ? ((SocketGuildUser)x).Nickname ?? x.Username : x.Username)) + "```";
                }
            } else if (GameState == GameState.Active)
            {
                message = GetActivePlayerList();
            } else
            {
                return;
            }
            ActiveChannel.SendMessageAsync(message);
        }

        public T GetPlayer(SocketUser user)
        {
            return Players.FirstOrDefault(x => x.User == user);
        }
    }
}
