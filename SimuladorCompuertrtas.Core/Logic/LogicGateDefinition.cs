namespace SimuladorCompuertrtas.Core.Logic;

public abstract class LogicGateDefinition(string id, string displayName, int minimumInputCount, bool supportsVariableInputCount) : ILogicGateDefinition
{
    public string Id { get; } = id;
    public string DisplayName { get; } = displayName;
    public int MinimumInputCount { get; } = minimumInputCount;
    public bool SupportsVariableInputCount { get; } = supportsVariableInputCount;

    public LogicState Evaluate(IReadOnlyList<LogicState> inputs)
    {
        ValidateInputCount(inputs.Count);
        return EvaluateCore(inputs);
    }

    public IReadOnlyList<TruthTableRow> CreateTruthTable(int? inputCount = null)
    {
        var count = inputCount ?? MinimumInputCount;
        ValidateInputCount(count);
        var rows = new List<TruthTableRow>(1 << count);
        for (var value = 0; value < (1 << count); value++)
        {
            var inputs = Enumerable.Range(0, count)
                .Select(index => (value & (1 << (count - index - 1))) != 0 ? LogicState.High : LogicState.Low)
                .ToArray();
            rows.Add(new TruthTableRow(inputs, Evaluate(inputs)));
        }

        return rows;
    }

    protected abstract LogicState EvaluateCore(IReadOnlyList<LogicState> inputs);

    private void ValidateInputCount(int count)
    {
        var valid = SupportsVariableInputCount ? count >= MinimumInputCount : count == MinimumInputCount;
        if (!valid)
            throw new ArgumentException($"{DisplayName} requires {(SupportsVariableInputCount ? $"at least {MinimumInputCount}" : MinimumInputCount)} input(s); received {count}.", nameof(count));
    }
}
