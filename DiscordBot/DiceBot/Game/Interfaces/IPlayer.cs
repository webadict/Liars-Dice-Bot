using Discord.WebSocket;

namespace DiscordBot.DiceBot.Game.Interfaces
{
    public interface IPlayer
    {
        bool Active { get; }

        SocketUser User { get; set; }
    }
}
