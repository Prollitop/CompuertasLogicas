using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using SimuladorCompuertrtas.Core.Models;
using SimuladorCompuertrtas.ViewModels;

namespace SimuladorCompuertrtas;

/// <summary>Hosts pointer gestures only; circuit manipulation and simulation remain in the view model/Core.</summary>
public partial class MainWindow : Window
{
    private CircuitComponentViewModel? _draggedComponent;
    private Point _dragOrigin;
    private Point _componentOrigin;
    private bool _isPanning;
    private Point _panOrigin;
    private Point _panTransformOrigin;

    public MainWindow() => InitializeComponent();

    private CircuitEditorViewModel Editor => ((MainWindowViewModel)DataContext).Editor;

    private void PaletteItem_PreviewMouseMove(object sender, MouseEventArgs eventArgs)
    {
        if (eventArgs.LeftButton != MouseButtonState.Pressed || sender is not Button { Tag: string type }) return;
        DragDrop.DoDragDrop((DependencyObject)sender, new DataObject("LogicLab.Component", type), DragDropEffects.Copy);
    }

    private void EditorSurface_DragOver(object sender, DragEventArgs eventArgs) => eventArgs.Effects = eventArgs.Data.GetDataPresent("LogicLab.Component") ? DragDropEffects.Copy : DragDropEffects.None;

    private void EditorSurface_Drop(object sender, DragEventArgs eventArgs)
    {
        if (!eventArgs.Data.GetDataPresent("LogicLab.Component")) return;
        var point = ToCircuitPoint(eventArgs.GetPosition(EditorSurface));
        Editor.AddComponent((string)eventArgs.Data.GetData("LogicLab.Component")!, point.X, point.Y);
    }

    private void Component_MouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
    {
        if (sender is not FrameworkElement { DataContext: CircuitComponentViewModel component }) return;
        Editor.SelectComponent(component); _draggedComponent = component; _dragOrigin = eventArgs.GetPosition(EditorSurface); _componentOrigin = new Point(component.X, component.Y);
        ((UIElement)sender).CaptureMouse(); eventArgs.Handled = true;
    }

    private void Component_MouseMove(object sender, MouseEventArgs eventArgs)
    {
        if (_draggedComponent is null || eventArgs.LeftButton != MouseButtonState.Pressed) return;
        var delta = eventArgs.GetPosition(EditorSurface) - _dragOrigin;
        Editor.MoveComponent(_draggedComponent, _componentOrigin.X + delta.X / Editor.Zoom, _componentOrigin.Y + delta.Y / Editor.Zoom);
    }

    private void Component_MouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
    {
        if (_draggedComponent is null) return;
        ((UIElement)sender).ReleaseMouseCapture(); _draggedComponent = null;
    }

    private void OutputPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
    {
        var component = FindComponent(sender as DependencyObject);
        if (component is null || component.Component.OutputPorts.Count == 0) return;
        Editor.SelectComponent(component); Editor.BeginWire(component); eventArgs.Handled = true;
    }

    private void InputPort_MouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
    {
        var component = FindComponent(sender as DependencyObject);
        if (component is null || sender is not FrameworkElement { DataContext: InputPort port }) return;
        Editor.CompleteWire(component, component.Component.InputPorts.IndexOf(port)); eventArgs.Handled = true;
    }

    private void EditorSurface_MouseMove(object sender, MouseEventArgs eventArgs)
    {
        if (_isPanning)
        {
            var offset = eventArgs.GetPosition(Viewport) - _panOrigin;
            PanTransform.X = _panTransformOrigin.X + offset.X;
            PanTransform.Y = _panTransformOrigin.Y + offset.Y;
        }
        if (Editor.IsWiring) { var point = ToCircuitPoint(eventArgs.GetPosition(EditorSurface)); Editor.UpdateWirePreview(point.X, point.Y); }
    }

    private void EditorSurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
    {
        if (Editor.IsWiring) Editor.CancelWire();
    }

    private void Wire_MouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
    {
        if (sender is FrameworkElement { DataContext: WireViewModel wire }) { Editor.SelectWire(wire); eventArgs.Handled = true; }
    }

    private void Window_KeyDown(object sender, KeyEventArgs eventArgs)
    {
        if (eventArgs.Key is Key.Delete or Key.Back) { Editor.DeleteSelection(); eventArgs.Handled = true; }
        if (eventArgs.Key == Key.Escape) Editor.CancelWire();
    }

    private void EditorSurface_MouseRightButtonDown(object sender, MouseButtonEventArgs eventArgs)
    {
        _isPanning = true; _panOrigin = eventArgs.GetPosition(Viewport); _panTransformOrigin = new Point(PanTransform.X, PanTransform.Y); Viewport.CaptureMouse(); eventArgs.Handled = true;
    }

    private void EditorSurface_MouseRightButtonUp(object sender, MouseButtonEventArgs eventArgs)
    {
        if (!_isPanning) return; _isPanning = false; Viewport.ReleaseMouseCapture(); eventArgs.Handled = true;
    }

    private Point ToCircuitPoint(Point point) => new(point.X / Editor.Zoom, point.Y / Editor.Zoom);

    private CircuitComponentViewModel? FindComponent(DependencyObject? element)
    {
        while (element is not null)
        {
            if (element is FrameworkElement { DataContext: CircuitComponentViewModel component }) return component;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    private void SaveCircuit_Click(object sender, RoutedEventArgs eventArgs)
    {
        var dialog = new SaveFileDialog { Filter = "Circuito LOGIC LAB (*.json)|*.json", DefaultExt = ".json" };
        if (dialog.ShowDialog(this) == true) Editor.SaveToFile(dialog.FileName);
    }

    private void LoadCircuit_Click(object sender, RoutedEventArgs eventArgs)
    {
        var dialog = new OpenFileDialog { Filter = "Circuito LOGIC LAB (*.json)|*.json", DefaultExt = ".json" };
        if (dialog.ShowDialog(this) == true) Editor.LoadFromFile(dialog.FileName);
    }

    private void Example_Click(object sender, RoutedEventArgs eventArgs)
    {
        if (sender is FrameworkElement { Tag: string type }) Editor.LoadExample(type);
    }
}
