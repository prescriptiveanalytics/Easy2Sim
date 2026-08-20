using Easy2Sim.Environment;
using Easy2Sim.Interfaces;
using Easy2Sim.Solvers.Discrete;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Dynamic;

/// <summary>
/// Default solver if each component should be called exactly once per simulation time increase.
/// </summary>
public class DynamicSolver : SolverBase, ICloneable<DynamicSolver>
{

    [JsonProperty]
    public DynamicSolverModel? DynamicSolverModel { get; set; }

    /// <summary>
    /// Better access to the simulation time during the simulation
    /// The real value is stored in the BaseModel
    /// </summary>
    [JsonIgnore]
    public new long SimulationTime => BaseModel.SimulationTime;

    /// <summary>
    /// Represents all data that is necessary to run one event based simulation.
    /// </summary>
    [JsonProperty]
    public sealed override BaseSolverModel BaseModel { get; set; }

    [JsonConstructor]
    public DynamicSolver()
    {
        Guid = Guid.NewGuid();
        DynamicSolverModel = null;
        BaseModel = new BaseSolverModel(null);
        SimulationEnvironment = null;
    }

    /// <summary>
    /// Default constructor for the dynamic solver.
    /// An environment reference is necessary.
    /// </summary>
    public DynamicSolver(SimulationEnvironment environment)
    {
        Guid = Guid.NewGuid();
        DynamicSolverModel = new DynamicSolverModel();
        BaseModel = new BaseSolverModel(environment);
        SimulationEnvironment = environment;
    }


    /// <summary>
    /// Calculate until a component has set the IsFinished in the Model.
    /// Each components DynamicCalculation is set once per SimulationTime.
    /// </summary>
    public override void CalculateFinish()
    {
        if (DynamicSolverModel == null)
            throw new Exception("Dynamic solver model is null, can not CaclulateFinish");
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
                BaseModel.SimulationTime = BaseModel.SimulationTime + DynamicSolverModel.SimulationStep;

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
            if (SimulationEnvironment.Model.Easy2SimLogging?.FrameworkDebuggingLogger != null)
                SimulationEnvironment.Model.Easy2SimLogging.FrameworkDebuggingLogger.Error(ex.ToString());
            else
            {
                throw new Exception(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Calculate to a specific simulation time <paramref name="maxTime"/>.
    /// Each components DynamicCalculation is set once per SimulationTime.
    /// </summary>
    public override void CalculateTo(long maxTime)
    {
        if (DynamicSolverModel == null)
            throw new Exception("Dynamic solver model is null, can not CalculateTo");

        if (SimulationEnvironment == null)
            throw new Exception("Simulation environment is null, can not CalculateTo"); ;

        SimulationEnvironment.LogEnvironmentInfo("Dynamic solver: calculate to " + maxTime);
        try
        {
            //Run until our simulation time is larger than the given limit
            for (long i = BaseModel.SimulationTime; i <= maxTime; i += DynamicSolverModel.SimulationStep)
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

    public DynamicSolver Clone()
    {
        DynamicSolver result = new DynamicSolver();
        result.BaseModel = BaseModel.Clone();
        result.DynamicSolverModel = DynamicSolverModel?.Clone();
        return result;
    }
}
