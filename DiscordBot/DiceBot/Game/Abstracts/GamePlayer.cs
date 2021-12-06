using Discord.WebSocket;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public abstract class GamePlayer
    {
        public SocketUser User { get; set; }

        protected GamePlayer(SocketUser user)
        {
            User = user;
        }

        public string Nickname
        {
            get
            {
                var nickname = User.Username;
                if (User is SocketGuildUser)
                {
                    nickname = ((SocketGuildUser)User).Nickname ?? nickname;
                }
                return nickname;
            }
        }
    }
}
