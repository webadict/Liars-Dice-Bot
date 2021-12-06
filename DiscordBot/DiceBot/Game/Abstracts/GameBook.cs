using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Interfaces;
using System;
using System.Collections.Generic;

namespace DiscordBot.DiceBot.Game.Abstracts
{
    public class GameBook
    {
        private Dictionary<ISocketMessageChannel, IGameController> GameDictionary { get; }

        public GameBook()
        {
            GameDictionary = new Dictionary<ISocketMessageChannel, IGameController>();
        }

        public void Add(ISocketMessageChannel channel, IGameController game)
        {
            if (GameDictionary.ContainsKey(channel))
            {
                GameDictionary[channel] = game;
            } else
            {
                GameDictionary.Add(channel, game);
            }
        }

        public bool ContainsChannel(ISocketMessageChannel channel)
        {
            return GameDictionary.ContainsKey(channel);
        }

        internal T GetGame<T>(ISocketMessageChannel channel) where T : IGameController
        {
            if (ContainsChannel(channel))
            {
                IGameController game = GameDictionary[channel];
                if (typeof(T).IsInstanceOfType(game))
                {
                    return (T)game;
                }
            }
            return default(T);
        }

        public bool ChannelHasGame<T>(ISocketMessageChannel channel) where T : IGameController
        {
            return GameDictionary[channel].GetType() == typeof(T);
        }

        public IGameController this[ISocketMessageChannel channel]
        {
            get
            {
                return GameDictionary[channel];
            }
            set
            {
                GameDictionary[channel] = value;
            }
        }
    }
}
