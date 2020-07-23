using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class MusicService
    {
        private MusicCore _musicCore;
        private IAudioClient _audioClient;
        public IAudioChannel AudioChannel { get; private set; }
        public bool IsPlay { get; set; }

        private CancellationTokenSource CancellationTokenSource;
        public MusicService()
        {
            _musicCore = new MusicCore();
            _audioClient = null;
            AudioChannel = null;
        }

        public async Task ConnectToVoice(IAudioChannel voiceChannel)
        {
            if (voiceChannel == null)
                return;

            _audioClient = await voiceChannel.ConnectAsync();
            AudioChannel = voiceChannel;
            Console.WriteLine($"Connected to channel {voiceChannel?.Id}");
            return;
        }

        public async Task DisconnectFromVoice()
        {
            if (_audioClient != null || AudioChannel != null)
            {
                await _audioClient.StopAsync();
                await AudioChannel.DisconnectAsync();
                Console.WriteLine($"Disconnected from channel {AudioChannel?.Id}");
                AudioChannel = null;
                _audioClient = null;
            }
        }

        public async Task PlayMusic(string path, bool search = false)
        {

            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
            }

            //Thread.Sleep(1000000);
            CancellationTokenSource = new CancellationTokenSource();
            
            
            MusicCore.playtypes type = MusicCore.playtypes.DEFAULT;
            if (path.ToLower().Contains("youtube.com") || path.ToLower().Contains("youtu.be") || search)
            {
                type = MusicCore.playtypes.YOUTUBE;
            }
            var output = _musicCore.Play(path, type);
            var discord = _audioClient.CreatePCMStream(AudioApplication.Voice);
            IsPlay = true;
            _ = Task.Run(async () =>
              {
                  try
                  {
                      await output.CopyToAsync(discord, 81920, CancellationTokenSource.Token);
                      await discord.FlushAsync(CancellationTokenSource.Token);
                      CancellationTokenSource.Token.ThrowIfCancellationRequested();

                      IsPlay = false;
                      CancellationTokenSource.Dispose();
                      CancellationTokenSource = null;
                  }
                  catch (Exception)
                  {
                      IsPlay = false;
                      CancellationTokenSource.Dispose();
                      CancellationTokenSource = null;
                  }
              });   

            return;
        }
        public Task StopMusic()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                while (CancellationTokenSource != null) { Thread.Sleep(1000); }
            }

            _musicCore.Stop();
            IsPlay = false;
            return Task.CompletedTask;
        }

    }
}
