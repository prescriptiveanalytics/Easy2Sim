using Newtonsoft.Json;
using Easy2Sim.Connect;

namespace Easy2Sim.Environment
{
    /// <summary>
    /// This class holds all data that is necessary for a simulation that is not related to a solver.
    /// The main data are the simulation components and connections.
    /// </summary>
    public class EnvironmentModel : IFrameworkBase
    {
        /// <summary>
        /// Unique Guid that can be used to uniquely identify class instances
        /// </summary>
        [JsonProperty]
        public Guid Guid { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Serilog logger that can be used to log to a wide variety of targets.
        /// This logger is not cloned when a SimulationEnvironment model
        /// </summary>
        [JsonIgnore]
        public Easy2SimLogging? Easy2SimLogging { get; set; }


        /// <summary>
        /// Counter that is used to keep track of the highest index 
        /// that is assigned to components in the simulation
        /// </summary>
        [JsonProperty]
        public int SimulationIndex {get; set; }

        /// <summary>
        /// List of all simulation object ordered by their simulation index
        /// </summary>
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public SortedList<int, SimulationBase> SimulationObjects { get; set; } 

        /// <summary>
        /// List of all connections in the simulation
        /// </summary>
        public List<Connection> Connections { get; set; } 


        public EnvironmentModel()
        {
            SimulationIndex = 0;
            Connections = new List<Connection>();
            SimulationObjects = new SortedList<int, SimulationBase>();
            Easy2SimLogging = new Easy2SimLogging();
        }

        /// <summary>
        /// Serialize to json uses the default constructor.
        /// </summary>
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
