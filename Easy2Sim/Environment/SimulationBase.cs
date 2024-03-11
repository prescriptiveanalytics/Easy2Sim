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
        /// Index in the simulation.
        /// In each time stamp in the simulation, components are called in order of the simulation index.
        /// </summary>
        [JsonProperty]
        public int Index => _simulationIndex;

        /// <summary>
        /// Set the simulation  <paramref name="index"/> of a component manually.
        /// Make sure that all indexes are correct!
        /// </summary>
        /// <param name="index">
        /// The index is typically automatically generated based on creation order of the components.
        /// This parameter allows to overwrite it.
        /// </param>
        public void SetIndexManually(int index)
        {
            _simulationIndex = index;
        }

        /// <summary>
        /// Set the simulation <paramref name="name"/> of a component manually.
        /// Make sure that all names are unique and correct!
        /// </summary>
        /// <param name="name">
        /// A unique name for each simulation component is generated automatically.
        /// This parameter allows to change it.  
        /// </param>
        public void SetNameManually(string name)
        {
            Name = name;
        }

        [JsonIgnore]
        private SimulationEnvironment? _simulationEnvironment;

        /// <summary>
        /// Simulation environment where the simulation component is registered.
        /// </summary>
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


        /// <summary>
        /// Solver where the component is registered.
        /// </summary>
        [JsonIgnore]
        public SolverBase? Solver
        {
            get
            {
                _solverBase = ComponentRegister.GetSolver(SolverGuid);

                return _solverBase;
            }
        }


        /// <summary>
        /// Automatically set the simulation index based on the current index in the environment.
        /// This method is called, when a new component is created.
        /// </summary>
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
        /// Returns all simulation bases for a given output parameter name
        /// </summary>
        /// <param name="output">Name of the output field or property</param>
        /// <returns></returns>
        public List<SimulationBase> GetConnectedInputComponents(string output)
        {
            if (SimulationEnvironment == null)
                return new List<SimulationBase>();

            List<SimulationBase> resultList = new List<SimulationBase>();
            List<SimulationBase> connectedComponents = SimulationEnvironment.Model.Connections
                .Where(x => x.Source == this)
                .Where(x => x.SourceName == output)
                .Select(x => x.Target).ToList();
            resultList.AddRange(connectedComponents);
            return resultList;
        }

        /// <summary>
        /// Returns all simulation bases for a given input parameter name
        /// </summary>
        /// <param name="input">Name of the input field or property</param>
        /// <returns></returns>
        public List<SimulationBase> GetConnectedOutputComponents(string input)
        {
            if (SimulationEnvironment == null)
                return new List<SimulationBase>();

            List<SimulationBase> resultList = new List<SimulationBase>();
            List<SimulationBase> connectedComponents = SimulationEnvironment.Model.Connections
                .Where(x => x.Target == this)
                .Where(x => x.TargetName == input)
                .Select(x => x.Source).ToList();
            resultList.AddRange(connectedComponents);
            return resultList;
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
        /// This method is called at the end of the simulation
        /// When calculate finish ends
        /// </summary>
        public virtual void End() { }
        /// <summary>
        /// Serialize to json uses the default constructor.
        /// Each component needs to be serializable.
        /// </summary>
        public abstract string SerializeToJson();

    }
}