using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers
{
    /// <summary>
    /// Base class for a solver model, which holds all data necessary for a solver to run a simulation
    /// </summary>
    public class BaseSolverModel : IFrameworkBase
    {
        /// <summary>
        /// Delay that happens after each simulation step.
        /// Used when a gui is connected to the simulation.
        /// </summary>
        [JsonProperty]
        public int Delay { get; set; }
        /// <summary>
        /// Unique Guid that can be used to uniquely identify class instances
        /// </summary>
        [JsonProperty]
        public Guid Guid { get; set; } = Guid.NewGuid();
        /// <summary>
        /// A solver only has information about the connections between components. 
        /// Therefore it needs a reference to the simulation environment in order to run the simulation.
        /// </summary>
        [JsonProperty]
        public Guid EnvironmentGuid { get; set; }

        /// <summary>
        /// Current time in the simulation
        /// </summary>
        [JsonProperty]
        public long SimulationTime { get; set; }

        /// <summary>
        /// Is the current simulation finished.
        /// Once this is set from a component, the simulation will stop in the next iteration.
        /// </summary>
        [JsonProperty]
        public bool IsFinished { get; set; }

        /// <summary>
        /// Contains results of the simulation
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> Results { get; set; }

        public BaseSolverModel()
        {
            Results = new Dictionary<string, string>();
            Delay = 0;
            IsFinished = false;
        }


        public BaseSolverModel(Guid environmentGuid)
        {
            Results = new Dictionary<string, string>();
            Delay = 0;
            IsFinished = false;
            EnvironmentGuid = environmentGuid;
        }
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
