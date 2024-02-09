using System.Reflection;

namespace Easy2Sim.Connect.Attributes
{
    /// <summary>
    /// Attribute which is used to define that a variable is used as output variable in the simulation
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class Output : AttributeBase
    {
    }
}
