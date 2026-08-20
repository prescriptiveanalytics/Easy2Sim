# Visualization (WPF)

Easy2Sim simulations can be visualized in WPF in two ways:

1. **Live charts in your own WPF application** — subscribe to value changes and update
   a chart (e.g. with [LiveCharts](https://lvcharts.com/)). Simple and ideal for a quick
   visualization of a few values.
2. **Custom visualization components** with the Easy2Sim.Visualization application —
   every simulation component gets its own WPF user control (e.g. a machine, a tank, a
   vehicle) that is placed on a canvas and updated from the visualization log. Suited
   for visualizing complete simulation models.

Both approaches build on the same mechanism: changes of `SimulationValue` fields raise
events and can additionally be written to the visualization log.

## Preparing the simulation

### Marking values for visualization

Add one of the visualization attributes to the `SimulationValue` fields that should be
visible in the visualization:

| Attribute | Behavior |
|-----------|----------|
| `Visualization` | The value is logged at every simulation time step. |
| `VisualizationOnChange` | The value is only logged when it changed. |
| `VisualizationInitialize` | The value is logged once during initialization (e.g. layout information). |

Attributes can be combined:

```csharp
FillLevel = new SimulationValue<double>(0.0, nameof(FillLevel), this,
    new List<SimulationValueAttributes>
    {
        SimulationValueAttributes.Parameter,
        SimulationValueAttributes.VisualizationOnChange
    });
```

### The visualization log

During a simulation run, marked values are written to the `VisualizationLogger` of the
environment as a semicolon-separated line:

```text
{SimulationTime};{ComponentClassName};{SimulationIndex};{PropertyName};{Value}
```

Example — a component of type `Tank` with simulation index 0 at time step 42:

```text
42;Tank;0;FillLevel;87.5
```

The logger is a normal Serilog logger, so any Serilog sink can be attached — write to a
file for later playback, or publish to MQTT for a live visualization in a separate
application:

```csharp
environment.Model.Easy2SimLogging.VisualizationLogger = new LoggerConfiguration()
    .WriteTo.File("visualization.log")
    .CreateLogger();
```

## Option 1: Live charts in your own WPF application

A complete, compilable example project ships with this documentation
(`examples/Easy2SimVisualizationWpf`). The important pieces:

### Project setup

The project targets `net8.0-windows`, enables WPF and references LiveCharts:

```xml title="Easy2SimVisualizationWpf.csproj (excerpt)"
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Easy2Sim" Version="0.3.0" />
  <PackageReference Include="LiveCharts.Wpf" Version="0.9.7" />
</ItemGroup>
```

### View model — run the simulation and feed the chart

```csharp title="MainWindowVm.cs"
--8<-- "Easy2SimVisualizationWpf/MainWindowVm.cs"
```

### Model — the data shown in the chart

```csharp title="MainWindowModel.cs"
--8<-- "Easy2SimVisualizationWpf/MainWindowModel.cs"
```

### View — bind a chart to the values

```xml title="MainWindow.xaml"
--8<-- "Easy2SimVisualizationWpf/MainWindow.xaml"
```

### Things to watch out for

- **Run the simulation on a background thread** (`Task.Run`), otherwise the UI freezes
  while the simulation calculates.
- **Set `solver.BaseModel.Delay`** — without a delay a small simulation finishes before
  the window is even shown.
- **Update UI bound data on the UI thread.** `SimulationValue.PropertyChanged` is raised
  on the simulation thread, therefore use `Application.Current.Dispatcher` to modify
  chart values.
- The simulation component from the [getting started](getting-started.md) example is
  reused unchanged — visualization works without modifying the simulation.

A variant of this approach that passes the values through the Serilog logging pipeline
(with a sink component and a custom `ILogEventSink`) is available for download:
[Sine code example](./Files/Sine.zip). The video below shows the result — the sine
output and random values visualized live:

![type:video](./Videos/cropped.mp4)

## Option 2: Custom visualization components (Easy2Sim.Visualization)

For larger models, Easy2Sim provides a visualization application that renders one WPF
user control per simulation component on a canvas. The application plays back a
visualization log file or receives live values via MQTT (using the `MqttVisualizationSink`
from Easy2Sim.Persist).

!!! note
    The `Easy2Sim.Visualization` and `Easy2Sim.Persist` packages are currently
    distributed through the project's own NuGet feed — they are not published on
    nuget.org yet.

### Naming and matching conventions

The visualization application connects simulation data and user controls by **name**:

- For a simulation component with class name `Tank`, a user control named
  `TankVisualization` is created (class name + `Visualization`).
- For every line in the visualization log, the **dependency property whose name matches
  the `SimulationValue` property name** is set via reflection. If your simulation
  component has a `SimulationValue<double>` named `FillLevel`, the control needs a
  dependency property named `FillLevel`.
- The `Id` of a control is the simulation index of the component, so multiple instances
  of the same component type are distinguished.

### Canvas size: the VisualizationArea component

Add one `VisualizationArea` component (namespace `Easy2Sim_Visualization.Components`) to
the simulation. Its `Width` and `Height` parameters are sent once via
`VisualizationInitialize` and define the size of the visualization canvas
(default: 1000 x 1000):

```csharp
VisualizationArea area = new VisualizationArea(environment, solver);
environment.SetParameter(area, nameof(VisualizationArea.Width), 1200.0);
environment.SetParameter(area, nameof(VisualizationArea.Height), 800.0);
```

### Writing a visualization control

A visualization control is a WPF `UserControl` that inherits from `UserControlBase`
(namespace `Easy2Sim_Visualization.Components`). The base class requires the members
`Id`, `Easy2SimName`, `Left`, `Top`, `ControlWidth` and `ControlHeight` — implement them
as dependency properties. All visual state of the component is exposed as additional
dependency properties whose names match the simulation value names.

The following example shows a complete control for a `Tank` simulation component with a
`SimulationValue<double>` named `FillLevel`:

```xml title="TankVisualization.xaml"
<components:UserControlBase x:Class="MyVisualization.Components.TankVisualization"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:components="clr-namespace:Easy2Sim_Visualization.Components;assembly=Easy2Sim_Visualization"
             Width="{Binding ControlWidth}"
             Height="{Binding ControlHeight}">
    <Viewbox HorizontalAlignment="Stretch" VerticalAlignment="Stretch">
        <!-- Draw on a fixed-size canvas inside a Viewbox:
             the control scales automatically to any size -->
        <Canvas Width="100" Height="200">
            <!-- Tank outline -->
            <Rectangle Stroke="Black" StrokeThickness="2"
                       Canvas.Left="10" Canvas.Top="10" Width="80" Height="180"
                       Fill="LightGray" />
            <!-- Fill level bar, height and position follow the FillLevel value -->
            <Rectangle Canvas.Left="10" Width="80" Fill="DodgerBlue"
                       Canvas.Top="{Binding FillLevelTop}"
                       Height="{Binding FillLevel}" />
            <TextBlock Text="{Binding FillLevel, StringFormat={}{0:0}}" FontSize="30"
                       Canvas.Left="30" Canvas.Top="80" />
        </Canvas>
    </Viewbox>
</components:UserControlBase>
```

```csharp title="TankVisualization.xaml.cs"
using System.Windows;

namespace MyVisualization.Components;

public partial class TankVisualization
{
    public TankVisualization()
    {
        InitializeComponent();
        DataContext = this; // Bind the XAML directly to the dependency properties
    }

    // --- Members required by UserControlBase ---

    public static readonly DependencyProperty IdProperty =
        DependencyProperty.Register(nameof(Id), typeof(int), typeof(TankVisualization), new PropertyMetadata(0));
    public override int Id
    {
        get => (int)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public static readonly DependencyProperty Easy2SimNameProperty =
        DependencyProperty.Register(nameof(Easy2SimName), typeof(string), typeof(TankVisualization), new PropertyMetadata(""));
    public override string Easy2SimName
    {
        get => (string)GetValue(Easy2SimNameProperty);
        set => SetValue(Easy2SimNameProperty, value);
    }

    public static readonly DependencyProperty LeftProperty =
        DependencyProperty.Register(nameof(Left), typeof(double), typeof(TankVisualization), new PropertyMetadata(0.0));
    public override double Left
    {
        get => (double)GetValue(LeftProperty);
        set => SetValue(LeftProperty, value);
    }

    public static readonly DependencyProperty TopProperty =
        DependencyProperty.Register(nameof(Top), typeof(double), typeof(TankVisualization), new PropertyMetadata(0.0));
    public override double Top
    {
        get => (double)GetValue(TopProperty);
        set => SetValue(TopProperty, value);
    }

    public static readonly DependencyProperty ControlWidthProperty =
        DependencyProperty.Register(nameof(ControlWidth), typeof(double), typeof(TankVisualization), new PropertyMetadata(100.0));
    public override double ControlWidth
    {
        get => (double)GetValue(ControlWidthProperty);
        set => SetValue(ControlWidthProperty, value);
    }

    public static readonly DependencyProperty ControlHeightProperty =
        DependencyProperty.Register(nameof(ControlHeight), typeof(double), typeof(TankVisualization), new PropertyMetadata(200.0));
    public override double ControlHeight
    {
        get => (double)GetValue(ControlHeightProperty);
        set => SetValue(ControlHeightProperty, value);
    }

    // --- Visualization state, names match the SimulationValue names ---

    // Matches SimulationValue<double> FillLevel of the Tank component.
    // Set automatically by the visualization application for every logged value.
    public static readonly DependencyProperty FillLevelProperty =
        DependencyProperty.Register(nameof(FillLevel), typeof(double), typeof(TankVisualization),
            new PropertyMetadata(0.0, OnFillLevelChanged));
    public double FillLevel
    {
        get => (double)GetValue(FillLevelProperty);
        set => SetValue(FillLevelProperty, value);
    }

    // Derived value used to position the fill level bar (the tank is 180 units high).
    // Recalculated whenever FillLevel changes, so the binding updates automatically.
    public static readonly DependencyProperty FillLevelTopProperty =
        DependencyProperty.Register(nameof(FillLevelTop), typeof(double), typeof(TankVisualization), new PropertyMetadata(190.0));
    public double FillLevelTop
    {
        get => (double)GetValue(FillLevelTopProperty);
        private set => SetValue(FillLevelTopProperty, value);
    }

    private static void OnFillLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TankVisualization control)
            control.FillLevelTop = 190 - control.FillLevel * 1.8;
    }
}
```

### Tips for visualization controls

- Set `DataContext = this` in the constructor and bind the XAML directly to the
  dependency properties.
- Draw the control on a fixed-size `Canvas` inside a `Viewbox` — the control then scales
  automatically to `ControlWidth`/`ControlHeight`.
- Use property-changed callbacks (`PropertyMetadata` with a callback) or derived
  properties to translate values into visuals — e.g. map a fill level to a color or a
  boolean to a visibility.
- Position and size can also be driven by the simulation itself: let the simulation
  component implement `IVisualizationComponent` (`Easy2Sim.Interfaces`) and mark
  `Left`, `Top`, `ControlWidth` and `ControlHeight` with `VisualizationInitialize`.
