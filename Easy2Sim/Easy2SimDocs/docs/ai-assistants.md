# AI coding assistants

This documentation is optimized for consumption by AI coding assistants (GitHub Copilot,
OpenCode, Claude Code, Cursor, ...), so they can scaffold and extend Easy2Sim projects
quickly and correctly.

## llms.txt

This site provides machine-readable entry points following the [llms.txt convention](https://llmstxt.org):

- [llms.txt](https://www.prescriptiveanalytics.at/Easy2Sim/llms.txt) — a compact index of the documentation
- [llms-full.txt](https://www.prescriptiveanalytics.at/Easy2Sim/llms-full.txt) — the **complete documentation in a single file**, including all code examples

Point your assistant at `llms-full.txt` when you want it to learn Easy2Sim from scratch.

## AGENTS.md template

Many coding agents automatically read an `AGENTS.md` file in the project root.
Copy the following template into your Easy2Sim project:

```markdown title="AGENTS.md"
# Project rules

- C#/.NET 8 simulation project using the Easy2Sim framework (NuGet package: `Easy2Sim`)
- Documentation: https://www.prescriptiveanalytics.at/Easy2Sim/
  (single-file version for LLMs: https://www.prescriptiveanalytics.at/Easy2Sim/llms-full.txt)

## Conventions

- Simulation components inherit from `SimulationBase` (namespace `Easy2Sim.Environment`)
- Each component has a parameterless constructor (needed for serialization) and a
  constructor `(SimulationEnvironment environment, SolverBase solver)` that calls
  `: base(environment, solver)` — always use the latter to create components
- State is exposed via public `SimulationValue<T>` fields (namespace `Easy2Sim.Connect`)
  with `[JsonProperty]` and an attribute: `Input`, `Output` or `Parameter`
- Connect components with `environment.AddConnection(source.OutputValue, target.InputValue)`
- Use `DynamicSolver` for continuous simulations (override `DynamicCalculation()`),
  `DiscreteSolver` for event-based simulations (override `DiscreteCalculation()` and
  schedule events with `solver.AddEvent(...)` / `solver.AddEventAtTime(...)`)
- Run a simulation with `solver.Initialize(); solver.CalculateTo(maxTime);`

## Definition of done

- `dotnet build` succeeds without warnings
- `dotnet run` executes the simulation without exceptions
```

## Tips

- Give the assistant the [getting started example](getting-started.md) as a starting
  point — it is a complete, runnable program.
- All code examples in this documentation are compiled against the current Easy2Sim
  source code, so an assistant can safely copy them.
