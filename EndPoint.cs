using System.Net.Sockets;

namespace DAAC
{
    /// <summary>
    /// The class to handle all the clients connected to the server.
    /// </summary>
    public class EndPoint
    {
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public string IP { get; set; }
        public bool Status { get; set; }
    }
}
