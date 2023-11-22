using Easy2Sim.Connect;
using Easy2Sim.Solvers;
using Newtonsoft.Json;
using System.Reflection;

namespace Easy2Sim.Environment
{
    /// <summary>
    /// Base class for simulation components.
    /// </summary>
    public abstract class SimulationBase : IFrameworkBase
    {
        [JsonProperty]
        public Guid Guid { get; internal set; } = Guid.NewGuid();

        [JsonProperty]
        private int _simulationIndex;

        /// <summary>
        /// Keep a reference to the current version of the Environment
        /// </summary>
        [JsonProperty]
        public Guid SimulationEnvironmentGuid { get; set; }

        /// <summary>
        /// Keep a reference to the current version of the Solver
        /// </summary>
        [JsonProperty]
        public Guid SolverGuid { get; set; }
        /// <summary>
        /// Unique name that is generated for each component
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }
        /// <summary>
        /// Index in the simulation, this value is used 
        /// </summary>
        [JsonProperty]
        public int Index => _simulationIndex;

        [JsonIgnore]
        private SimulationEnvironment? _simulationEnvironment;

        [JsonIgnore]
        public SimulationEnvironment? SimulationEnvironment
        {
            get
            {
                _simulationEnvironment = ComponentRegister.GetEnvironment(SimulationEnvironmentGuid);

                return _simulationEnvironment;
            }
        }

        [JsonIgnore]
        private SolverBase? _solverBase;


        [JsonIgnore]
        public SolverBase? Solver
        {
            get
            {
                _solverBase = ComponentRegister.GetSolver(SolverGuid);

                return _solverBase;
            }
        }



        private void SetSimulationIndex()
        {
            if (SimulationEnvironment == null) return;
            _simulationIndex = SimulationEnvironment.Model.SimulationIndex;
            SimulationEnvironment.Model.SimulationIndex = _simulationIndex + 1;
        }

        /// <summary>
        /// Do not use this constructor, this is only used for serialization and deserialization
        /// </summary>
        protected SimulationBase()
        {
            Name = "";
            _simulationIndex = -1;

        }

        /// <summary>
        /// If a environment or environment guid is given the component is added to the environments components.
        /// The solver is necessary for the simulation.
        /// </summary>
        protected SimulationBase(SimulationEnvironment environment, SolverBase solver) : this(environment.Guid, solver.Guid) { }

        /// <summary>
        /// If a environment or environment guid is given the component is added to the environments components.
        /// The solver is necessary for the simulation.
        /// </summary>
        protected SimulationBase(Guid environmentGuid, Guid solverGuid) : this(environmentGuid)
        {
            SolverGuid = solverGuid;
        }

        /// <summary>
        /// If a environment or environment guid is given the component is added to the environments components
        /// </summary>
        protected SimulationBase(SimulationEnvironment environment) : this(environment.Guid) { }

        /// <summary>
        /// If a environment or environment guid is given the component is added to the environments components
        /// </summary>
        protected SimulationBase(Guid environmentGuid)
        {
            SimulationEnvironmentGuid = environmentGuid;
            SetSimulationIndex();
            Guid = Guid.NewGuid();
            Type t = GetType();
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo field in fields)
            {
                // Check if the property has the MyCustomAttribute applied
                if (field.GetCustomAttribute(typeof(AttributeBase)) is AttributeBase attributeBase)
                {
                    attributeBase.FieldInfo = field;
                    attributeBase.SimulationBase = this;
                }
            }

            Name = t.Name + Index;
            SimulationEnvironment?.AddComponent(this);
        }


        /// <summary>
        /// Initialize is called once before the simulation starts.
        /// Typical cpu expensive actions are done here: e.g. file access to initialize a component
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// This method is called before the first iteration of the simulation. 
        /// </summary>
        public virtual void StartSimulation() { }

        /// <summary>
        /// This method is called every time a event for this component is executed
        /// </summary>
        public virtual void DiscreteCalculation() { }

        /// <summary>
        /// Called at every iteration of a dynamic calculation
        /// </summary>
        public virtual void DynamicCalculation() { }

        /// <summary>
        /// Serialize to json uses the default constructor.
        /// </summary>
        public abstract string SerializeToJson();
    }
}