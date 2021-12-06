using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.LiarsDice
{
    public class LiarsDicePlayer : GamePlayer, IPlayer
    {
        public DiceCollection Dice { get; set; } = new DiceCollection();
        private bool _active = false;
        public bool HasRevealed { get; set; } = false;
        public bool HasFlipped { get; set; } = false;

        public bool Active => _active && Dice.Count > 0;

        public LiarsDicePlayer(SocketUser user) : base(user)
        {
        }

        public void ResetPlayer(Random random, LiarsDiceMods mods)
        {
            Dice.RollDice(random, mods);
            HasRevealed = false;
            HasFlipped = false;
        }

        public void ActivatePlayer()
        {
            _active = true;
        }

        public void RemoveDie(int number = 1)
        {
            Dice.RemoveDie(number);
        }

        public string RevealADie(int rank)
        {
            if (HasRevealed)
            {
                return null;
            }
            HasRevealed = true;
            if (Dice.Any(x => x.Value == rank))
            {
                return $"{User.Mention} has revealed a {rank}.";
            }
            else
            {
                return $"{User.Mention} has revealed no {rank}s.";
            }
        }

        public string FlipADie(int face1, int face2)
        {
            if (HasFlipped)
            {
                return null;
            }
            HasFlipped = true;
            if (Dice.Any(x => x.Value == face1))
            {
                Dice.First(x => x.Value == face1).Value = face2;
            }
            return $"If {User.Mention} has any {face1}s, one has been flipped to a {face2}.";
        }

        public List<int> GetDice()
        {
            return Dice.GetDice();
        }

        public string GetDiceString(LiarsDiceMods mods)
        {
            return Dice.GetDiceString(mods);
        }

        public void SetDice(int numberOfDice, int numberOfSides)
        {
            Dice.SetDice(numberOfDice, numberOfSides);
        }

        public void SetDiceSides(int numberOfSides)
        {
            Dice.SetDiceSides(numberOfSides);
        }

        public int GetRankCount(int rank, LiarsDiceMods mods)
        {
            return Dice.GetRankCount(rank, mods);
        }

        public int GetTotal(LiarsDiceMods mods)
        {
            return Dice.GetTotalCount(mods);
        }
    }
}
