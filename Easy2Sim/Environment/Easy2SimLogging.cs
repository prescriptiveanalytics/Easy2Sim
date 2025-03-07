using Serilog.Core;
using System.Net.Sockets;
using System.Text;

namespace Easy2Sim.Environment;

/// <summary>
/// The GeneralLogging of the Easy2Sim Framework.
/// Set when creating a new Environment.
/// <code>
/// SimulationEnvironment environment = new();
/// GeneralLogging l1 = new LoggerConfiguration()
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
    /// Components can use this logger to log information
    /// </summary>
    private Logger? _generalLogger;
    public Logger? GeneralLogging
    {
        get => _generalLogger;
        set
        {
            _generalLogger = value;
            FrameworkDebuggingLogger?.Information("Set general logger in Environment");
        }
    }

    /// <summary>
    /// If this logger is set, a simulation sends visualization information over MQTT
    /// </summary>
    private Logger? _visualizationLogger;
    public Logger? VisualizationLogger
    {
        get => _visualizationLogger;
        set
        {
            _visualizationLogger = value;
            FrameworkDebuggingLogger?.Information("Set visualization logger in Environment");
        }
    }


    /// <summary>
    /// If this logger is set, the Framework can be debugged.
    /// Not implemented for all parts of the Framework yet.
    /// </summary>
    private Logger? _frameworkDebuggingLogger;
    public Logger? FrameworkDebuggingLogger
    {
        get => _frameworkDebuggingLogger;
        set
        {
            _frameworkDebuggingLogger = value;
            value?.Information("Set framework debugging logger");
        }
    }



    /// <summary>
    /// Method can be used to send information over a Udp client.
    /// Udp is not used at the moment.
    /// </summary>
    public void UdpInformation(string message, string ipAddress = "127.0.0.1", int port = 12345)
    {
        UdpClient udpClient = new UdpClient();
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, ipAddress, port);
    }
}
