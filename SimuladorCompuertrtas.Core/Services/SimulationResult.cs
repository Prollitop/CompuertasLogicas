using SimuladorCompuertrtas.Core.Logic;

namespace SimuladorCompuertrtas.Core.Services;

public sealed class SimulationResult(IReadOnlyDictionary<string, LogicState> outputStates)
{
    public IReadOnlyDictionary<string, LogicState> OutputStates { get; } = outputStates;
    public LogicState GetOutput(string componentId) => OutputStates.TryGetValue(componentId, out var value) ? value : throw new KeyNotFoundException($"Output component '{componentId}' was not evaluated.");
}
