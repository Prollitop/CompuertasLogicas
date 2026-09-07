using SimuladorCompuertrtas.Core.Logic;

namespace SimuladorCompuertrtas.ViewModels;

public sealed class TruthTableRowViewModel(TruthTableRow row, bool isCurrent) : ViewModelBase
{
    public string Inputs => string.Join(" · ", row.Inputs.Select(state => state == LogicState.High ? "1" : "0"));
    public string Output => row.Output == LogicState.High ? "1" : "0";
    public bool IsCurrent { get; } = isCurrent;
}
