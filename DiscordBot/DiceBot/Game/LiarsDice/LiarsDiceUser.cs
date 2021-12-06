using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.LiarsDice
{
    public class LiarsDiceUser
    {
        public SocketUser User { get; set; }
        public int Wins { get; set; }
        public int GamesPlayed { get; set; }

        public LiarsDiceUser(SocketUser user)
        {
            User = user;
        }

        public double CalculateWinRate()
        {
            if (GamesPlayed <= 0) return 0;
            return (double) Wins / GamesPlayed;
        }
    }
}
