using System.Collections;
using Easy2Sim.Environment;
using Newtonsoft.Json;
using Easy2Sim.Interfaces;
using Easy2Sim.Solvers.Discrete;
using System.Reflection;

namespace Easy2Sim.Connect;

/// <summary>
/// This class defines connections in the simulation framework
/// </summary>
public class Connection<T> : IConnection, ICloneable<Connection<T>>
{
    [JsonProperty]
    public bool IsComponentConnection { get; }

    public void Reapply()
    {
        if (Source != null)
            Source.PropertyChanged += SourceOnPropertyChanged;
        else
            SimulationEnvironment?.LogEnvironmentError("Can not reapply connection: Source is null");
    }


    /// <summary>
    /// Unique Guid that can be used to uniquely identify class instances
    /// </summary>
    [JsonProperty]
    public Guid Guid { get; set; } = Guid.NewGuid();

    
    /// <summary>
    /// Returns the current environment in which the connection is registered.
    /// Returns null if no environment can be found.
    /// </summary>
    [JsonIgnore]
    public SimulationEnvironment? SimulationEnvironment { get; set; }


    [JsonIgnore]
    public SimulationBase? SourceObject
    {
        get
        {
            SimulationBase? source = SimulationEnvironment?.Model.SimulationObjects.Values.FirstOrDefault(x => x.Easy2SimName == SourceName);
            if (source == null)
                SimulationEnvironment?.LogEnvironmentError("SourceObject can not be found in connection: " + Guid);
            return source;
        }
    }
    [JsonIgnore]
    public SimulationBase? TargetObject
    {
        get
        {
            SimulationBase? target = SimulationEnvironment?.Model.SimulationObjects.Values.FirstOrDefault(x => x.Easy2SimName == TargetName);
            if (target == null)
                SimulationEnvironment?.LogEnvironmentError("TargetObject can not be found in connection: " + Guid);
            return target;
        }
    }


    [JsonIgnore]
    public SimulationValue<T>? Source
    {
        get
        {
            try
            {
                SimulationBase? simObject = SimulationEnvironment?.Model.SimulationObjects
                    .FirstOrDefault(x => x.Value.Easy2SimName == SourceName).Value;
                PropertyInfo[]? properties = simObject?.GetType().GetProperties();
                // Get the PropertyInfo for the 'Value' property
                PropertyInfo? propertyInfo = properties?.FirstOrDefault(x => x.Name.Contains(SourceProperty));
                if (propertyInfo != null)
                {
                    return (SimulationValue<T>?)propertyInfo.GetValue(simObject);
                }

                FieldInfo[]? fInfos = simObject?.GetType().GetFields();
                FieldInfo? fInfo = fInfos?.FirstOrDefault(x => x.Name.Contains(SourceProperty));
                if (fInfo != null)
                {
                    return (SimulationValue<T>?)fInfo.GetValue(simObject);
                }
            }
            catch (Exception e)
            {
                SimulationEnvironment?.LogEnvironmentError($"Exception {e.ToString()} while retrieving Source in connection: {this.ToString()} (guid: " + Guid + ")");
                return null;
            }

            SimulationEnvironment?.LogEnvironmentError($"Source not found in connection: {this.ToString()}" + Guid);
            return null;
        }
    }
    [JsonIgnore]
    public SimulationValue<T>? Target
    {
        get
        {
            try
            {

                SimulationBase? simObject = SimulationEnvironment?.Model.SimulationObjects
                    .FirstOrDefault(x => x.Value.Easy2SimName == TargetName).Value;
                PropertyInfo[]? properties = simObject?.GetType().GetProperties();
                // Get the PropertyInfo for the 'Value' property
                PropertyInfo? propertyInfo = properties?.FirstOrDefault(x => x.Name.Contains(TargetProperty));
                if (propertyInfo != null)
                {
                    return (SimulationValue<T>?)propertyInfo.GetValue(simObject);
                }

                FieldInfo[]? fInfos = simObject?.GetType().GetFields();
                FieldInfo? fInfo = fInfos?.FirstOrDefault(x => x.Name.Contains(TargetProperty));
                if (fInfo != null)
                {
                    return (SimulationValue<T>?)fInfo.GetValue(simObject);
                }
            }
            catch (Exception e)
            {

                SimulationEnvironment?.LogEnvironmentError($"Exception {e} while retrieving Target in connection: {this.ToString()} (guid: " + Guid +")");
                return null;
            }
            SimulationEnvironment?.LogEnvironmentError($"Target not found in connection: {this.ToString()}" + Guid);
            return null;
        }
    }
    [JsonIgnore]
    public dynamic? DynamicTarget
    {
        get
        {
            SimulationBase? simObject = SimulationEnvironment?.Model.SimulationObjects
                .FirstOrDefault(x => x.Value.Easy2SimName == TargetName).Value;
            PropertyInfo[]? properties = simObject?.GetType().GetProperties();
            // Get the PropertyInfo for the 'Value' property
            PropertyInfo? propertyInfo = properties?.FirstOrDefault(x => x.Name.Contains(TargetProperty));
            if (propertyInfo != null)
            {
                return (dynamic?)propertyInfo.GetValue(simObject);
            }

            FieldInfo[]? fInfos = simObject?.GetType().GetFields();
            FieldInfo? fInfo = fInfos?.FirstOrDefault(x => x.Name.Contains(TargetProperty));
            if (fInfo != null)
            {
                return (dynamic?)fInfo.GetValue(simObject);
            }

            return null;
        }
    }


    [JsonProperty]
    public string SourceName { get; set; }
    [JsonProperty]
    public string TargetName { get; set; }
    [JsonProperty]
    public string SourceProperty { get; set; }
    [JsonProperty]
    public string TargetProperty { get; set; }

    [JsonConstructor]
    public Connection(bool isComponentConnection = false)
    {
        SourceName = string.Empty;
        TargetName = string.Empty;
        SourceProperty = string.Empty;
        TargetProperty = string.Empty;
        SimulationEnvironment = null;
        IsComponentConnection = isComponentConnection;
    }
    public Connection(SimulationValue<T>? source, SimulationValue<T>? target, SimulationEnvironment environment, bool isComponentConnection)
    {
        IsComponentConnection = isComponentConnection;
        SimulationEnvironment = environment;
        if (source != null && target != null)
        {
            SourceName = source.ParentName;
            TargetName = target.ParentName;

            SourceProperty = source.PropertyName;
            TargetProperty = target.PropertyName;

            if (Source != null)
                Source.PropertyChanged += SourceOnPropertyChanged;
        }
        else
        {
            SourceName = "";
            TargetName = "";
            SourceProperty = "";
            TargetProperty = "";
            SimulationEnvironment?.LogEnvironmentError("Can not create connection: source or target is null");
        }
    }
    public Connection(string sourceParent, string sourceProperty, string targetParent, string targetProperty, SimulationEnvironment environment, bool isComponentConnection)
    {
        IsComponentConnection = isComponentConnection;
        SimulationEnvironment = environment;
        SourceName = sourceParent;
        TargetName = targetParent;

        SourceProperty = sourceProperty;
        TargetProperty = targetProperty;

        if (Source != null)
            Source.PropertyChanged += SourceOnPropertyChanged;

    }


    private void SourceOnPropertyChanged(object? sender, PropertyValueChangedEventArgs<T> e)
    {
        if (e.NewValue is IList && DynamicTarget?.Value is IList)
        {
            DynamicTarget.Value.AddRange(e.NewValue);
            DynamicTarget.ValueChanged = true;
        }
        else if (DynamicTarget?.Value is List<T> targetList)
        {
            if (e.NewValue != null) 
                targetList.Add(e.NewValue);
            DynamicTarget.ValueChanged = true;
        }
        else
            //Sets the ValueChanged of the target to true
            Target?.SetValue(e.NewValue);

        if (e.Solver is DiscreteSolver discreteSolver)
            if (TargetObject != null)
            {
                switch (e.SimulationEventType)
                {
                    case SimulationEventType.DiscreteCalculation:
                        discreteSolver.AddEvent(TargetObject);
                        break;
                    case SimulationEventType.PostCalculation:
                        discreteSolver.AddAfterTimeEvent(TargetObject);
                        break;
                    case SimulationEventType.NoEvent:
                        break;
                }
            }
    }

    public Connection<T> Clone()
    {
        Connection<T> result = new Connection<T>(IsComponentConnection);
        result.SimulationEnvironment = null;
        result.SourceName = SourceName;
        result.SourceProperty = SourceProperty;

        result.TargetName = TargetName;
        result.TargetProperty = TargetProperty;
        
        return result;
    }

    public override string ToString()
    {
        return $"{SourceName}\\{SourceProperty} => {TargetName}\\{TargetProperty}";
    }
}
