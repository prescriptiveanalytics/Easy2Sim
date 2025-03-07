using Easy2Sim.Environment;
using Easy2Sim.Solvers.Discrete;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Dynamic;

/// <summary>
/// Default solver if each component should be called exactly once per simulation time increase.
/// </summary>
public class DynamicSolver : SolverBase, IDisposable
{
    /// <summary>
    /// The model holds the current state of the solver e.g. simulation time, time increase...
    /// We keep a variable of exact type.
    /// </summary>
    [JsonProperty("dynamicSolverModel")]
    private DynamicSolverModel _dynamicSolverModel;

    [JsonIgnore]
    public DynamicSolverModel DynamicSolverModel => _dynamicSolverModel;

    /// <summary>
    /// Better access to the simulation time during the simulation
    /// The real value is stored in the BaseModel
    /// </summary>
    [JsonIgnore] 
    public long SimulationTime => BaseModel.SimulationTime;

    /// <summary>
    /// Represents all data that is necessary to run one event based simulation.
    /// </summary>
    [JsonProperty("model")]
    public sealed override BaseSolverModel BaseModel { get; set; }

    [JsonConstructor]
    public DynamicSolver(DynamicSolverModel dynamicSolverModel, BaseSolverModel model)
    {
        Guid = Guid.NewGuid();
        _dynamicSolverModel = dynamicSolverModel;
        BaseModel = model;
        ComponentRegister.AddSolver(Guid, this);
    }


    /// <summary>
    /// Default constructor for the dynamic solver.
    /// A environment reference is necessary.
    /// </summary>
    public DynamicSolver(Guid environment)
    {
        Guid = Guid.NewGuid();
        _dynamicSolverModel = new DynamicSolverModel();
        BaseModel = new BaseSolverModel(environment);
        ComponentRegister.AddSolver(Guid, this);
    }
    /// <summary>
    /// Default constructor for the dynamic solver.
    /// A environment reference is necessary.
    /// </summary>
    public DynamicSolver(SimulationEnvironment environment) : this(environment.Guid) { }


    /// <summary>
    /// Calculate until a component has set the IsFinished in the Model.
    /// Each components DynamicCalculation is set once per SimulationTime.
    /// </summary>
    public override void CalculateFinish()
    {
        if (SimulationEnvironment == null)
            return;

        SimulationEnvironment.LogEnvironmentInfo("Dynamic solver: calculate finish");
        try
        {
            //Stop once a component finishes the simulation
            while (!BaseModel.IsFinished)
            {
                //Components are stored in a sorted list, this means we can just iterate all simulation objects
                //and respect the simulation index
                foreach (SimulationBase simulationComponent in SimulationEnvironment.Model.SimulationObjects.Values)
                {
                    simulationComponent.DynamicCalculation();
                }

                SimulationEnvironment.LogVisualizationParameters(BaseModel.SimulationTime);


                foreach (SimulationBase simulationBase in SimulationEnvironment.Model.SimulationObjects.Values)
                {
                    simulationBase.ResetValueChanged();
                }

                //We increase the time after all components have finished the current step
                //In case we increase it before, we e.g. can not simulate simulation time 0
                BaseModel.SimulationTime = BaseModel.SimulationTime + _dynamicSolverModel.SimulationStep;

                //Delay is helpful for gui programming, as the simulation without delays would be way to fast in most cases
                if (BaseModel.Delay > 0)
                    Thread.Sleep(BaseModel.Delay);
            }

            foreach (SimulationBase simulationBase in SimulationEnvironment.Model.SimulationObjects.Values)
            {
                simulationBase.End();
            }
        }
        catch (Exception ex)
        {
            SimulationEnvironment.Model.Easy2SimLogging.FrameworkDebuggingLogger.Error(ex.ToString());
        }
    }

    /// <summary>
    /// Calculate to a specific simulation time <paramref name="maxTime"/>.
    /// Each components DynamicCalculation is set once per SimulationTime.
    /// </summary>
    public override void CalculateTo(long maxTime)
    {

        if (SimulationEnvironment == null)
            return;

        SimulationEnvironment.LogEnvironmentInfo("Dynamic solver: calculate to " + maxTime);
        try
        {
            //Run until our simulation time is larger than the given limit
            for (long i = BaseModel.SimulationTime; i <= maxTime; i += _dynamicSolverModel.SimulationStep)
            {
                BaseModel.SimulationTime = i;

                foreach (SimulationBase simulationComponent in SimulationEnvironment.Model.SimulationObjects.Values)
                {
                    simulationComponent.DynamicCalculation();
                }

                SimulationEnvironment.LogVisualizationParameters(BaseModel.SimulationTime);

                foreach (SimulationBase simulationBase in SimulationEnvironment.Model.SimulationObjects.Values)
                {
                    simulationBase.ResetValueChanged();
                }
                if (BaseModel.Delay > 0)
                    Thread.Sleep(BaseModel.Delay);
            }
        }
        catch (Exception ex)
        {
            SimulationEnvironment.LogEnvironmentFatal(ex.ToString());
        }
    }

    /// <summary>
    /// Initialize can be called before the simulation starts.
    /// Typically computational expensive operations are done in the Initialize method.
    /// Each components "Initialize()" method is called once and than all connections are updated
    /// </summary>
    public override void Initialize()
    {
        if (SimulationEnvironment == null)
            return;

        SimulationEnvironment.LogEnvironmentInfo("Dynamic solver: initialize");
        try
        {
            foreach (SimulationBase simulationComponent in SimulationEnvironment.Model.SimulationObjects.Values)
            {
                simulationComponent.Initialize();
            }
            SimulationEnvironment.LogVisualizationInitializeParameters(BaseModel.SimulationTime);
        }
        catch (Exception ex)
        {
            SimulationEnvironment.LogEnvironmentFatal(ex.ToString());
        }
    }


    /// <summary>
    /// Make sure the solver is removed from the component register once it is disposed
    /// </summary>
    public void Dispose()
    {
        ComponentRegister.RemoveSolver(Guid);
    }
}
