using Easy2Sim.Connect;
using Easy2Sim.Interfaces;
using Easy2Sim.Solvers;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Easy2Sim.Solvers.Discrete;

namespace Easy2Sim.Environment;

/// <summary>
/// Base class for simulation components.
/// </summary>
public abstract class SimulationBase : IFrameworkBase
{
    [JsonProperty]
    public Guid Guid { get; internal set; } = Guid.NewGuid();

    private SimulationEnvironment? _environment;
    [JsonIgnore]
    public SimulationEnvironment? SimulationEnvironment
    {
        get => _environment;
        set
        {
            _environment = value;
            Type type = this.GetType();
            //Check properties
            foreach (PropertyInfo propertyInfo in type.GetProperties())
                if (propertyInfo.PropertyType.IsSimulationValue())
                {
                    dynamic? simulationValue = propertyInfo.GetValue(this);
                    if (simulationValue == null)
                        continue;
                    if (SimulationEnvironment != null)
                        simulationValue.SimulationEnvironment = SimulationEnvironment;
                }

            //Check fields
            foreach (FieldInfo fieldInfo in type.GetFields())
                if (fieldInfo.FieldType.IsSimulationValue())
                {
                    dynamic? simulationValue = fieldInfo.GetValue(this);
                    if (simulationValue == null)
                        continue;
                    if (SimulationEnvironment != null)
                        simulationValue.SimulationEnvironment = SimulationEnvironment;
                }
        }
    }

    [JsonProperty]
    private int _simulationIndex;

    /// <summary>
    /// Unique name that is generated for each component
    /// </summary>
    [JsonProperty]
    public string Easy2SimName { get; private set; }

    /// <summary>
    /// VisualizationName
    /// </summary>
    [JsonProperty]
    public SimulationValue<string> VisualizationName { get; set; }

    /// <summary>
    /// Index in the simulation.
    /// In each time stamp in the simulation, components are called in order of the simulation index.
    /// </summary>
    [JsonProperty]
    public int Index => _simulationIndex;


    /// <summary>
    /// Set the simulation  <paramref name="index"/> of a component manually.
    /// Make sure that all indexes are correct!
    /// </summary>
    /// <param name="index">
    /// The index is typically automatically generated based on creation order of the components.
    /// This parameter allows to overwrite it.
    /// </param>
    public void SetIndexManually(int index)
    {
        _simulationIndex = index;
    }

    /// <summary>
    /// Set the simulation <paramref name="name"/> of a component manually.
    /// Make sure that all names are unique and correct!
    /// </summary>
    /// <param name="name">
    /// A unique name for each simulation component is generated automatically.
    /// This parameter allows to change it.  
    /// </param>
    public void SetNameManually(string name)
    {
        Easy2SimName = name;
        UpdateSimulationValueParents();
    }

    private void UpdateSimulationValueParents()
    {
        try
        {
            Type type = this.GetType();

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.PropertyType.IsSimulationValue())
                {
                    dynamic? simulationValue = propertyInfo.GetValue(this);
                    if (simulationValue == null)
                        continue;
                    simulationValue.ParentName = Easy2SimName;
                }
            }

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (fieldInfo.FieldType.IsSimulationValue())
                {
                    dynamic? simulationValue = fieldInfo.GetValue(this);
                    if (simulationValue == null)
                        continue;
                    simulationValue.ParentName = Easy2SimName;
                }
            }
        }
        catch (Exception ex)
        {
            this.LogError("Error while updating simulation values");
            this.LogError(ex.ToString());
        }
    }




    /// <summary>
    /// Solver where the component is registered.
    /// </summary>
    [JsonIgnore]
    public SolverBase? Solver { get; set; }

    public string LoggingName =>
        !string.IsNullOrEmpty(VisualizationName.Value) ? VisualizationName.Value : Easy2SimName;


    /// <summary>
    /// Automatically set the simulation index based on the current index in the environment.
    /// This method is called, when a new component is created.
    /// </summary>
    private void SetSimulationIndex()
    {
        if (SimulationEnvironment == null) return;
        _simulationIndex = SimulationEnvironment.Model.SimulationIndex;
        SimulationEnvironment.Model.SimulationIndex = _simulationIndex + 1;
    }

    /// <summary>
    /// Do not use this constructor, this is only used for serialization and deserialization
    /// </summary>
    [JsonConstructor]
    protected SimulationBase()
    {
        Easy2SimName = "";
        VisualizationName = new SimulationValue<string>(string.Empty, nameof(VisualizationName), this,
            SimulationValueAttributes.Parameter);
        _simulationIndex = -1;

    }

    /// <summary>
    /// If a environment or environment guid is given the component is added to the environments components.
    /// The solver is necessary for the simulation.
    /// </summary>
    protected SimulationBase(SimulationEnvironment environment, SolverBase solver)
    {
        Solver = solver;
        SimulationEnvironment = environment;
        SetSimulationIndex();
        Guid = Guid.NewGuid();
        Type t = GetType();
        Easy2SimName = t.Name + Index;
        VisualizationName = new SimulationValue<string>(string.Empty, nameof(VisualizationName), this,
            SimulationValueAttributes.Parameter);
        SimulationEnvironment?.AddComponent(this);
    }
    /// <summary>
    /// If a environment or environment guid is given the component is added to the environments components
    /// </summary>
    protected SimulationBase(SimulationEnvironment environment)
    {
        SimulationEnvironment = environment;
        SetSimulationIndex();
        Guid = Guid.NewGuid();
        Type t = GetType();
        Easy2SimName = t.Name + Index;
        VisualizationName = new SimulationValue<string>(string.Empty, nameof(VisualizationName), this,
            SimulationValueAttributes.Parameter);
        SimulationEnvironment?.AddComponent(this);
    }


    /// <summary>
    /// Returns all simulation bases for a given output parameter name
    /// </summary>
    /// <param name="output">Name of the output field or property</param>
    /// <returns></returns>
    public List<SimulationBase> GetConnectedInputComponents()
    {
        if (SimulationEnvironment == null)
            return new List<SimulationBase>();

        List<SimulationBase> resultList = new List<SimulationBase>();
        List<SimulationBase> connectedComponents =
        SimulationEnvironment.Model.Connections
            .Where(x => x.SourceObject == this)
            .Select(x => x.TargetObject).Cast<SimulationBase>().ToList();
        resultList.AddRange(connectedComponents);
        return resultList;
    }

    /// <summary>
    /// Returns all simulation bases for a given input parameter name
    /// </summary>
    /// <param name="input">Name of the input field or property</param>
    /// <returns></returns>
    public List<SimulationBase> GetConnectedOutputComponents()
    {
        if (SimulationEnvironment == null)
            return new List<SimulationBase>();

        List<SimulationBase> resultList = new List<SimulationBase>();
        List<SimulationBase?> connectedComponents = SimulationEnvironment.Model.Connections
            .Where(x => x.TargetObject == this)
            .Select(x => x.SourceObject).ToList();
        foreach (SimulationBase? connectedComponent in connectedComponents)
        {
            if (connectedComponent == null) continue;
            resultList.Add(connectedComponent);
        }
        return resultList;
    }


    /// <summary>
    /// Initialize is called once before the simulation starts.
    /// Typical cpu expensive actions are done here: e.g. file access to initialize a component
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// This method is called before the first iteration of the simulation. 
    /// </summary>
    public virtual void StartSimulation() { }

    /// <summary>
    /// This method is called every time a event for this component is executed
    /// </summary>
    public virtual void DiscreteCalculation() { }

    /// <summary>
    /// Called at every iteration of a dynamic calculation
    /// </summary>
    public virtual void DynamicCalculation() { }
    /// <summary>
    /// Can be used to process Feedback in the same simulation time stamp
    /// </summary>
    public virtual void PostCalculation() { }

    /// <summary>
    /// This method is called at the end of the simulation
    /// When calculate finish ends
    /// </summary>
    public virtual void End() { }


    public void CheckCorrectInitialization([NotNull] SimulationEnvironment? simulationEnvironment, [NotNull] SolverBase? solver)
    {
        ArgumentNullException.ThrowIfNull(simulationEnvironment, "simulationEnvironment");
        ArgumentNullException.ThrowIfNull(solver, "solver");
    }
    public void CheckCorrectInitialization([NotNull] SimulationEnvironment? simulationEnvironment, [NotNull] SolverBase? solver, [NotNull]string? name)
    {
        ArgumentNullException.ThrowIfNull(simulationEnvironment, "simulationEnvironment");
        ArgumentNullException.ThrowIfNull(solver, "solver");
        ArgumentNullException.ThrowIfNull(name, "name");
    }
}