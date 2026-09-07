namespace SimuladorCompuertrtas.Core.Logic;

public sealed record TruthTableRow(IReadOnlyList<LogicState> Inputs, LogicState Output);
