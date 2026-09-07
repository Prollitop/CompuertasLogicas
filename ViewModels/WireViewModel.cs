using SimuladorCompuertrtas.Core.Models;
using SimuladorCompuertrtas.Core.Logic;

namespace SimuladorCompuertrtas.ViewModels;

public sealed class WireViewModel(Connection connection) : ViewModelBase
{
    private bool _isSelected;
    public Connection Connection { get; } = connection;
    public double X1 { get; private set; }
    public double Y1 { get; private set; }
    public double X2 { get; private set; }
    public double Y2 { get; private set; }
    public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
    public LogicState State => Connection.Source.State;
    public void RefreshSignal() => OnPropertyChanged(nameof(State));

    public void UpdateEndpoints(CircuitComponentViewModel source, CircuitComponentViewModel target, int targetInputIndex)
    {
        X1 = source.X + 142; Y1 = source.Y + 48;
        X2 = target.X - 2; Y2 = target.Y + GetInputOffset(target, targetInputIndex);
        OnPropertyChanged(nameof(X1)); OnPropertyChanged(nameof(Y1)); OnPropertyChanged(nameof(X2)); OnPropertyChanged(nameof(Y2));
    }

    public static double GetInputOffset(CircuitComponentViewModel component, int index) => component.IsOutput ? 48 : component.InputCount <= 1 ? 48 : 28 + index * (40d / (component.InputCount - 1));
}
