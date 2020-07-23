using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpCALC;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HtmlAgilityPack;

namespace DiscordBot
{
    public class cm : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        {
            return ReplyAsync(echo, false, null, null);
        }

        [Command("Calc", RunMode = RunMode.Async)]
        public async Task CalcAsync([Remainder] string exc = null)
        {
            if (exc != null)
            {
                await ReplyAsync(exc + " = " + Calculator.calculate(exc).val.Replace(',', '.'), false, null, null);
            }
        }

        [Command("$", RunMode = RunMode.Async)]
        public async Task DolAsync([Remainder] string num = "1")
        {
            try
            {

                HtmlNode tbody = await SiteWorker.ParseSite();
                double init = double.Parse(num.Replace('.', ','));
                await Context.Channel.SendMessageAsync(DolAsync(tbody, init, "UAH")
                                                + '\n' + DolAsync(tbody, init, "RUB")
                                                + '\n' + DolAsync(tbody, init, "BYN")
                                                + '\n' + DolAsync(tbody, init, "CNY")
                                                + '\n' + DolAsync(tbody, init, "EUR"),
                                                false, null, null);
                return;
            }
            catch (Exception ex)
            {
                await Program.LogAsync(new LogMessage(LogSeverity.Error, nameof(DolAsync), ex.Message, ex));
            }

        }
        internal static double DolAsync(HtmlNode tbody, string type)
        {
            for (int i = 1; i < tbody.ChildNodes.Count; i += 2)
            {
                HtmlNode tr = tbody.ChildNodes.ElementAt(i);
                if (tr.ChildNodes.ElementAt(3).InnerHtml == type)
                {
                    return (float.Parse(tr.ChildNodes.ElementAt(9).InnerHtml) / int.Parse(tr.ChildNodes.ElementAt(5).InnerHtml));
                }
            }
            return 0;
        }

        internal static string DolAsync(HtmlNode tbod, double ini, string type)
        {
            string str = ini + " $ = ";
            double dolini = DolAsync(tbod, "USD") * ini;
            if (type == "UAH")
            {
                str += Math.Round(dolini, 2);
            }
            else
            {
                str += Math.Round(dolini / DolAsync(tbod, type), 2);
            }
            return str + " " + type;
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task HAsync(string find = null, [Remainder] int count = 3)
        {
            if ((base.Context.Channel as ITextChannel).IsNsfw)
            {
                Uri Url;
                if (find != null)
                {
                    Url = new Uri("https://rt.pornhub.com/video/search?search=" + find);
                }
                else
                {
                    Url = new Uri("https://rt.pornhub.com");
                }
                string content = await SiteWorker.HTTPDownload(Url);
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(content);
                List<string> links = new List<string>();
                foreach (HtmlNode a in htmlDocument.DocumentNode.SelectNodes("//a"))
                {
                    if (a.HasClass("linkVideoThumb"))
                    {
                        links.Add("http://rt.pornhub.com" + a.GetAttributeValue("href", ""));
                    }
                }
                string outlinks = "";
                for (int i = 0; i < ((count > links.Count<string>()) ? links.Count<string>() : count); i++)
                {
                    outlinks += (links[Program.Rand.Next(0, links.Count<string>() - 1)]) + "\n\n";
                }
                await Context.Channel.SendMessageAsync(outlinks, false, null, null);
                links = null;
            }
        }

        [Command("setwellcome", RunMode = RunMode.Async)]
        public async Task Setwellcome()
        {
            using (BotDbContext dbcontext = new BotDbContext())
            {
                DiscordGuildModel GuildModel = dbcontext.GetGuild(Context.Guild.Id);

                if (GuildModel == null)
                    GuildModel = dbcontext.GreateGuild(new DiscordGuildModel() { GuildId = Context.Guild.Id });

                if (GuildModel.WellcomeChannelId == Context.Channel.Id)
                {
                    GuildModel.WellcomeChannelId = default;
                    await ReplyAsync("Wellcome channel removed");
                }
                else
                {
                    GuildModel.WellcomeChannelId = Context.Channel.Id;
                    await ReplyAsync("Wellcome channel set");
                }

                dbcontext.Update(GuildModel);
                dbcontext.SaveChanges();
            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveChannel()
        {
            Bot bot = Program.GetBot(Context.Client);
            MusicService musicService = bot.GetMusicService(Context.Guild.Id);
            if (musicService != null)
            {
                await musicService.DisconnectFromVoice();
                bot.RemoveMusicService(Context.Guild.Id);
            }
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel()
        {
            await JoinChannelByUser(Context.User);
        }

        public async Task<MusicService> JoinChannelByUser(SocketUser User)
        {
            if (User is IVoiceState voiceState)
            {
                if (voiceState.VoiceChannel != null)
                    return await JoinChannel(voiceState.VoiceChannel);
            }
            return null;
        }

        public async Task<MusicService> JoinChannel(IVoiceChannel audioChannel)
        {
            Bot bot = Program.GetBot(Context.Client);
            MusicService musicService = bot.GetMusicService(Context.Guild.Id);
            if (musicService == null)
            {
                musicService = new MusicService();
                bot.AddMusicService(Context.Guild.Id, musicService);
            }
            
                if (audioChannel != null)
                {
                    if (musicService.AudioChannel != audioChannel)
                    {
                        await musicService.ConnectToVoice(audioChannel);
                    }
                }
            return await Task.FromResult(musicService);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayMusic([Remainder] string music)
        {
            Bot bot = Program.GetBot(Context.Client);
            Console.WriteLine("Wait pool...");
            bot.ResPool.WaitOne();
            Console.WriteLine("Pool is got");

            MusicService musicService = await JoinChannelByUser(Context.User);

            if (musicService.IsPlay)
            {
                await musicService.StopMusic();
                Console.WriteLine("stop music");
            }

            bool starts = false;
            string searchcommand = "search ";
            if (music.StartsWith(searchcommand))
            {
                starts = true;
                music = music.Substring(searchcommand.Length);
            }

            if (musicService != null)
                await musicService.PlayMusic(music, starts);
            Console.WriteLine("release pool");
            bot.ResPool.Release(1);
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopMusic()
        {
            Bot bot = Program.GetBot(Context.Client);
            MusicService musicService = bot.GetMusicService(Context.Guild.Id);
            if (musicService != null)
            {
                await musicService.StopMusic();
            }
        }

    }
}
