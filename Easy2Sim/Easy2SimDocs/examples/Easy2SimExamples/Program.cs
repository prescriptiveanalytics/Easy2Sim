using Easy2Sim.Environment;
using Easy2Sim.Solvers.Dynamic;
using Easy2SimExamples;

// 1. Create the simulation environment and a solver
SimulationEnvironment environment = new SimulationEnvironment();
DynamicSolver solver = new DynamicSolver(environment);

// 2. Create simulation components - they register themselves in the environment
Sine sine = new Sine(environment, solver);

// Print the output value every 10 time steps
sine.Output.PropertyChanged += (_, _) =>
{
    if (solver.SimulationTime % 10 == 0)
        Console.WriteLine($"t={solver.SimulationTime,3}  output={sine.Output.Value,8:F4}");
};

// 3. Initialize the simulation (calls Initialize() on every component)
solver.Initialize();

// 4. Run the simulation until time step 100
solver.CalculateTo(100);
