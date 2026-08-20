using Easy2Sim.Connect;
using Newtonsoft.Json;
using Easy2Sim.Interfaces;

namespace Easy2Sim.Environment;

/// <summary>
/// This class holds all data that is necessary for a simulation that is not related to a solver.
/// The main data are the simulation components and connections.
/// </summary>
public class EnvironmentModel : IFrameworkBase, ICloneable<EnvironmentModel>
{
    /// <summary>
    /// Unique Guid that can be used to uniquely identify class instances
    /// </summary>
    [JsonProperty]
    public Guid Guid { get; set; }


    /// <summary>
    /// Serilog logger that can be used to log to a wide variety of targets.
    /// This logger is not cloned when a SimulationEnvironment model
    /// </summary>
    [JsonIgnore]
    public Easy2SimLogging? Easy2SimLogging { get; set; }


    /// <summary>
    /// Counter that is used to keep track of the highest index 
    /// that is assigned to components in the simulation
    /// </summary>
    [JsonProperty]
    public int SimulationIndex { get; set; }

    /// <summary>
    /// List of all simulation object ordered by their simulation index
    /// </summary>
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
    public SortedList<int, SimulationBase> SimulationObjects { get; set; }

    /// <summary>
    /// List of all connections in the simulation
    /// </summary>
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
    public List<IConnection> Connections { get; set; }

    [JsonProperty]
    public List<ComponentConnection> ComponentConnections { get; set; }

    [JsonConstructor]
    public EnvironmentModel()
    {
        Guid = Guid.NewGuid();
        ComponentConnections = new List<ComponentConnection>();
        SimulationIndex = 0;
        Connections = new List<IConnection>();
        SimulationObjects = new SortedList<int, SimulationBase>();
        Easy2SimLogging = new Easy2SimLogging();
    }

    public EnvironmentModel Clone()
    {
        EnvironmentModel result = new EnvironmentModel();
        result.Easy2SimLogging = Easy2SimLogging?.Clone();
        foreach (IConnection connection in Connections)
        {

            dynamic dynConnection = connection;
            IConnection? newConnection = null;

            try
            {
                newConnection = dynConnection.Clone();
            }
            catch (Exception)
            {
            }
            if (newConnection != null)
            {
                result.Connections.Add(newConnection);
                continue;
            }

            string json = connection.SerializeToJson();
            IConnection? clone = SimulationEnvironment.Deserialize<IConnection>(json);
            if (clone != null)
                result.Connections.Add(clone);
        }

        foreach (ComponentConnection componentConnection in ComponentConnections)
            result.ComponentConnections.Add(componentConnection.Clone());

        foreach (KeyValuePair<int, SimulationBase> pair in SimulationObjects)
        {
            dynamic dynSimulationBase = pair.Value;
            SimulationBase? tryClone = null;
            try
            {
                tryClone = dynSimulationBase.Clone();
            }
            catch (Exception)
            {
                Console.WriteLine("Environment-Model-Clone: Clone not implemented for " + pair.Value.GetType());
            }

            if (tryClone != null)
            {
                result.SimulationObjects.Add(pair.Key, tryClone);
                continue;
            }
            string json = (pair.Value as IFrameworkBase).SerializeToJson();
            SimulationBase? clone = SimulationEnvironment.Deserialize<SimulationBase>(json);
            if (clone != null)
                result.SimulationObjects.Add(pair.Key, clone);
        }

        result.SimulationIndex = SimulationIndex;

        return result;
    }
}
