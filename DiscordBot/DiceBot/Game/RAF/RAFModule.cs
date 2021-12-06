using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.RAF.Actions;
using System.Threading.Tasks;

namespace DiscordBot.DiceBot.Game.RAF
{
    public class RAFModule : ModuleBase<SocketCommandContext>
    {
        private RAFController _rafController;

        public RAFModule(RAFController rafController)
        {
            _rafController = rafController;
        }

        public bool IsInChannel(SocketCommandContext context)
        {
            return context.Channel == _rafController.ActiveChannel;
        }

        [Command("start")]
        [Summary("Starts a game in sign-ups.")]
        public Task StartGameAsync()
        {
            if (!IsInChannel(Context))
            {
                return null;
            }
            if (_rafController.GameState != GameState.Inactive)
            {
                return null;
            }
            _rafController.EnterSignup();
            return ReplyAsync("New game started. Type `!in` to join.");
        }

        [RequireOwner]
        [Command("forcego")]
        [Alias("forcestart")]
        public Task ForceStartGameAsync()
        {
            if (!IsInChannel(Context))
            {
                return null;
            }
            _rafController.StartGame();
            return null;
        }

        [Command("in")]
        [Alias("join")]
        [Summary("Join into the game.")]
        public Task JoinGameAsync()
        {
            if (!IsInChannel(Context))
            {
                return null;
            }
            if (_rafController.GameState != GameState.InSignups)
            {
                return null;
            }
            var user = Context.User;
            if (_rafController.GetPlayer(user) != null)
            {
                return ReplyAsync($"{user.Mention} is already in the game.");
            }
            var player = new RAFPlayer(user);
            _rafController.AddPlayer(player);
            return ReplyAsync($"{user.Mention} has joined the game.");
        }

        [Command("none")]
        public Task NoneAsync()
        {
            RAFPlayer user = _rafController.GetPlayerByID(Context.User);
            if (user == null || !_rafController.ActivePlayers.Contains(user))
            {
                return null;
            }
            _rafController.UseAction(user, new NoneAction());
            return ReplyAsync($"You are now shooting the sky.");
        }

        [Command("shoot")]
        [Alias("kill")]
        public Task ShootPlayerNumberAsync(int playerNum)
        {
            RAFPlayer shooter = _rafController.GetPlayerByID(Context.User);
            if (shooter == null || !_rafController.ActivePlayers.Contains(shooter))
            {
                return null;
            }
            if (playerNum > _rafController.Players.Count)
            {
                return null;
            }
            RAFPlayer player = _rafController.Players[playerNum - 1];
            if (shooter == player)
            {
                return ReflectKillAsync();
            }
            if (!_rafController.ActivePlayers.Contains(player))
            {
                return null;
            }
            _rafController.UseAction(shooter, new KillAction(shooter, player));
            return ReplyAsync($"You are now shooting {player.User.Username}");
        }

        [Command("reflectkill")]
        public Task ReflectKillAsync()
        {
            RAFPlayer user = _rafController.GetPlayerByID(Context.User);
            if (user == null || !_rafController.ActivePlayers.Contains(user))
            {
                return null;
            }
            _rafController.UseAction(user, new ReflectKillAction(user));
            return ReplyAsync($"You are now shooting yourself.");
        }

        [Command("shoot")]
        [Alias("kill")]
        public Task ShootStringAsync(string item)
        {
            if (item.ToLower() == "sky")
            {
                return NoneAsync();
            }
            if (item.ToLower() == "self")
            {
                return ReflectKillAsync();
            }
            RAFPlayer target = _rafController.GetPlayer(item);
            if (target != null)
            {
                return ShootUserAsync(target);
            }
            return null;
        }
        
        public Task ShootUserAsync(RAFPlayer target)
        {
            RAFPlayer shooter = _rafController.GetPlayerByID(Context.User);
            if (shooter == null || !_rafController.ActivePlayers.Contains(shooter))
            {
                return null;
            }
            if (shooter == target)
            {
                return ReflectKillAsync();
            }
            if (!_rafController.ActivePlayers.Contains(target))
            {
                return null;
            }
            _rafController.UseAction(shooter, new KillAction(shooter, target));
            return ReplyAsync($"You are now shooting {target.User.Username}");
        }
    }
}
