using Easy2Sim.Connect;
using Easy2Sim.Environment;
using Easy2Sim.Solvers;
using Newtonsoft.Json;

namespace Easy2SimExamples;

/// <summary>
/// A simulation component that produces a sine wave.
/// </summary>
public class Sine : SimulationBase // (1)
{
    [JsonProperty]
    public SimulationValue<double> Output;

    [JsonProperty]
    public SimulationValue<double> Amplitude;
    [JsonProperty]
    public SimulationValue<double> Frequency;
    [JsonProperty]
    public SimulationValue<double> Offset;
    [JsonProperty]
    public SimulationValue<int> NumberOfSamples;

    // The parameterless constructor is needed for serialization (2)
    public Sine()
    {
        Amplitude = new SimulationValue<double>(1.0, nameof(Amplitude), this, SimulationValueAttributes.Parameter);
        Frequency = new SimulationValue<double>(1.0, nameof(Frequency), this, SimulationValueAttributes.Parameter);
        Offset = new SimulationValue<double>(0.0, nameof(Offset), this, SimulationValueAttributes.Parameter);
        Output = new SimulationValue<double>(0.0, nameof(Output), this, SimulationValueAttributes.Output); // (3)
        NumberOfSamples = new SimulationValue<int>(100, nameof(NumberOfSamples), this, SimulationValueAttributes.Parameter);
    }

    // Use this constructor when you create components, it registers the component in the environment (4)
    public Sine(SimulationEnvironment environment, SolverBase solver) : base(environment, solver)
    {
        Amplitude = new SimulationValue<double>(1.0, nameof(Amplitude), this, SimulationValueAttributes.Parameter);
        Frequency = new SimulationValue<double>(1.0, nameof(Frequency), this, SimulationValueAttributes.Parameter);
        Offset = new SimulationValue<double>(0.0, nameof(Offset), this, SimulationValueAttributes.Parameter);
        Output = new SimulationValue<double>(0.0, nameof(Output), this, SimulationValueAttributes.Output);
        NumberOfSamples = new SimulationValue<int>(100, nameof(NumberOfSamples), this, SimulationValueAttributes.Parameter);
    }

    // DynamicCalculation is called once per time step by the dynamic solver (5)
    public override void DynamicCalculation()
    {
        if (Solver == null) return;

        double timeInSeconds = (double)Solver.SimulationTime / NumberOfSamples.Value;
        double angle = 2 * Math.PI * Frequency.Value * timeInSeconds + Offset.Value;
        double sineValue = Amplitude.Value * Math.Sin(angle);

        Output.SetValue(sineValue, SimulationEventType.DiscreteCalculation);
    }
}
