using SimuladorCompuertrtas.Core.Logic;
using SimuladorCompuertrtas.Core.Models;

namespace SimuladorCompuertrtas.Core.Services;

/// <summary>Stateless combinational evaluator. Feedback cycles are deliberately rejected in Phase 1.</summary>
public sealed class CircuitSimulator
{
    public SimulationResult Evaluate(Circuit circuit)
    {
        ArgumentNullException.ThrowIfNull(circuit);
        var visiting = new HashSet<Component>();
        var evaluated = new HashSet<Component>();
        foreach (var component in circuit.Components) EvaluateComponent(circuit, component, visiting, evaluated);
        return new SimulationResult(circuit.Components.OfType<OutputComponent>().ToDictionary(output => output.Id, output => output.Input.State));
    }

    private static void EvaluateComponent(Circuit circuit, Component component, ISet<Component> visiting, ISet<Component> evaluated)
    {
        if (evaluated.Contains(component)) return;
        if (!visiting.Add(component)) throw new CircuitSimulationException($"Unsupported feedback cycle detected at component '{component.Id}'.");
        switch (component)
        {
            case InputComponent: break;
            case LogicGate gate:
                foreach (var input in gate.InputPorts) input.State = ReadInput(circuit, input, visiting, evaluated);
                gate.Output.State = gate.Definition.Evaluate(gate.InputPorts.Select(port => port.State).ToArray());
                break;
            case OutputComponent output:
                output.Input.State = ReadInput(circuit, output.Input, visiting, evaluated);
                break;
            default: throw new CircuitSimulationException($"Unsupported component type '{component.GetType().Name}'.");
        }
        visiting.Remove(component); evaluated.Add(component);
    }

    private static LogicState ReadInput(Circuit circuit, InputPort input, ISet<Component> visiting, ISet<Component> evaluated)
    {
        Connection connection;
        try { connection = circuit.GetConnectionTo(input); }
        catch (KeyNotFoundException) { throw new CircuitSimulationException($"Required input port '{input.Id}' is unconnected."); }
        EvaluateComponent(circuit, connection.Source.Component, visiting, evaluated);
        return connection.Source.State;
    }
}
