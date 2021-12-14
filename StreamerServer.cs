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

        public void setDispatcher(Dispatcher dispatch)
        {
            dispatcher = dispatch;
        }

        public void StartServer()
        {
            loop = true;
            listener = new TcpListener(IPAddress.Any, 5769);
            listener.Start(10);
            thread = new Thread(BackThread);
            thread.Start();
        }
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

