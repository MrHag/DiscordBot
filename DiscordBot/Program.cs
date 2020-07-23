using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot
{
    class Program
    {
        public static Random Rand = new Random();

        private static Dictionary<DiscordSocketClient, Bot> _bots = new Dictionary<DiscordSocketClient, Bot>();

        public static void AddBot(DiscordSocketClient client, Bot bot)
        {
            _bots.Add(client, bot);
        }

        public static void RemoveBot(DiscordSocketClient client)
        {
            _bots.Remove(client);
        }

        public static Bot GetBot(DiscordSocketClient client)
        {
            try
            {
                return _bots[client];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void Main()
        {
            Bot bot = new Bot();
            bot.Start();
        }

        public static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString(null, true, true, DateTimeKind.Local, new int?(11)));
            return Task.CompletedTask;
        }
    }
}
