using System;
using System.Threading;

namespace Miner_WPF
{
    class Program
    {
        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        //[System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            // Only one instance
            bool createNew;
            Mutex mutex = new Mutex(true, "UlordMiner", out createNew);
            if (!createNew)
            {
                Commons.Configuration.ShowErrMessage("已经有一个客户端在您的计算机上运行！");
                Environment.Exit(0);
                return;
            }
            Miner_WPF.App app = new Miner_WPF.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
