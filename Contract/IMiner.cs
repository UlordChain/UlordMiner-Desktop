using System;
using System.Diagnostics;
using System.Net;

namespace Contract
{
    public interface IMiner
    {
        bool Run(string file, string arg, DataReceivedEventHandler outputHandler = default(DataReceivedEventHandler), DataReceivedEventHandler errorHandler = default(DataReceivedEventHandler));
        bool Alive(string file, string arg);
        bool Terminate(string file);
        IPerformance GetPerformance<T>(string file, string arg, IPEndPoint endPoint, string request, Func<string, T> parseResult, Action<IPerformance, T> setPerformance, out bool alive);
    }
}
