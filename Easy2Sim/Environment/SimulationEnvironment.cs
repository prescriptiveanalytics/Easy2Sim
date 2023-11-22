using Serilog.Core;
using System.Reflection;
using Newtonsoft.Json;
using Easy2Sim.Connect.Attributes;
using Easy2Sim.Connect;
using Easy2Sim.Solvers;
using Easy2Sim.Solvers.Discrete;
using Parameter = Easy2Sim.Connect.Attributes.Parameter;
using Easy2Sim.Solvers.Dynamic;

namespace Easy2Sim.Environment
{
    public class SimulationEnvironment : IFrameworkBase, IDisposable
    {
        /// <summary>
        /// Represents all data that is necessary to run one simulation
        /// </summary>
        [JsonProperty]
        public EnvironmentModel Model { get; private set; }

        /// <summary>
        /// Unique Guid that can be used to uniquely identify class instances
        /// </summary>
        [JsonProperty]
        public Guid Guid { get; set; } = Guid.NewGuid();

        public SimulationEnvironment()
        {
            Model = new EnvironmentModel();
            ComponentRegister.AddComponent(Guid, this);
        }

        /// <summary>
        /// Add a simulations component to the simulation
        /// </summary>
        /// <param name="simulationBase"></param>
        public void AddComponent(SimulationBase simulationBase)
        {
            Model.SimulationObjects.Add(simulationBase.Index, simulationBase);
        }

        public void AddConnection(SimulationBase output, string outputName, SimulationBase input, string inputName)
        {
            FieldInfo? outputField = output.GetType().GetFields().FirstOrDefault(x => x.Name == outputName);
            FieldInfo? inputField = input.GetType().GetFields().FirstOrDefault(x => x.Name == inputName);

            PropertyInfo? outputInfo = output.GetType().GetProperties().FirstOrDefault(x => x.Name.Contains(outputName));
            PropertyInfo? inputInfo = input.GetType().GetProperties().FirstOrDefault(x => x.Name.Contains(inputName));

            if ((outputField == null) && (outputInfo == null))
            {
                LogError($"Connection ignored: {output.Name} => {input.Name} because output is null");
                return;
            }

            if ((inputField == null) && (inputInfo == null))
            {
                LogError($"Connection ignored: {output.Name} => {input.Name} because output is null");
                return;
            }


            Connection con = new Connection
            {
                EnvironmentGuid = Guid
            };

            if (outputField != null)
                if (outputField.GetCustomAttribute(typeof(Output)) is Output)
                    con.SourceType = outputField.FieldType.Name;


            if (outputInfo != null)
                if (outputInfo.GetCustomAttribute(typeof(Output)) is Output)
                    con.SourceType = outputInfo.PropertyType.Name;

            if (inputField != null)
                if (inputField.GetCustomAttribute(typeof(Input)) is Input)
                    con.TargetType = inputField.FieldType.Name;


            if (inputInfo != null)
                if (inputInfo.GetCustomAttribute(typeof(Input)) is Input)
                    con.TargetType = inputInfo.PropertyType.Name;

            con.SourceGuid = output.Guid;
            con.SourceName = outputName;
            con.TargetGuid = input.Guid;
            con.TargetName = inputName;
            Model.Connections.Add(con);
        }



        /// <summary>
        /// Sets the logger that is used in the environment.
        /// The default logger that is used logs to the console.
        /// </summary>
        /// <param name="logger"></param>
        public void SetLogConfiguration(Logger logger)
        {
            if (Model.Easy2SimLogging != null) 
                Model.Easy2SimLogging.Logger = logger;
        }

        public void UdpInformation(string message, string ipAddress = "127.0.0.1", int port = 12345)
        {
            if (Model.Easy2SimLogging != null) 
                Model.Easy2SimLogging.UdpInformation(message, ipAddress, port);
        }




        public void LogInformation(string message)
        {
            Model.Easy2SimLogging?.Logger?.Information(message);
        }
        public void LogDebug(string message)
        {
            Model.Easy2SimLogging?.Logger?.Debug(message);
        }
        public void LogError(string message)
        {
            Model.Easy2SimLogging?.Logger?.Error(message);
        }
        public void LogFatal(string message)
        {
            Model.Easy2SimLogging?.Logger?.Fatal(message);
        }


        /// <summary>
        /// Remove this component from the register once it is disposed
        /// </summary>
        public void Dispose()
        {
            ComponentRegister.RemoveComponent(Guid);
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
            foreach (KeyValuePair<int, SimulationBase> component in Model.SimulationObjects)
            {
                if (component.Value.Guid == target)
                {
                    return component.Value;
                }
            }
            return null;
        }
        /// <summary>
        /// Returns a simulation component based on the components name
        /// </summary>
        public SimulationBase? GetComponentByName(string name)
        {
            foreach (KeyValuePair<int, SimulationBase> component in Model.SimulationObjects)
            {
                if (component.Value.Name == name)
                {
                    return component.Value;
                }
            }
            return null;
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
                string solverJson = JsonConvert.SerializeObject(solver);

                if (solver is DiscreteSolver)
                    solverResult = JsonConvert.DeserializeObject<DiscreteSolver>(solverJson);
                if (solver is DynamicSolver)
                    solverResult = JsonConvert.DeserializeObject<DynamicSolver>(solverJson);
                if (solverResult == null)
                    throw new Exception("Solver could not be deserialized");

                solverResult.Guid = Guid.NewGuid();
                ComponentRegister.AddComponent(solverResult.Guid, solverResult);


                string environmentJson = environment.SerializeToJson();
                resultEnvironment = JsonConvert.DeserializeObject<SimulationEnvironment>(environmentJson);
                if (resultEnvironment == null)
                    throw new Exception("Environment could not be deserialized");
                resultEnvironment.Guid = Guid.NewGuid();
                ComponentRegister.AddComponent(resultEnvironment.Guid, resultEnvironment);

                solverResult.BaseModel.EnvironmentGuid = resultEnvironment.Guid;

                foreach (Connection con in resultEnvironment.Model.Connections)
                    con.EnvironmentGuid = resultEnvironment.Guid;

                foreach (SimulationBase simBase in resultEnvironment.Model.SimulationObjects.Values)
                {
                    simBase.SimulationEnvironmentGuid = resultEnvironment.Guid;
                    simBase.SolverGuid = solverResult.Guid;
                }
            }
            catch (Exception ex)
            {
                environment.LogError(ex.ToString());
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
        public void SetParameter(SimulationBase simulationBase, string parameterName, object encoding)
        {
            Type type = simulationBase.GetType();
            PropertyInfo? property = type.GetProperty(parameterName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                if (Attribute.GetCustomAttribute(property, typeof(Parameter)) is Parameter)
                    property.SetValue(simulationBase, encoding);
            }
            FieldInfo? field = type.GetField(parameterName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                if (Attribute.GetCustomAttribute(field, typeof(Parameter)) is Parameter)
                    field.SetValue(simulationBase, encoding);
            }
        }

        /// <summary>
        /// Serialize to json uses the default constructor.
        /// </summary>
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
