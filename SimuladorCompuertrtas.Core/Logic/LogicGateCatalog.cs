namespace SimuladorCompuertrtas.Core.Logic;

/// <summary>Extensible registry of definitions shown in later UI phases.</summary>
public sealed class LogicGateCatalog
{
    private readonly Dictionary<string, ILogicGateDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public LogicGateCatalog(IEnumerable<ILogicGateDefinition>? definitions = null)
    {
        foreach (var definition in definitions ?? CreateStandardDefinitions()) Register(definition);
    }

    public IEnumerable<ILogicGateDefinition> Definitions => _definitions.Values;
    public void Register(ILogicGateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!_definitions.TryAdd(definition.Id, definition)) throw new InvalidOperationException($"A gate with id '{definition.Id}' is already registered.");
    }
    public ILogicGateDefinition GetRequired(string id) => _definitions.TryGetValue(id, out var definition) ? definition : throw new KeyNotFoundException($"Gate '{id}' is not registered.");
    public static IReadOnlyList<ILogicGateDefinition> CreateStandardDefinitions() => [new AndGateDefinition(), new OrGateDefinition(), new NotGateDefinition(), new NandGateDefinition(), new NorGateDefinition(), new XorGateDefinition(), new XnorGateDefinition()];
}
