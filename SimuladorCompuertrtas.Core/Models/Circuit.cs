namespace SimuladorCompuertrtas.Core.Models;

public sealed class Circuit
{
    private readonly List<Component> _components = [];
    private readonly List<Connection> _connections = [];
    public IReadOnlyList<Component> Components => _components;
    public IReadOnlyList<Connection> Connections => _connections;

    public void AddComponent(Component component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (_components.Any(existing => existing.Id == component.Id)) throw new InvalidOperationException($"Component '{component.Id}' already exists.");
        _components.Add(component);
    }

    /// <summary>Removes a component and every logical connection attached to one of its ports.</summary>
    public bool RemoveComponent(Component component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (!_components.Remove(component)) return false;
        _connections.RemoveAll(connection => connection.Source.Component == component || connection.Target.Component == component);
        return true;
    }
    public Connection Connect(OutputPort source, InputPort target)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(target);
        if (!_components.Contains(source.Component) || !_components.Contains(target.Component)) throw new InvalidOperationException("Both port components must belong to the circuit.");
        if (_connections.Any(connection => connection.Source == source && connection.Target == target)) throw new InvalidOperationException("The connection already exists.");
        if (_connections.Any(connection => connection.Target == target)) throw new InvalidOperationException($"Input port '{target.Id}' is already connected.");
        var connection = new Connection(source, target); _connections.Add(connection); return connection;
    }
    public Connection Connect(Port source, Port target) => (source, target) switch
    {
        (OutputPort output, InputPort input) => Connect(output, input),
        _ => throw new InvalidOperationException("Connections must run from an output port to an input port.")
    };
    public bool RemoveConnection(Connection connection) => _connections.Remove(connection);
    public Connection GetConnectionTo(InputPort port) => _connections.SingleOrDefault(connection => connection.Target == port) ?? throw new KeyNotFoundException($"No connection targets port '{port.Id}'.");
}
