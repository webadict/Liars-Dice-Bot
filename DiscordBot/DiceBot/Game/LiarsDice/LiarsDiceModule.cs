using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.DiceBot.Game.LiarsDice
{
    public class LiarsDiceModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameBook _gameBook;

        public LiarsDiceModule(GameBook gameBook)
        {
            _gameBook = gameBook;
        }

        [Command("players")]
        [Summary("Retrieve a list of players in the game.")]
        public Task GetPlayersAsync()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            return ReplyAsync(liarsDiceController.GetPlayerList());
        }

        [Command("bid")]
        [Summary("Makes a bid.")]
        public Task BidAsync(
            [Summary("The of the dice.")]
            int total)
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            var user = Context.User;
            LiarsDicePlayer player = liarsDiceController.GetPlayer(user);
            if (player != null)
            {
                liarsDiceController.Bid(player, total);
            }
            return null;
        }
        

        [Command("bid")]
        [Summary("Makes a bid.")]
        public Task BidAsync(
            [Summary("The quantity to bid.")]
            int quantity,
            [Summary("The rank to bid.")]
            int rank)
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            var user = Context.User;
            LiarsDicePlayer player = liarsDiceController.GetPlayer(user);
            if (player != null)
            {
                liarsDiceController.Bid(player, quantity, rank);
            }
            return null;
        }

        [Command("challenge")]
        [Alias("liar")]
        public Task ChallengeAsync()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            var user = Context.User;
            LiarsDicePlayer player = liarsDiceController.GetPlayer(user);
            if (player != null)
            {
                liarsDiceController.Challenge(player);
            }
            return null;
        }

        [Command("deadon")]
        [Alias("dead-on")]
        public Task DeadOnAsync()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            var user = Context.User;
            LiarsDicePlayer player = liarsDiceController.GetPlayer(user);
            if (player != null)
            {
                liarsDiceController.DeadOn(player);
            }
            return null;
        }

        [Command("dice")]
        public Task GetDiceAsync()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            var user = Context.User;
            LiarsDicePlayer player = liarsDiceController.GetPlayer(user);
            if (player != null)
            {
                liarsDiceController.SendDice(player);
            }
            return null;
        }

        [Command("commands")]
        public Task Commands()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            return null;
        }

        [Command("help")]
        public Task Help()
        {
            //return null;
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            //var help = "For ";


            var message = "To bet, type `!bet [quantity] [rank]`.\nIf you believe that the previous bid was greater than the number of dice of that face, type `!challenge`.\nIf you believe that the previous bid is exactly correct, type `!deadon`";
            if (liarsDiceController.Mods.Wilds)
            {
                message += "\n1s appear as `*` and count as any number. They cannot be bid.";
            }
            if (liarsDiceController.Mods.SixesOnly)
            {
                message += "\nAll non-6s appear as `X` and cannot be bid.";
            }
            return ReplyAsync(message);
        }

        [Command("timer")]
        public Task TimerAsync(int timer)
        {
            return null;
        }
        
        [Command("cheatmode")]
        public Task CheatAsync()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            if (Context.User.Username == "webadict" && Context.User.Discriminator == "0347")
            {
                return ReplyAsync(Context.User.Mention + " is already cheating.");
            }
            return ReplyAsync(Context.User.Mention + " has activated cheat mode!");
        }

        [Command("hotdog")]
        public Task HotdogAsync()
        {
            return null;
        }

        [Command("test")]
        public Task SaveCurrentGame()
        {
            return null;
        }

        [Command("spectate")]
        [RequireOwner]
        public Task SpectateAsync()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            Context.User.SendMessageAsync(liarsDiceController.RevealDiceString()).Wait();
            return null;
        }

        [Command("reveal")]
        public Task RevealAsync(int rank)
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            if (!liarsDiceController.Mods.Reveal || rank < 1 || rank > liarsDiceController.Mods.NumberOfSides)
            {
                return null;
            }
            var user = Context.User;
            LiarsDicePlayer player = liarsDiceController.GetPlayer(user);
            if (player != null && !player.HasRevealed)
            {
                var reveal = player.RevealADie(rank);
                if (reveal != null)
                {
                    ReplyAsync(reveal).Wait();
                }
            }
            return null;
        }

        [Command("flip")]
        public Task FlipDieAsync(int face1, int face2)
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            if (!liarsDiceController.Mods.FlipDie || face1 < 1 || face1 > liarsDiceController.Mods.NumberOfSides)
            {
                return null;
            }
            if (!liarsDiceController.Mods.FlipDie || face2 < 1 || face2 > liarsDiceController.Mods.NumberOfSides)
            {
                return null;
            }
            var user = Context.User;
            LiarsDicePlayer player = liarsDiceController.GetPlayer(user);
            if (player != null && !player.HasFlipped)
            {
                var reveal = player.FlipADie(face1, face2);
                if (reveal != null)
                {
                    ReplyAsync(reveal).Wait();
                }
            }
            return null;
        }

        [Command("mods")]
        public Task ListAvailableMods()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            return ReplyAsync(liarsDiceController.Mods.ModString());
        }

        [Command("notify")]
        public Task NotifyAsync()
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            if (liarsDiceController.GameState != GameState.Active)
            {
                return null;
            }
            liarsDiceController.NotifyCurrentPlayer();
            return null;
        }

        [RequireOwner]
        [Command("mod")]
        public Task ModifyGame(params string[] mods)
        {
            LiarsDiceController liarsDiceController = _gameBook.GetGame<LiarsDiceController>(Context.Channel);
            if (liarsDiceController == null)
            {
                return null;
            }
            if (liarsDiceController.GameState != GameState.InSignups)
            {
                return null;
            }
            List<string> modList = new List<string>();

            foreach(var mode in mods)
            {
                bool added = false;
                string mod;
                switch (mode)
                {
                    default:
                        continue;
                    case "d4":
                        mod = LiarsDiceMods.D4String;
                        if (liarsDiceController.Mods.SixesOnly)
                        {
                            modList.Add($"Incompatible mod: {mod}");
                            continue;
                        }
                        added = liarsDiceController.Mods.NumberOfSides != 4;
                        liarsDiceController.Mods.NumberOfSides = added ? 4 : 6;
                        break;
                    case "sd":
                        mod = LiarsDiceMods.SuddenDeathString;
                        added = liarsDiceController.Mods.NumberOfDice != 1;
                        liarsDiceController.Mods.NumberOfDice = added ? 1 : 5;
                        break;
                    case "rev":
                    case "revolution":
                        mod = LiarsDiceMods.RevolutionString;
                        added = !liarsDiceController.Mods.Revolution;
                        liarsDiceController.Mods.Revolution = !liarsDiceController.Mods.Revolution;
                        break;
                    case "wilds":
                    case "wild":
                        mod = LiarsDiceMods.WildsString;
                        if (liarsDiceController.Mods.SixesOnly)
                        {
                            modList.Add($"Incompatible mod: {mod}");
                            continue;
                        }
                        added = !liarsDiceController.Mods.Wilds;
                        liarsDiceController.Mods.Wilds = !liarsDiceController.Mods.Wilds;
                        break;
                    case "pool":
                        mod = LiarsDiceMods.PoolString;
                        added = !liarsDiceController.Mods.Pool;
                        liarsDiceController.Mods.Pool = !liarsDiceController.Mods.Pool;
                        break;
                    case "reveal":
                        mod = LiarsDiceMods.RevealString;
                        added = liarsDiceController.Mods.Reveal = !liarsDiceController.Mods.Reveal;
                        break;
                    case "blind":
                        mod = LiarsDiceMods.BlindString;
                        added = liarsDiceController.Mods.Blind = !liarsDiceController.Mods.Blind;
                        break;
                    case "flip":
                        mod = LiarsDiceMods.FlipString;
                        added = liarsDiceController.Mods.FlipDie = !liarsDiceController.Mods.FlipDie;
                        break;
                    case "chaos":
                        mod = LiarsDiceMods.ChaosString;
                        added = liarsDiceController.Mods.Chaos = !liarsDiceController.Mods.Chaos;
                        break;
                    case "stupid":
                        mod = LiarsDiceMods.StupidString;
                        added = liarsDiceController.Mods.Stupid = !liarsDiceController.Mods.Stupid;
                        break;
                    case "total":
                    case "count":
                        mod = LiarsDiceMods.CountString;
                        added = liarsDiceController.Mods.CountPips = !liarsDiceController.Mods.CountPips;
                        break;
                    case "sixesonly":
                    case "6sonly":
                        mod = LiarsDiceMods.SixesOnlyString;
                        if (liarsDiceController.Mods.NumberOfSides < 6 || liarsDiceController.Mods.Wilds)
                        {
                            modList.Add($"Incompatible mod: {mod}");
                            continue;
                        }
                        added = liarsDiceController.Mods.SixesOnly = !liarsDiceController.Mods.SixesOnly;
                        break;
                }
                if (added)
                {
                    modList.Add($"Added mod: {mod}");
                } else
                {
                    modList.Add($"Removed mod: {mod}");

                }
            }
            return ReplyAsync(string.Join("\n", modList));
        }
    }
}
