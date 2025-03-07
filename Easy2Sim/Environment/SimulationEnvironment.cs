using System.Reflection;
using Newtonsoft.Json;
using Easy2Sim.Connect;
using Easy2Sim.Solvers;
using Easy2Sim.Solvers.Discrete;
using Easy2Sim.Solvers.Dynamic;
using Easy2Sim.Interfaces;
using Serilog.Core;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using DotNetGraph.Compilation;

namespace Easy2Sim.Environment;

public class SimulationEnvironment : IFrameworkBase, IDisposable
{
    /// <summary>
    /// Represents all data that is necessary to run one simulation
    /// </summary>
    [JsonProperty("model")]
    public EnvironmentModel Model { get; private set; }


    [JsonIgnore]
    private Guid _guid;

    /// <summary>
    /// Unique Guid that can be used to uniquely identify class instances
    /// </summary>
    [JsonIgnore]
    public Guid Guid
    {
        get => _guid;
        set
        {
            _guid = value;
        }
    }

    [JsonConstructor]
    public SimulationEnvironment(EnvironmentModel model)
    {
        Model = model;
        _guid = Guid.NewGuid();
        ComponentRegister.AddEnvironment(Guid, this);

        //Set simulation base to correct environment guid
        foreach (SimulationBase simBase in Model.SimulationObjects.Values)
            simBase.SimulationEnvironmentGuid = Guid;

        //Set connection environment guid
        foreach (IConnection connection in Model.Connections)
            connection.EnvironmentGuid = Guid;

        //Set simulation value environment guid
        foreach (SimulationBase simulationBase in Model.SimulationObjects.Values)
        {

            //Set property guids
            foreach (PropertyInfo info in simulationBase.GetType().GetProperties())
                if (IsSimulationValue(info.PropertyType))
                    SetGuidForSimulationValue(info.GetValue(simulationBase), Guid);

            //Set field guids
            foreach (FieldInfo fInfo in simulationBase.GetType().GetFields())
                if (IsSimulationValue(fInfo.FieldType))
                    SetGuidForSimulationValue(fInfo.GetValue(simulationBase), Guid);
        }

        RecheckConnections();
    }

    private static bool IsSimulationValue(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SimulationValue<>);
    }


    private static void SetGuidForSimulationValue(dynamic simValue, Guid guid)
    {
        simValue.EnvironmentGuid = guid;
    }

    public SimulationEnvironment()
    {
        Guid = Guid.NewGuid();
        Model = new EnvironmentModel();
        ComponentRegister.AddEnvironment(Guid, this);
    }


    /// <summary>
    /// Add a simulations component to the simulation
    /// </summary>
    /// <param name="simulationBase"></param>
    public void AddComponent(SimulationBase simulationBase)
    {
        Model.SimulationObjects.Add(simulationBase.Index, simulationBase);
        if (simulationBase.SimulationEnvironmentGuid == Guid.Empty)
            this.LogEnvironmentWarning($"Component {simulationBase.Easy2SimName} with index {simulationBase.Index} has no environment set");

        if (simulationBase.SolverGuid == Guid.Empty)
            this.LogEnvironmentWarning($"Component {simulationBase.Easy2SimName} with index {simulationBase.Index} has no solver set");

        this.LogEnvironmentInfo($"Added component to the environment: {simulationBase.Easy2SimName} with index {simulationBase.Index}");
    }

    /// <summary>
    /// Add a new connection between to components in the environment.
    /// <code>
    /// SimulationEnvironment environment = new SimulationEnvironment();
    /// DiscreteSolver solver = new DiscreteSolver(environment);
    /// 
    /// Component1 comp1 = new Component1(environment, solver);
    /// Component2 comp2 = new Component2(environment, solver);
    /// 
    /// environment.AddConnection(comp1, "Output", comp2, "Input");
    /// </code>
    public void AddConnection<T>(SimulationValue<T> source, SimulationValue<T> target, bool isComponentConnection = false)
    {
        Connection<T> connection = new Connection<T>(source, target, this.Guid, isComponentConnection);

        Model.Connections.Add(connection);
        this.LogEnvironmentInfo($"Added connection:{connection.ToString()}");
    }

    public void AddConnection<T>(SimulationValue<T> source, string targetParent, string targetProperty,
        bool isComponentConnection = false)
    {
        Connection<T> connection = new Connection<T>(source.ParentName, source.PropertyName, targetParent, targetProperty, this.Guid, isComponentConnection);

        Model.Connections.Add(connection);
        this.LogEnvironmentInfo($"Added connection: {connection.ToString()}");

    }


    public void AddFeedbackConnection<T, T1>(FeedbackSimulationValue<T, T1> source, FeedbackSimulationValue<T, T1> target,
                                             bool isComponentConnection = false)
    {
        FeedbackConnection<T, T1> connection = new FeedbackConnection<T, T1>(source, target, this.Guid, isComponentConnection);

        Model.Connections.Add(connection);
        this.LogEnvironmentInfo($"Added feedback connection: {connection.ToString()}");

    }

    public void AddComponentConnection(SimulationBase simulationBase1, SimulationBase simulationBase2, Logger? logger = null)
    {
        try
        {
            if (logger != null)
                logger.Error($"Start creating component connection");

            //Get In"Variable" properties
            List<dynamic> sim1In = simulationBase1.GetSimulationValues(SimulationValueAttributes.Input);
            List<dynamic> sim2In = simulationBase2.GetSimulationValues(SimulationValueAttributes.Input);
           
            //Get Out"Variable" properties
            List<dynamic> sim1Out = simulationBase1.GetSimulationValues(SimulationValueAttributes.Output);
            List<dynamic> sim2Out = simulationBase2.GetSimulationValues(SimulationValueAttributes.Output);

            if (logger != null)
                logger.Error($"{simulationBase1.Easy2SimName}: {sim1In.Count} in, {sim1Out.Count} out. {simulationBase2.Easy2SimName}: {sim2In.Count} in, {sim2Out.Count} out");
            else
                this.LogEnvironmentInfo($"{simulationBase1.Easy2SimName}: {sim1In.Count} in, {sim1Out.Count} out. {simulationBase2.Easy2SimName}: {sim2In.Count} in, {sim2Out.Count} out");


            foreach (dynamic sim1Output in sim1Out)
            {
                foreach (dynamic sim2Input in sim2In)
                {
                    if (sim1Output.PropertyName == sim2Input.PropertyName ||
                        sim1Output.PropertyName.Replace("Out", "") == sim2Input.PropertyName.Replace("In", ""))
                    {
                        if (logger != null)
                            logger.Error($"New connection: {sim1Output.ToString()} - {sim2Input.ToString()}");

                        string targetParent = sim2Input.ParentName;
                        string targetProperty = sim2Input.PropertyName;
                        Type T = sim1Output.GenericType;
                        AddConnection(sim1Output,
                            targetParent, targetProperty,
                            true);
                    }
                }
            }
            foreach (dynamic sim2Output in sim2Out)
            {
                foreach (dynamic sim1Input in sim1In)
                {
                    if (sim2Output.PropertyName == sim1Input.PropertyName ||
                        sim2Output.PropertyName.Replace("Out", "") == sim1Input.PropertyName.Replace("In", ""))
                    {
                        if (logger != null)
                            logger.Error($"New connection: {sim2Output.ToString()} - {sim1Input.ToString()}");

                        string targetParent = sim1Input.ParentName;
                        string targetProperty = sim1Input.PropertyName;

                        AddConnection(sim2Output, targetParent, targetProperty, true);
                    }
                }
            }


            //Get In"Variable" properties
            List<dynamic> sim1InFeedback = simulationBase1.GetFeedbackSimulationValues(SimulationValueAttributes.Input);
            List<dynamic> sim2InFeedback = simulationBase2.GetFeedbackSimulationValues(SimulationValueAttributes.Input);

            //Get Out"Variable" properties
            List<dynamic> sim1OutFeedback = simulationBase1.GetFeedbackSimulationValues(SimulationValueAttributes.Output);
            List<dynamic> sim2OutFeedback = simulationBase2.GetFeedbackSimulationValues(SimulationValueAttributes.Output);

            foreach (dynamic sim1Output in sim1OutFeedback)
            {
                foreach (dynamic sim2Input in sim2InFeedback)
                {
                    if (sim1Output.PropertyName == sim2Input.PropertyName ||
                        sim1Output.PropertyName.Replace("Out", "") == sim2Input.PropertyName.Replace("In", ""))
                    {
                        if (logger != null)
                            logger.Error($"New feedback connection: {sim1Output.ToString()} - {sim2Input.ToString()}");

                        AddFeedbackConnection(sim1Output, sim2Input, true);
                    }
                }
            }
            foreach (dynamic sim2Output in sim2OutFeedback)
            {
                foreach (dynamic sim1Input in sim1InFeedback)
                {
                    if (sim2Output.PropertyName == sim1Input.PropertyName ||
                        sim2Output.PropertyName.Replace("Out", "") == sim1Input.PropertyName.Replace("In", ""))
                    {
                        if (logger != null)
                            logger.Error($"New feedback connection: {sim2Output.ToString()} - {sim1Input.ToString()}");

                        AddFeedbackConnection(sim2Output, sim1Input, true);
                    }
                }
            }
            if (logger != null)
                logger.Information($"Added feedback connection to the model: {simulationBase1.Easy2SimName} - {simulationBase2.Easy2SimName}");

            this.Model.ComponentConnections.Add((simulationBase1.Easy2SimName, simulationBase2.Easy2SimName));


        }
        catch (Exception ex)
        {
            this.LogEnvironmentError($"Can not establish component connection from {simulationBase1.Easy2SimName} to {simulationBase2.Easy2SimName}");
            this.LogEnvironmentError(ex.ToString());
        }


    }


    /// <summary>
    /// Remove this component from the register once it is disposed
    /// </summary>
    public void Dispose()
    {
        ComponentRegister.RemoveEnvironment(Guid);
    }



    /// <summary>
    /// It is either necessary to specify the solver when creating a simulation 
    /// component or call this method once before starting a simulation
    /// </summary>
    /// <param name="solver"></param>
    public void SetSolverForComponents(SolverBase solver)
    {
        foreach (SimulationBase item in Model.SimulationObjects.Values)
            item.SolverGuid = solver.Guid;

    }

    /// <summary>
    /// Returns a component based on its guid
    /// </summary>
    public SimulationBase? GetComponentByGuid(Guid target)
    {
        foreach (SimulationBase simBase in Model.SimulationObjects.Values)
        {
            if (simBase.Guid == target)
                return simBase;
        }
        return null;
    }
    /// <summary>
    /// Returns a simulation component based on the components name
    /// </summary>
    public SimulationBase? GetComponentByName(string name)
    {
        foreach (SimulationBase simBase in Model.SimulationObjects.Values)
        {
            if (simBase.Easy2SimName == name)
                return simBase;
        }
        return null;
    }

    /// <summary>
    /// Returns a list containing all components based on the given name
    /// </summary>
    public List<SimulationBase> GetComponentsContainingName(string name)
    {
        List<SimulationBase> components = new();
        foreach (KeyValuePair<int, SimulationBase> component in Model.SimulationObjects)
        {
            if (component.Value.Easy2SimName.Contains(name))
            {
                components.Add(component.Value);
            }
        }
        return components;
    }

    /// <summary>
    /// Duplicate a solver and environment pair, set references of the components 
    /// </summary>
    public static (SimulationEnvironment, SolverBase) Duplicate(SimulationEnvironment environment, SolverBase solver)
    {
        SolverBase? solverResult = null;
        SimulationEnvironment? resultEnvironment = null;

        try
        {
            string solverJson = "";

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            if (solver is DiscreteSolver discSolver)
                solverJson = JsonConvert.SerializeObject(discSolver, settings);
            else if (solver is DynamicSolver dynamicSolver)
                solverJson = JsonConvert.SerializeObject(dynamicSolver, settings);
            else
                solverJson = JsonConvert.SerializeObject(solver, settings);


            // ============ NOTE ==============
            // Deserializing uses a own constructor that registeres the solver and environment to the  component register
            // This uses a new Guid

            if (solver is DiscreteSolver)
                solverResult = JsonConvert.DeserializeObject<DiscreteSolver>(solverJson, settings);
            if (solver is DynamicSolver)
                solverResult = JsonConvert.DeserializeObject<DynamicSolver>(solverJson, settings);
            if (solverResult == null)
                throw new Exception("Solver could not be deserialized");

            string environmentJson = (environment as IFrameworkBase).SerializeToJson();
            resultEnvironment = JsonConvert.DeserializeObject<SimulationEnvironment>(environmentJson, settings);
            if (resultEnvironment == null)
                throw new Exception("Environment could not be deserialized");

            solverResult.BaseModel.EnvironmentGuid = resultEnvironment.Guid;

            foreach (dynamic con in resultEnvironment.Model.Connections)
                con.EnvironmentGuid = resultEnvironment.Guid;

            foreach (SimulationBase simBase in resultEnvironment.Model.SimulationObjects.Values)
            {
                simBase.SimulationEnvironmentGuid = resultEnvironment.Guid;
                simBase.SolverGuid = solverResult.Guid;
            }
        }
        catch (Exception ex)
        {
            environment.Model.Easy2SimLogging.FrameworkDebuggingLogger.Error(ex.ToString());
        }

        if (solverResult != null && resultEnvironment != null)
        {
            return (resultEnvironment, solverResult);
        }

        throw new Exception("Either solver or environment not initialized");
    }

    /// <summary>
    /// Set the parameter of a simulation base in the environment
    /// </summary>
    public void SetParameter(SimulationBase simulationBase, string parameterName, object parameterValue, Logger? logger  = null)
    {
        Type type = simulationBase.GetType();
        PropertyInfo? property = type.GetProperty(parameterName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
        {
            dynamic simulationValue = property.GetValue(simulationBase);
            if (simulationValue.Attributes.Contains(SimulationValueAttributes.Parameter))
            {
                simulationValue.SetParameter(parameterValue);
                logger?.Information($"Set parameter {simulationBase}-{parameterName} to {parameterValue}");
            }
        }
        FieldInfo? field = type.GetField(parameterName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            dynamic simulationValue = field.GetValue(simulationBase);
            if (simulationValue.Attributes.Contains(SimulationValueAttributes.Parameter))
            {
                simulationValue.SetParameter(parameterValue);
                logger?.Information($"Set parameter {simulationBase}-{parameterName} to {parameterValue}");
            }
        }
    }

    private void RecheckConnections()
    {
        foreach (IConnection connection in Model.Connections)
        {
            connection.Reapply();
        }

    }

    async public void CreateGraphFile(string path)
    {
        var graph = new DotGraph().WithIdentifier("Simulation graph");
        foreach (KeyValuePair<int, SimulationBase> pair in Model.SimulationObjects)
        {
            var myNode = new DotNode()
                .WithIdentifier(pair.Value.Easy2SimName)
                .WithShape(DotNodeShape.Ellipse)
                .WithLabel(pair.Value.Easy2SimName)
                .WithFillColor(DotColor.GhostWhite)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotNodeStyle.Filled)
                .WithWidth(0.5)
                .WithHeight(0.5)
                .WithPenWidth(1.5);

            // Add the node to the graph
            graph.Add(myNode);
        }

        foreach (IConnection connection in Model.Connections)
        {
            var myEdge = new DotEdge().From(connection.SourceObject.Easy2SimName)
                .To(connection.TargetObject.Easy2SimName);
            graph.Add(myEdge);
        }

        await using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions());
        await graph.CompileAsync(context);

        var result = writer.GetStringBuilder().ToString();

        // Save it to a file
        File.WriteAllText(path, result);

    }

    public void ClearConnections()
    {
        Model.Connections.Clear();
    }

    public List<dynamic> SimulationParameters
    {
        get
        {
            List<dynamic> result = new List<dynamic>();

            //Iterate all simulation objects
            foreach (SimulationBase simulationBase in Model.SimulationObjects.Values)
            {
                Type type = simulationBase.GetType();
                //Check properties
                foreach (PropertyInfo propertyInfo in type.GetProperties())
                    if (IsSimulationValue(propertyInfo.PropertyType))
                    {
                        dynamic simulationValue = propertyInfo.GetValue(simulationBase);
                        if (simulationValue == null)
                            continue;
                        if (simulationValue.Attributes.Contains(SimulationValueAttributes.Parameter))
                        {
                            result.Add(simulationValue);
                        }
                    }

                //Check fields
                foreach (FieldInfo fieldInfo in type.GetFields())
                    if (IsSimulationValue(fieldInfo.FieldType))
                    {
                        dynamic simulationValue = fieldInfo.GetValue(simulationBase); 
                        if (simulationValue == null)
                            continue;
                        if (simulationValue.Attributes.Contains(SimulationValueAttributes.Parameter))
                            result.Add(simulationValue);
                    }
            }

            return result;
        }
    }

    public void LogVisualizationParameters(long simulationTime)
    {
        if (Model.Easy2SimLogging?.VisualizationLogger == null)
            return;

        try
        {
            foreach (SimulationBase simulationBase in Model.SimulationObjects.Values)
            {

                Type type = simulationBase.GetType();

                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    if (IsSimulationValue(propertyInfo.PropertyType))
                    {
                        dynamic simulationValue = propertyInfo.GetValue(simulationBase);
                        if (simulationValue == null)
                            continue;
                        if (simulationValue.Attributes.Contains(SimulationValueAttributes.Visualization))
                        {
                            LoggingExtensions.LogVisualizationInformation(simulationBase, simulationTime, simulationValue);
                        }

                        if (simulationValue.Attributes.Contains(SimulationValueAttributes.VisualizationOnChange) &&
                            simulationValue.ValueChanged)
                        {
                            LoggingExtensions.LogVisualizationInformation(simulationBase, simulationTime, simulationValue);
                        }
                    }
                }

                foreach (FieldInfo fieldInfo in type.GetFields())
                {
                    if (IsSimulationValue(fieldInfo.FieldType))
                    {
                        dynamic simulationValue = fieldInfo.GetValue(simulationBase);
                        if (simulationValue == null)
                            continue;
                        if (simulationValue.Attributes.Contains(SimulationValueAttributes.Visualization))
                        {

                            LoggingExtensions.LogVisualizationInformation(simulationBase, simulationTime, simulationValue);
                        }

                        if (simulationValue.Attributes.Contains(SimulationValueAttributes.VisualizationOnChange) &&
                            simulationValue.ValueChanged)
                        {
                            LoggingExtensions.LogVisualizationInformation(simulationBase, simulationTime, simulationValue);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Model.Easy2SimLogging.VisualizationLogger.Error("Can not log visualization parameters");
            Model.Easy2SimLogging.VisualizationLogger.Error(ex.ToString());
        }
    }

    public void LogVisualizationInitializeParameters(long simulationTime)
    {
        if (Model.Easy2SimLogging?.VisualizationLogger == null)
            return;
        try
        {
            foreach (SimulationBase simulationBase in Model.SimulationObjects.Values)
            {

                Type type = simulationBase.GetType();

                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    if (IsSimulationValue(propertyInfo.PropertyType))
                    {
                        dynamic simulationValue = propertyInfo.GetValue(simulationBase);
                        if (simulationValue.Attributes.Contains(SimulationValueAttributes.VisualizationInitialize))
                        {
                            LoggingExtensions.LogVisualizationInformation(simulationBase, simulationTime, simulationValue);
                        }
                    }
                }

                foreach (FieldInfo fieldInfo in type.GetFields())
                {
                    if (IsSimulationValue(fieldInfo.FieldType))
                    {
                        dynamic simulationValue = fieldInfo.GetValue(simulationBase);
                        if (simulationValue.Attributes.Contains(SimulationValueAttributes.VisualizationInitialize))
                        {
                            LoggingExtensions.LogVisualizationInformation(simulationBase, simulationTime, simulationValue);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Model.Easy2SimLogging.VisualizationLogger.Error("Can not log visualization initialize parameters");
            Model.Easy2SimLogging.VisualizationLogger.Error(ex.ToString());
        }
    }
}
