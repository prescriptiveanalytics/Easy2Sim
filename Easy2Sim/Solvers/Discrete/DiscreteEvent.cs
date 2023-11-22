using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Discrete
{
    /// <summary>
    /// One event in the discrete simulation.
    /// </summary>
    public class DiscreteEvent : IFrameworkBase
    {
        /// <summary>
        /// Unique Guid that can be used to identify class instances
        /// </summary>
        [JsonProperty]
        public Guid Guid { get; internal set; } = Guid.NewGuid();

        /// <summary>
        /// Timestamp at which the event should be handled
        /// </summary>
        [JsonProperty]
        public long TimeStamp;

        /// <summary>
        /// Name of the component which the event should be simulated at the specified time
        /// </summary>
        [JsonProperty]
        public string ComponentName { get; set; }



        /// <summary>
        /// Constructor that is used for serialization.
        /// Should not be used, as a environment guid is necessary.
        /// </summary>
        public DiscreteEvent()
        {
            ComponentName = "";
            TimeStamp = 0;
        }

        /// <summary>
        /// Default constructor that should be used. 
        /// </summary>
        public DiscreteEvent(long timeStamp, string componentName)
        {
            TimeStamp = timeStamp;
            ComponentName = componentName;
        }

        public override string ToString()
        {
            return "Event for " + ComponentName + " at " + TimeStamp;
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
