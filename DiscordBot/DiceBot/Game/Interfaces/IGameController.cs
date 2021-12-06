using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using System.Collections.Generic;

namespace DiscordBot.DiceBot.Game.Interfaces
{
    public interface IGameController
    {
        SocketTextChannel ActiveChannel { get; set; }
        GameState GameState { get; }
        
        #region Game

        void EnterSignups();
        void CreateGame();
        void StopGame();
        void AddPlayer(SocketUser user);
        void RemovePlayer(SocketUser user);
        void GetPlayers();

        #endregion Game

    }
}
