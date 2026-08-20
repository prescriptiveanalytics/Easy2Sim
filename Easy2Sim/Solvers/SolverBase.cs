using Easy2Sim.Environment;
using Easy2Sim.Interfaces;
using Easy2Sim.Solvers.Discrete;
using Easy2Sim.Solvers.Dynamic;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers;

/// <summary>
/// Base class for different solver in the simulation framework.
/// </summary>
public abstract class SolverBase : IFrameworkBase
{
    [JsonIgnore]
    public Guid Guid { get; set; }


    /// <summary>
    /// Contains
    /// </summary>
    [JsonProperty]
    public abstract BaseSolverModel BaseModel { get; set; }

    /// <summary>
    /// Simpler access to the BaseModel/SimulationTime
    /// </summary>
    [JsonIgnore]
    public long SimulationTime => BaseModel.SimulationTime;

    private SimulationEnvironment? _simulationEnvironment;
    [JsonIgnore]
    public SimulationEnvironment? SimulationEnvironment
    {
        get => _simulationEnvironment;
        set
        {
            _simulationEnvironment = value;
            if (BaseModel != null)
                BaseModel.SimulationEnvironment = value;
        }
    }

    /// <summary>
    /// If the solver is a discrete solver it is returned.
    /// Otherwise returns null.
    /// </summary>
    [JsonIgnore]
    public DiscreteSolver? AsDiscreteSolver
    {
        get
        {
            if (this is DiscreteSolver discreteSolver)
                return discreteSolver;
            return null;
        }
    }

    /// <summary>
    /// If the solver is a discrete solver it is returned.
    /// Otherwise returns null.
    /// </summary>
    [JsonIgnore]
    public DynamicSolver? AsDynamicSolver
    {
        get
        {
            if (this is DynamicSolver dynamicSolver)
                return dynamicSolver;
            return null;
        }
    }

    /// <summary>
    /// This method is called before a simulation is started and can be used to initialize components
    /// </summary>
    public abstract void Initialize();
    /// <summary>
    /// Calculate until a specified time <paramref name="maxTime"/> is reached.
    /// </summary>
    /// <param name="maxTime"></param>
    public abstract void CalculateTo(long maxTime);
    /// <summary>
    /// Calculate until a component finishes the simulation
    /// </summary>
    public abstract void CalculateFinish();
}
