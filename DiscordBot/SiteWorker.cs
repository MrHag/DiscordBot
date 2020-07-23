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


namespace DiscordBot
{
    class SiteWorker
    {
        private SiteWorker() { }
        static public async Task<HtmlNode> ParseSite()
        {

            Uri Url = new Uri("https://bank.gov.ua/markets/exchangerates");
            string content = await HTTPDownload(Url);
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(content);
            HtmlNode tbody = html.GetElementbyId("exchangeRates").ChildNodes.ElementAt(3);
            return tbody;

        }

        static public async Task<string> HTTPDownload(Uri ResourceURI)
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

                await Program.LogAsync(new LogMessage(LogSeverity.Error, nameof(HTTPDownload) + ": ", ex.Message, ex));

            }
            return "";
        }

    }
}
