using Easy2Sim.Connect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Easy2Sim.Environment
{
    internal static class SimulationBaseExtensions
    {
        /// <summary>
        /// Check if a type is a SimulationValue<>
        /// </summary>
        public static bool IsSimulationValue(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SimulationValue<>);
        }
        public static bool IsFeedbackSimulationValue(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FeedbackSimulationValue<,>);
        }

        internal static List<dynamic> GetSimulationValues(this SimulationBase simbase, SimulationValueAttributes attribute)
        {
            List<dynamic> simulationValues = new List<dynamic>();
            Type type = simbase.GetType();
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.PropertyType.IsSimulationValue())
                {
                    dynamic simulationValue = propertyInfo.GetValue(simbase);
                    if (simulationValue == null)
                        continue;
                    if (simulationValue.Attributes.Contains(attribute))
                    {
                        simulationValues.Add(simulationValue);
                    }
                }
            }
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (fieldInfo.FieldType.IsSimulationValue())
                {
                    dynamic simulationValue = fieldInfo.GetValue(simbase);
                    if (simulationValue == null)
                        continue;
                    if (simulationValue.Attributes.Contains(attribute))
                    {
                        simulationValues.Add(simulationValue);
                    }
                }
            }

            return simulationValues;
        }
        internal static List<dynamic> GetFeedbackSimulationValues(this SimulationBase simbase, SimulationValueAttributes attribute)
        {
            List<dynamic> simulationValues = new List<dynamic>();
            Type type = simbase.GetType();
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.PropertyType.IsFeedbackSimulationValue())
                {
                    dynamic simulationValue = propertyInfo.GetValue(simbase);
                    if (simulationValue == null)
                        continue;
                    if (simulationValue.Attributes.Contains(attribute))
                    {
                        simulationValues.Add(simulationValue);
                    }
                }
            }
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (fieldInfo.FieldType.IsFeedbackSimulationValue())
                {
                    dynamic simulationValue = fieldInfo.GetValue(simbase);
                    if (simulationValue == null)
                        continue;
                    if (simulationValue.Attributes.Contains(attribute))
                    {
                        simulationValues.Add(simulationValue);
                    }
                }
            }

            return simulationValues;
        }
    }
}
