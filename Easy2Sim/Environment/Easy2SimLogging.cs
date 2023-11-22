using Serilog.Core;
using System.Net.Sockets;
using System.Text;

namespace Easy2Sim.Environment
{
    /// <summary>
    /// Class that can be used to send information over a udp port
    /// </summary>
    public class Easy2SimLogging
    {
        public Logger? Logger { get; set; }


        public void UdpInformation(string message, string ipAddress = "127.0.0.1", int port = 12345)
        {
            UdpClient udpClient = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, ipAddress, port);
        }
    }
}
