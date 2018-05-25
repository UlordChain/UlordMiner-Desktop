using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Common
{
    public class Connection
    {
        private const int TIMEOUT = 5 * 1000;
        private IPEndPoint endPoint;
        public Connection(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }
        #region Received
        public T Received<T>(string request, long size, Func<string, T> parseResult, Action<Exception> action = null)
        {
            string str = string.Empty;
            byte[] bys = new byte[size];
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = TIMEOUT, ReceiveTimeout = TIMEOUT })
                {
                    socket.Connect(endPoint);
                    socket.Receive(bys);
                }

                str = Encoding.UTF8.GetString(bys).Trim('\0');
                return parseResult(str);
            }
            catch (Exception ex)
            {
                action?.Invoke(ex);
                return default(T);
            }
        }
        public T Received<T>(string request, Func<string, T> parseResult, Action<Exception> action = null)
        {
            return Received<T>(request, 4096, parseResult, action);
        }
        #endregion
    }
}
