using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Dynamic
{
    /// <summary>
    /// Holds all data for a dynamic simulation.
    /// </summary>
    public class DynamicSolverModel : IFrameworkBase
    {
        /// <summary>
        /// This value represents the simulation time increase after each simulation step.
        /// </summary>
        [JsonProperty]
        public int SimulationStep { get; set; }

        [JsonProperty]
        public Guid Guid {get;set;}

        private void SetDefaultValues()
        {
            SimulationStep = 1;
        }

        /// <summary>
        /// Constructor that is used for serialization.
        /// Should not be used, as a environment guid is necessary.
        /// </summary>
        public DynamicSolverModel()
        {
            SetDefaultValues();
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
