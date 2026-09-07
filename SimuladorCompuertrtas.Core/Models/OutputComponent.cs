namespace SimuladorCompuertrtas.Core.Models;

public sealed class OutputComponent : Component
{
    public OutputComponent(string id, string displayName) : base(id, displayName)
    {
        Input = new InputPort($"{id}.in", this, "Input");
        InputPorts.Add(Input);
    }
    public InputPort Input { get; }
}
