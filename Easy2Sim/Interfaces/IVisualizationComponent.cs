using Easy2Sim.Connect;
using Newtonsoft.Json;

namespace Easy2Sim.Interfaces;

public interface IVisualizationComponent
{
    [JsonProperty]
    public SimulationValue<int> Left { get; set; }
    [JsonProperty]
    public SimulationValue<int> Top { get; set; }
    [JsonProperty]
    public SimulationValue<double> ControlWidth { get; set; }
    [JsonProperty]
    public SimulationValue<double> ControlHeight { get; set; }
}