## Dynamic solver

In a dynamic calculation, each components "DynamicCalculation" method is 
executed once per time step.

=== "Dynamic solver example "
	
    ``` { .csharp .annotate .select }

    SimulationEnvironment environment = new SimulationEnvironment();
    DynamicSolver solver = new DynamicSolver(environment);// (1)

    Sine sine = new Sine(environment, solver);

    solver.CalculateTo(100);
    ```    
	{ .annotate }

	1. Create the dynamic solver


## Discrete Solver
When using a discrete solver, each component is added to time step 0.


=== "Discrete solver example "
	
    ``` { .csharp .annotate .select }

    SimulationEnvironment environment = new SimulationEnvironment();
    DiscreteSolver discreteSolver = new DiscreteSolver(environment);// (1)

    Sine sine = new Sine(environment, solver);
    
    discreteSolver.AddEvent(sine); // (2)
    discreteSolver.AddEventAtTime(sine, 20); // (3)
    
    solver.CalculateTo(100);
    ```    
	{ .annotate }

	1. Create the discrete solver
    2. Add an event for sine at the current time step
    3. Add an event for sine at time step 20


Further events have to be added. This are the possible ways to add events:

1. **DiscreteSolver/AddEvent(SimulationBase simulationBase)**

	If a component should add a event in the same simulation time the solvers AddEvent can be used.
   
2. **DiscreteSolver/AddEventAtTime(SimulationBase simulationBase, long simulationTime)**

	Similar to the first variant, however, a time can be specified
   
3. **Connection changed**

	If two components are connected and the value changes, an event is automatically added for the connected component.
	To recognize a change the C# Equals method is used on the objects.
   
