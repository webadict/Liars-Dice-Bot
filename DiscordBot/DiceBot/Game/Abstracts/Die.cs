namespace DiscordBot.DiceBot.Game.Abstracts
{
    public class Die
    {
        public int Sides { get; set; }
        public int Value { get; set; }

        public Die(int sides = 6)
        {
            Sides = sides;
        }
    }
}
