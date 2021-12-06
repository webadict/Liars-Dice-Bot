using System;
using System.Collections.Generic;

namespace DiscordBot.DiceBot.Game.LiarsDice
{
    public class LiarsDiceMods
    {
        public static string D4String = "Dice are d4s";
        public static string SuddenDeathString = "Sudden Death! Only 1 die!";
        public static string RevolutionString = "Revolution! Bidding the same rank and quality swaps the order of ranks (1s become high and 6s become low).";
        public static string WildsString = "1s are Wild.";
        public static string PoolString = "An extra dice pool exists that no one sees.";
        public static string RevealString = "You may reveal a single die you do or don't have.";
        public static string BlindString = "You see everyone else's dice instead of your own.";
        public static string ChaosString = "Random modifiers are added each round.";
        public static string FlipString = "You may choose to `!flip` a die from one number to another once per round. If you don't own that die, this does nothing.";
        public static string StupidString = "All dice have a 50% chance of being 1.";
        public static string AnyChallengeString = "Any player can `!challenge` a bid. (This doesn't include !deadon.)";
        public static string CountString = "When a player has one die left, the total is bid instead.";
        public static string SixesOnlyString = "Only 6s can be bid. All other dice display as Xs.";

        /// <summary>
        /// True if 1s are wild.
        /// </summary>
        public bool Wilds { get; set; }

        /// <summary>
        /// True if bidding the same rank and quantity flips the highest rank.
        /// </summary>
        public bool Revolution { get; set; }

        /// <summary>
        /// The number of sides of the dice.
        /// </summary>
        public int NumberOfSides { get; set; }

        /// <summary>
        /// The number of dice to start with.
        /// </summary>
        public int NumberOfDice { get; set; }

        /// <summary>
        /// True if there is a middle pool of dice that aren't held by other players.
        /// This pool shrinks with the average held dice.
        /// </summary>
        public bool Pool { get; set; }

        /// <summary>
        /// True if players can reveal their dice.
        /// </summary>
        public bool Reveal { get; set; }

        /// <summary>
        /// True if the players can flip a single die of one side to another side.
        /// </summary>
        public bool FlipDie { get; set; }

        /// <summary>
        /// True if players see other player's dice and not their own.
        /// </summary>
        public bool Blind { get; set; }

        /// <summary>
        /// True if each round has random mods enabled.
        /// </summary>
        public bool Chaos { get; set; }

        /// <summary>
        /// True if all dice have a 50% chance of being 1.
        /// </summary>
        public bool Stupid { get; set; }

        /// <summary>
        /// True if any player can challenge a bid.
        /// </summary>
        public bool AnyChallenge { get; set; }

        /// <summary>
        /// True if the sum is used instead of the quantity of ranks.
        /// </summary>
        public bool CountPips { get; set; }

        /// <summary>
        /// True if sixes are the only number that can be bid.
        /// </summary>
        public bool SixesOnly { get; set; }

        public void ResetGame()
        {
            NumberOfSides = 6;
            NumberOfDice = 5;
            Revolution = false;
            Wilds = false;
            Pool = false;
            Reveal = false;
            Blind = false;
            Chaos = false;
            FlipDie = false;
            Stupid = false;
            CountPips = false;
            SixesOnly = false;
        }

        public string FormatModString(string modStr)
        {
            return modStr + "\n";
        }

        public string ModString()
        {
            var message = "";
            if (NumberOfSides != 6)
            {
                message += FormatModString(D4String);
            }
            if (Revolution)
            {
                message += FormatModString(RevolutionString);
            }
            if (Wilds)
            {
                message += FormatModString(WildsString);
            }
            if (Pool)
            {
                message += FormatModString(PoolString);
            }
            if (Reveal)
            {
                message += FormatModString(RevealString);
            }
            if (Blind)
            {
                message += FormatModString(BlindString);
            }
            if (FlipDie)
            {
                message += FormatModString(FlipString);
            }
            if (Stupid)
            {
                message += FormatModString(StupidString);
            }
            if (CountPips)
            {
                message += FormatModString(CountString);
            }
            if (SixesOnly)
            {
                message += FormatModString(SixesOnlyString);
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                return "There are no active mods.";
            }
            return "```" + message + "```";
        }

        public string PerformChaos(Random random)
        {
            var messages = new List<string>();
            var next = random.Next(2);
            if (next == 1)
            {
                NumberOfSides = 4;
                messages.Add(D4String);
            } else
            {
                NumberOfSides = 6;
            }
            Wilds = random.Next(2) == 1;
            if (Wilds) messages.Add(WildsString);
            Revolution = random.Next(2) == 1;
            if (Revolution) messages.Add(RevolutionString);
            Pool = random.Next(2) == 1;
            if (Pool) messages.Add(PoolString);
            Reveal = random.Next(2) == 1;
            if (Reveal) messages.Add(RevealString);
            Blind = random.Next(2) == 1;
            if (Blind) messages.Add(BlindString);

            if (messages.Count == 0)
            {
                return "No modifiers this round!";
            }
            else
            {
                return "Modifiers:\n" + string.Join("\n", messages);
            }
        }
    }
}
