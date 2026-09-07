namespace SimuladorCompuertrtas.Core.Models;

/// <summary>Logical link from an output port to an input port; contains no rendering data.</summary>
public sealed record Connection(OutputPort Source, InputPort Target);
