namespace SimuladorCompuertrtas.Core.Logic;

/// <summary>WPF-independent contract implemented by every gate type.</summary>
public interface ILogicGateDefinition
{
    string Id { get; }
    string DisplayName { get; }
    int MinimumInputCount { get; }
    bool SupportsVariableInputCount { get; }
    LogicState Evaluate(IReadOnlyList<LogicState> inputs);
    IReadOnlyList<TruthTableRow> CreateTruthTable(int? inputCount = null);
}
