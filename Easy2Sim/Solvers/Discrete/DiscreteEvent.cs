using Easy2Sim.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Easy2Sim.Solvers.Discrete;

/// <summary>
/// One event in the discrete simulation.
/// </summary>
public record struct DiscreteEvent : IFrameworkBase, IComparable<DiscreteEvent>
{
    /// <summary>
    /// Unique Guid that can be used to identify class instances
    /// </summary>
    [JsonProperty]
    public Guid Guid { get; } 

    /// <summary>
    /// Timestamp at which the event should be handled
    /// </summary>
    [JsonProperty]
    public long TimeStamp { get; }

    /// <summary>
    /// In case of loops, this value keeps track of the order of events
    /// </summary>
    [JsonProperty]
    public long TimeStampIndex { get; set; }

    /// <summary>
    /// Name of the component which the event should be simulated at the specified time
    /// </summary>
    [JsonProperty]
    public string ComponentName { get; }

    /// <summary>
    /// Constructor that is used for serialization.
    /// Should not be used, as a environment guid is necessary.
    /// </summary>
    [JsonConstructor]
    public DiscreteEvent()
    {
        Guid = Guid.NewGuid();
        ComponentName = "";
        TimeStamp = 0;
        TimeStampIndex = 0;
    }

    public bool Equals(DiscreteEvent? other)
    {
        if(other == null)
            return false;
        return Guid == other.Value.Guid;
    }

    public override int GetHashCode() => Guid.GetHashCode();

    /// <summary>
    /// Default constructor that should be used. 
    /// </summary>
    /// <param name="timeStamp">Time step of the event</param>
    /// <param name="componentName">Unique name of the component</param>
    public DiscreteEvent(long timeStamp, string componentName)
    {
        Guid = Guid.NewGuid();
        TimeStamp = timeStamp;
        ComponentName = componentName;
        TimeStampIndex = 0;
    }

    public override string ToString()
    {
        return "Event for " + ComponentName + " at " + TimeStamp;
    }


    public int CompareTo(DiscreteEvent other)
    {
        if (TimeStamp != other.TimeStamp)
            return TimeStamp.CompareTo(other.TimeStamp);
        return TimeStampIndex.CompareTo(other.TimeStampIndex);
    }
}
