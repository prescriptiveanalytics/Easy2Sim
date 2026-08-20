# Solvers

The solver controls *when* components are calculated. Easy2Sim provides two main solvers:

- **DynamicSolver** (namespace `Easy2Sim.Solvers.Dynamic`): every component's
  `DynamicCalculation()` is executed once per simulation time step. Use it for
  dynamic (continuous) simulations.
- **DiscreteSolver** (namespace `Easy2Sim.Solvers.Discrete`): a component's
  `DiscreteCalculation()` is only executed when an event for it is processed.
  Use it for discrete-event simulations.

Both solvers share the same lifecycle:

```csharp
solver.Initialize();              // calls Initialize() on every component, once
solver.CalculateTo(100);          // run until simulation time 100
// or
solver.CalculateFinish();         // run until the simulation is finished
```

## Dynamic solver

In a dynamic calculation, each component's `DynamicCalculation()` method is executed
once per time step, in the order of the components' simulation indexes.

The following complete program is the getting started example — it is compiled and run
as part of the documentation example project:

```csharp title="Program.cs"
--8<-- "Easy2SimExamples/Program.cs"
```

Output:

```text
t=  0  output=  0.0000
t= 10  output=  0.5878
t= 20  output=  0.9511
t= 30  output=  0.9511
t= 40  output=  0.5878
t= 50  output=  0.0000
t= 60  output= -0.5878
t= 70  output= -0.9511
t= 80  output= -0.9511
t= 90  output= -0.5878
t=100  output= -0.0000
```

## Discrete solver

With the discrete solver, a component is only calculated when an **event** for it exists
in the event list. Each event points to a simulation time; the solver always processes
the event with the lowest simulation time next. `CalculateFinish()` stops when no events
are left or a component finishes the simulation; `CalculateTo(maxTime)` additionally stops
when the next event lies beyond `maxTime`.

The following complete example defines a `Clock` component that schedules a tick every
10 time units and a `Printer` component that is triggered automatically through a
connection:

```csharp title="DiscreteExample.cs"
--8<-- "Easy2SimExamples/DiscreteExample.cs"
```

Call it from your program entry point:

```csharp
Easy2SimExamples.DiscreteExample.Run();
```

Output:

```text
t=  0  printer received tick 0
t= 10  printer received tick 10
t= 20  printer received tick 20
t= 30  printer received tick 30
t= 40  printer received tick 40
t= 50  printer received tick 50
```

### Ways to add events

1. **`DiscreteSolver.AddEvent(SimulationBase simulationBase)`**

    Adds an event for the component at the current simulation time.

2. **`DiscreteSolver.AddEventAtTime(SimulationBase simulationBase, long simulationTime)`**

    Adds an event for the component at a specific simulation time.

3. **Connection changed**

    If two components are connected and the source value changes, an event for the
    connected target component is automatically added at the current simulation time.
    This is how the `Printer` in the example above is triggered — no events are
    scheduled for it manually.

4. **`DiscreteSolver.AddEventForAllComponents()` / `AddEventForAllComponentsAtTime(long time)`**

    Adds an event for every component in the environment.
