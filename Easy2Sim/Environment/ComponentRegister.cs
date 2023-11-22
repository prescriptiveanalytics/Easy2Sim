using Easy2Sim.Solvers;

namespace Easy2Sim.Environment
{
    /// <summary>
    /// This class holds a list of all SimulationEnvironments and Solvers.
    /// To avoid cyclic dependencies and allow serialization, children only know Ids of their parents, 
    /// e.g. a simulation component only holds a guid of the simulation environment.
    /// </summary>
    public static class ComponentRegister
    {
        /// <summary>
        /// Dictionary of all Simulation environments and solvers that currently exist.
        /// </summary>
        public static Dictionary<Guid, object> RegisteredComponents { get; set; }
        static ComponentRegister()
        {
            RegisteredComponents = new Dictionary<Guid, object>();
        }
        /// <summary>
        /// Get one specific Simulation Environment by its unique Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SimulationEnvironment? GetEnvironment(Guid id)
        {
            if (RegisteredComponents.TryGetValue(id, out object? component))
                return component as SimulationEnvironment;

            return null;
        }

        /// <summary>
        /// Get one specific Solver by its unique ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SolverBase? GetSolver(Guid id)
        {
            if (RegisteredComponents.TryGetValue(id, out object? component))
                return component as SolverBase;

            return null;
        }

        /// <summary>
        /// Add any component (solver, environment) to the component register
        /// </summary>
        public static void AddComponent(Guid componentGuid, object component)
        {
            RegisteredComponents.Add(componentGuid, component);
        }

        /// <summary>
        /// Remove any component (solver, environment) from the component register
        /// </summary>
        public static void RemoveComponent(Guid componentGuid)
        {
            RegisteredComponents.Remove(componentGuid);
        }

    }
}
