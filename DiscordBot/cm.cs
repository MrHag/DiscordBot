using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CSharpCALC;
using Discord;
using Discord.Commands;
using HtmlAgilityPack;

namespace D2
{
    public class cm : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
        {
            return this.ReplyAsync(echo, false, null, null);
        }

        [Command("Calc", RunMode = RunMode.Async)]
        public async Task CalcAsync([Remainder] string exc = null)
        {
            if (exc != null)
            {
                await this.ReplyAsync(exc + " = " + Calculator.calculate(exc).val.Replace(',', '.'), false, null, null);
            }
        }

        [Command("$", RunMode = RunMode.Async)]
        public async Task DolAsync([Remainder] string num = "1")
        {     
            try
            {
                
                HtmlNode tbody = await ParseSite();
                double init = double.Parse(num.Replace('.', ','));
                await Context.Channel.SendMessageAsync(DolAsync(tbody, init, "UAH")
                                                +'\n'+ DolAsync(tbody, init, "RUB")
                                                +'\n'+ DolAsync(tbody, init, "BYN")
                                                +'\n'+ DolAsync(tbody, init, "CNY")
                                                +'\n'+ DolAsync(tbody, init, "EUR"),
                                                false, null, null);
                return;
            }
            catch (Exception ex)
            {
                await Program.LogAsync(new LogMessage(LogSeverity.Error, nameof(DolAsync), ex.Message, ex));
            }
            
        }

        public async Task<HtmlNode> ParseSite()
        {

                Uri Url = new Uri("https://bank.gov.ua/markets/exchangerates");
                string content = await HTTPDownload(Url);
                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(content);
                HtmlNode tbody = html.GetElementbyId("exchangeRates").ChildNodes.ElementAt(3);
                return tbody;

        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task HAsync([Remainder] string find = null)
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
                string content = await this.HTTPDownload(Url);
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
                for (int i = 0; i < ((5 > links.Count<string>()) ? links.Count<string>() : 5); i++)
                {
                    await Context.Channel.SendMessageAsync(links[Program.Rand.Next(0, links.Count<string>() - 1)], false, null, null);
                }
                links = null;
            }
        }

        public async Task<string> HTTPDownload(Uri ResourceURI)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback((object s, X509Certificate cert, X509Chain ch, SslPolicyErrors sec) => true));
            ServicePointManager.DefaultConnectionLimit = 50;
            CookieContainer cookieContainer = new CookieContainer();
            HttpWebRequest httpRequest = WebRequest.CreateHttp(ResourceURI);
            try
            {
                httpRequest.CookieContainer = cookieContainer;
                httpRequest.Timeout = (int)TimeSpan.FromSeconds(15.0).TotalMilliseconds;
                httpRequest.AllowAutoRedirect = true;
                httpRequest.AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate);
                httpRequest.ServicePoint.Expect100Continue = false;
                httpRequest.UserAgent = "Mozilla / 5.0(Windows NT 6.1; WOW32; Trident / 7.0; rv: 11.0) like Gecko";
                httpRequest.Accept = "ext/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate;q=0.8");
                httpRequest.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");

                return new StreamReader(
                    (await httpRequest.GetResponseAsync()).GetResponseStream()
                                       ).ReadToEnd();
                
            }
            
            catch (Exception ex)
            {

                await Program.LogAsync(new LogMessage(LogSeverity.Error, nameof(HTTPDownload)+": ", ex.Message, ex));

            }
            return "";
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
	}
}
