using Easy2Sim.Connect;
using Easy2Sim.Environment;
using Easy2Sim.Solvers;
using Easy2Sim.Solvers.Discrete;
using Newtonsoft.Json;

namespace Easy2SimExamples;

/// <summary>
/// A simulation component that produces a tick every 10 simulation time units.
/// </summary>
public class Clock : SimulationBase
{
    [JsonProperty]
    public SimulationValue<long> Tick;

    public Clock()
    {
        Tick = new SimulationValue<long>(0, nameof(Tick), this, SimulationValueAttributes.Output);
    }

    public Clock(SimulationEnvironment environment, SolverBase solver) : base(environment, solver)
    {
        Tick = new SimulationValue<long>(0, nameof(Tick), this, SimulationValueAttributes.Output);
    }

    // DiscreteCalculation is called by the discrete solver whenever an event
    // for this component is processed
    public override void DiscreteCalculation()
    {
        if (Solver == null) return;

        // Publish the current simulation time to all connected components
        Tick.SetValue(Solver.SimulationTime, SimulationEventType.DiscreteCalculation);

        // Schedule the next tick 10 time units later
        Solver.AsDiscreteSolver?.AddEventAtTime(this, Solver.SimulationTime + 10);
    }
}

/// <summary>
/// A simulation component that prints every value it receives on its input.
/// </summary>
public class Printer : SimulationBase
{
    [JsonProperty]
    public SimulationValue<long> TickIn;

    public Printer()
    {
        TickIn = new SimulationValue<long>(0, nameof(TickIn), this, SimulationValueAttributes.Input);
    }

    public Printer(SimulationEnvironment environment, SolverBase solver) : base(environment, solver)
    {
        TickIn = new SimulationValue<long>(0, nameof(TickIn), this, SimulationValueAttributes.Input);
    }

    public override void DiscreteCalculation()
    {
        Console.WriteLine($"t={Solver?.SimulationTime,3}  printer received tick {TickIn.Value}");
    }
}

public static class DiscreteExample
{
    public static void Run()
    {
        SimulationEnvironment environment = new SimulationEnvironment();
        DiscreteSolver solver = new DiscreteSolver(environment);

        Clock clock = new Clock(environment, solver);
        Printer printer = new Printer(environment, solver);

        // Connect the clock output to the printer input.
        // Whenever Tick changes, the new value is copied to TickIn and an event
        // for the printer is added automatically at the current simulation time.
        environment.AddConnection(clock.Tick, printer.TickIn);

        solver.Initialize();

        // Schedule the first event at the current simulation time (0).
        // All further events are scheduled by the clock itself.
        solver.AddEvent(clock);

        solver.CalculateTo(50);
    }
}
