using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Discrete;

/// <summary>
/// Event based solver.
/// Each component can add events at a specific time for a specific component.
/// </summary>
public class DiscreteSolver : SolverBase, IDisposable
{
    /// <summary>
    /// Represents all additional information that is necessary for the discrete solver
    /// </summary>
    [JsonProperty("discreteSolver")]
    private DiscreteSolverModel _discreteSolverModel;

    [JsonIgnore]
    public DiscreteSolverModel DiscreteSolverModel => _discreteSolverModel;

    /// <summary>
    /// Better access to the simulation time during the simulation
    /// The real value is stored in the BaseModelMode
    /// </summary>
    [JsonIgnore]
    public long SimulationTime => BaseModel.SimulationTime;


    /// <summary>
    /// Represents all data that is necessary to run one event based simulation.
    /// </summary>
    [JsonProperty("baseModel")]
    public sealed override BaseSolverModel BaseModel { get; set; }

    [JsonConstructor]
    public DiscreteSolver(BaseSolverModel baseModel, DiscreteSolverModel discreteSolver)
    {
        this.BaseModel = baseModel;
        this._discreteSolverModel = discreteSolver;
        Guid = Guid.NewGuid();
        ComponentRegister.AddSolver(Guid, this);
    }


    /// <summary>
    /// Default constructor that should be used. 
    /// A environment reference is necessary for the solver, as the environment holds the simulation components.
    /// </summary>
    public DiscreteSolver(SimulationEnvironment e) : this(e.Guid) { }


    /// <summary>
    /// Default constructor that should be used. 
    /// A environment reference is necessary for the solver, as the environment holds the simulation components.
    /// </summary>
    public DiscreteSolver(Guid e)
    {
        Guid = Guid.NewGuid();
        _discreteSolverModel = new DiscreteSolverModel();
        BaseModel = new BaseSolverModel(e);
        ComponentRegister.AddSolver(Guid, this);
    }

    /// <summary>
    /// Calculate until no more events are in the event list 
    /// or until the simulation has been finished by a component.
    /// </summary>
    public override void CalculateFinish()
    {
        if (SimulationEnvironment == null)
            return;
        SimulationEnvironment.LogEnvironmentInfo("Discrete solver: calculate finish");
        try
        {
            // Stopping condition:
            // A component sets the simulation to finished or
            // no events left
            while (!BaseModel.IsFinished && _discreteSolverModel.AnyEvent)
            {

                long nextTimeStamp = -1;
                if (_discreteSolverModel.EventList.Any())
                    nextTimeStamp = _discreteSolverModel.EventList.Keys.Min();

                if (nextTimeStamp > BaseModel.SimulationTime || nextTimeStamp == -1)
                {
                    SimulationEnvironment.LogEnvironmentInfo("Discrete solver, run after time events");
                    while (DiscreteSolverModel.AnyAfterTimeEvent(BaseModel.SimulationTime))
                    {
                        DiscreteEvent? nextAfterTimeEvent = GetNextAfterTimeEvent(BaseModel.SimulationTime);
                        if (nextAfterTimeEvent.HasValue)
                        {
                            SimulationBase? compAfterTime = SimulationEnvironment.GetComponentByName(nextAfterTimeEvent.Value.ComponentName);
                            compAfterTime.PostCalculation();

                            compAfterTime.LogVisualizationParameters(BaseModel.SimulationTime);
                            compAfterTime.ResetValueChanged();
                        }
                    }
                }
                if (_discreteSolverModel.EventList.Count == 0)
                    continue;


                DiscreteEvent? discreteEvent = GetNextEvent();

                if (!discreteEvent.HasValue)
                    break;
                if (discreteEvent.Value.TimeStamp > BaseModel.SimulationTime)
                {
                    SimulationEnvironment.LogEnvironmentInfo("Discrete solver, time: " + discreteEvent.Value.TimeStamp);
                    BaseModel.SimulationTime = discreteEvent.Value.TimeStamp;
                }

                SimulationBase? comp = SimulationEnvironment.GetComponentByName(discreteEvent.Value.ComponentName);

                if (comp == null)
                {
                    comp.LogError("CalculateFinish: does not exist in the environment");
                    continue;
                }
                comp?.DiscreteCalculation();
                comp?.LogVisualizationParameters(discreteEvent.Value.TimeStamp);
                comp?.ResetValueChanged();

                if (BaseModel.Delay > 0)
                    Thread.Sleep(BaseModel.Delay);
            }

            if (!BaseModel.IsFinished && !_discreteSolverModel.AnyEvent)
                BaseModel.IsFinished = true;

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
    /// Calculate to the specified <paramref name="maxTime"/> time stamp.
    /// </summary>
    /// <param name="maxTime"></param>
    public override void CalculateTo(long maxTime)
    {
        if (SimulationEnvironment == null)
            return;
        SimulationEnvironment.LogEnvironmentInfo("Discrete solver: calculate to " + maxTime);

        try
        {
            // Stopping condition:
            // A component sets the simulation to finished or
            // no events left or
            // the simulation time is larger than the given max time
            while (!BaseModel.IsFinished && _discreteSolverModel.AnyEvent)
            {
                long nextTimeStamp = -1;
                if (_discreteSolverModel.EventList.Any())
                    nextTimeStamp = _discreteSolverModel.EventList.Keys.Min();

                if (nextTimeStamp > BaseModel.SimulationTime || nextTimeStamp == -1)
                {
                    SimulationEnvironment.LogEnvironmentInfo("Discrete solver, run after time events");
                    while (DiscreteSolverModel.AnyAfterTimeEvent(BaseModel.SimulationTime))
                    {
                        DiscreteEvent? nextAfterTimeEvent = GetNextAfterTimeEvent(BaseModel.SimulationTime);
                        if (nextAfterTimeEvent.HasValue)
                        {
                            SimulationBase? compAfterTime = SimulationEnvironment.GetComponentByName(nextAfterTimeEvent.Value.ComponentName);
                            compAfterTime.PostCalculation();

                            compAfterTime.LogVisualizationParameters(BaseModel.SimulationTime);
                            compAfterTime.ResetValueChanged();
                        }
                    }
                }

                if (_discreteSolverModel.EventList.Count == 0)
                    continue;

                DiscreteEvent? discreteEvent = GetNextEvent();
                if (!discreteEvent.HasValue)
                    break;

                if (discreteEvent.Value.TimeStamp > maxTime)
                {
                    BaseModel.IsFinished = true;
                    break;
                }

                if (discreteEvent.Value.TimeStamp > BaseModel.SimulationTime)
                {

                    SimulationEnvironment.LogEnvironmentInfo("Discrete solver, time: " + discreteEvent.Value.TimeStamp);
                    BaseModel.SimulationTime = discreteEvent.Value.TimeStamp;
                }

                SimulationBase? comp = SimulationEnvironment.GetComponentByName(discreteEvent.Value.ComponentName);
                if (comp == null)
                {
                    SimulationEnvironment.LogEnvironmentWarning("CalculateTo: " + discreteEvent.Value.ComponentName + "does not exist in the environment");
                    continue;
                }
                comp.DiscreteCalculation();

                comp.LogVisualizationParameters(discreteEvent.Value.TimeStamp);
                comp.ResetValueChanged();

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
    /// Add a event for each component at the current simulation time
    /// </summary>
    public void AddEventForAllComponents()
    {
        AddEvents(SimulationEnvironment.Model.SimulationObjects.Values);
    }

    /// <summary>
    /// Remove all events and post events for a specific simulation base
    /// </summary>
    /// <param name="simulationBase"></param>
    public void RemoveAllEventsForComponent(SimulationBase simulationBase)
    {
        DiscreteSolverModel.RemoveEventsForEasy2SimName(simulationBase.Easy2SimName);
        DiscreteSolverModel.RemoveAfterTimeEventsForEasy2SimName(simulationBase.Easy2SimName);
    }

    /// <summary>
    /// Add a event for each component at a specific simulation time
    /// </summary>
    public void AddEventForAllComponentsAtTime(long time)
    {
        AddEventsAtTime(SimulationEnvironment.Model.SimulationObjects.Values, time);
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

        SimulationEnvironment.LogEnvironmentInfo("Discrete solver: initialize");
        try
        {
            //Snapshot the simulation indexes
            //In case that a Component creates other objects during the initialization
            List<int> simulationIndexes = SimulationEnvironment.Model.SimulationObjects.Keys.ToList();

            foreach (int index in simulationIndexes)
            {
                SimulationBase simBase = SimulationEnvironment.Model.SimulationObjects[index];
                simBase.Initialize();
                simBase.LogVisualizationInitializeParameters(this.BaseModel.SimulationTime);
            }
        }
        catch (Exception ex)
        {
            SimulationEnvironment.LogEnvironmentFatal(ex.ToString());
        }
    }
    /// <summary>
    /// Returns the event with the lowest simulation index in the next simulation time.
    /// </summary>
    /// <returns></returns>
    public DiscreteEvent? GetNextAfterTimeEvent(long timeStamp)
    {
        try
        {
            if(DiscreteSolverModel.AfterTimeEventList.Count == 0)
                return null;
            if (DiscreteSolverModel.AfterTimeEventList.ContainsKey(timeStamp) && DiscreteSolverModel.AfterTimeEventList[timeStamp].Count == 0)
                return null;
            //next event
            DiscreteEvent discreteEvent = DiscreteSolverModel.AfterTimeEventList[timeStamp].First();
            DiscreteSolverModel.AfterTimeEventList[timeStamp].Remove(discreteEvent);
            if (DiscreteSolverModel.AfterTimeEventList[timeStamp].Count == 0)
                DiscreteSolverModel.AfterTimeEventList.Remove(timeStamp);

            return discreteEvent;
        }
        catch (Exception e)
        {
            SimulationEnvironment?.LogEnvironmentFatal(e.ToString());
        }
        //An error occurred, we return null
        return null;
    }


    /// <summary>
    /// Returns the event with the lowest simulation index in the next simulation time.
    /// </summary>
    /// <returns></returns>
    public DiscreteEvent? GetNextEvent()
    {
        try
        {
            if(DiscreteSolverModel.EventList.Count == 0)
                return null;
            //next event
            long minKey = DiscreteSolverModel.EventList.Keys.Min();
            DiscreteEvent discreteEvent = DiscreteSolverModel.EventList[minKey].First();

            DiscreteSolverModel.EventList[minKey].Remove(discreteEvent);
            if (DiscreteSolverModel.EventList[minKey].Count == 0)
                DiscreteSolverModel.EventList.Remove(minKey);

            DiscreteSolverModel.HistoricEvents.Add(discreteEvent);
            DiscreteSolverModel.HistoricEvents.RemoveWhere(x => x.TimeStamp < minKey); 

            return discreteEvent;
        }
        catch (Exception e)
        {
            SimulationEnvironment?.LogEnvironmentFatal(e.ToString());
        }
        return null;
    }

    /// <summary>
    /// Add an event for <paramref name="simulationBase"/> to our event list at the current simulation time.
    /// If our model allows loops, a component can be added multiple times at the same simulation time.
    /// Otherwise, it is ignored if it exists already at this simulation time.
    /// </summary>
    public void AddEvent(SimulationBase simulationBase)
    {
        if (SimulationEnvironment == null)
            return;

        AddEventAtTime(simulationBase, BaseModel.SimulationTime);
    }
    /// <summary>
    /// Add an even for a collection of simulation bases, e.g. when the output of a component changes a event for all inputs is generated
    /// </summary>
    /// <param name="connectedInputs"></param>
    public void AddEvents(IEnumerable<SimulationBase> connectedInputs)
    {
        foreach (SimulationBase simBase in connectedInputs)
            AddEvent(simBase);
    }

    /// <summary>
    /// Add an event to our event list at a specified simulation time.
    /// If our model allows loops, a component can be added multiple times at the same simulation time.
    /// Otherwise it is ignored if it exists already at this simulation time.
    /// </summary>
    public void AddEventAtTime(SimulationBase simulationBase, long simulationTime)
    {
        DiscreteSolverModel.AddEvent(simulationTime, simulationBase);
    }

    /// <summary>
    /// Add an even for a collection of simulation bases, e.g. when the output of a component changes a event for all inputs is generated
    /// </summary>
    public void AddEventsAtTime(IEnumerable<SimulationBase> simulationBases, long simulationTime)
    {
        foreach (SimulationBase simBase in simulationBases)
            AddEventAtTime(simBase, simulationTime);
    }


    /// <summary>
    /// Make sure the solver is removed from the component register once it is disposed
    /// </summary>
    public void Dispose()
    {
        ComponentRegister.RemoveSolver(Guid);
    }
    public void AddAfterTimeEvent(SimulationBase? simulationBase)
    {
        if (simulationBase == null || simulationBase.Easy2SimName == "")
        {
            SimulationEnvironment.LogEnvironmentFatal("After time event is null or has no component");
            throw new Exception("After time event is null or has no component");
        }
        DiscreteSolverModel.AddAfterTimeEvent(BaseModel.SimulationTime, simulationBase);
    }
}
