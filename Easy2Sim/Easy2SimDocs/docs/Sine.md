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

   
=== "Simple simulation component - Sine" 

    ``` { .csharp .annotate .select }
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

        public Sine() // (4)
        {
            Amplitude = new SimulationValue<double>(1.0, nameof(Amplitude), this, SimulationValueAttributes.Parameter);
            Frequency = new SimulationValue<double>(10.0, nameof(Frequency), this, SimulationValueAttributes.Parameter);
            Offset = new SimulationValue<double>(0.0, nameof(Offset), this, SimulationValueAttributes.Parameter);
            Output = new SimulationValue<double>(0.0, nameof(Output), this, SimulationValueAttributes.Output);
            NumberOfSamples = new SimulationValue<int>(100, nameof(NumberOfSamples), this, SimulationValueAttributes.Parameter);
        }

        public Sine(SimulationEnvironment environment, SolverBase solverBase) : base(environment, solverBase) // (3)
        {
            Amplitude = new SimulationValue<double>(1.0, nameof(Amplitude), this, SimulationValueAttributes.Parameter);
            Frequency = new SimulationValue<double>(10.0, nameof(Frequency), this, SimulationValueAttributes.Parameter);
            Offset = new SimulationValue<double>(0.0, nameof(Offset), this, SimulationValueAttributes.Parameter);
            Output = new SimulationValue<double>(0.0, nameof(Output), this, SimulationValueAttributes.Output); // (2)
            NumberOfSamples = new SimulationValue<int>(100, nameof(NumberOfSamples), this, SimulationValueAttributes.Parameter);
        }

        public override void DynamicCalculation() // (5)
        {
            if (Solver == null) return;

            double timeInSeconds = (double)Solver.BaseModel.SimulationTime / NumberOfSamples.Value;
            double angle = 2 * Math.PI * Frequency.Value * timeInSeconds + Offset.Value;
            double sineValue = Amplitude.Value * Math.Sin(angle);

            Output.SetValue(sineValue, SimulationEventType.DiscreteCalculation);
        }
    }
    ```    
	{ .annotate }

	1. Base class of simulation components
	2. Output defines, that this Property can be connected to a Input of another component
	3. Use this constructor when you create components, it will register the components in the framework
	4. Empty constructor is needed for serialization
    5. DynamicCalculation is called in every simulated time step in the Dynamic solver
	