using Easy2Sim.Environment;
using Newtonsoft.Json;
using System.Reflection;

namespace Easy2Sim.Connect
{
    /// <summary>
    /// Base class for parameters, input, and output attributes 
    /// </summary>
    public abstract class AttributeBase : Attribute, IFrameworkBase
    {
        
        /// <summary>
        /// Unique Guid that can be used to uniquely identify class instances
        /// </summary>
        [JsonProperty]
        public Guid Guid { get; internal set; } = Guid.NewGuid();
        

        [JsonProperty]
        public string ConnectionType { get; set; }
        /// <summary>
        /// Type of the Simulation base that holds the variable which should be connected
        /// </summary>
        [JsonProperty]
        public FieldInfo? FieldInfo { get; set; }
        /// <summary>
        /// Base object that holds the variable that should be connected
        /// </summary>
        [JsonProperty]
        public SimulationBase? SimulationBase { get; set; }

        /// <summary>
        /// Constructor that is used for serialization.
        /// </summary>
        public AttributeBase()
        {
            ConnectionType = string.Empty;
        }
        public AttributeBase(string connectionType)
        {
            ConnectionType = connectionType;
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
