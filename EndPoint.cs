using System.Net.Sockets;

namespace DAAC
{
    public class EndPoint
    {
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public string IP { get; set; }
        public bool Status { get; set; }
    }
}
