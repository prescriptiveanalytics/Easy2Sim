using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Discrete
{
    /// <summary>
    /// Holds all data for a discrete simulation.
    /// </summary>
    public class DiscreteSolverModel : IFrameworkBase
    {
        /// <summary>
        /// Can the a component be added multiple times at the same simulation time
        /// </summary>
        [JsonProperty]
        public bool AllowLoops { get; set; }


        /// <summary>
        /// List of all current events
        /// </summary>
        [JsonProperty]
        public List<DiscreteEvent> EventList { get; set; }


        /// <summary>
        /// Keep track of all components that have been added to a event list at a specific time
        /// </summary>
        [JsonProperty]
        public Dictionary<long, List<string>> ComponentsAtSimulationTime { get; set; }

        [JsonProperty]
        public Guid Guid { get; set; }

        /// <summary>
        /// Constructor that is used for serialization.
        /// Should not be used, as a environment guid is necessary.
        /// </summary>
        public DiscreteSolverModel()
        {
            ComponentsAtSimulationTime = new Dictionary<long, List<string>>();
            EventList = new List<DiscreteEvent>();
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
