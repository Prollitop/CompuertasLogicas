namespace SimuladorCompuertrtas.Core.Logic;

public sealed class AndGateDefinition() : LogicGateDefinition("and", "AND", 2, true)
{ protected override LogicState EvaluateCore(IReadOnlyList<LogicState> inputs) => inputs.All(value => value == LogicState.High) ? LogicState.High : LogicState.Low; }
public sealed class OrGateDefinition() : LogicGateDefinition("or", "OR", 2, true)
{ protected override LogicState EvaluateCore(IReadOnlyList<LogicState> inputs) => inputs.Any(value => value == LogicState.High) ? LogicState.High : LogicState.Low; }
public sealed class NotGateDefinition() : LogicGateDefinition("not", "NOT", 1, false)
{ protected override LogicState EvaluateCore(IReadOnlyList<LogicState> inputs) => inputs[0] == LogicState.High ? LogicState.Low : LogicState.High; }
public sealed class NandGateDefinition() : LogicGateDefinition("nand", "NAND", 2, true)
{ protected override LogicState EvaluateCore(IReadOnlyList<LogicState> inputs) => inputs.All(value => value == LogicState.High) ? LogicState.Low : LogicState.High; }
public sealed class NorGateDefinition() : LogicGateDefinition("nor", "NOR", 2, true)
{ protected override LogicState EvaluateCore(IReadOnlyList<LogicState> inputs) => inputs.Any(value => value == LogicState.High) ? LogicState.Low : LogicState.High; }
public sealed class XorGateDefinition() : LogicGateDefinition("xor", "XOR", 2, true)
{ protected override LogicState EvaluateCore(IReadOnlyList<LogicState> inputs) => inputs.Count(value => value == LogicState.High) % 2 == 1 ? LogicState.High : LogicState.Low; }
public sealed class XnorGateDefinition() : LogicGateDefinition("xnor", "XNOR", 2, true)
{ protected override LogicState EvaluateCore(IReadOnlyList<LogicState> inputs) => inputs.Count(value => value == LogicState.High) % 2 == 1 ? LogicState.Low : LogicState.High; }
