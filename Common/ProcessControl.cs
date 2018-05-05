using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace Common
{
    public class ProcessControl
    {
        #region Create process
        public static bool CreateProcess(string file, string command, DataReceivedEventHandler outputHandler = default(DataReceivedEventHandler), DataReceivedEventHandler errorHandler = default(DataReceivedEventHandler), EventHandler exitHandler = default(EventHandler))
        {
            if (ExistProcess(file, command))
            {
                return true;
            }
            if (TerminateProcess(file))
            {
                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(file, command);
                    processStartInfo.CreateNoWindow = true;
                    processStartInfo.UseShellExecute = false;
                    processStartInfo.RedirectStandardOutput = true;
                    processStartInfo.RedirectStandardError = true;

                    Process process = Process.Start(processStartInfo);
                    process.OutputDataReceived += outputHandler;
                    process.ErrorDataReceived += errorHandler;
                    process.Exited += exitHandler;
                    process.EnableRaisingEvents = true;
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        } 
        #endregion
        #region Terminate process
        public static bool TerminateProcess(string file)
        {
            bool isSuccess = true;
            foreach (Process item in GetProcesses(file))
            {
                try
                {
                    item.Kill();
                }
                catch
                {
                    if (ExistProcess(item.Id))
                    {
                        isSuccess = false;
                    }
                }
            }
            return isSuccess;
        }
        private static bool ExistProcess(int processId)
        {
            try
            {
                return Process.GetProcessById(processId) != null;
            }
            catch
            {
                return false;
            }
        }
        private static Process[] GetProcesses(string file)
        {
            try
            {
                return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(file)).Where(p => p.MainModule.FileName == file).ToArray();
            }
            catch
            {
                return new Process[0];
            }
        }
        #endregion
        #region Exist process
        public static bool ExistProcess(string file, string command)
        {
            string commandLine = $"\"{file}\" {command}";
            try
            {
                using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ExecutablePath=\"{file.Replace("\\", "\\\\")}\""))
                {
                    using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
                    {
                        return managementObjectCollection.Count > 0 ? managementObjectCollection.Cast<ManagementObject>().Count(p => IsMatch(Convert.ToString(p["CommandLine"]), commandLine)) > 0 : false;
                    };
                }
            }
            catch
            {
            }
            return false;
        }
        private static bool IsMatch(string src, string target)
        {
            src = Regex.Replace(src, "\\s+", "").Replace("//", "\\");
            target = Regex.Replace(target, "\\s+", "").Replace("//", "\\");
            return src == target;
        }
        #endregion
    }
}
