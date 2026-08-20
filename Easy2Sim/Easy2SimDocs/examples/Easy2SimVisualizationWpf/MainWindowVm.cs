using System.Windows;
using Easy2Sim.Environment;
using Easy2Sim.Solvers.Dynamic;
using Easy2SimExamples;

namespace Easy2SimVisualizationWpf;

public class MainWindowVm
{
    public MainWindowModel Model { get; } = new MainWindowModel();

    public MainWindowVm()
    {
        // Run the simulation on a background thread so the UI stays responsive
        Task.Run(RunSimulation);
    }

    private void RunSimulation()
    {
        SimulationEnvironment environment = new SimulationEnvironment();
        DynamicSolver solver = new DynamicSolver(environment);

        // Slow the simulation down - without a delay it finishes
        // before the window is even shown
        solver.BaseModel.Delay = 20;

        Sine sine = new Sine(environment, solver);

        // Every SimulationValue raises a PropertyChanged event when its value changes.
        // Chart updates must happen on the UI thread, therefore the Dispatcher is used.
        sine.Output.PropertyChanged += (_, _) =>
        {
            double value = sine.Output.Value;
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Keep only the last 200 values in the chart
                if (Model.SineValues.Count > 200)
                    Model.SineValues.RemoveAt(0);
                Model.SineValues.Add(value);
            });
        };

        solver.Initialize();
        solver.CalculateTo(500);
    }
}
