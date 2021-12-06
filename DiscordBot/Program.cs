using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private IServiceProvider _serviceProvider;

        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _serviceProvider = new Initialize(null, _client).BuildServiceProvider();
            CommandHandler commandHandler = (CommandHandler) _serviceProvider.GetService(typeof(CommandHandler));
            await commandHandler.InitializeAsync();

            _client.Log += Log;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            
            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            var token = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
