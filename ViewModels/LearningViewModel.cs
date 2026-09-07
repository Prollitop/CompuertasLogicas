using System.Collections.ObjectModel;
using System.Windows.Input;
using SimuladorCompuertrtas.Commands;
using SimuladorCompuertrtas.Core.Logic;

namespace SimuladorCompuertrtas.ViewModels;

public sealed class LearningViewModel : ViewModelBase
{
    private readonly IReadOnlyList<ILogicGateDefinition> _gates = LogicGateCatalog.CreateStandardDefinitions();
    private int _index;
    private LogicState[] _inputs = [LogicState.Low, LogicState.Low];
    public ObservableCollection<TruthTableRowViewModel> TruthTable { get; } = [];
    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand ToggleFirstInputCommand { get; }
    public ICommand ToggleSecondInputCommand { get; }
    public ILogicGateDefinition Gate => _gates[_index];
    public string Description => Gate.Id switch
    {
        "and" => "Produce 1 cuando todas las entradas son 1.", "or" => "Produce 1 cuando al menos una entrada es 1.", "not" => "Invierte la señal de entrada.",
        "nand" => "Invierte la salida de AND.", "nor" => "Invierte la salida de OR.", "xor" => "Produce 1 cuando las entradas son diferentes.", _ => "Produce 1 cuando las entradas son iguales."
    };
    public string ExperimentPrompt => Gate.MinimumInputCount == 1 ? "Activa A y observa cómo se invierte el resultado." : "Activa A y B; compara el resultado con la fila resaltada.";
    public string InputA => _inputs[0] == LogicState.High ? "A: ON" : "A: OFF";
    public string InputB => _inputs.Length == 1 ? "" : _inputs[1] == LogicState.High ? "B: ON" : "B: OFF";
    public string Result => Gate.Evaluate(_inputs) == LogicState.High ? "Resultado: 1 · ON" : "Resultado: 0 · OFF";

    public LearningViewModel()
    {
        PreviousCommand = new RelayCommand(_ => { _index = (_index + _gates.Count - 1) % _gates.Count; ResetInputs(); });
        NextCommand = new RelayCommand(_ => { _index = (_index + 1) % _gates.Count; ResetInputs(); });
        ToggleFirstInputCommand = new RelayCommand(_ => Toggle(0)); ToggleSecondInputCommand = new RelayCommand(_ => Toggle(1)); Refresh();
    }
    private void Toggle(int index) { if (index < _inputs.Length) { _inputs[index] = _inputs[index] == LogicState.High ? LogicState.Low : LogicState.High; Refresh(); } }
    private void ResetInputs() { _inputs = Enumerable.Repeat(LogicState.Low, Gate.MinimumInputCount).ToArray(); Refresh(); }
    private void Refresh()
    {
        TruthTable.Clear();
        foreach (var row in Gate.CreateTruthTable()) TruthTable.Add(new TruthTableRowViewModel(row, row.Inputs.SequenceEqual(_inputs)));
        OnPropertyChanged(nameof(Gate)); OnPropertyChanged(nameof(Description)); OnPropertyChanged(nameof(ExperimentPrompt)); OnPropertyChanged(nameof(InputA)); OnPropertyChanged(nameof(InputB)); OnPropertyChanged(nameof(Result));
    }
}
