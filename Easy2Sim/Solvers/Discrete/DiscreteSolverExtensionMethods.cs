using Easy2Sim.Connect;
using Easy2Sim.Environment;
using Serilog.Core;
using System.Reflection;

namespace Easy2Sim.Solvers.Discrete
{
    static class DiscreteSolverExtensionMethods
    {
        public static Dictionary<Type, List<PropertyInfo>> VisualizationProperties { get; set; } = new Dictionary<Type, List<PropertyInfo>>();
        public static Dictionary<Type, List<PropertyInfo>> VisualizationInitializeProperties { get; set; } = new Dictionary<Type, List<PropertyInfo>>();
        public static Dictionary<Type, List<PropertyInfo>> VisualizationOnChangeProperties { get; set; } = new Dictionary<Type, List<PropertyInfo>>();
        public static Dictionary<Type, List<FieldInfo>> VisualizationFields { get; set; } = new Dictionary<Type, List<FieldInfo>>();
        public static Dictionary<Type, List<FieldInfo>> VisualizationInitializeFields { get; set; } = new Dictionary<Type, List<FieldInfo>>();
        public static Dictionary<Type, List<FieldInfo>> VisualizationOnChangeFields { get; set; } = new Dictionary<Type, List<FieldInfo>>();


        /// <summary>
        /// Check and log visualization and visualizationOnChange parameters in the discrete solver
        /// </summary>
        public static void LogVisualizationParameters(this SimulationBase simulationBase, long discreteEventTimeStamp)
        {
            if (simulationBase?.SimulationEnvironment?.Model?.Easy2SimLogging?.VisualizationLogger == null)
                return;

            try
            {
                Type type = simulationBase.GetType();

                if (!VisualizationProperties.ContainsKey(type))
                {
                    VisualizationProperties.Add(type, new List<PropertyInfo>());
                    foreach (PropertyInfo propertyInfo in type.GetProperties())
                    {
                        if (propertyInfo.PropertyType.IsSimulationValue())
                        {
                            dynamic simulationValue = propertyInfo.GetValue(simulationBase);
                            if (simulationValue == null)
                                continue;

                            if (simulationValue.Attributes.Contains(SimulationValueAttributes.Visualization))
                                VisualizationProperties[type].Add(propertyInfo);
                        }
                    }
                }


                foreach (PropertyInfo info in VisualizationProperties[type])
                {
                    dynamic simulationValue = info.GetValue(simulationBase);
                    if (simulationValue == null)
                        continue;

                    LoggingExtensions.LogVisualizationInformation(simulationBase, discreteEventTimeStamp, simulationValue);
                }


                if (!VisualizationOnChangeProperties.ContainsKey(type))
                {
                    VisualizationOnChangeProperties.Add(type, new List<PropertyInfo>());
                    foreach (PropertyInfo propertyInfo in type.GetProperties())
                    {
                        if (propertyInfo.PropertyType.IsSimulationValue())
                        {
                            dynamic simulationValue = propertyInfo.GetValue(simulationBase);
                            if (simulationValue == null)
                                continue;

                            if (simulationValue.Attributes.Contains(SimulationValueAttributes
                                    .VisualizationOnChange))
                                VisualizationOnChangeProperties[type].Add(propertyInfo);
                        }
                    }
                }


                foreach (PropertyInfo info in VisualizationOnChangeProperties[type])
                {
                    dynamic simulationValue = info.GetValue(simulationBase);
                    if (simulationValue == null)
                        continue;
                    if (simulationValue.ValueChanged)
                        LoggingExtensions.LogVisualizationInformation(simulationBase, discreteEventTimeStamp, simulationValue);
                }



                if (!VisualizationFields.ContainsKey(type))
                {
                    VisualizationFields.Add(type, new List<FieldInfo>());
                    foreach (FieldInfo fieldInfo in type.GetFields())
                    {
                        if (fieldInfo.FieldType.IsSimulationValue())
                        {
                            dynamic simulationValue = fieldInfo.GetValue(simulationBase);
                            if (simulationValue == null)
                                continue;
                            if (simulationValue.Attributes.Contains(SimulationValueAttributes.Visualization))
                            {
                                VisualizationFields[type].Add(fieldInfo);
                            }
                        }
                    }
                }


                foreach (FieldInfo info in VisualizationFields[type])
                {
                    dynamic simulationValue = info.GetValue(simulationBase);
                    if (simulationValue == null)
                        continue;

                    LoggingExtensions.LogVisualizationInformation(simulationBase, discreteEventTimeStamp, simulationValue);
                }


                if (!VisualizationOnChangeFields.ContainsKey(type))
                {
                    VisualizationOnChangeFields.Add(type, new List<FieldInfo>());

                    foreach (FieldInfo fieldInfo in type.GetFields())
                    {
                        if (fieldInfo.FieldType.IsSimulationValue())
                        {
                            dynamic simulationValue = fieldInfo.GetValue(simulationBase);
                            if (simulationValue == null)
                                continue;
                            if (simulationValue.Attributes.Contains(SimulationValueAttributes.VisualizationOnChange))
                            {
                                VisualizationOnChangeFields[type].Add(fieldInfo);
                            }
                        }
                    }
                }


                foreach (FieldInfo info in VisualizationOnChangeFields[type])
                {
                    dynamic simulationValue = info.GetValue(simulationBase);
                    if (simulationValue == null)
                        continue;
                    if (simulationValue.ValueChanged)
                        LoggingExtensions.LogVisualizationInformation(simulationBase, discreteEventTimeStamp,
                            simulationValue);
                }

            }
            catch (Exception ex)
            {
                simulationBase.LogError("Can not log visualization parameters");
                simulationBase.LogError(ex.ToString());
            }
        }


        /// <summary>
        /// Check and log visualizationInitialize parameters in the discrete solver
        /// </summary>
        public static void LogVisualizationInitializeParameters(this SimulationBase simulationBase, long discreteEventTimeStamp)
        {
            if (simulationBase?.SimulationEnvironment?.Model?.Easy2SimLogging?.VisualizationLogger == null)
                return;

            try
            {
                Type type = simulationBase.GetType();

                if (!VisualizationInitializeProperties.ContainsKey(type))
                {
                    VisualizationInitializeProperties.Add(type, new List<PropertyInfo>());
                    foreach (PropertyInfo propertyInfo in type.GetProperties())
                    {
                        if (propertyInfo.PropertyType.IsSimulationValue())
                        {
                            dynamic simulationValue = propertyInfo.GetValue(simulationBase);
                            if (simulationValue == null)
                                continue;
                            if (simulationValue.Attributes.Contains(SimulationValueAttributes.VisualizationInitialize))
                            {
                                VisualizationInitializeProperties[type].Add(propertyInfo);
                            }
                        }
                    }
                }

                if (!VisualizationInitializeFields.ContainsKey(type))
                {
                    VisualizationInitializeFields.Add(type, new List<FieldInfo>());
                    foreach (FieldInfo fieldInfo in type.GetFields())
                    {
                        if (fieldInfo.FieldType.IsSimulationValue())
                        {
                            dynamic simulationValue = fieldInfo.GetValue(simulationBase);
                            if (simulationValue == null)
                                continue;
                            if (simulationValue.Attributes.Contains(SimulationValueAttributes.VisualizationInitialize))
                            {
                                VisualizationInitializeFields[type].Add(fieldInfo);
                            }
                        }
                    }
                }

                foreach (PropertyInfo info in VisualizationInitializeProperties[type])
                {
                    dynamic simulationValue = info.GetValue(simulationBase);
                    if (simulationValue == null)
                        continue;

                    LoggingExtensions.LogVisualizationInformation(simulationBase, discreteEventTimeStamp, simulationValue);
                }

                foreach (FieldInfo info in VisualizationInitializeFields[type])
                {
                    dynamic simulationValue = info.GetValue(simulationBase);
                    if (simulationValue == null)
                        continue;

                    LoggingExtensions.LogVisualizationInformation(simulationBase, discreteEventTimeStamp, simulationValue);
                }
            }
            catch (Exception ex)
            {
                simulationBase.LogError("Can not log visualization initialize parameters");
                simulationBase.LogError(ex.ToString());
            }
        }


        public static void ResetValueChanged(this SimulationBase simulationBase)
        {
            if (simulationBase.SimulationEnvironment == null)
                return;
            try
            {
                Type type = simulationBase.GetType();

                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    if (propertyInfo.PropertyType.IsSimulationValue())
                    {
                        dynamic simulationValue = propertyInfo.GetValue(simulationBase);
                        if (simulationValue == null)
                            continue;
                        simulationValue.ValueChanged = false;
                    }
                }

                foreach (FieldInfo fieldInfo in type.GetFields())
                {
                    if (fieldInfo.FieldType.IsSimulationValue())
                    {
                        dynamic simulationValue = fieldInfo.GetValue(simulationBase);
                        if (simulationValue == null)
                            continue;
                        simulationValue.ValueChanged = false;
                    }
                }
            }
            catch (Exception ex)
            {
                simulationBase.LogError("Can not reset the value changed");
                simulationBase.LogError(ex.ToString());
            }
        }

        public static void AddEventInNextTimeStep(this SimulationBase simulationBase)
        {
            if (simulationBase?.Solver?.AsDiscreteSolver != null)
            {
                simulationBase.Solver.AsDiscreteSolver.AddEventAtTime(simulationBase, simulationBase.Solver.SimulationTime + 1);
            }
            else if (simulationBase != null)
            {
                simulationBase.LogError("Can not add event, solver null");
            }
        }
    }
}
