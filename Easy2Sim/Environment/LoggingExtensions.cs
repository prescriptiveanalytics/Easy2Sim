using Easy2Sim.Connect;
using System.Text;

namespace Easy2Sim.Environment
{
    public static class LoggingExtensions
    {

        public static void LogInformation(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            StringBuilder sb = new StringBuilder();
            long? simulationTime = component.Solver?.SimulationTime;
            if (simulationTime != null)
                sb.Append($"[{simulationTime}]");
            sb.Append($"[{component.LoggingName}]");
            sb.Append("[INFORMATION]: ");
            sb.Append(message);
            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Information(sb.ToString());
        }   

        public static void LogInformationNoFormatting(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Information(message);
        }

        public static void LogVisualizationInformation(this SimulationBase component, long timestamp, dynamic simulationValue)
        {
            if (component.SimulationEnvironment?.Model?.Easy2SimLogging?.VisualizationLogger == null)
            {
                return;
            }
            Type type = component.GetType();

            string message = $"{timestamp};{type.Name};{component.Index};{simulationValue.PropertyName};{simulationValue.Value}";
            if (component.VisualizationName != null)
                message += $";{component.VisualizationName.Value}";
            component.SimulationEnvironment.Model.Easy2SimLogging.VisualizationLogger.Information(message);
        }

        public static void LogNonSimulationValueVisualizationInformation(this SimulationBase component, long timestamp, string propertyName, string value)
        {
            if (component.SimulationEnvironment?.Model?.Easy2SimLogging?.VisualizationLogger == null)
            {
                return;
            }
            Type type = component.GetType();

            string message = $"{timestamp};{type.Name};{component.Index};{propertyName};{value}";
            if (component.VisualizationName != null)
                message += $";{component.VisualizationName.Value}";
            component.SimulationEnvironment.Model.Easy2SimLogging.VisualizationLogger.Information(message);
        }

        public static void LogError(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            StringBuilder sb = new StringBuilder();
            long? simulationTime = component.Solver?.SimulationTime;
            if (simulationTime != null)
                sb.Append($"[{simulationTime}]");
            sb.Append($"[{component.LoggingName}]");
            sb.Append("[ERROR]: ");
            sb.Append(message);
            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Error(sb.ToString());
        }
        public static void LogErrorNoFormatting(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Error(message);
        }

        public static void LogFatal(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            StringBuilder sb = new StringBuilder();
            long? simulationTime = component.Solver?.SimulationTime;
            if (simulationTime != null)
                sb.Append($"[{simulationTime}]");
            sb.Append($"[{component.LoggingName}]");
            sb.Append("[FATAL]: ");
            sb.Append(message);
            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Fatal(sb.ToString());
        }
        public static void LogFatalNoFormatting(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Fatal(message);
        }
        public static void LogDebug(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            StringBuilder sb = new StringBuilder();
            long? simulationTime = component.Solver?.SimulationTime;
            if (simulationTime != null)
                sb.Append($"[{simulationTime}]");
            sb.Append($"[{component.LoggingName}]");
            sb.Append("[DEBUG]: ");
            sb.Append(message);
            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Debug(sb.ToString());
        }
        public static void LogDebugNoFormatting(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Debug(message);
        }

        public static void LogWarning(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            StringBuilder sb = new StringBuilder();
            long? simulationTime = component.Solver?.SimulationTime;
            if (simulationTime != null)
                sb.Append($"[{simulationTime}]");
            sb.Append($"[{component.LoggingName}]");
            sb.Append("[WARNING]: ");
            sb.Append(message);
            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Warning(sb.ToString());
        }
        public static void LogWarningNoFormatting(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Warning(message);
        }
        public static void LogVerbose(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            StringBuilder sb = new StringBuilder();
            long? simulationTime = component.Solver?.SimulationTime;
            if (simulationTime != null)
                sb.Append($"[{simulationTime}]");
            sb.Append($"[{component.LoggingName}]");
            sb.Append("[VERBOSE]: ");
            sb.Append(message);
            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Verbose(sb.ToString());
        }
        public static void LogVerboseNoFormatting(this SimulationBase component, string message)
        {
            if (component.SimulationEnvironment == null)
                return;

            component.SimulationEnvironment.Model.Easy2SimLogging?.GeneralLogging?.Verbose(message);
        }


        public static void LogEnvironmentInfo(this SimulationEnvironment environment, string message)
        {
            environment.Model.Easy2SimLogging?.FrameworkDebuggingLogger?.Information(message);
        }
        public static void LogEnvironmentWarning(this SimulationEnvironment environment, string message)
        {
            environment.Model.Easy2SimLogging?.FrameworkDebuggingLogger?.Warning(message);
        }
        public static void LogEnvironmentVerbose(this SimulationEnvironment environment, string message)
        {
            environment.Model.Easy2SimLogging?.FrameworkDebuggingLogger?.Verbose(message);
        }


        public static void LogEnvironmentDebug(this SimulationEnvironment environment, string message)
        {
            environment.Model.Easy2SimLogging?.FrameworkDebuggingLogger?.Debug(message);
        }

        public static void LogEnvironmentFatal(this SimulationEnvironment environment, string message)
        {
            environment.Model.Easy2SimLogging?.FrameworkDebuggingLogger?.Fatal(message);
        }
        public static void LogEnvironmentError(this SimulationEnvironment environment, string message)
        {
            environment.Model.Easy2SimLogging?.FrameworkDebuggingLogger?.Error(message);
        }
    }
}
