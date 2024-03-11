using Easy2Sim.Connect;
using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Discrete
{
    /// <summary>
    /// Event based solver.
    /// Each component can add events at a specific time for a specific component.
    /// </summary>
    public class DiscreteSolver : SolverBase, IDisposable
    {
        /// <summary>
        /// Represents all additional information that is necessary for the discrete solver
        /// </summary>
        [JsonProperty] 
        private DiscreteSolverModel _discreteSolverModel;

        [JsonIgnore] 
        public DiscreteSolverModel DiscreteSolverModel => _discreteSolverModel;

        /// <summary>
        /// Better access to the simulation time during the simulation
        /// The real value is stored in the BaseModel
        /// </summary>
        [JsonIgnore] 
        public long SimulationTime => BaseModel.SimulationTime;


        /// <summary>
        /// Represents all data that is necessary to run one event based simulation.
        /// </summary>
        [JsonProperty]
        public sealed override BaseSolverModel BaseModel { get; set; }

        /// <summary>
        /// Solver that is used for serialization.
        /// </summary>
        public DiscreteSolver()
        {
            _discreteSolverModel = new DiscreteSolverModel();
            BaseModel = new BaseSolverModel();
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
            _discreteSolverModel = new DiscreteSolverModel();
            BaseModel = new BaseSolverModel(e);
            ComponentRegister.AddComponent(Guid, this);
        }

        /// <summary>
        /// Calculate until no more events are in the event list 
        /// or until the simulation has been finished by a component.
        /// </summary>
        public override void CalculateFinish()
        {
            if (SimulationEnvironment == null)
                return;

            try
            {
                // Stopping condition:
                // A component sets the simulation to finished or
                // no events left
                while (!BaseModel.IsFinished && _discreteSolverModel.EventList.Any())
                {
                    DiscreteEvent? discreteEvent = GetNextEvent();

                    if (discreteEvent == null)
                        break;
                    if (discreteEvent.TimeStamp > BaseModel.SimulationTime)
                        BaseModel.SimulationTime = discreteEvent.TimeStamp;

                    SimulationBase? comp = SimulationEnvironment.GetComponentByName(discreteEvent.ComponentName);
                    comp?.DiscreteCalculation();

                    if (comp == null)
                    {
                        SimulationEnvironment.LogError("CalculateFinish: " + discreteEvent.ComponentName + "does not exist in the environment");
                        continue;
                    }
                    UpdateConnections(comp);
                    if (BaseModel.Delay > 0)
                        Thread.Sleep(BaseModel.Delay);
                }

                foreach (SimulationBase simulationBase in SimulationEnvironment.Model.SimulationObjects.Values)
                {
                    simulationBase.End();
                    UpdateConnections(simulationBase);
                }
            }
            catch (Exception ex)
            {
                SimulationEnvironment.LogError(ex.ToString());
            }
        }

        public override void CalculateTo(long maxTime)
        {
            if (SimulationEnvironment == null)
                return;

            try
            {
                // Stopping condition:
                // A component sets the simulation to finished or
                // no events left or
                // the simulation time is larger than the given max time
                while (!BaseModel.IsFinished && _discreteSolverModel.EventList.Any())
                {
                    DiscreteEvent? discreteEvent = GetNextEvent();
                    if (discreteEvent == null)
                        break;

                    if (discreteEvent.TimeStamp > maxTime)
                    {
                        BaseModel.IsFinished = true;
                        break;
                    }

                    if (discreteEvent.TimeStamp > BaseModel.SimulationTime)
                        BaseModel.SimulationTime = discreteEvent.TimeStamp;

                    SimulationBase? comp = SimulationEnvironment.GetComponentByName(discreteEvent.ComponentName);
                    if (comp == null)
                    {
                        SimulationEnvironment.LogError("CalculateTo: " + discreteEvent.ComponentName + "does not exist in the environment");
                        continue;
                    }
                    comp.DiscreteCalculation();

                    UpdateConnections(comp);
                    if (BaseModel.Delay > 0)
                        Thread.Sleep(BaseModel.Delay);
                }
            }
            catch (Exception ex)
            {
                SimulationEnvironment.LogError(ex.ToString());
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

            try
            {
                foreach (SimulationBase simulationComponent in SimulationEnvironment.Model.SimulationObjects.Values)
                {
                    simulationComponent.Initialize();
                    UpdateConnections(simulationComponent);
                }
            }
            catch (Exception ex)
            {
                SimulationEnvironment.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Update all connections where the source is the simulation base that has just been evaluated
        /// </summary>
        private void UpdateConnections(SimulationBase simulationComponent)
        {
            List<Connection>? connections = SimulationEnvironment?.Model.Connections;
            if (connections == null)
                return;
            foreach (Connection con in connections.Where(x => x.Source == simulationComponent))
            {
                //Has the value of the connection changed?
                if (!con.HasChanged)
                    continue;

                //We need a target to add a event
                if (con.Target == null)
                    continue;

                //Set the current value of the connection
                con.CurrentValue = con.SourceValue;
                //Update the value in the target
                con.SetValue();
            }
        }

        /// <summary>
        /// Returns the event with the lowest simulation index in the next simulation time.
        /// </summary>
        /// <returns></returns>
        public DiscreteEvent? GetNextEvent()
        {
            //next event
            DiscreteEvent? discreteEvent = null;
            //lowest found simulation index
            long simulationIndex = int.MaxValue;
            try
            {
                //If we do not have events in our list we need to return null
                if (DiscreteSolverModel.EventList.Count == 0)
                    return null;

                //Find the next simulation time => minimal time of all events
                long nextSimTime = DiscreteSolverModel.EventList.Select(x => x.TimeStamp).Min();

                //We only need information about events from the current simulation time
                DiscreteSolverModel.HistoricEvents.RemoveAll(x => x.TimeStamp < nextSimTime);
                //Iterate all events and find the event with the lowest simulation index
                foreach (DiscreteEvent e in DiscreteSolverModel.EventList.Where(x => x.TimeStamp == nextSimTime))
                {
                    SimulationBase? comp = SimulationEnvironment?.GetComponentByName(e.ComponentName);
                    if (comp == null)
                        continue;

                    if (e.TimeStampIndex < simulationIndex)
                    {
                        simulationIndex = e.TimeStampIndex;
                        discreteEvent = e;
                    }
                }

                //Remove the event from the event list and return it 
                if (discreteEvent != null)
                {
                    DiscreteSolverModel.EventList.Remove(discreteEvent);
                    DiscreteSolverModel.HistoricEvents.Add(discreteEvent);

                }

                return discreteEvent;

            }
            catch (Exception e)
            {
                SimulationEnvironment?.LogError(e.ToString());
            }
            //An error occurred, we return null
            return null;
        }

        /// <summary>
        /// Add an event to our event list at the current simulation time.
        /// If our model allows loops, a component can be added multiple times at the same simulation time.
        /// Otherwise it is ignored if it exists already at this simulation time.
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
        /// Serialize to json uses the default constructor.
        /// </summary>
        public override string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Make sure the solver is removed from the component register once it is disposed
        /// </summary>
        public void Dispose()
        {
            ComponentRegister.RemoveComponent(Guid);
        }

        public void AddEventForConnection(string connectionName, SimulationBase simBase, long time = -1)
        {
            List<Connection> outConnections = new List<Connection>();
            List<Connection> inConnections = new List<Connection>();
            foreach (Connection connection in SimulationEnvironment.Model.Connections)
            {
                if (connection.SourceName == connectionName && connection.Source == simBase)
                    outConnections.Add(connection);

                else if (connection.TargetName == connectionName && connection.Target == simBase)
                    inConnections.Add(connection);
            }

            if (time == -1)
            {
                foreach (var outConnection in outConnections)
                    AddEvent(outConnection.Target);

                foreach (var inConnection in inConnections)
                    AddEvent(inConnection.Source);
            }
            else
            {
                foreach (var outConnection in outConnections)
                    AddEventAtTime(outConnection.Target, time);

                foreach (var inConnection in inConnections)
                    AddEventAtTime(inConnection.Source, time);
            }
        }

    }
}
