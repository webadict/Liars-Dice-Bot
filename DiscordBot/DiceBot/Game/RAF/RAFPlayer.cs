using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.RAF.Actions;

namespace DiscordBot.DiceBot.Game.RAF
{
    public class RAFPlayer : GamePlayer
    {
        public int Health { get; protected set; }

        public bool IsDead => Health <= 0;
        public bool Active => !IsDead;
        public ActionOption ActionOption { get; set; }

        protected int _delayedDamage = 0;

        public RAFPlayer(SocketUser user) : base(user)
        {
            Health = 1;
            ActionOption = ActionOption.NONE;
        }

        public void TakeDamage(int damage = 1)
        {
            _delayedDamage += damage;
        }

        public int EndTurn()
        {
            if (ActionOption == ActionOption.REFLECTING_KILLING)
            {
                _delayedDamage = 1;
            }
            Health -= _delayedDamage;
            int _ = _delayedDamage;
            _delayedDamage = 0;
            return _;
        }
    }
}
