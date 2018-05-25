using Common;
using Contract;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Net;

namespace Ulord
{
    [Export(typeof(IMiner))]
    public class Miner : IMiner
    {
        private ProcessControl processControl;
        private Connection connection;
        public Miner()
        {
            processControl = new ProcessControl();
        }
        public bool Alive(string file, string arg)
        {
            return processControl.Alive;
        }
        public bool Terminate(string file)
        {
            return processControl.TerminateProcess(file);
        }

        public IPerformance GetPerformance<T>(string file, string arg, IPEndPoint endPoint, string request, Func<string, T> parseResult, Action<IPerformance, T> setPerformance, out bool alive)
        {
            IPerformance performance = new Performance();
            if (alive = Alive(file, arg))
            {
                if (connection == null)
                {
                    connection = new Connection(endPoint);
                }
                T result = connection.Received<T>(request, parseResult);
                if (result != null)
                {
                    setPerformance(performance, result);
                }
            }
            return performance;
        }

        public bool Run(string file, string arg, DataReceivedEventHandler outputHandler = default(DataReceivedEventHandler), DataReceivedEventHandler errorHandler = default(DataReceivedEventHandler))
        {
            return processControl.CreateProcess(file, arg, outputHandler, errorHandler);
        }
    }
}
