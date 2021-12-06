using Discord;
using Discord.Commands;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.NoThanks;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.DiceBot.Game.NoThanks
{
    public class NoThanksModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameBook _gameBook;

        public NoThanksModule(GameBook gameBook)
        {
            _gameBook = gameBook;
        }

        [Command("tokens")]
        public Task TokensAsync()
        {
            NoThanksController noThanksController = _gameBook.GetGame<NoThanksController>(Context.Channel);
            if (noThanksController == null)
            {
                return null;
            }
            var user = Context.User;
            NoThanksPlayer player = noThanksController.GetPlayer(user);
            if (player != null)
            {
                noThanksController.SendStatus(player);
            }
            return null;
        }

        [Command("take")]
        public Task TakeCardAsync()
        {
            NoThanksController noThanksController = _gameBook.GetGame<NoThanksController>(Context.Channel);
            if (noThanksController == null)
            {
                return null;
            }
            var user = Context.User;
            NoThanksPlayer player = noThanksController.GetPlayer(user);
            if (player != null)
            {
                noThanksController.TakeCard(player);
            }
            return null;
        }

        [Command("pass")]
        public Task PassAsync()
        {
            NoThanksController noThanksController = _gameBook.GetGame<NoThanksController>(Context.Channel);
            if (noThanksController == null)
            {
                return null;
            }
            var user = Context.User;
            NoThanksPlayer player = noThanksController.GetPlayer(user);
            if (player != null)
            {
                noThanksController.Pass(player);
            }
            return null;
        }
    }
}
