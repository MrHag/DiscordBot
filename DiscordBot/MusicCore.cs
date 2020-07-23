using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class MusicCore
    {
        private Process _process;
        public MusicCore()
        {

        }

        public enum playtypes { DEFAULT, YOUTUBE };

        public Stream Play(string path, playtypes type)
        {
            if (type == playtypes.YOUTUBE)
            {
                _process = StartYoutubeDLProcess(path);
            }
            else
            {
                _process = StartProcess(path);
            }
            if (!_process.HasExited)
            {
                return _process.StandardOutput.BaseStream;
            }
            return null;
        }

        public void Stop()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.KillChildProcesses();
                _process.Kill();
            }
        }

        public void Pause()
        {


        }

        public void Resume()
        {



        }

        private Process StartProcess(string filePath)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{filePath}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private Process StartYoutubeDLProcess(string url)
        {
            return Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --default-search ytsearch -o - \"{url}\" | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            });
        }

    }
}
