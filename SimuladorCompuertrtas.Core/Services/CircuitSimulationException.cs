namespace SimuladorCompuertrtas.Core.Services;

public sealed class CircuitSimulationException(string message) : InvalidOperationException(message);
