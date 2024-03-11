using Serilog.Core;
using System.Net.Sockets;
using System.Text;

namespace Easy2Sim.Environment
{
    /// <summary>
    /// The Logger of the Easy2Sim Framework.
    /// Set when creating a new Environment.
    /// <code>
    /// SimulationEnvironment environment = new();
    /// Logger l1 = new LoggerConfiguration()
    /// .MinimumLevel.Verbose()
    /// .WriteTo.Console()
    /// .CreateLogger();
    /// environment.SetLogConfiguration(l1);
    /// </code>
    /// 
    /// </summary>
    public class Easy2SimLogging
    {
        /// <summary>
        /// Serilog is used for logging.
        /// Can be used to log to any destination which serilog supports.
        /// Make sure to set the correct minimum logging level.
        /// </summary>
        public Logger? Logger { get; set; }

        /// <summary>
        /// Method can be used to send information over a Udp client.
        /// </summary>
        /// <param name="message">
        /// Message to send
        /// </param>
        /// <param name="ipAddress">
        /// Destination ip Address
        /// </param>
        /// <param name="port">
        /// Destination port where the message should be sent
        /// </param>
        public void UdpInformation(string message, string ipAddress = "127.0.0.1", int port = 12345)
        {
            UdpClient udpClient = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, ipAddress, port);
        }
    }
}
