using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.DiceBot.NewFolder
{
    public class HotdogGenerator
    {
        Random random;

        public string GenerateCondiment()
        {
            List<string> condiments = new List<string>()
            {
                "ketchup",
                "catsup",
                "mustard",
                "relish",
                "mayonaisse",
                "ranch",
                "peanut butter",
                "{fruit} jelly",

            };
            return condiments[random.Next(condiments.Count)];
        }

        public string GenerateFruit()
        {
            List<string> fruits = new List<string>
            {
                "strawberry",
                "grape",
                "watermelon",
                "lemon",
                "blueberry",
                "raspberry",
            };
            return "";
        }
    }
}
