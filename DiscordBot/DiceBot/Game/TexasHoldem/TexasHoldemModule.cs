using Discord.Commands;
using DiscordBot.DiceBot.Game.Abstracts;
using System.Threading.Tasks;

namespace DiscordBot.DiceBot.Game.TexasHoldem
{
    public class TexasHoldemModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameBook _gameBook;

        public TexasHoldemModule(GameBook gameBook)
        {
            _gameBook = gameBook;
        }

        [Command("call")]
        public Task CallBetAsync()
        {
            TexasHoldemController texasHoldemController = _gameBook.GetGame<TexasHoldemController>(Context.Channel);
            if (texasHoldemController == null)
            {
                return null;
            }
            var user = Context.User;
            TexasHoldemPlayer player = texasHoldemController.GetPlayer(user);
            if (player != null)
            {
                texasHoldemController.Call(player);
            }
            return null;
        }

        [Command("check")]
        public Task CheckBetAsync()
        {
            TexasHoldemController texasHoldemController = _gameBook.GetGame<TexasHoldemController>(Context.Channel);
            if (texasHoldemController == null)
            {
                return null;
            }
            var user = Context.User;
            TexasHoldemPlayer player = texasHoldemController.GetPlayer(user);
            if (player != null)
            {
                texasHoldemController.Check(player);
            }
            return null;
        }

        [Command("raise")]
        public Task RaiseBetAsync(int raise)
        {
            TexasHoldemController texasHoldemController = _gameBook.GetGame<TexasHoldemController>(Context.Channel);
            if (texasHoldemController == null)
            {
                return null;
            }
            var user = Context.User;
            TexasHoldemPlayer player = texasHoldemController.GetPlayer(user);
            if (player != null)
            {
                texasHoldemController.Raise(player, raise);
            }
            return null;
        }

        [Command("fold")]
        public Task FoldAsync()
        {
            TexasHoldemController texasHoldemController = _gameBook.GetGame<TexasHoldemController>(Context.Channel);
            if (texasHoldemController == null)
            {
                return null;
            }
            var user = Context.User;
            TexasHoldemPlayer player = texasHoldemController.GetPlayer(user);
            if (player != null)
            {
                texasHoldemController.Fold(player);
            }
            return null;
        }

        [Command("allin")]
        public Task AllInAsync()
        {
            TexasHoldemController texasHoldemController = _gameBook.GetGame<TexasHoldemController>(Context.Channel);
            if (texasHoldemController == null)
            {
                return null;
            }
            var user = Context.User;
            TexasHoldemPlayer player = texasHoldemController.GetPlayer(user);
            if (player != null)
            {
                texasHoldemController.AllIn(player);
            }
            return null;
        }

        [Command("cards")]
        [Alias("status")]
        public Task StatusAsync()
        {
            TexasHoldemController texasHoldemController = _gameBook.GetGame<TexasHoldemController>(Context.Channel);
            if (texasHoldemController == null)
            {
                return null;
            }
            var user = Context.User;
            TexasHoldemPlayer player = texasHoldemController.GetPlayer(user);
            if (player != null)
            {
                texasHoldemController.SendStatus(player);
            }
            return null;
        }
    }
}
