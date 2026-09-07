using SimuladorCompuertrtas.Core.Logic;

namespace SimuladorCompuertrtas.Core.Models;

public sealed class LogicGate : Component
{
    public LogicGate(string id, ILogicGateDefinition definition, int? inputCount = null, string? displayName = null) : base(id, displayName ?? definition?.DisplayName ?? throw new ArgumentNullException(nameof(definition)))
    {
        Definition = definition;
        var count = inputCount ?? definition.MinimumInputCount;
        if (!definition.SupportsVariableInputCount && count != definition.MinimumInputCount || definition.SupportsVariableInputCount && count < definition.MinimumInputCount)
            throw new ArgumentException($"Invalid input count for {definition.DisplayName}.", nameof(inputCount));
        for (var index = 0; index < count; index++) InputPorts.Add(new InputPort($"{id}.in{index}", this, $"Input {index + 1}"));
        Output = new OutputPort($"{id}.out", this, "Output");
        OutputPorts.Add(Output);
    }
    public ILogicGateDefinition Definition { get; }
    public OutputPort Output { get; }
}
