using System.Text.Json;
using SimuladorCompuertrtas.Core.Logic;
using SimuladorCompuertrtas.Core.Models;

namespace SimuladorCompuertrtas.Core.Services;

/// <summary>JSON persistence that keeps files independent from the WPF editor.</summary>
public sealed class CircuitDocumentService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly LogicGateCatalog _catalog;

    public CircuitDocumentService(LogicGateCatalog? catalog = null) => _catalog = catalog ?? new LogicGateCatalog();

    public string Save(Circuit circuit)
    {
        ArgumentNullException.ThrowIfNull(circuit);
        var document = new CircuitDocument(
            circuit.Components.Select(CreateComponent).ToList(),
            circuit.Connections.Select(connection => new ConnectionDocument(connection.Source.Component.Id, connection.Source.Id, connection.Target.Component.Id, connection.Target.Id)).ToList());
        return JsonSerializer.Serialize(document, Options);
    }

    public Circuit Load(string json)
    {
        try
        {
            var document = JsonSerializer.Deserialize<CircuitDocument>(json, Options) ?? throw new InvalidDataException("El archivo no contiene un circuito.");
            var circuit = new Circuit();
            foreach (var item in document.Components ?? []) circuit.AddComponent(CreateComponent(item));
            foreach (var item in document.Connections ?? [])
            {
                var source = circuit.Components.SingleOrDefault(component => component.Id == item.SourceComponentId)?.OutputPorts.SingleOrDefault(port => port.Id == item.SourcePortId);
                var target = circuit.Components.SingleOrDefault(component => component.Id == item.TargetComponentId)?.InputPorts.SingleOrDefault(port => port.Id == item.TargetPortId);
                if (source is null || target is null) throw new InvalidDataException("El archivo contiene una conexión con un puerto inexistente.");
                circuit.Connect(source, target);
            }
            return circuit;
        }
        catch (JsonException exception) { throw new InvalidDataException("El archivo no es un circuito JSON válido.", exception); }
        catch (InvalidOperationException exception) { throw new InvalidDataException("El circuito guardado contiene datos incompatibles.", exception); }
        catch (ArgumentException exception) { throw new InvalidDataException("El circuito guardado tiene una configuración de componente inválida.", exception); }
        catch (KeyNotFoundException exception) { throw new InvalidDataException("El circuito guardado usa una compuerta desconocida.", exception); }
    }

    private static ComponentDocument CreateComponent(Component component) => component switch
    {
        InputComponent input => new ComponentDocument(component.Id, "switch", component.DisplayName, component.Position?.X ?? 0, component.Position?.Y ?? 0, 0, input.State),
        OutputComponent => new ComponentDocument(component.Id, "led", component.DisplayName, component.Position?.X ?? 0, component.Position?.Y ?? 0, 0, LogicState.Low),
        LogicGate gate => new ComponentDocument(component.Id, gate.Definition.Id, component.DisplayName, component.Position?.X ?? 0, component.Position?.Y ?? 0, gate.InputPorts.Count, LogicState.Low),
        _ => throw new InvalidDataException($"El componente '{component.Id}' no se puede guardar.")
    };

    private Component CreateComponent(ComponentDocument item)
    {
        Component component = item.Type.ToLowerInvariant() switch
        {
            "switch" => new InputComponent(item.Id, item.DisplayName, item.InputState),
            "led" => new OutputComponent(item.Id, item.DisplayName),
            _ => new LogicGate(item.Id, _catalog.GetRequired(item.Type), item.InputCount, item.DisplayName)
        };
        component.Position = new ComponentPosition(item.X, item.Y);
        return component;
    }

    private sealed record CircuitDocument(List<ComponentDocument>? Components, List<ConnectionDocument>? Connections);
    private sealed record ComponentDocument(string Id, string Type, string DisplayName, double X, double Y, int InputCount, LogicState InputState);
    private sealed record ConnectionDocument(string SourceComponentId, string SourcePortId, string TargetComponentId, string TargetPortId);
}
