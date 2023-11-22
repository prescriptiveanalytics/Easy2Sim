using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers
{
    /// <summary>
    /// Base class for different solver in the simulation framework.
    /// </summary>
    public abstract class SolverBase : IFrameworkBase
    {
        /// <summary>
        /// Unique id, of each individual component
        /// </summary>
        [JsonProperty]
        public Guid Guid { get; internal set; } = Guid.NewGuid();


        /// <summary>
        /// Contains
        /// </summary>
        [JsonIgnore]
        public abstract BaseSolverModel BaseModel { get; set; }

        [JsonIgnore]
        public SimulationEnvironment? SimulationEnvironment => ComponentRegister.GetEnvironment(BaseModel.EnvironmentGuid);


        /// <summary>
        /// This method is called before a simulation is started and can be used to initialize components
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// Calculate until a specified time is reached.
        /// </summary>
        /// <param name="maxTime"></param>
        public abstract void CalculateTo(long maxTime);
        /// <summary>
        /// Calculate until a component finishes the simulation
        /// </summary>
        public abstract void CalculateFinish();
        /// <summary>
        /// Serialize this component to json, needs to be implemented in each solver individually
        /// </summary>
        /// <returns></returns>
        public abstract string SerializeToJson();
    }
}
