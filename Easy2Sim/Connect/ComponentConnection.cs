using Easy2Sim.Interfaces;

namespace Easy2Sim.Connect;

/// <summary>
/// Specifies two names of components.
/// This is used to make all possible connections between these components
/// </summary>
public struct ComponentConnection(string component1, string component2) : ICloneable<ComponentConnection>
{
    public string Component1 { get; set; } = component1;
    public string Component2 { get; set; } = component2;

    public ComponentConnection Clone()
    {
        return new ComponentConnection(Component1, Component2);
    }

}