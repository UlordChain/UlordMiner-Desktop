using Common;
using Contract;
using Miner_WPF.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace Miner_WPF.Commons
{
    public class Configuration
    {
        private static object asyncConfigRoot = new object();
        private static object asyncCounterRoot = new object();
        private static readonly string CONFIGPATH = "config.json";
        private static PerformanceCounter performanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        private static string file;
        private static IMiner miner;
        private static Config config;
        private static string request;
        private static IPEndPoint endPoint;
        private static Func<Config, string> format;

        private static Func<string, Result> parseResult;
        private static Action<IPerformance, Result> setPerformance;

        private static EventHandler exitHandler;
        private static DataReceivedEventHandler errorHandler;
        private static DataReceivedEventHandler outputHandler;

        private static string validateCode;

        internal static void Init(IMiner minerd, Action<string> action = null)
        {
            file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ulordrig.exe");

            action?.Invoke("正在获取最新挖矿软件信息...");
            AppInfo appInfo = FileHelper.GetAppInfo("https://testnet-pool.ulord.one/api/rig_stats");
            if (!File.Exists(file) || FileVersionInfo.GetVersionInfo(file).ProductVersion != appInfo.Version.Trim('\n') || FileHelper.ComputeFileMD5(file) != appInfo.MD5.ToUpper())
            {
                action?.Invoke("正在更新挖矿软件...");
                FileHelper.DownloadFile(appInfo.Address, file);
            }
            action?.Invoke("正在校验挖矿软件...");
            validateCode = appInfo.MD5.ToUpper();
            string fileMD5 = FileHelper.ComputeFileMD5(file);
            if (fileMD5 != validateCode)
            {
                throw new FileLoadException();
            }

            miner = minerd;
            Load();
            request = "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"getMinerParam\"}";
            endPoint = new IPEndPoint(IPAddress.Loopback, 8087);
            format = c => $"-o \"stratum+tcp://{c.Url}\" -u \"{(string.IsNullOrEmpty(c.Id) ? c.User : $"{c.User}.{c.Id}")}\" -p \"{c.Pass}\" -t {c.Thread} --max-cpu-usage 75";

            parseResult = s =>
            {
                s = s.Substring(s.IndexOf('H')).Replace("/ ", ":").Replace(" ", "").Replace("Hashrate", "{\"Hashrate\"").Replace("accept", ",\"Accept\"").Replace("total", ",\"Total\"") + "}";
                return JsonConvert.DeserializeObject<Result>(s);
            };
            setPerformance = (p, r) => p.SetPerformance(r.Total, r.Accept, r.Hashrate, 0);

            outputHandler += (s, e) => DebugMessageHandle(e.Data);
            errorHandler += (s, e) => DebugMessageHandle(e.Data);
            exitHandler += (s, e) => DebugMessageHandle($"Exited with {(s as Process).ExitCode}");
        }
        #region Load & Save
        public static void Load()
        {
            config = new Config();
            try
            {
                Config lastConfig = FileOperation.SolatedStorageRead<Config>(CONFIGPATH);
                if (lastConfig != null)
                {
                    lock (asyncConfigRoot)
                    {
                        config.SetConfig(lastConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugExceptionHandle(ex);
            }
        }
        public static void Save()
        {
            try
            {
                lock (asyncConfigRoot)
                {
                    FileOperation.SolatedStorageWrite<Config>(CONFIGPATH, config);
                }
            }
            catch (Exception ex)
            {
                DebugExceptionHandle(ex);
            }
        }
        #endregion
        #region Notify information
        public static void ShowErrMessage(string message)
        {
            MessageBox.Show(message, "系统消息", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void DebugExceptionHandle(Exception ex)
        {
            DebugMessageHandle($"Exception: {ex}");
        }
        public static void DebugMessageHandle(string message)
        {
            Debug.WriteLine($"{DateTime.Now}: {message}");
        }
        #endregion
        #region Get perfmon & IsRun & Run & Terminate
        public static IPerformance GetPerfmon(out bool isRun)
        {
            return miner.GetPerformance(file, format(config), endPoint, request, parseResult, setPerformance, out isRun);
        }
        public static bool IsRun()
        {
            return miner.Alive(file, format(config));
        }
        public static bool TryRun(Config newConfig)
        {
            try
            {
                if (FileHelper.ComputeFileMD5(file) != validateCode)
                {
                    throw new FileLoadException();
                }
            }
            catch
            {
                TryTerminate();
                return false;
            }
            if (miner.Run(file, format(newConfig), outputHandler, errorHandler, exitHandler))
            {
                lock (asyncConfigRoot)
                {
                    config.SetConfig(newConfig);
                }
                return true;
            }
            return false;
        }
        public static bool TryTerminate()
        {
            if (miner.Terminate(file))
            {
                return true;
            }
            return false;
        }
        #endregion
        #region Boot start
        public static bool IsBootStart()
        {
            return Process.GetCurrentProcess().MainModule.FileName.Equals(Win32Native.GetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Run", "UlordMiner"));
        }
        public static bool BootStart(bool power, Action<bool> action = null)
        {
            try
            {
                if (power)
                {
                    Win32Native.SetRegistryKey(@"Software\Microsoft\Windows\CurrentVersion\Run", new RegistryKeyValue("UlordMiner", Process.GetCurrentProcess().MainModule.FileName));
                }
                else
                {
                    Win32Native.DeleteRegistryKey(@"Software\Microsoft\Windows\CurrentVersion\Run", "UlordMiner");
                }
                return true;
            }
            catch
            {
                action?.Invoke(power);
                return false;
            }
        }
        #endregion
        #region Automatic & Idle & CPU usage
        public static void SetAutomatic(bool automatic)
        {
            lock (asyncConfigRoot)
            {
                config.Automatic = automatic;
            }
        }
        public static bool GetAutomatic()
        {
            lock (asyncConfigRoot)
            {
                return config.Automatic;
            }
        }
        public static bool IsIdle()
        {
            return Win32Native.GetIdleTime() >= 2 * 60 * 1000;
        }
        public static Config GetConfig()
        {
            return config;
        }
        public static float GetCPUUsage(Action<Exception> action = null)
        {
            try
            {
                lock (asyncCounterRoot)
                {
                    return performanceCounter.NextValue();
                }
            }
            catch (Exception ex)
            {
                action?.Invoke(ex);
                return default(float);
            }
        }
        #endregion
    }
}
