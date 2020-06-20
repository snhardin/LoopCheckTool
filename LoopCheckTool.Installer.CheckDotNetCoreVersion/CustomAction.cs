using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

namespace LoopCheckTool.Installer.CheckDotNetCoreVersion
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CheckDotNetCoreVersion(Session session)
        {
            // Simply check dotnet command output until https://github.com/dotnet/designs/blob/master/accepted/2020/install-locations.md
            // is implemented.
            Process proc = new Process();
            proc.StartInfo.FileName = "dotnet.exe";
            proc.StartInfo.Arguments = "--list-runtimes";

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.UseShellExecute = false;

            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = false;

            StringBuilder output = new StringBuilder();
            proc.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);

            StringBuilder errorOutput = new StringBuilder();
            proc.ErrorDataReceived += (sender, args) => errorOutput.AppendLine(args.Data);

            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            proc.WaitForExit();

            if (proc.ExitCode == 0 && FindCompatibleDotNetCoreVersion(output.ToString()))
            {
                return ActionResult.Success;
            }

            Record record = new Record(2);
            record[0] = "[1]";
            record[1] = "No DotNet Core runtime has been detected on your computer. This program " +
                "requires .NET Core >= 3.1. Please install the runtime before attempting to " +
                "run this program.";
            session.Message(InstallMessage.Error, record);

            return ActionResult.Success;
        }

        private static bool FindCompatibleDotNetCoreVersion(string input)
        {
            // Using not necessary here. See https://docs.microsoft.com/en-us/dotnet/api/system.io.stringreader?view=netframework-2.0
            StringReader reader = new StringReader(input);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(' ');
                if (parts.Length >= 2 && parts[0].Equals("Microsoft.NETCore.App"))
                {
                    if (parts[1].StartsWith("3.1"))
                        return true;
                }
            }

            return false;
        }
    }
}
