using Newtonsoft.Json;

namespace Easy2Sim.Interfaces;

/// <summary>
/// Base interface for each class in the framework.
/// SerializeToJson uses the default constructor.
/// This needs to be implemented in each class.
/// </summary>
public interface IFrameworkBase
{
    /// <summary>
    /// The guid allows to uniquely identify each element in the simulation.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Serialize to json uses the default constructor.
    /// </summary>
    public string SerializeToJson()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        return JsonConvert.SerializeObject(this, settings);
    }
}
