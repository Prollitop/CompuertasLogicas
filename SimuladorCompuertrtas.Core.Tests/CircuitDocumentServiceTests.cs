using SimuladorCompuertrtas.Core.Logic;
using SimuladorCompuertrtas.Core.Models;
using SimuladorCompuertrtas.Core.Services;

namespace SimuladorCompuertrtas.Core.Tests;

public sealed class CircuitDocumentServiceTests
{
    [Fact]
    public void Saves_and_loads_components_positions_states_and_connections()
    {
        var circuit = new Circuit(); var input = new InputComponent("a", "A", LogicState.High) { Position = new ComponentPosition(10, 20) }; var gate = new LogicGate("and", new AndGateDefinition()) { Position = new ComponentPosition(40, 50) }; var output = new OutputComponent("led", "LED");
        foreach (var component in new Component[] { input, gate, output }) circuit.AddComponent(component);
        circuit.Connect(input.Output, gate.InputPorts[0]);
        var json = new CircuitDocumentService().Save(circuit);
        var loaded = new CircuitDocumentService().Load(json);
        Assert.Equal(3, loaded.Components.Count); Assert.Single(loaded.Connections);
        var loadedInput = Assert.IsType<InputComponent>(loaded.Components.Single(component => component.Id == "a"));
        Assert.Equal(LogicState.High, loadedInput.State); Assert.Equal(new ComponentPosition(10, 20), loadedInput.Position);
    }

    [Fact]
    public void Rejects_invalid_document() => Assert.Throws<InvalidDataException>(() => new CircuitDocumentService().Load("not json"));
}
