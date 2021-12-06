using Discord.Commands;
using DiscordBot.DiceBot.Game.Abstracts;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.DiceBot.Game.Skull
{
    public class SkullModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameBook _gameBook;

        public SkullModule(GameBook gameBook)
        {
            _gameBook = gameBook;
        }
        
        //[Command("players")]
        //[Summary("Retrieve a list of players in the game.")]
        //public Task GetPlayersAsync()
        //{
        //    SkullController liarsDiceController = _gameBook.GetGame<SkullController>(Context.Channel);
        //    if (liarsDiceController == null)
        //    {
        //        return null;
        //    }
        //    if (liarsDiceController.ActivePlayers.Count == 0)
        //    {
        //        return ReplyAsync("There are currently no players.");
        //    }
        //    var players = string.Join("\n", liarsDiceController.ActivePlayers.Select(x => string.Format("{0} - {1} {2}", x.User.Username, x.Tokens.Count, x.Tokens.Count > 1 ? "dice" : "die")));

        //    return ReplyAsync($"```**Players:**\n{players}```");
        //}

        //[Command("bid")]
        //[Summary("Makes a bid.")]
        //public Task BidAsync(
        //    [Summary("The quantity to bid.")]
        //    int quantity)
        //{
        //    SkullController liarsDiceController = _gameBook.GetGame<SkullController>(Context.Channel);
        //    if (liarsDiceController == null)
        //    {
        //        return null;
        //    }
        //    var user = Context.User;
        //    SkullPlayer player = liarsDiceController.GetPlayer(user);
        //    if (player != null)
        //    {
        //        liarsDiceController.Bid(player, quantity);
        //    }
        //    return null;
        //}

        //[Command("challenge")]
        //public Task ChallengeAsync()
        //{
        //    SkullController liarsDiceController = _gameBook.GetGame<SkullController>(Context.Channel);
        //    if (liarsDiceController == null)
        //    {
        //        return null;
        //    }
        //    var user = Context.User;
        //    SkullPlayer player = liarsDiceController.GetPlayer(user);
        //    if (player != null)
        //    {
        //        liarsDiceController.Challenge(player);
        //    }
        //    return null;
        //}

        //[Command("dice")]
        //public Task GetDiceAsync()
        //{
        //    SkullController liarsDiceController = _gameBook.GetGame<SkullController>(Context.Channel);
        //    if (liarsDiceController == null)
        //    {
        //        return null;
        //    }
        //    var user = Context.User;
        //    SkullPlayer player = liarsDiceController.GetPlayer(user);
        //    if (player != null)
        //    {
        //        liarsDiceController.SendDice(player);
        //    }
        //    return null;
        //}
    }
}
