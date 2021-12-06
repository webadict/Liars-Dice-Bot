using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.LiarsDice;
using DiscordBot.DiceBot.Game.RAF;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DiscordBot
{
    public class Initialize
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;

        // Ask if there are existing CommandService and DiscordSocketClient
        // instance. If there are, we retrieve them and add them to the
        // DI container; if not, we create our own.
        public Initialize(CommandService commands = null, DiscordSocketClient client = null)
        {
            _commands = commands ?? new CommandService();
            _client = client ?? new DiscordSocketClient();
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton<GameBook>()
            .AddSingleton<RAFController>()
            .AddSingleton<Random>()
            // You can pass in an instance of the desired type
            //.AddSingleton(new NotificationService())
            // ...or by using the generic method.
            //
            // The benefit of using the generic method is that 
            // ASP.NET DI will attempt to inject the required
            // dependencies that are specified under the constructor s
            // for us.
            //.AddSingleton<DatabaseService>()
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }
}
