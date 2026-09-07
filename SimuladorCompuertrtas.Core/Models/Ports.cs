using SimuladorCompuertrtas.Core.Logic;

namespace SimuladorCompuertrtas.Core.Models;

public abstract class Port(string id, Component component, string displayName)
{
    public string Id { get; } = id;
    public Component Component { get; } = component;
    public string DisplayName { get; } = displayName;
}

public sealed class InputPort(string id, Component component, string displayName) : Port(id, component, displayName)
{
    public LogicState State { get; internal set; }
}

public sealed class OutputPort(string id, Component component, string displayName) : Port(id, component, displayName)
{
    public LogicState State { get; internal set; }
}
