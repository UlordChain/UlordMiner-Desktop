using Common;
using Contract;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace Test
{
    class Program
    {
        public delegate bool HandlerRoutine(int ctrlType);
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handlerRoutine, bool Add);

        [Import]
        public IMiner Miner { set; get; }
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Inject();
            program.Test();
        }

        public void Inject()
        {
            Console.WriteLine("Load component...");
            Console.ReadLine();
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));
            CompositionContainer compositionContainer = new CompositionContainer(directoryCatalog);
            try
            {
                compositionContainer.ComposeParts(this);
            }
            catch (ChangeRejectedException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            Console.WriteLine("Load component successed!");
            Console.ReadLine();
        }

        private void Test()
        {
            #region Initialize
            Console.WriteLine("Init component...");
            Console.ReadLine();
            string configFile = "config.json";
            string request = "Results";
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ulordrig.exe");
            Func<Config, string> format = c => $"-a \"cryptonight\" -o \"stratum+tcp://{c.Url}\" -u \"{(string.IsNullOrEmpty(c.Id) ? c.User : $"{c.User}.{c.Id}")}\" -p \"{c.Pass}\" -t {c.Thread}  --max-cpu-usage {c.CpuUsage}"/*+" -q"*/;
            #region Load config
            Console.WriteLine("Load last config...");
            Console.ReadLine();
            Config config = new Config();
            Config lastConfig = LoadConfig(configFile);
            if (lastConfig != null)
            {
                config.Url = lastConfig.Url;
                config.Pass = lastConfig.Pass;
                config.User = lastConfig.User;
                config.Id = lastConfig.Id;
                config.Thread = lastConfig.Thread;
                config.CpuUsage = lastConfig.CpuUsage;
                config.Automatic = lastConfig.Automatic;
                Console.WriteLine($"Last config: {lastConfig.Url}");
            }
            else
            {
                config.Url = "18.221.153.180:7200";
                config.Pass = "x";
                config.User = "uToXvL3zuQJk1rkw19nGoXPFEmRrmwnREx";
                config.Id = string.Empty;
                config.Thread = 2;
                config.CpuUsage = 50;
                config.Automatic = false;
            }
            Console.WriteLine("Load last config successed!");
            Console.ReadLine();
            #endregion
            #region Performance
            Func<string, Result> fr = s =>
            {
                s = s.Substring(s.IndexOf('H')).Replace("/ ", ":").Replace(" ", "").Replace("Hashrate", "{\"Hashrate\"").Replace("accept", ",\"Accept\"").Replace("total", ",\"Total\"") + "}";
                return JsonConvert.DeserializeObject<Result>(s);
            };
            Action<IPerformance, Result> rp = (p, r) => p.SetPerformance(r.Total, r.Accept, r.Hashrate, 0);
            #endregion
            DataReceivedEventHandler dataReceivedEventHandler = (s, e) => Debug.WriteLine(e.Data);
            Console.WriteLine("Init component successed!");
            Console.ReadLine();
            #endregion

            #region Function test
            Console.WriteLine("Start mining...");
            Console.ReadLine();
            bool isRun = Miner.Run(file, format(config), dataReceivedEventHandler, dataReceivedEventHandler);
            bool exit = false;
            HandlerRoutine handlerRoutine = (i) =>
            {
                switch ((dwCtrlType)i)
                {
                    case dwCtrlType.CTRL_C_EVENT:
                        Console.WriteLine("Terminate mining...");
                        isRun = !Miner.Terminate(file);
                        Console.WriteLine($"Terminate mining {(!isRun ? "successed" : "Failed")}!");

                        Console.WriteLine("Get mining status...");
                        isRun = Miner.Alive(file, format(config));
                        Console.WriteLine($"Mining status: {(isRun ? "Runing" : "Stopped")}");

                        Console.WriteLine("Save config...");
                        SaveConfig(configFile, config);
                        Console.WriteLine("Save config successed!");
                        exit = true;
                        return true;
                    case dwCtrlType.CTRL_BREAK_EVENT:
                    case dwCtrlType.CTRL_CLOSE_EVENT:
                    case dwCtrlType.CTRL_LOGOFF_EVENT:
                    case dwCtrlType.CTRL_SHUTDOWN_EVENT:
                    default:
                        return false;
                }
            };
            Console.WriteLine($"Start mining {(isRun ? "successed" : "Failed")}!");
            bool setSuccess = SetConsoleCtrlHandler(handlerRoutine, true);
            Console.WriteLine($"Inject Crtl+C event handler {(setSuccess ? "successed" : "failed")}");
            Console.ReadLine();

            Console.WriteLine("Get perfmon...");
            Console.ReadLine();
            while (true)
            {
                if (exit)
                {
                    break;
                }
                IPerformance perfmon = Miner.GetPerformance(file, format(config), new IPEndPoint(IPAddress.Loopback, 8087), request, fr, rp, out isRun);
                Console.WriteLine($"Performance: {JsonConvert.SerializeObject(perfmon)}");
                Thread.Sleep(1000);
            }
            #endregion

            Console.ReadLine();
        }
        static void SaveConfig(string file, Config config, StorgaeType strorageType = StorgaeType.SolatedStorage)
        {
            if (strorageType == StorgaeType.LocalStorage)
            {
                FileOperation.LocalWrite<Config>(file, config);
            }
            else
            {
                string isolatedFile = $"Configuration/Mining/{file}";
                FileOperation.SolatedStorageWrite<Config>(isolatedFile, config);
            }
        }
        static Config LoadConfig(string file, StorgaeType strorageType = StorgaeType.SolatedStorage)
        {
            if (strorageType == StorgaeType.LocalStorage)
            {
                return FileOperation.LocalRead<Config>(file);
            }
            else
            {
                string isolatedFile = $"Configuration/Mining/{file}";
                return FileOperation.SolatedStorageRead<Config>(isolatedFile);
            }
        }
    }
    enum StorgaeType
    {
        LocalStorage,
        SolatedStorage
    }
    enum dwCtrlType
    {
        CTRL_C_EVENT = 0,
        //A CTRL+C signal was received, either from keyboard input or from a signal generated by the GenerateConsoleCtrlEvent function.
        CTRL_BREAK_EVENT = 1,
        //A CTRL+BREAK signal was received, either from keyboard input or from a signal generated by GenerateConsoleCtrlEvent.
        CTRL_CLOSE_EVENT = 2,
        //A signal that the system sends to all processes attached to a console when the user closes the console (either by clicking Close on the console window's window menu, or by clicking the End Task button command from Task Manager).
        CTRL_LOGOFF_EVENT = 5,
        //A signal that the system sends to all console processes when a user is logging off. This signal does not indicate which user is logging off, so no assumptions can be made.
        CTRL_SHUTDOWN_EVENT = 6
        //A signal that the system sends when the system is shutting down. Interactive applications are not present by the time the system sends this signal, therefore it can be received only be services in this situation. Services also have their own notification mechanism for shutdown events. For more information, see Handler.
    }
}
