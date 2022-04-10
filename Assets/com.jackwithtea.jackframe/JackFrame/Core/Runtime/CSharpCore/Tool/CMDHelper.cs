using System;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace JackFrame {

    public static class CMDHelper {

        public static Process RunCMD(string command, string args, string workDir = null) {

            ProcessStartInfo startInfo = new ProcessStartInfo(command);
            startInfo.Arguments = args;
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true; 
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = UTF8Encoding.UTF8;
            startInfo.StandardErrorEncoding = UTF8Encoding.UTF8;
            if (!string.IsNullOrEmpty(workDir)) {
                startInfo.WorkingDirectory = workDir;
            }

            Process process = Process.Start(startInfo);
            return process;
            
        }

        public static Process RunProtoc(string inDir, string outDir) {

            var process = RunCMD("protoc", "-I=" + inDir + " --csharp_out=" + outDir + " " + inDir + "/*.proto");
            string msg = process.StandardOutput.ReadToEnd();
            string err = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(msg)) {
                PLog.ForceLog(msg);
            }
            if (!string.IsNullOrEmpty(err)) {
                PLog.ForceLog(err);
            }

            return process;

        }

    }

}