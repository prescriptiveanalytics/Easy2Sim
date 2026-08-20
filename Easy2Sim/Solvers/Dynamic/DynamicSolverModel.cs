using Easy2Sim.Interfaces;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Dynamic;

/// <summary>
/// Holds all data for a dynamic simulation.
/// </summary>
public class DynamicSolverModel : IFrameworkBase, ICloneable<DynamicSolverModel>
{
    /// <summary>
    /// This value represents the simulation time increase after each simulation step.
    /// </summary>
    [JsonProperty]
    public int SimulationStep { get; set; }

    [JsonIgnore]
    public Guid Guid {get;set;}

    private void SetDefaultValues()
    {
        Guid = Guid.NewGuid();
        SimulationStep = 1;
    }

    /// <summary>
    /// Constructor that is used for serialization.
    /// Should not be used, as a environment guid is necessary.
    /// </summary>
    public DynamicSolverModel()
    {
        SetDefaultValues();
    }

    public DynamicSolverModel Clone()
    {
        DynamicSolverModel dynamicSolverModel = new DynamicSolverModel();
        dynamicSolverModel.SimulationStep = SimulationStep;
        return dynamicSolverModel;
    }
}
