using SimuladorCompuertrtas.Core.Logic;
using SimuladorCompuertrtas.Core.Models;
using SimuladorCompuertrtas.Core.Services;

namespace SimuladorCompuertrtas.Core.Tests;

public sealed class CircuitSimulatorTests
{
    [Fact]
    public void Connects_and_propagates_a_simple_input_gate_output_circuit()
    {
        var circuit = new Circuit(); var left = new InputComponent("left", "Left", LogicState.High); var right = new InputComponent("right", "Right", LogicState.Low); var gate = new LogicGate("and", new AndGateDefinition()); var output = new OutputComponent("result", "Result");
        foreach (var component in new Component[] { left, right, gate, output }) circuit.AddComponent(component);
        circuit.Connect(left.Output, gate.InputPorts[0]); circuit.Connect(right.Output, gate.InputPorts[1]); circuit.Connect(gate.Output, output.Input);
        var result = new CircuitSimulator().Evaluate(circuit);
        Assert.Equal(LogicState.Low, result.GetOutput("result"));
        right.State = LogicState.High;
        Assert.Equal(LogicState.High, new CircuitSimulator().Evaluate(circuit).GetOutput("result"));
    }

    [Fact]
    public void Evaluates_combined_circuit_in_dependency_order()
    {
        var circuit = new Circuit(); var a = new InputComponent("a", "A", LogicState.High); var b = new InputComponent("b", "B", LogicState.Low); var or = new LogicGate("or", new OrGateDefinition()); var not = new LogicGate("not", new NotGateDefinition()); var output = new OutputComponent("out", "Out");
        foreach (var component in new Component[] { a, b, or, not, output }) circuit.AddComponent(component);
        circuit.Connect(a.Output, or.InputPorts[0]); circuit.Connect(b.Output, or.InputPorts[1]); circuit.Connect(or.Output, not.InputPorts[0]); circuit.Connect(not.Output, output.Input);
        Assert.Equal(LogicState.Low, new CircuitSimulator().Evaluate(circuit).GetOutput("out"));
    }

    [Fact]
    public void Rejects_duplicate_and_incompatible_connections()
    {
        var circuit = new Circuit(); var input = new InputComponent("in", "In"); var output = new OutputComponent("out", "Out"); circuit.AddComponent(input); circuit.AddComponent(output);
        circuit.Connect(input.Output, output.Input);
        Assert.Throws<InvalidOperationException>(() => circuit.Connect(input.Output, output.Input));
        Assert.Throws<InvalidOperationException>(() => circuit.Connect((Port)input.Output, input.Output));
    }

    [Fact]
    public void Creates_expected_component_ports_and_rejects_external_components()
    {
        var gate = new LogicGate("and", new AndGateDefinition(), 3);
        Assert.Equal(3, gate.InputPorts.Count);
        Assert.Single(gate.OutputPorts);
        var circuit = new Circuit(); var input = new InputComponent("in", "In"); var externalOutput = new OutputComponent("external", "External");
        circuit.AddComponent(input);
        Assert.Throws<InvalidOperationException>(() => circuit.Connect(input.Output, externalOutput.Input));
    }

    [Fact]
    public void Rejects_unconnected_inputs_and_feedback_cycles()
    {
        var incomplete = new Circuit(); var gate = new LogicGate("not", new NotGateDefinition()); incomplete.AddComponent(gate);
        Assert.Throws<CircuitSimulationException>(() => new CircuitSimulator().Evaluate(incomplete));
        var cyclic = new Circuit(); var first = new LogicGate("first", new NotGateDefinition()); var second = new LogicGate("second", new NotGateDefinition()); cyclic.AddComponent(first); cyclic.AddComponent(second); cyclic.Connect(first.Output, second.InputPorts[0]); cyclic.Connect(second.Output, first.InputPorts[0]);
        Assert.Throws<CircuitSimulationException>(() => new CircuitSimulator().Evaluate(cyclic));
    }

    [Fact]
    public void Removing_component_removes_attached_connections()
    {
        var circuit = new Circuit(); var input = new InputComponent("in", "In"); var output = new OutputComponent("out", "Out");
        circuit.AddComponent(input); circuit.AddComponent(output); circuit.Connect(input.Output, output.Input);
        Assert.True(circuit.RemoveComponent(input));
        Assert.Empty(circuit.Connections);
        Assert.Single(circuit.Components);
    }
}
