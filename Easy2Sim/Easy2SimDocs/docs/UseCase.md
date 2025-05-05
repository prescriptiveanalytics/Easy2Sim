# Using Easy2Sim

!!! note annotate
    Make sure that you know which simulation type should be implemented before you continue. (1)

1. Simulation types can be e.g. discrete event simulation or dynamic simulation

To run a simulation this framework uses two key components:

- **Simulation environment**
	- Components
	- Links
	- Logging (Serilog)
- Simulation specific **Solver**
	- How are the connections resolved?
	- How often are components called 

When creating a new component, it registers in the simulation environment.

## Running a simulation

To start a simulation the steps are as follows:

1. Implement simulation library
2. Instantiate simulation environment and solver
3. Add components to environment
4. Add connections to the environment
5. Run the simulation 


## Implement a sine simulation component and visualize in xaml

=== "Simulation component - Sine" 

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
	
=== "Simulation component - LogInformationSink" 

    ``` { .csharp .annotate .select }
	using Easy2Sim.Connect.Attributes;
	using Easy2Sim.Environment;
	using Easy2Sim.Solvers;
	using Newtonsoft.Json;
	
	namespace StandardLibrary.Various
	{
		public class LogInformationSink : SimulationBase
		{
			[Input]
			[JsonProperty]
			public string Input;
	
			public LogInformationSink(){}
			
			public LogInformationSink(SimulationEnvironment environment, SolverBase solver) : base(environment, solver)
			{
				Input = "";
			}
			public override void DynamicCalculation()
			{
				SimulationEnvironment.LogInformation(Input.ToString()); // (1)
			}
	
			public override string SerializeToJson()
			{
				return JsonConvert.SerializeObject(this);
			}
		}
	}
	```
	{ .annotate }

	1. Information received through the input is logged with the logging framework given in the simulation environment
	
=== "SimpleConsoleSink.cs"

    ``` { .csharp .annotate }
	using Easy2SimVisualizationExample.Model;
	using Serilog.Core;
	using Serilog.Events;
	
	namespace Easy2SimVisualizationExample
	{
		public class SimpleConsoleSink : ILogEventSink
		{
			private MainWindowModel mainWindowModel;
	
			public SimpleConsoleSink(MainWindowModel mainWindowModel)
			{
				this.mainWindowModel = mainWindowModel;
			}
	
			public void Emit(LogEvent logEvent)
			{
				// (1)
				if (double.TryParse(logEvent.RenderMessage(), out double value))
				{
					if (mainWindowModel.DataPoints1.Count > 50)
					{
						mainWindowModel.DataPoints1.RemoveAt(0);
					}
					mainWindowModel.DataPoints1.Add(value);
				}
			}
		}
	}

	```
	{ .annotate }

	1. This sink connects the simulation with the visualization and adds generated data points to the model.
	
=== "MainWindowVm.cs"

    ``` { .csharp .annotate }
	using Easy2Sim.Solvers.Dynamic;
	using Easy2SimVisualizationExample.Model;
	using Serilog;
	using Serilog.Core;
	using StandardLibrary.Mathematical.Source;
	using StandardLibrary.Various;
	using System.ComponentModel;
	using System.Threading;
	using System.Threading.Tasks;
	using Easy2Sim.Environment;
	
	namespace Easy2SimVisualizationExample.ViewModel
	{
		public class MainWindowVm : INotifyPropertyChanged
		{
			public MainWindowModel MainWindowModel { get; set; }
	
			/// <summary>
			/// Used for xaml code completion
			/// </summary>
			public MainWindowVm() { }
	
			public MainWindowVm(object v)
			{
				MainWindowModel = new MainWindowModel(); // (1)
	
				Logger l1 = new LoggerConfiguration() // (2)
				.WriteTo.Sink(new SimpleConsoleSink(MainWindowModel)) // Use the custom console sink
				.CreateLogger();
	
	
				Task.Run(() => // (3)
				{
					// (4)
					SimulationEnvironment simulationEnvironment = new SimulationEnvironment();
					DynamicSolver solver = new DynamicSolver(simulationEnvironment);
					solver.BaseModel.Delay = 300;
					simulationEnvironment.SetLogConfiguration(l1);
	
					// (5)
					SineProperties sine = new SineProperties(simulationEnvironment, solver);
					LogInformationSink logInformationSink = new LogInformationSink(simulationEnvironment, solver);
					simulationEnvironment.AddConnection(sine, "Output", logInformationSink, "Input");
	
					// (6)
					solver.Initialize();
					solver.CalculateTo(1000000);
				});
			}
			public event PropertyChangedEventHandler? PropertyChanged;
		}
	}

    ```
	{ .annotate }

	1. Create a model that holds the current data for visualization
	2. The logger is used pass generated information to the MainWindowModel
	3. Start a background task that runs the simulation so that the gui is not blocked
	4. Initialize one environment, solver, and set the delay between the generated values to 300 ms
	5. Instantiate one Sine component, one sink and connect both components
	6. Initialize and start the simulation
	
	
=== "MainWindowModel.cs"
	``` { .csharp .annotate }
	using LiveCharts;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;
	
	namespace Easy2SimVisualizationExample.Model
	{
		public class MainWindowModel : INotifyPropertyChanged
		{
			private ChartValues<double> dataPoints1;
	
			// (1)
			public ChartValues<double> DataPoints1
			{
				get { return dataPoints1; }
				set
				{
					dataPoints1 = value;
					OnPropertyChanged();
				}
			}
	
			public MainWindowModel()
			{
				DataPoints1 = new ChartValues<double> { };
			}
			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
	
			public event PropertyChangedEventHandler? PropertyChanged;
		}
	}
	```
	{ .annotate }

	1. Property for all generated chart values
		
=== "MainWindow.xaml"
	``` { .xml }
	<Window x:Class="Easy2SimVisualizationExample.MainWindow"
			xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
			xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
			xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
			xmlns:local="clr-namespace:Easy2SimVisualizationExample"
			xmlns:dataContext="clr-namespace:Easy2SimVisualizationExample.ViewModel"
			xmlns:lvc="clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf"
			mc:Ignorable="d"
			Title="Visualization preview" Height="450" Width="800">
		<Window.DataContext>
			<dataContext:MainWindowVm/>
		</Window.DataContext>
		<Grid>
			<lvc:CartesianChart>
				<lvc:CartesianChart.Series>
					<lvc:LineSeries Title="Data" Values="{Binding MainWindowModel.DataPoints1}" />
				</lvc:CartesianChart.Series>
			</lvc:CartesianChart>
		</Grid>
	</Window>
	```
	
Visualization of random values and the Sine (Sine uses code from above):

![type:video](./Videos/cropped.mp4)