using Easy2Sim.Solvers;

namespace Easy2Sim.Environment;

/// <summary>
/// This class holds a list of all SimulationEnvironments and Solvers.
/// To avoid cyclic dependencies and allow serialization, children only know Ids of their parents, 
/// e.g. a simulation component only holds a guid of the simulation environment.
/// </summary>
public static class ComponentRegister
{
    public static object LockEnvironments { get; set; }
    public static object LockSolvers { get; set; }
    /// <summary>
    /// Dictionary of all Simulation environments and solvers that currently exist.
    /// </summary>
    public static Dictionary<Guid, SimulationEnvironment> Environments { get; set; }
    public static Dictionary<Guid, SolverBase> Solvers { get; set; }
    static ComponentRegister()
    {
        LockSolvers = new object();
        LockEnvironments = new object();
        Environments = new Dictionary<Guid, SimulationEnvironment>();
        Solvers = new Dictionary<Guid, SolverBase>();
    }
    /// <summary>
    /// Get one specific Simulation Environment by its unique guid <paramref name="id"/>
    /// </summary>
    /// <param name="id">
    /// </param>
    /// <returns></returns>
    public static SimulationEnvironment? GetEnvironment(Guid id)
    {
        lock (LockEnvironments)
        {
            if (Environments.TryGetValue(id, out SimulationEnvironment? environment))
                return environment;
        }

        return null;
    }

    /// <summary>
    /// Get one specific Solver by its unique guid <paramref name="id"/>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static SolverBase? GetSolver(Guid id)
    {
        lock (LockSolvers)
        {
            if (Solvers.TryGetValue(id, out SolverBase? solver))
                return solver;
        }

        return null;
    }

    /// <summary>
    /// Add any component (solver, environment) to the component register
    /// </summary>
    /// <param name="componentGuid">Guid of the component which should be added</param>
    /// <param name="environment">Environment which is added</param>
    public static void AddEnvironment(Guid componentGuid, SimulationEnvironment environment)
    {
        lock (LockEnvironments)
        {
            Environments.TryAdd(componentGuid, environment);
        }
    }

    /// <summary>
    /// Add any component (solver, environment) to the component register
    /// </summary>
    /// <param name="componentGuid">Guid of the component which should be added</param>
    /// <param name="solver">Solver which should be added</param>
    public static void AddSolver(Guid componentGuid, SolverBase solver)
    {
        lock (LockSolvers)
        {
            Solvers.TryAdd(componentGuid, solver);
        }
    }

    /// <summary>
    /// Remove any component (solver, environment) from the component register
    /// </summary>
    /// <param name="componentGuid"> <c>Guid</c> of the component which should be removed</param>
    public static void RemoveEnvironment(Guid componentGuid)
    {
        lock (LockEnvironments)
        {
            Environments.Remove(componentGuid);
        }
    }
    /// <summary>
    /// Remove any component (solver, environment) from the component register
    /// </summary>
    /// <param name="componentGuid"> <c>Guid</c> of the component which should be removed</param>
    public static void RemoveSolver(Guid componentGuid)
    {
        lock (LockSolvers)
        {
            Solvers.Remove(componentGuid);
        }
    }

}
