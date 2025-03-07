using Easy2Sim.Environment;
using Easy2Sim.Interfaces;
using Easy2Sim.Solvers;
using Newtonsoft.Json;

namespace Easy2Sim.Connect;

/// <summary>
/// A property changed event, where the new value of the property is transmitted
/// </summary>
/// <typeparam name="T"></typeparam>
public class PropertyValueChangedEventArgs<T> : EventArgs, IFrameworkBase
{
    [JsonProperty]
    public Guid Guid { get; }
    [JsonProperty]
    public T? NewValue { get; }
    [JsonProperty]
    public T? OldValue { get; }
    [JsonIgnore]
    public SolverBase? Solver  => ComponentRegister.GetSolver(SolverGuid);

    [JsonProperty]
    public SimulationEventType SimulationEventType { get; set; }
    [JsonIgnore]
    public Guid SolverGuid { get; set; }

    [JsonConstructor]
    public PropertyValueChangedEventArgs() { }
    public PropertyValueChangedEventArgs(T? newValue, T? oldValue, SolverBase solver, SimulationEventType type)
    {
        NewValue = newValue;
        OldValue = oldValue;
        SolverGuid  = solver.Guid;
        SimulationEventType = type;
        Guid = Guid.NewGuid();
    }
}
