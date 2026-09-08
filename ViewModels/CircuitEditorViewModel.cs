using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using SimuladorCompuertrtas.Commands;
using SimuladorCompuertrtas.Core.Logic;
using SimuladorCompuertrtas.Core.Models;
using SimuladorCompuertrtas.Core.Services;

namespace SimuladorCompuertrtas.ViewModels;

public sealed class CircuitEditorViewModel : ViewModelBase
{
    private Circuit _circuit = new();
    private readonly CircuitSimulator _simulator = new();
    private readonly LogicGateCatalog _gates = new();
    private readonly CircuitDocumentService _documents;
    private CircuitComponentViewModel? _selectedComponent;
    private WireViewModel? _selectedWire;
    private CircuitComponentViewModel? _wireSource;
    private double _zoom = 1;
    private string _statusMessage = "Listo. Agrega componentes o arrástralos al laboratorio.";

    public ObservableCollection<CircuitComponentViewModel> Components { get; } = [];
    public Circuit Circuit => _circuit;
    public ObservableCollection<WireViewModel> Wires { get; } = [];
    public ICommand AddComponentCommand { get; }
    public ICommand ClearCircuitCommand { get; }
    public ICommand DeleteSelectionCommand { get; }
    public ICommand ToggleInputCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; OnPropertyChanged(); } }
    public double Zoom { get => _zoom; private set { _zoom = Math.Clamp(value, .5, 1.8); OnPropertyChanged(); OnPropertyChanged(nameof(ZoomPercentage)); } }
    public string ZoomPercentage => $"Zoom: {Zoom:P0}";
    public CircuitComponentViewModel? SelectedComponent { get => _selectedComponent; private set { _selectedComponent = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectionDescription)); } }
    public string SelectionDescription => SelectedComponent is null ? "Sin selección" : $"{SelectedComponent.Name} · {SelectedComponent.StateText}";
    public bool IsWiring => _wireSource is not null;
    public double WirePreviewX1 => _wireSource is null ? 0 : _wireSource.X + 142;
    public double WirePreviewY1 => _wireSource is null ? 0 : _wireSource.Y + 48;
    public double WirePreviewX2 { get; private set; }
    public double WirePreviewY2 { get; private set; }

    public CircuitEditorViewModel()
    {
        _documents = new CircuitDocumentService(_gates);
        AddComponentCommand = new RelayCommand(parameter => AddComponent(parameter?.ToString() ?? "and"));
        ClearCircuitCommand = new RelayCommand(_ => ClearCircuit());
        DeleteSelectionCommand = new RelayCommand(_ => DeleteSelection());
        ToggleInputCommand = new RelayCommand(parameter => { if (parameter is CircuitComponentViewModel item && item.Component is InputComponent input) { input.State = input.State == LogicState.High ? LogicState.Low : LogicState.High; Evaluate(); } });
        ZoomInCommand = new RelayCommand(_ => Zoom += .1); ZoomOutCommand = new RelayCommand(_ => Zoom -= .1); ResetZoomCommand = new RelayCommand(_ => Zoom = 1);
    }

    public void AddComponent(string type, double? x = null, double? y = null)
    {
        var id = $"{type.ToLowerInvariant()}-{Guid.NewGuid():N}";
        Component component = type.ToLowerInvariant() switch
        {
            "switch" => new InputComponent(id, "SWITCH"),
            "led" => new OutputComponent(id, "LED"),
            _ => new LogicGate(id, _gates.GetRequired(type))
        };
        _circuit.AddComponent(component);
        var viewModel = new CircuitComponentViewModel(component);
        var column = Components.Count % 4; var row = Components.Count / 4;
        viewModel.SetPosition(x ?? 90 + column * 185, y ?? 90 + row * 130);
        Components.Add(viewModel); SelectComponent(viewModel);
        StatusMessage = $"{viewModel.Name} agregado al circuito.";
    }

    public void SelectComponent(CircuitComponentViewModel component)
    {
        if (_selectedComponent is not null) _selectedComponent.IsSelected = false;
        if (_selectedWire is not null) { _selectedWire.IsSelected = false; _selectedWire = null; }
        SelectedComponent = component; component.IsSelected = true;
    }

    public void SelectWire(WireViewModel wire)
    {
        if (_selectedWire is not null) _selectedWire.IsSelected = false;
        if (_selectedComponent is not null) { _selectedComponent.IsSelected = false; SelectedComponent = null; }
        _selectedWire = wire; wire.IsSelected = true; StatusMessage = "Cable seleccionado. Presiona Supr para eliminarlo.";
    }

    public void MoveComponent(CircuitComponentViewModel component, double x, double y) { component.SetPosition(x, y); UpdateWires(); }
    public void BeginWire(CircuitComponentViewModel source) { _wireSource = source; OnPropertyChanged(nameof(IsWiring)); OnPropertyChanged(nameof(WirePreviewX1)); OnPropertyChanged(nameof(WirePreviewY1)); StatusMessage = "Arrastra el cable hasta un puerto de entrada."; }
    public void UpdateWirePreview(double x, double y) { if (_wireSource is null) return; WirePreviewX2 = x; WirePreviewY2 = y; OnPropertyChanged(nameof(WirePreviewX2)); OnPropertyChanged(nameof(WirePreviewY2)); }
    public void CompleteWire(CircuitComponentViewModel target, int inputIndex)
    {
        if (_wireSource is null) return;
        try { var connection = _circuit.Connect(_wireSource.Component.OutputPorts[0], target.Component.InputPorts[inputIndex]); var wire = new WireViewModel(connection); Wires.Add(wire); UpdateWires(); StatusMessage = "Conexión creada."; Evaluate(); }
        catch (Exception exception) { StatusMessage = $"Conexión no válida: {exception.Message}"; }
        finally { CancelWire(); }
    }
    public void CancelWire() { _wireSource = null; OnPropertyChanged(nameof(IsWiring)); }
    public void DeleteSelection()
    {
        if (_selectedWire is not null) { _circuit.RemoveConnection(_selectedWire.Connection); Wires.Remove(_selectedWire); _selectedWire = null; StatusMessage = "Conexión eliminada."; return; }
        if (_selectedComponent is null) return;
        _circuit.RemoveComponent(_selectedComponent.Component); Components.Remove(_selectedComponent); Wires.Clear(); foreach (var connection in _circuit.Connections) Wires.Add(new WireViewModel(connection)); SelectedComponent = null; UpdateWires(); StatusMessage = "Componente eliminado.";
    }
    public void ClearCircuit() { _circuit.Components.ToList().ForEach(component => _circuit.RemoveComponent(component)); Components.Clear(); Wires.Clear(); SelectedComponent = null; StatusMessage = "Circuito limpiado."; }
    public void SaveToFile(string path)
    {
        try { File.WriteAllText(path, _documents.Save(_circuit)); StatusMessage = "Circuito guardado correctamente."; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { StatusMessage = $"No se pudo guardar el circuito: {exception.Message}"; }
    }
    public void LoadFromFile(string path)
    {
        try
        {
            LoadCircuit(_documents.Load(File.ReadAllText(path)));
            StatusMessage = "Circuito cargado y simulado.";
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or KeyNotFoundException)
        {
            StatusMessage = $"No se pudo cargar el circuito: {exception.Message}";
        }
    }
    public void LoadExample(string type)
    {
        ClearCircuit();
        var a = AddExample("switch", "Switch A", 90, 140); var gateType = type.ToLowerInvariant();
        if (gateType == "wire") { var led = AddExample("led", "LED", 410, 140); Connect(a, 0, led, 0); Evaluate(); return; }
        var b = gateType == "not" ? null : AddExample("switch", "Switch B", 90, 290);
        var gate = AddExample(gateType, gateType.ToUpperInvariant(), 290, 190); var output = AddExample("led", "LED", 510, 190);
        Connect(a, 0, gate, 0); if (b is not null) Connect(b, 0, gate, 1); Connect(gate, 0, output, 0); Evaluate();
    }
    public void LoadChallengeShell(int inputCount)
    {
        ClearCircuit();
        for (var index = 0; index < inputCount; index++) AddExample("switch", $"Switch {(char)('A' + index)}", 90, 130 + index * 130);
        AddExample("led", "LED", 510, 190);
        StatusMessage = "Desafío preparado: añade y conecta las compuertas permitidas.";
    }
    private void Evaluate()
    {
        try { _simulator.Evaluate(_circuit); StatusMessage = "Simulación actualizada."; }
        catch (CircuitSimulationException exception) { StatusMessage = $"Circuito incompleto: {exception.Message}"; }
        foreach (var component in Components) component.RefreshSignal();
        foreach (var wire in Wires) wire.RefreshSignal();
        OnPropertyChanged(nameof(SelectionDescription));
    }
    private CircuitComponentViewModel AddExample(string type, string name, double x, double y)
    {
        AddComponent(type, x, y); var component = Components[^1]; component.Component.DisplayName = name; component.RefreshSignal(); return component;
    }
    private void Connect(CircuitComponentViewModel source, int sourceIndex, CircuitComponentViewModel target, int targetIndex)
    {
        var connection = _circuit.Connect(source.Component.OutputPorts[sourceIndex], target.Component.InputPorts[targetIndex]);
        Wires.Add(new WireViewModel(connection)); UpdateWires();
    }
    private void LoadCircuit(Circuit circuit)
    {
        _circuit = circuit; Components.Clear(); Wires.Clear();
        foreach (var component in circuit.Components) Components.Add(new CircuitComponentViewModel(component));
        foreach (var connection in circuit.Connections) Wires.Add(new WireViewModel(connection));
        UpdateWires(); Evaluate();
    }
    private void UpdateWires()
    {
        foreach (var wire in Wires)
        {
            var source = Components.Single(component => component.Component == wire.Connection.Source.Component);
            var target = Components.Single(component => component.Component == wire.Connection.Target.Component);
            wire.UpdateEndpoints(source, target, target.Component.InputPorts.IndexOf(wire.Connection.Target));
        }
    }
}
