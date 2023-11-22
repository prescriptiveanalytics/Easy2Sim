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
        [JsonProperty] private DiscreteSolverModel _discreteSolverModel;

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
                while (!BaseModel.IsFinished && _discreteSolverModel.EventList.Any() && BaseModel.SimulationTime <= maxTime)
                {
                    DiscreteEvent? discreteEvent = GetNextEvent();
                    if (discreteEvent == null)
                        break;

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
                AddEvent(con.Target);
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
            int simulationIndex = int.MaxValue;
            try
            {
                //If we do not hav events in our list we need to return null
                if (_discreteSolverModel.EventList.Count == 0)
                    return null;

                //Find the next simulation time => minimal time of all events
                long nextSimTime = _discreteSolverModel.EventList.Select(x => x.TimeStamp).Min();

                //Iterate all events and find the event with the lowest simulation index
                foreach (DiscreteEvent e in _discreteSolverModel.EventList.Where(x => x.TimeStamp == nextSimTime))
                {
                    SimulationBase? comp = SimulationEnvironment?.GetComponentByName(e.ComponentName);
                    if (comp == null)
                        continue;

                    if (comp.Index < simulationIndex)
                    {
                        simulationIndex = comp.Index;
                        discreteEvent = e;
                    }
                }

                //Remove the event from the event list and return it 
                if (discreteEvent != null)
                    _discreteSolverModel.EventList.Remove(discreteEvent);
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

            AddEventToTime(simulationBase, BaseModel.SimulationTime);
        }

        /// <summary>
        /// Add an event to our event list at a specified simulation time.
        /// If our model allows loops, a component can be added multiple times at the same simulation time.
        /// Otherwise it is ignored if it exists already at this simulation time.
        /// </summary>
        public void AddEventAtTime(SimulationBase simulationBase, long simulationTime)
        {
            if (SimulationEnvironment == null)
                return;

            AddEventToTime(simulationBase, simulationTime);
        }

        private void AddEventToTime(SimulationBase simulationBase, long simulationTime)
        {
            if (_discreteSolverModel.AllowLoops)
            {
                //If no event has been added at this simulation time yet, add a empty list
                if (!_discreteSolverModel.ComponentsAtSimulationTime.ContainsKey(simulationTime))
                    _discreteSolverModel.ComponentsAtSimulationTime[simulationTime] = new List<string>();

                //We allow loops, so we only add the name to keep track of endless loops
                _discreteSolverModel.ComponentsAtSimulationTime[simulationTime].Add(simulationBase.Name);

                //Add the event
                _discreteSolverModel.EventList.Add(new DiscreteEvent(simulationTime, simulationBase.Name));
            }
            else
            {
                //If no event has been added at this simulation time yet, add a empty list
                if (!_discreteSolverModel.ComponentsAtSimulationTime.ContainsKey(simulationTime))
                    _discreteSolverModel.ComponentsAtSimulationTime[simulationTime] = new List<string>();

                //Only allow each component once at each time step
                if (!_discreteSolverModel.ComponentsAtSimulationTime[simulationTime].Contains(simulationBase.Name))
                {
                    _discreteSolverModel.ComponentsAtSimulationTime[simulationTime].Add(simulationBase.Name);
                    _discreteSolverModel.EventList.Add(new DiscreteEvent(simulationTime, simulationBase.Name));
                }
            }
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
    }
}
