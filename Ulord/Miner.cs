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
        private Connection connection;
        public bool Alive(string file, string arg)
        {
            return ProcessControl.ExistProcess(file, arg);
        }
        public bool Terminate(string file)
        {
            return ProcessControl.TerminateProcess(file);
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
                T result = connection.Received<T>(request, true, parseResult);
                if (result != null)
                {
                    setPerformance(performance, result);
                }
            }
            return performance;
        }

        public bool Run(string file, string arg, DataReceivedEventHandler outputHandler = default(DataReceivedEventHandler), DataReceivedEventHandler errorHandler = default(DataReceivedEventHandler), EventHandler exitHandler = default(EventHandler))
        {
            return ProcessControl.CreateProcess(file, arg, outputHandler, errorHandler, exitHandler);
        }
    }
}
