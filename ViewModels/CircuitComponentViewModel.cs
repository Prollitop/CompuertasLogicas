using SimuladorCompuertrtas.Core.Logic;
using SimuladorCompuertrtas.Core.Models;

namespace SimuladorCompuertrtas.ViewModels;

public sealed class CircuitComponentViewModel(Component component) : ViewModelBase
{
    private bool _isSelected;
    public Component Component { get; } = component;
    public string Id => Component.Id;
    public string Name => Component.DisplayName;
    public bool IsInput => Component is InputComponent;
    public bool IsOutput => Component is OutputComponent;
    public bool IsGate => Component is LogicGate;
    public string GateId => (Component as LogicGate)?.Definition.Id ?? string.Empty;
    public int InputCount => Component.InputPorts.Count;
    public double X => Component.Position?.X ?? 0;
    public double Y => Component.Position?.Y ?? 0;
    public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
    public LogicState OutputState => Component.OutputPorts.FirstOrDefault()?.State ?? LogicState.Low;
    public LogicState InputState => Component.InputPorts.FirstOrDefault()?.State ?? LogicState.Low;
    public string StateText => (IsOutput ? InputState : OutputState) == LogicState.High ? "1 · ON" : "0 · OFF";

    public void SetPosition(double x, double y)
    {
        Component.Position = new ComponentPosition(Math.Max(0, x), Math.Max(0, y));
        OnPropertyChanged(nameof(X)); OnPropertyChanged(nameof(Y));
    }

    public void RefreshSignal()
    {
        OnPropertyChanged(nameof(Name)); OnPropertyChanged(nameof(OutputState)); OnPropertyChanged(nameof(InputState)); OnPropertyChanged(nameof(StateText));
    }
}
