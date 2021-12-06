using DiscordBot.DiceBot.Game.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.DiceBot.Game.LiarsDice
{
    public class DiceCollection : List<Die>
    {
        public void SetDice(int number, int sides = 6)
        {
            this.Clear();
            for (int i = 0; i < number; i++)
            {
                this.Add(new Die(sides));
            }
        }

        public void SetDiceSides(int sides)
        {
            ForEach(x => x.Sides = sides);
        }

        public void RemoveDie(int number = 1)
        {
            if (this.Count <= number)
            {
                this.Clear();
            }
            else
            {
                this.RemoveRange(0, number);
            }
        }

        public void RollDice(Random random, LiarsDiceMods mods)
        {
            foreach (Die die in this)
            {
                if (mods.Stupid)
                {
                    if (random.Next(2) == 1)
                    {
                        die.Value = 1;
                    } else
                    {
                        die.Value = random.Next(die.Sides - 1) + 2;
                    }
                } else
                {
                    die.Value = random.Next(die.Sides) + 1;
                }
            }
        }

        public int GetRankCount(int rank, LiarsDiceMods mods)
        {
            return this.Count(x =>
            {
                if (x.Value == 1 && mods.Wilds)
                {
                    return true;
                }
                return x.Value == rank;
            });
        }

        public int GetTotalCount(LiarsDiceMods mods)
        {
            return this.Sum(x => x.Value);
        }

        public List<int> GetDice()
        {
            return this.Select(x => x.Value).OrderBy(x => x).ToList();
        }

        public string GetDiceString(LiarsDiceMods mods)
        {
            var dice = GetDice();
            string diceString;
            if (mods.Wilds)
            {
                diceString = string.Join(", ", dice.Select(x => x == 1 ? "*" : x.ToString()));
            }
            else if (mods.SixesOnly)
            {
                diceString = string.Join(", ", dice.Select(x => x != 6 ? "X" : x.ToString()));
            }
            else
            {
                diceString = string.Join(", ", dice.Select(x => x.ToString()));
            }
            return diceString;
        }
    }
}
