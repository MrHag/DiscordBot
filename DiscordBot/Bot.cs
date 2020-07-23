using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot
{
    class Bot
    {

        private CommandService _commands;

        private DiscordSocketClient _client;

        private Dictionary<ulong, MusicService> _musicServices;

        public Semaphore ResPool = new Semaphore(1, 1);

        public Bot()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
               // WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
            });

            _musicServices = new Dictionary<ulong, MusicService>();

            Program.AddBot(_client, this);

            _commands = new CommandService();
            _client.Log += Program.LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.UserJoined += _client_UserJoined;
            _client.JoinedGuild += _client_JoinedGuild;
        }

        private Task _client_JoinedGuild(SocketGuild arg)
        {
            using (BotDbContext dbcontext = new BotDbContext())
            {
                DiscordGuildModel GuildModel = dbcontext.GetGuild(arg.Id);

                if (GuildModel == null)
                    dbcontext.GreateGuild(new DiscordGuildModel() { GuildId = arg.Id });
            }

            return Task.CompletedTask;
        }

        private Task _client_UserJoined(SocketGuildUser arg)
        {
            using (BotDbContext dbcontext = new BotDbContext())
            {
                DiscordGuildModel GuildModel = dbcontext.GetGuild(arg.Guild.Id);

                if (GuildModel == null)
                    GuildModel = dbcontext.GreateGuild(new DiscordGuildModel() { GuildId = arg.Guild.Id });

                if (GuildModel.WellcomeChannelId != default)
                {
                    var channel = arg.Guild.GetChannel(GuildModel.WellcomeChannelId) as IMessageChannel;
                    if (channel != null)
                        channel.SendMessageAsync("Hi " + arg.Mention + ". Happy to see you.");
                }
            }

            return Task.CompletedTask;
        }

        public void Start()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(Discord.TokenType.Bot, ClassForKey.GetKEY(), true);
            await _client.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            await Task.Delay(-1);
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
                if ((Discord.Commands.MessageExtensions.HasCharPrefix(message, '>', ref argPos) || Discord.Commands.MessageExtensions.HasMentionPrefix(message, _client.CurrentUser, ref argPos)) && !message.Author.IsBot)
                {
                    SocketCommandContext context = new SocketCommandContext(_client, message);
                    await _commands.ExecuteAsync(context, argPos, null, 0);
                }
            }
        }

        public void AddMusicService(ulong GuildId, MusicService musicService)
        {
            _musicServices.Add(GuildId, musicService);
        }

        public void RemoveMusicService(ulong GuildId)
        {
            _musicServices.Remove(GuildId);
        }

        public MusicService GetMusicService(ulong GuildId)
        {
            try
            {
                return _musicServices[GuildId];
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
