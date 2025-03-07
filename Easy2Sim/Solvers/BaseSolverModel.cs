using Easy2Sim.Interfaces;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers;

/// <summary>
/// Base class for a solver model, which holds all data necessary for a solver to run a simulation
/// </summary>
public class BaseSolverModel : IFrameworkBase
{
    /// <summary>
    /// Delay that happens after each simulation step.
    /// Used when a gui is connected to the simulation.
    /// </summary>
    [JsonProperty]
    public int Delay { get; set; }
    /// <summary>
    /// Unique Guid that can be used to uniquely identify class instances
    /// </summary>
    [JsonIgnore]
    public Guid Guid { get; set; }
    /// <summary>
    /// A solver only has information about the connections between components. 
    /// Therefore it needs a reference to the simulation environment in order to run the simulation.
    /// </summary>
    [JsonProperty]
    public Guid EnvironmentGuid { get; set; }

    /// <summary>
    /// Current time in the simulation
    /// </summary>
    [JsonProperty]
    public long SimulationTime { get; set; }


    /// <summary>
    /// Configurations, how the current simulation time is converted to a date
    /// </summary>
    [JsonProperty]
    public DateTimeConfigurations DateTimeConfigurations { get; set; }

    [JsonIgnore]
    public DateTime CurrentSimulationDate => DateTimeConfigurations.GetCurrentSimulationDate(SimulationTime);


    /// <summary>
    /// Is the current simulation finished.
    /// Once this is set from a component, the simulation will stop in the next iteration.
    /// </summary>
    [JsonProperty]
    public bool IsFinished { get; set; }

    /// <summary>
    /// Contains results of the simulation
    /// </summary>
    [JsonProperty]
    public Dictionary<string, string> Results { get; set; }

    [JsonProperty]
    public string SimulationName { get; set; }

    /// <summary>
    /// Single fitness value for a simulation
    /// </summary>
    [JsonProperty]
    public Double Fitness { get; set; }

    public BaseSolverModel()
    {
        DateTimeConfigurations = new DateTimeConfigurations();
        Guid = Guid.NewGuid();
        Results = new Dictionary<string, string>();
        Delay = 0;
        IsFinished = false;
    }
    public BaseSolverModel(Guid environmentGuid)
    {
        DateTimeConfigurations = new DateTimeConfigurations();
        Guid = Guid.NewGuid();
        Results = new Dictionary<string, string>();
        Delay = 0;
        IsFinished = false;
        EnvironmentGuid = environmentGuid;
    }
}
