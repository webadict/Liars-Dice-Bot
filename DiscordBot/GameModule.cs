using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using DiscordBot.DiceBot.Game.LiarsDice;
using DiscordBot.DiceBot.Game.NoThanks;
using DiscordBot.DiceBot.Game.TexasHoldem;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameBook _gameBook;
        private readonly List<SocketTextChannel> _activeChannels = new List<SocketTextChannel>();

        public GameModule(GameBook gameBook)
        {
            _gameBook = gameBook;
        }

        private IGameController GetGame(IMessageChannel channel)
        {
            if (!typeof(SocketTextChannel).IsInstanceOfType(channel))
            {
                return null;
            }
            SocketTextChannel socketTextChannel = (SocketTextChannel)channel;
            if (_gameBook.ContainsChannel(socketTextChannel))
            {
                return _gameBook[socketTextChannel];
            }
            return null;
        }

        private bool IsActiveTextChannel(ISocketMessageChannel channel)
        {
            return typeof(SocketTextChannel).IsInstanceOfType(channel) && channel.Name == "liars_dice"; // && _activeChannels.Contains((SocketTextChannel)channel);
        }

        [RequireOwner]
        [Command("activate")]
        [Summary("Allows this bot to play games on this channel")]
        public Task ActivateChannelAsync()
        {
            var channel = Context.Channel;
            if (typeof(SocketTextChannel).IsInstanceOfType(channel)
                && !_activeChannels.Contains((SocketTextChannel)channel))
            {
                _activeChannels.Add((SocketTextChannel)channel);
                return ReplyAsync("This channel is now active!");
            }
            return null;
        }
        
        [Command("start")]
        [Summary("Starts a game")]
        public Task StartGameAsync(string game)
        {
            var channel = Context.Channel;
            if (!IsActiveTextChannel(channel))
            {
                return null;
            }
            SocketTextChannel socketChannel = (SocketTextChannel)channel;
            IGameController gameController = GetGame(socketChannel);
            if (gameController != null && gameController.GameState != GameState.Inactive)
            {
                return null;
            }
            string message = "A new game of {0} has started. Type `!in` to join.";
            string gameName = null;
            switch (game.ToLower())
            {
                //case "raf":
                //    gameController = new RAFController();
                //    break;
                case "liar":
                case "liars":
                case "dice":
                    gameController = new LiarsDiceController(new System.Random());
                    gameName = "Liar's Dice";
                    break;
                case "nothanks":
                case "nt":
                    gameController = new NoThanksController(new System.Random());
                    gameName = "No Thanks";
                    break;
                case "texas":
                case "tehas":
                case "holdem":
                    gameController = new TexasHoldemController(new System.Random());
                    gameName = "Texas Hold'em";
                    break;
                default:
                    return null;
            }
            gameController.ActiveChannel = socketChannel;
            _gameBook.Add(socketChannel, gameController);
            gameController.EnterSignups();
            return ReplyAsync(string.Format(message, gameName));
        }

        [Command("in")]
        [Alias("join")]
        [Summary("Join a game in sign-ups")]
        public Task JoinGameAsync()
        {
            var channel = Context.Channel;
            IGameController game = GetGame(channel);
            if (game == null)
            {
                return null;
            }
            var user = Context.User;
            game.AddPlayer(user);
            return null;
        }

        [Command("forcein")]
        [Alias("forcejoin")]
        [RequireOwner]
        public Task ForceJoinGameAsync(IUser user)
        {
            var channel = Context.Channel;
            IGameController game = GetGame(channel);
            if (game == null)
            {
                return null;
            }
            if (user is SocketUser socketUser)
            {
                game.AddPlayer(socketUser);
            }
            return null;
        }

        [Command("out")]
        [Alias("leave")]
        [Summary("Leave a game in sign-ups")]
        public Task LeaveGameAsync()
        {
            var channel = Context.Channel;
            IGameController game = GetGame(channel);
            if (game == null)
            {
                return null;
            }
            var user = Context.User;
            game.RemovePlayer(user);
            return null;
        }

        [Command("forceout")]
        [Alias("forceleave")]
        [RequireOwner]
        public Task ForceLeaveGameAsync(SocketUser user)
        {
            var channel = Context.Channel;
            IGameController game = GetGame(channel);
            if (game == null)
            {
                return null;
            }
            if (user is SocketUser socketUser)
            {
                game.RemovePlayer(socketUser);
            }
            return null;
        }

        [Command("go")]
        [Summary("Start a game in sign-ups")]
        public Task GoAsync()
        {
            var channel = Context.Channel;
            IGameController game = GetGame(channel);
            if (game == null)
            {
                return null;
            }
            game.CreateGame();
            return null;
        }

        [RequireOwner]
        [Command("stop")]
        [Summary("Stops a game in sign-ups.")]
        public Task EndGameAsync()
        {
            var channel = Context.Channel;
            IGameController game = GetGame(channel);
            if (game == null)
            {
                return null;
            }
            game.StopGame();
            return ReplyAsync("Game has been stopped.");
        }

        [Command("players")]
        [Summary("Retrieve a list of players in the game.")]
        public Task GetPlayersAsync()
        {
            var channel = Context.Channel;
            IGameController game = GetGame(channel);
            if (game == null)
            {
                return null;
            }
            game.GetPlayers();
            return null;
        }

        [Command("games")]
        [Summary("Displays a list of games.")]
        public Task AvailableGames()
        {
            var channel = Context.Channel;
            if (!IsActiveTextChannel(channel))
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("List of games:\n");
            sb.Append("Liar's Dice - `!start liar`\n");
            sb.Append("No Thanks - `!start nothanks`\n");
            
            return ReplyAsync(sb.ToString());
        }

        [Command("help")]
        public Task Help()
        {
            var channel = Context.Channel;
            if (!IsActiveTextChannel(channel))
            {
                return null;
            }
            return ReplyAsync("To start a game, type `!start <game>`. For a list of available games, type `!games`.");
        }
    }
}
