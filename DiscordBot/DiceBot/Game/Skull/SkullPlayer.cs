using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.Skull
{
    public class SkullPlayer : GamePlayer, IPlayer
    {
        public List<Token> Tokens { get; } = new List<Token>();
        private bool _active = false;

        public bool Active => _active && Tokens.Count > 0;

        public SkullPlayer(SocketUser user) : base(user)
        {
        }

        public void ActivatePlayer()
        {
            _active = true;
        }

        public void SetTokens(int skulls = 1, int flowers = 3)
        {
            Tokens.Clear();
            for (int i = 0; i < skulls; i++)
            {
                Tokens.Add(new SkullToken());
            }
            for (int i = 0; i < flowers; i++)
            {
                Tokens.Add(new FlowerToken());
            }

        }

        public void RemoveToken(int number = 1)
        {
            if (Tokens.Count <= number)
            {
                Tokens.Clear();
            } else
            {
            }
        }

        public List<string> GetTokens()
        {
            return Tokens.Select(x => x.Name).OrderBy(x => x).ToList();
        }

        public string GetDiceString(bool wild = false)
        {
            var message = "";
            return message;
        }

        public void RandomizeTokens(Random random)
        {
        }

        public int GetRankCount(int rank, bool wild = false)
        {
            int count = 0;
            return count;
        }
    }
}
