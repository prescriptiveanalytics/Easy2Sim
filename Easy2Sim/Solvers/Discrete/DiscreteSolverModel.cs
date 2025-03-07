using System.Security.Cryptography.X509Certificates;
using Easy2Sim.Environment;
using Easy2Sim.Interfaces;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Discrete;

/// <summary>
/// Holds all data for a discrete simulation.
/// </summary>
public class DiscreteSolverModel : IFrameworkBase
{
    /// <summary>
    /// Each object in the framework should have a unique guid
    /// </summary>
    [JsonIgnore]
    public Guid Guid { get; set; }
    /// <summary>
    /// Can the component be added multiple times at the same simulation time.
    /// This allows loops between components in one time stamp.
    /// </summary>
    [JsonProperty]
    public bool AllowLoops { get; set; }

    /// <summary>
    /// Can the component be added multiple times at the same simulation time.
    /// This allows loops between components in one time stamp.
    /// </summary>
    [JsonProperty]
    public bool AllowAfterTimeLoops { get; set; }


    /// <summary>
    /// Keep track of historic events in the current time stamp
    /// </summary>
    [JsonProperty]
    public SortedSet<DiscreteEvent> HistoricEvents { get; set; }

    /// <summary>
    /// List of all current events
    /// </summary>
    [JsonProperty]
    public SortedDictionary<long, SortedSet<DiscreteEvent>> EventList { get; set; }


    /// <summary>
    /// Keep track of historic after time events
    /// </summary>
    [JsonProperty]
    public SortedSet<DiscreteEvent> HistoricAfterEvents { get; set; }

    /// <summary>
    /// List of all current events
    /// </summary>
    [JsonProperty]
    public SortedDictionary<long, SortedSet<DiscreteEvent>> AfterTimeEventList { get; set; }
    /// <summary>
    /// Add an event to a specific <paramref name="timeStamp"/> for a simulation component <paramref name="simBase"/>
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <param name="simBase"></param>
    public void AddEvent(long timeStamp, SimulationBase simBase)
    {
        DiscreteEvent result = new DiscreteEvent(timeStamp, simBase.Easy2SimName);
        if (AllowLoops)
        {
            if (!EventList.ContainsKey(timeStamp))
                EventList.Add(timeStamp, new SortedSet<DiscreteEvent>());
            result.TimeStampIndex = simBase.Index +
                                    HistoricEvents.Count(x => x.ComponentName == simBase.Easy2SimName) *
                                    simBase.SimulationEnvironment.Model.SimulationIndex;
            EventList[timeStamp].Add(result);
            simBase.SimulationEnvironment.LogEnvironmentInfo($"Added event for {simBase.Easy2SimName} at {timeStamp}");
        }
        else
        {
            if (!EventList.ContainsKey(timeStamp))
                EventList.Add(timeStamp, new SortedSet<DiscreteEvent>());
            else
            {
                if (EventList[timeStamp].Any(x => x.ComponentName == simBase.Easy2SimName))
                    return;
            }
            result.TimeStampIndex = simBase.Index;
            EventList[timeStamp].Add(result);
            simBase.SimulationEnvironment.LogEnvironmentInfo($"Added event for {simBase.Easy2SimName} at {timeStamp}");
        }
    }




    /// <summary>
    /// Constructor that is used for serialization.
    /// Should not be used, as a environment guid is necessary.
    /// </summary>
    public DiscreteSolverModel()
    {
        Guid = Guid.NewGuid();
        HistoricEvents = new SortedSet<DiscreteEvent>();
        HistoricAfterEvents = new SortedSet<DiscreteEvent>();
        EventList = new SortedDictionary<long, SortedSet<DiscreteEvent>>();
        AfterTimeEventList = new SortedDictionary<long, SortedSet<DiscreteEvent>>();
    }


    public bool AnyEvent => EventList.Any() || AfterTimeEventList.Any();

    public void AddAfterTimeEvent(long timeStamp, SimulationBase simBase)
    {
        DiscreteEvent result = new DiscreteEvent(timeStamp, simBase.Easy2SimName);
        if (AllowAfterTimeLoops)
        {

            if (!AfterTimeEventList.ContainsKey(timeStamp))
                AfterTimeEventList.Add(timeStamp, new SortedSet<DiscreteEvent>());

            result.TimeStampIndex = simBase.Index +
                                    HistoricAfterEvents.Where(x => x.TimeStamp == timeStamp)
                                        .Count(x => x.ComponentName == simBase.Easy2SimName) *
                                    simBase.SimulationEnvironment.Model.SimulationIndex;
            AfterTimeEventList[timeStamp].Add(result);
            simBase.SimulationEnvironment.LogEnvironmentInfo($"Added post event for {simBase.Easy2SimName} at {timeStamp}");
        }
        else
        {
            if (!AfterTimeEventList.ContainsKey(timeStamp))
                AfterTimeEventList.Add(timeStamp, new SortedSet<DiscreteEvent>());
            else
            {
                if (AfterTimeEventList[timeStamp].Any(x => x.ComponentName == simBase.Easy2SimName))
                    return;
            }
            result.TimeStampIndex = simBase.Index;
            simBase.SimulationEnvironment.LogEnvironmentInfo($"Added post event for {simBase.Easy2SimName} at {timeStamp}");
            AfterTimeEventList[timeStamp].Add(result);
        }
    }

    /// <summary>
    /// Check if there is at least one after time event for a specific time
    /// </summary>
    public bool AnyAfterTimeEvent(long baseModelSimulationTime)
    {
        return AfterTimeEventList.ContainsKey(baseModelSimulationTime) && AfterTimeEventList[baseModelSimulationTime].Any();
    }

    public DiscreteEvent GetNextAfterTimeEvent()
    {
        throw new NotImplementedException();
    }

    public void RemoveEventsForEasy2SimName(string componentName)
    {
        List<long> keysToRemove = new List<long>();
    
        keysToRemove.Clear();
        foreach (long key in EventList.Keys)
        {
            SortedSet<DiscreteEvent> events = EventList[key];
            events.RemoveWhere(x => x.ComponentName == componentName);
            if (events.Count == 0)
                keysToRemove.Add(key);
        }

        foreach (long l in keysToRemove)
            EventList.Remove(l);
    }

    public void RemoveAfterTimeEventsForEasy2SimName(string componentName)
    {
        List<long> keysToRemove = new List<long>();
        foreach (long key in AfterTimeEventList.Keys)
        {
            SortedSet<DiscreteEvent> events = AfterTimeEventList[key];
            events.RemoveWhere(x => x.ComponentName == componentName);
            if (events.Count == 0)
                keysToRemove.Add(key);
        }

        foreach (long l in keysToRemove)
            AfterTimeEventList.Remove(l);
    }
}
