using Easy2Sim.Environment;
using Newtonsoft.Json;
using System.Reflection;

namespace Easy2Sim.Connect
{
    /// <summary>
    /// This class defines connections in the simulation framework
    /// </summary>
    public class Connection : IFrameworkBase
    {

        /// <summary>
        /// Unique Guid that can be used to uniquely identify class instances
        /// </summary>
        [JsonProperty]
        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Guid of the environment in which the connection is registered
        /// </summary>
        [JsonProperty]
        public Guid EnvironmentGuid { get; set; }


        /// <summary>
        /// Returns the current environment in which the connection is registered.
        /// Returns null if no environment can be found.
        /// </summary>
        [JsonIgnore]
        private SimulationEnvironment? SimulationEnvironment => ComponentRegister.GetEnvironment(EnvironmentGuid);


        /// <summary>
        /// Stores the current value of the connection.
        /// This is used to determine if the value has changed.
        /// </summary>
        [JsonProperty]
        internal object? CurrentValue { get; set; }

        #region Source information
        /// <summary>
        /// String representation of the source type.
        /// </summary>
        [JsonProperty]
        internal string SourceType { get; set; }
        /// <summary>
        /// Name of the source variable
        /// </summary>
        [JsonProperty]
        internal string SourceName { get; set; }
        /// <summary>
        /// Base object of the source 
        /// </summary>
        [JsonProperty]
        internal Guid SourceGuid { get; set; }


        /// <summary>
        /// Returns the simulation base object of the source registered in the environment of the connection.
        /// Is null if no environment can be found or if the guid of the source can not be found in the environment.
        /// </summary>
        [JsonIgnore]
        public SimulationBase? Source => SimulationEnvironment?.GetComponentByGuid(SourceGuid);

        /// <summary>
        /// Check if the source is a field. If it is a field return the FieldInfo.
        /// Otherwise return null;
        /// </summary>
        [JsonIgnore]
        internal FieldInfo? SourceField
        {
            get
            {
                FieldInfo? outputField = Source?.GetType().GetFields().FirstOrDefault(x => x.Name == SourceName);
                return outputField;
            }
        }
        /// <summary>
        /// Check if the source is a property. If it is a field return the PropertyInfo.
        /// Otherwise return null;
        /// </summary>
        [JsonIgnore]
        internal PropertyInfo? SourcePropertyInfo
        {
            get
            {
                PropertyInfo? sourceInfo = Source?.GetType().GetProperties().FirstOrDefault(x => x.Name == SourceName);
                return sourceInfo;
            }
        }

        #endregion

        #region target information
        /// <summary>
        /// String representation of the target type.
        /// </summary>
        [JsonProperty]
        internal string TargetType { get; set; }
        /// <summary>
        /// Name of the target variable
        /// </summary>
        [JsonProperty]
        internal string TargetName { get; set; }
        /// <summary>
        /// Base object of the target 
        /// </summary>
        [JsonProperty]
        internal Guid TargetGuid { get; set; }


        /// <summary>
        /// Check if the target is a field. If it is a field return the FieldInfo.
        /// Otherwise return null;
        /// </summary>
        [JsonIgnore]
        internal FieldInfo? TargetField
        {
            get
            {
                FieldInfo? inputField = Target?.GetType().GetFields().FirstOrDefault(x => x.Name == TargetName);
                return inputField;
            }
        }
        /// <summary>
        /// Check if the target is a property. If it is a field return the PropertyInfo.
        /// Otherwise return null;
        /// </summary>
        [JsonIgnore]
        internal PropertyInfo? TargetPropertyInfo
        {
            get
            {
                PropertyInfo? sourceInfo = Target?.GetType().GetProperties().FirstOrDefault(x => x.Name == SourceName);
                return sourceInfo;
            }
        }

        [JsonIgnore]
        public SimulationBase? Target => SimulationEnvironment?.GetComponentByGuid(TargetGuid);

        #endregion

        public Connection()
        {
            SourceName = string.Empty;
            SourceType = string.Empty;

            TargetName = string.Empty;
            TargetType = string.Empty;
        }

        [JsonIgnore]
        public object? SourceValue
        {
            get
            {
                if (Source == null)
                    return null;
                //Check if a field exists with a value
                if (SourceField != null)
                    return SourceField.GetValue(Source);
                //Check if a property exists that holds a value
                if (SourcePropertyInfo != null)
                    return SourcePropertyInfo.GetValue(Source);
                return null;
            }
        }

        [JsonIgnore]
        public bool HasChanged
        {
            get
            {
                if (SourceValue == null)
                    return false;
                if (SourceValue != null && CurrentValue == null)
                    return true;
                return SourceValue != null && SourceValue.Equals(CurrentValue);
            }
        }

        public void SetValue()
        {
            if (SourceType == TargetType)
            {
                object? value = SourceValue;
                if (TargetField != null)
                {
                    TargetField.SetValue(Target, value);
                    return;
                }

                if (TargetPropertyInfo != null)
                    TargetPropertyInfo.SetValue(Target, value);

            }
            else
            {

                if (TargetType == nameof(String))
                {
                    object? value = SourceValue;
                    if (TargetField != null)
                    {
                        TargetField.SetValue(Target, value?.ToString());
                        return;
                    }

                    if (TargetPropertyInfo != null)
                        TargetPropertyInfo.SetValue(Target, value?.ToString());
                }

            }

        }


        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
