# Getting started

This guide creates a complete, runnable simulation from scratch: a `Sine` component
that produces a sine wave, executed by the dynamic solver.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or newer

## 1. Create a new project

```powershell
dotnet new console -n MyFirstSim
cd MyFirstSim
```

## 2. Add the Easy2Sim package

```powershell
dotnet add package Easy2Sim
```

## 3. Add your first simulation component

Every simulation component inherits from `SimulationBase` and exposes its state as
`SimulationValue<T>` fields. Add a new file `Sine.cs`:

```csharp title="Sine.cs"
--8<-- "Easy2SimExamples/Sine.cs"
```

## 4. Run the simulation

Replace the content of `Program.cs`:

```csharp title="Program.cs"
--8<-- "Easy2SimExamples/Program.cs"
```

## 5. Execute

```powershell
dotnet run
```

Expected output:

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

## How a simulation runs

Every Easy2Sim simulation follows the same steps:

1. Implement your simulation components (inherit from `SimulationBase`)
2. Create a `SimulationEnvironment` and a solver
3. Create the components — they register themselves in the environment
4. Add connections between components (optional)
5. Call `solver.Initialize()`, then run with `solver.CalculateTo(maxTime)` or `solver.CalculateFinish()`

!!! note
    Decide which simulation type you need before you start: use a `DynamicSolver`
    when every component should be calculated once per time step (continuous
    simulation), or a `DiscreteSolver` when components should only run when events
    occur (discrete-event simulation). See [Solvers](Solvers.md) for details and a
    complete discrete-event example.

## Next steps

- [Components and connections](Components.md) — simulation values, execution order and connections in detail
- [Solvers](Solvers.md) — dynamic vs. discrete-event solver
- [Visualization (WPF)](visualization.md) — visualize the simulation live with charts or custom WPF components
