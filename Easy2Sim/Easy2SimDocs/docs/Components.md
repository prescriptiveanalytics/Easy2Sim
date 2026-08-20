# Components and connections

## What is a simulation component?
A simulation component describes a logical part of a system that should be modeled.
An example would be a model of a flow shop. This model can have simulation components for:

  - Machines 
  - Vehicles
  - Operators

Each simulation component has attributes that describe its current state.
A machine can for example have:

  - Unique Id: Unique identifier of the machine
  - Processing Time: Time it takes to do one task at the machine
  - Setup time: Time it takes to swap the tool of the machine
  - Energy consumption: How much energy is consumed while idle/producing?
  - Buffer Size: Size of the buffer for raw material that is used at this machine

## The SimulationBase class

Every simulation component inherits from the base class `SimulationBase`
(namespace `Easy2Sim.Environment`). A component needs two constructors:

- a **parameterless constructor**, which is required for serialization
- a constructor `(SimulationEnvironment environment, SolverBase solver)` that calls
  `: base(environment, solver)` and registers the component in the environment.
  **Always use this constructor when you create components.**

Once instantiated, the environment automatically assigns a simulation index based on the
order of instantiation. This index defines the execution order of the components within
one time step. Typically the simulation index starts at 0 and is increased by one per
instantiated component. It can be changed with `SimulationBase.SetIndexManually(int index)` —
even negative values are allowed. Make sure that all indexes stay unique!

```csharp
SimulationEnvironment environment = new SimulationEnvironment();
DiscreteSolver solver = new DiscreteSolver(environment);

Sine sine1 = new Sine(environment, solver); // (1)
Sine sine2 = new Sine(environment, solver); // (2)

sine1.SetIndexManually(3); // (3)
```

1. `sine1` gets the simulation index 0
2. `sine2` gets the simulation index 1
3. `sine1` now has the simulation index 3

### Lifecycle methods

Components can override the following methods:

| Method | Called when |
|--------|-------------|
| `Initialize()` | Once before the simulation starts (via `solver.Initialize()`). Use it for expensive setup, e.g. file access. |
| `DynamicCalculation()` | Once per simulation time step, when a `DynamicSolver` is used. |
| `DiscreteCalculation()` | Whenever the `DiscreteSolver` processes an event for this component. |
| `PostCalculation()` | To process feedback within the same simulation time step. |
| `End()` | Once after the simulation has finished. |

## Simulation values

The state of a component is exposed via public fields or properties of type
`SimulationValue<T>` (namespace `Easy2Sim.Connect`), each decorated with `[JsonProperty]`
(Newtonsoft.Json) and created with one or more `SimulationValueAttributes`:

| Attribute | Meaning |
|-----------|---------|
| `Input` | The component receives information through this value. |
| `Output` | The component publishes information through this value; it can be connected to an `Input` of another component. |
| `Parameter` | A simulation parameter that can be set from outside, e.g. from Excel. |
| `Visualization` | Updated multiple times during a simulation run (for visualization/logging). |
| `VisualizationOnChange` | Logged for visualization whenever the value changes. |
| `VisualizationInitialize` | Pushed once to the visualization during initialization. |

## A complete simulation component

The following `Sine` component is part of the compilable example project that ships with
this documentation, so it always matches the current API:

```csharp title="Sine.cs"
--8<-- "Easy2SimExamples/Sine.cs"
```

1. Base class of every simulation component
2. The parameterless constructor is needed for serialization
3. `Output` defines that this value can be connected to an `Input` of another component
4. Use this constructor when you create components, it registers the component in the environment
5. `DynamicCalculation` is called once per time step by the dynamic solver

## What is a connection in the simulation?

A connection describes an information flow between components in the simulation.
E.g. a machine could inform a vehicle that it needs more material for further production or
a machine finishes a production and informs the next machine.

Another example is a component `InputParser` that parses sensor data.
The parsed results can be provided to other components via a connection.

### Creating connections

Connections are created between a source `SimulationValue<T>` (an `Output`) and a target
`SimulationValue<T>` (an `Input`) of the same type:

```csharp
environment.AddConnection(clock.Tick, printer.TickIn);
```

Whenever the source value changes, the new value is copied to the target value. If a
`DiscreteSolver` is used, an event for the target component is additionally added at the
current simulation time — the target component reacts automatically to every change.

Alternatively, `environment.AddComponentConnection(component1, component2)` automatically
connects all values whose names match (e.g. `Tick` → `Tick`, or `TickOut` → `TickIn`).

A complete, runnable example with two connected components can be found on the
[Solvers](Solvers.md#discrete-solver) page.
