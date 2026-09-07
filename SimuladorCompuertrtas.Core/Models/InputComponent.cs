using SimuladorCompuertrtas.Core.Logic;

namespace SimuladorCompuertrtas.Core.Models;

public sealed class InputComponent : Component
{
    public InputComponent(string id, string displayName, LogicState initialState = LogicState.Low) : base(id, displayName)
    {
        Output = new OutputPort($"{id}.out", this, "Output") { State = initialState };
        OutputPorts.Add(Output);
    }
    public OutputPort Output { get; }
    public LogicState State { get => Output.State; set => Output.State = value; }
}
