using Easy2Sim.Environment;
using Newtonsoft.Json;
using Easy2Sim.Interfaces;
using Easy2Sim.Solvers.Discrete;
using System.Reflection;

namespace Easy2Sim.Connect;

/// <summary>
/// This class defines connections in the simulation framework
/// </summary>
public class FeedbackConnection<T, T1> : IConnection
{
    /// <summary>
    /// Is the connection created by a component connection
    /// </summary>
    public bool IsComponentConnection { get; }

    //Reapplies the connection, in case the connections are serialized and deserialized
    public void Reapply()
    {
        if (Source != null && Target != null)
        {
            Source.PropertyChanged += SourceOnPropertyChanged;
            Target.FeedbackPropertyChanged += FeedbackValueChanged;
        }
        else
            SimulationEnvironment?.LogEnvironmentError("Can not reapply connection: Source is null");
    }


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
    public SimulationEnvironment? SimulationEnvironment => ComponentRegister.GetEnvironment(EnvironmentGuid);

    [JsonIgnore]
    public SimulationBase? SourceObject
    {
        get
        {
            SimulationBase? source = SimulationEnvironment?.Model.SimulationObjects.Values.FirstOrDefault(x => x.Easy2SimName == Source?.ParentName);
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
            SimulationBase? target = SimulationEnvironment?.Model.SimulationObjects.Values.FirstOrDefault(x => x.Easy2SimName == Target?.ParentName);
            if (target == null)
                SimulationEnvironment?.LogEnvironmentError("TargetObject can not be found in connection: " + Guid);
            return target;
        }
    }


    [JsonIgnore]
    public FeedbackSimulationValue<T, T1>? Source
    {
        get
        {
            try
            {
                SimulationBase? simObject = SimulationEnvironment?.Model.SimulationObjects
                    .FirstOrDefault(x => x.Value.Easy2SimName == SourceName).Value;
                PropertyInfo[] properties = simObject.GetType().GetProperties();
                // Get the PropertyInfo for the 'Value' property
                PropertyInfo propertyInfo = properties.FirstOrDefault(x => x.Name.Contains(SourceProperty));
                if (propertyInfo != null)
                {
                    return (FeedbackSimulationValue<T, T1>?)propertyInfo.GetValue(simObject);
                }

                FieldInfo[] fInfos = simObject.GetType().GetFields();
                FieldInfo fInfo = fInfos.FirstOrDefault(x => x.Name.Contains(SourceProperty));
                if (fInfo != null)
                {
                    return (FeedbackSimulationValue<T, T1>?)fInfo.GetValue(simObject);
                }
            }
            catch (Exception e)
            {
                SimulationEnvironment?.LogEnvironmentError("Exception while retrieving Source in feedback connection: " + Guid);
                return null;
            }

            SimulationEnvironment?.LogEnvironmentError("Source not found in feedback connection: " + Guid);
            return null;
        }
    }
    [JsonIgnore]
    public FeedbackSimulationValue<T, T1>? Target
    {
        get
        {
            try
            {

                SimulationBase simObject = SimulationEnvironment.Model.SimulationObjects
                    .FirstOrDefault(x => x.Value.Easy2SimName == TargetName).Value;
                PropertyInfo[] properties = simObject.GetType().GetProperties();
                // Get the PropertyInfo for the 'Value' property
                PropertyInfo propertyInfo = properties.FirstOrDefault(x => x.Name.Contains(TargetProperty));
                if (propertyInfo != null)
                {
                    return (FeedbackSimulationValue<T, T1>)propertyInfo.GetValue(simObject);
                }

                FieldInfo[] fInfos = simObject.GetType().GetFields();
                FieldInfo fInfo = fInfos.FirstOrDefault(x => x.Name.Contains(TargetProperty));
                if (fInfo != null)
                {
                    return (FeedbackSimulationValue<T, T1>)fInfo.GetValue(simObject);
                }
            }
            catch (Exception e)
            {
                SimulationEnvironment?.LogEnvironmentError("Exception while retrieving Target in feedback connection: " + Guid);
                return null;
            }
            SimulationEnvironment?.LogEnvironmentError("Target not found in feedback connection: " + Guid);
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
    public FeedbackConnection()
    {
        SourceName = string.Empty;
        TargetName = string.Empty;
        SourceProperty = string.Empty;
        TargetProperty = string.Empty;
    }
    public FeedbackConnection(FeedbackSimulationValue<T, T1>? source, FeedbackSimulationValue<T, T1>? target, Guid environmentGuid, bool isComponentConnection)
    {
        IsComponentConnection = isComponentConnection;
        EnvironmentGuid = environmentGuid;
        if (source != null && target != null)
        {
            SourceName = source.ParentName;
            TargetName = target.ParentName;

            SourceProperty = source.PropertyName;
            TargetProperty = target.PropertyName;

            if (Source != null)
                Source.PropertyChanged += SourceOnPropertyChanged;
            
            if (Target != null)
                Target.FeedbackPropertyChanged += FeedbackValueChanged;
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

    private void FeedbackValueChanged(object? sender, PropertyValueChangedEventArgs<T1> e)
    {
        Source?.SetFeedbackValue(e.NewValue);
        if (e.Solver is DiscreteSolver discreteSolver)
            if (SourceObject != null)
            {
                discreteSolver.AddAfterTimeEvent(SourceObject);

                SimulationEnvironment?.LogEnvironmentInfo($"Connection feedback value changed, add event for {SourceObject.Easy2SimName} at {discreteSolver.SimulationTime}");
            }

    }


    private void SourceOnPropertyChanged(object? sender, PropertyValueChangedEventArgs<T> e)
    {
        //Sets the ValueChanged of the target to true
        Target?.SetValue(e.NewValue);
        if (e.Solver is DiscreteSolver discreteSolver)
            if (TargetObject != null)
            {
                discreteSolver.AddEvent(TargetObject);

                SimulationEnvironment?.LogEnvironmentInfo($"Connection value changed, add event for {TargetObject.Easy2SimName} at {discreteSolver.SimulationTime}");
            }
    }

    public override string ToString()
    {
        return $"{SourceName}\\{SourceProperty} => {TargetName}\\{TargetProperty}";
    }

}
