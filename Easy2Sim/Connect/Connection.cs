using System.Collections;
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

        /// <summary>
        /// If the source is an IEnumerable we can also link individual values of this IEnumerable 
        /// </summary>
        internal int SourceIndex { get; set; }


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

        /// <summary>
        /// If the target is an IEnumerable we can also link individual values of this IEnumerable 
        /// </summary>
        internal int TargetIndex { get; set; }
        #endregion

        public Connection()
        {
            SourceName = string.Empty;
            SourceType = string.Empty;

            TargetName = string.Empty;
            TargetType = string.Empty;

            SourceIndex = -1;
            TargetIndex = -1;
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
                {
                    object returnValue = SourceField.GetValue(Source);
                    if (SourceIndex >= 0)
                    {
                        if (returnValue is IList list)
                            return list[SourceIndex];
                        if (returnValue is Array array)
                            return array.GetValue(SourceIndex);
                    }

                    return returnValue;
                }

                //Check if a property exists that holds a value
                if (SourcePropertyInfo != null)
                {
                    object returnValue = SourcePropertyInfo.GetValue(Source);

                    if (SourceIndex >= 0)
                    {
                        if (returnValue is IList list)
                            return list[SourceIndex];
                        if (returnValue is Array array)
                            return array.GetValue(SourceIndex);
                    }

                    return returnValue;
                }
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
            if ((SourceType != TargetType) && (TargetType != nameof(String))) return;
            object? value = SourceValue;
            if (TargetField != null)
            {
                if (TargetIndex >= 0)
                {
                    if (TargetField.GetValue(Target) is IList targetList)
                    {
                        if (TargetType == nameof(String))
                            targetList[TargetIndex] = value.ToString();
                        else
                            targetList[TargetIndex] = value;
                        return;
                    }

                    if (TargetField.GetValue(Target) is Array targetArray)
                    {
                        if (TargetType == nameof(String))
                            targetArray.SetValue(value.ToString(), TargetIndex);
                        else
                            targetArray.SetValue(value, TargetIndex);
                        return;
                    }
                }
                if (TargetType == nameof(String))
                    TargetField.SetValue(Target, value.ToString());
                else
                    TargetField.SetValue(Target, value);
                return;
            }
            else
            {
                if (TargetPropertyInfo == null) return;
                if (TargetIndex >= 0)
                {
                    if (TargetPropertyInfo.GetValue(Target) is IList targetList)
                    {
                        if (TargetType == nameof(String))
                            targetList[TargetIndex] = value.ToString();
                        else
                            targetList[TargetIndex] = value;

                        return;
                    }

                    if (TargetPropertyInfo.GetValue(Target) is Array targetArray)
                    {
                        if (TargetType == nameof(String))
                            targetArray.SetValue(value.ToString(), TargetIndex);
                        else
                            targetArray.SetValue(value, TargetIndex);
                        return;
                    }
                }


                if (TargetType == nameof(String))
                {
                    TargetPropertyInfo.SetValue(Target, value.ToString());
                }
                else
                    TargetPropertyInfo.SetValue(Target, value);
            }
        }


        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
