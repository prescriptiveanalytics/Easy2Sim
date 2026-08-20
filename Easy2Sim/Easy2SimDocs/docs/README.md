# Easy2Sim

## Description
Easy2Sim is an open source C# simulation framework developed by the [RISC Software GmbH](https://www.risc-software.at).
The goal of the framework is to allow fast development of simulation libraries and
a good connection to other programs. The framework supports [dynamic (continuous)](https://en.wikipedia.org/wiki/Continuous_simulation) and [discrete-event](https://en.wikipedia.org/wiki/Discrete-event_simulation) simulation. By default the framework runs deterministically.

A dynamic simulation describes a system that changes at every time step, e.g. water running down a river
or a temperature regulation for a room. In a discrete-event simulation, events happen that change the system,
e.g. a digital clock that changes the time every second.

The simulation framework has been built by the RISC Software GmbH in the [Secure Prescriptive Analytics project](https://www.prescriptiveanalytics.at/).

## Installation

Easy2Sim is available as a [NuGet package](https://www.nuget.org/packages/Easy2Sim) and targets .NET 8:

```powershell
dotnet add package Easy2Sim
```

## Next steps

- [Getting started](getting-started.md) — create and run your first simulation in a few minutes
- [Components and connections](Components.md) — learn how to build your own simulation components
- [Solvers](Solvers.md) — choose between the dynamic and the discrete-event solver
- [Visualization (WPF)](visualization.md) — live charts and custom visualization components
- [AI coding assistants](ai-assistants.md) — use Easy2Sim with AI coding assistants (llms.txt, AGENTS.md template)
