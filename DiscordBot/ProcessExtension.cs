using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    static class ProcessExtension
    {
        public static void KillChildProcesses(this Process process)
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
           "SELECT * " +
           "FROM Win32_Process " +
           "WHERE ParentProcessId=" + process.Id);
            ManagementObjectCollection collection = searcher.Get();
            if (collection.Count > 0)
            {
                foreach (var item in collection)
                {
                    UInt32 childProcessId = (UInt32)item["ProcessId"];
                    if ((int)childProcessId != Process.GetCurrentProcess().Id)
                    {
                        Process childProcess = Process.GetProcessById((int)childProcessId);
                        childProcess.Kill();
                    }
                }
            }

        }
    }
}
