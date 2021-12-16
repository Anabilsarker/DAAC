using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace DAAC
{
    public class StreamerServer
    {
        private static readonly StreamerServer _instance = new StreamerServer();
        public static StreamerServer Instance { get { return _instance; } }
        public ObservableCollection<EndPoint> endPoints = new ObservableCollection<EndPoint>();
        private Dispatcher dispatcher;
        TcpListener listener;
        bool loop = true;
        Thread thread;

        /// <summary>
        /// Sets the dispatcher object to StreamerServer.
        /// </summary>
        /// <param name="dispatch"></param>
        public void setDispatcher(Dispatcher dispatch)
        {
            dispatcher = dispatch;
        }

        /// <summary>
        /// Starts TCP Listener Enabling upto 10 Clients
        /// to be connected.
        /// </summary>
        public void StartServer()
        {
            loop = true;
            listener = new TcpListener(IPAddress.Any, 5769);
            listener.Start(10);
            thread = new Thread(BackThread);
            thread.Start();
        }

        /// <summary>
        /// Disconnects all the clients and stops TCP Server.
        /// </summary>
        public void StopServer()
        {
            loop = false;
            try
            {
                foreach (var endpoint in endPoints)
                {
                    endpoint.Client.Close();
                }
                endPoints.Clear();
                if (listener != null)
                {
                    listener.Stop();
                }
                thread?.Abort();
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Sets a new cilent to Endpoint when a client connects to server.
        /// </summary>
        void BackThread()
        {
            while (loop)
            {
                try
                {
                    var client = listener.AcceptTcpClient();
                    var stream = client.GetStream();
                    Thread.Sleep(200);
                    dispatcher.Invoke(()=> { endPoints.Add(new EndPoint { Client = client, Stream = stream, IP = client.Client.RemoteEndPoint.ToString().Split(':')[0], Status = true }); });
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}

