using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Common
{
    public class ProcessControl
    {
        private bool alive;
        public bool Alive => alive;
        #region Create process
        public bool CreateProcess(string file, string command, DataReceivedEventHandler outputHandler = default(DataReceivedEventHandler), DataReceivedEventHandler errorHandler = default(DataReceivedEventHandler))
        {
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
                    process.Exited += (s, e) => alive = false;
                    process.EnableRaisingEvents = true;
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    alive = true;
                    return true;
                }
                catch
                {
                    alive = false;
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
        public bool TerminateProcess(string file)
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
        private bool ExistProcess(int processId)
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
        private Process[] GetProcesses(string file)
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
    }
}
