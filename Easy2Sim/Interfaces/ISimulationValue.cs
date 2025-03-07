using Easy2Sim.Connect;

namespace Easy2Sim.Interfaces;

public interface ISimulationValue
{
    public List<SimulationValueAttributes> Attributes { get; set; }
}