using Easy2Sim.Connect;
using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Dynamic
{
    /// <summary>
    /// Default solver if each component should be called exactly once per simulation time increase.
    /// </summary>
    public class DynamicSolver : SolverBase, IDisposable
    {
        /// <summary>
        /// The model holds the current state of the solver e.g. simulation time, time increase...
        /// We keep a variable of exact type.
        /// </summary>
        [JsonProperty]
        private DynamicSolverModel _dynamicSolverModel;

        /// <summary>
        /// Better access to the simulation time during the simulation
        /// The real value is stored in the BaseModel
        /// </summary>
        [JsonIgnore] public long SimulationTime => BaseModel.SimulationTime;

        /// <summary>
        /// Represents all data that is necessary to run one event based simulation.
        /// </summary>
        public sealed override BaseSolverModel BaseModel { get; set; }

        /// <summary>
        /// Do not use this constructor, this constructor is used for serialization only.
        /// </summary>
        public DynamicSolver()
        {
            _dynamicSolverModel = new DynamicSolverModel();
            BaseModel = new BaseSolverModel();
        }


        /// <summary>
        /// Default constructor for the dynamic solver.
        /// A environment reference is necessary.
        /// </summary>
        public DynamicSolver(Guid environment)
        {
            _dynamicSolverModel = new DynamicSolverModel();
            BaseModel = new BaseSolverModel(environment);
            ComponentRegister.AddComponent(Guid, this);
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
                        UpdateConnections(simulationComponent);
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
                    UpdateConnections(simulationBase);
                }
            }
            catch (Exception ex)
            {
                SimulationEnvironment.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Calculate to a specific simulation time.
        /// Each components DynamicCalculation is set once per SimulationTime.
        /// </summary>
        public override void CalculateTo(long maxTime)
        {

            if (SimulationEnvironment == null)
                return;

            try
            {
                //Run until our simulation time is larger than the given limit
                for (long i = BaseModel.SimulationTime; i < maxTime; i += _dynamicSolverModel.SimulationStep)
                {
                    foreach (SimulationBase simulationComponent in SimulationEnvironment.Model.SimulationObjects.Values)
                    {
                        simulationComponent.DynamicCalculation();
                        UpdateConnections(simulationComponent);
                    }
                    //We increase the time after all components have finished the current step
                    //In case we increase it before, we e.g. can not simulate simulation time 0
                    BaseModel.SimulationTime = i;

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
        /// Update all connections where the source is the simulation base that has just been evaluated
        /// </summary>
        private void UpdateConnections(SimulationBase value)
        {
            List<Connection>? connections = SimulationEnvironment?.Model.Connections;
            if (connections == null)
                return;
            foreach (Connection con in connections.Where(x => x.Source == value))
            {
                con.CurrentValue = con.SourceValue;
                con.SetValue();
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
