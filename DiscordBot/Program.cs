using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace D2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
                {
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
                });

            _commands = new CommandService();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += this.MessageReceivedAsync;
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(Discord.TokenType.Bot, ClassForKey.GetKEY(), true);
            await _client.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            await Task.Delay(-1);
        }

        public static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString(null, true, true, DateTimeKind.Local, new int?(11)));
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine(string.Format("{0} is connected!", _client.CurrentUser));
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message != null)
            {
                int argPos = 0;
                if ((Discord.Commands.MessageExtensions.HasCharPrefix(message, '►', ref argPos) || Discord.Commands.MessageExtensions.HasMentionPrefix(message, this._client.CurrentUser, ref argPos)) && !message.Author.IsBot)
                {
                    SocketCommandContext context = new SocketCommandContext(_client, message);
                    await _commands.ExecuteAsync(context, argPos, null, 0);
                }
            }
        }

        public static Random Rand = new Random();

        private readonly CommandService _commands;

        private readonly DiscordSocketClient _client;
    }
}
