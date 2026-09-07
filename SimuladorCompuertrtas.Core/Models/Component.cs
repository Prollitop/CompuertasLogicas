namespace SimuladorCompuertrtas.Core.Models;

public abstract class Component(string id, string displayName)
{
    public string Id { get; } = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("A component id is required.", nameof(id)) : id;
    public string DisplayName { get; set; } = displayName;
    public ComponentPosition? Position { get; set; }
    public IList<InputPort> InputPorts { get; } = new List<InputPort>();
    public IList<OutputPort> OutputPorts { get; } = new List<OutputPort>();
}
