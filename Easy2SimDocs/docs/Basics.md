# Basic concepts in the Framework

To create a new simulation component, the base class SimulationBase should be used.
Once instantiated, the Environment will automatically set a simulation index based on the order of instantiation.
This index defines the execution order of the components in one time step.
If this should be changed the method SimulationBase\SetIndexManually(int index) can be used.
Typically this simulation index starts at 0 and is increased by one per instantiated component.
When setting it manually, even negative values can be used.

=== "Index example - Main "
	
    ``` { .csharp .annotate .select }
	SimulationEnvironment environment = new SimulationEnvironment();
	DiscreteSolver solver = new DiscreteSolver(environment);

	Sine sine1 = new(environment, solver); // (1)
	Sine sine2 = new(environment, solver); // (2)
	
	sine1.SetIndexManually(3) // (3)
    ```    
	{ .annotate }

	1. sine1 has the simulation index 0
	2. sine2 has the simulation index 1
	2. sine1 has the simulation index 3

In a dynamic calculation, each components "DynamicCalculation" method is executed once per time step.
When using a discrete solver, each component is added to time step 0.
Further events have to be added. This are the possible ways to add events:

1. **DiscreteSolver/AddEvent(SimulationBase simulationBase)**

	If a component should add a event in the same simulation time the solvers AddEvent can be used.
   
2. **DiscreteSolver/AddEventAtTime(SimulationBase simulationBase, long simulationTime)**

	Similar to the first variant, however, a time can be specified
   
3. **Connection changed**

	If two components are connected and the value changes, an event is automatically added for the connected component.
	To recognize a change the C# Equals method is used on the objects.
   
   
=== "Simple simulation component - Sine" 

    ``` { .csharp .annotate .select }
	using Easy2Sim.Connect.Attributes;
	using Easy2Sim.Environment;
	using Easy2Sim.Solvers;
	using Newtonsoft.Json;
	
	namespace StandardLibrary.Mathematical.Source
	{
		public class Sine : SimulationBase // (1)
		{
			[Output] // (2)
			[JsonProperty] // (3)
			public double Output;
			[JsonProperty]
			public double Amplitude;
			[JsonProperty]
			public double Frequency;
			[JsonProperty]
			public double Offset;
			[JsonProperty]
			public int NumberOfSamples;
			public Sine() // (4)
			{
				Amplitude = 1.0;
				Frequency = 10.0;
				Offset = 0;
				Output = 0.0;
				NumberOfSamples = 100;
			}
			public Sine(SimulationEnvironment environment, SolverBase solverBase) : base(environment, solverBase) // (5)
			{
				Amplitude = 1.0;
				Frequency = 10.0;
				Offset = 0;
				Output = 0.0;
				NumberOfSamples = 100;
			}
			public override void DynamicCalculation() // (6)
			{
				if (Solver == null) return;
	
				double timeInSeconds = (double)Solver.BaseModel.SimulationTime / NumberOfSamples;
				double angle = 2 * Math.PI * Frequency * timeInSeconds + Offset;
				double sineValue = Amplitude * Math.Sin(angle);
	
				Output = sineValue;
			}
			public override string SerializeToJson() // (7)
			{
				return JsonConvert.SerializeObject(this);
			}
		}
	}
    ```    
	{ .annotate }

	1. Each simulation component needs to implement SimulationBase
	2. Output defines, that this Property can be connected to a Input of another component
	3. To allow serialization and deserialization, each component needs to be serializable
	4. An empty constructor is needed for the serialization framework 
	5. Use this constructor when you create components, it will register the components in the framework
	6. DynamicCalculation is called in every simulated time step in the Dynamic solver
	7. The SerializeToJson method needs to be overwritten in order to allow the framework to serialize this component
	