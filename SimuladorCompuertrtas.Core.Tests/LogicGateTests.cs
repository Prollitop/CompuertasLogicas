using SimuladorCompuertrtas.Core.Logic;

namespace SimuladorCompuertrtas.Core.Tests;

public sealed class LogicGateTests
{
    public static TheoryData<ILogicGateDefinition, LogicState, LogicState, LogicState> BinaryTruthTables => new()
    {
        { new AndGateDefinition(), LogicState.Low, LogicState.Low, LogicState.Low }, { new AndGateDefinition(), LogicState.Low, LogicState.High, LogicState.Low }, { new AndGateDefinition(), LogicState.High, LogicState.Low, LogicState.Low }, { new AndGateDefinition(), LogicState.High, LogicState.High, LogicState.High },
        { new OrGateDefinition(), LogicState.Low, LogicState.Low, LogicState.Low }, { new OrGateDefinition(), LogicState.Low, LogicState.High, LogicState.High }, { new OrGateDefinition(), LogicState.High, LogicState.Low, LogicState.High }, { new OrGateDefinition(), LogicState.High, LogicState.High, LogicState.High },
        { new NandGateDefinition(), LogicState.Low, LogicState.Low, LogicState.High }, { new NandGateDefinition(), LogicState.Low, LogicState.High, LogicState.High }, { new NandGateDefinition(), LogicState.High, LogicState.Low, LogicState.High }, { new NandGateDefinition(), LogicState.High, LogicState.High, LogicState.Low },
        { new NorGateDefinition(), LogicState.Low, LogicState.Low, LogicState.High }, { new NorGateDefinition(), LogicState.Low, LogicState.High, LogicState.Low }, { new NorGateDefinition(), LogicState.High, LogicState.Low, LogicState.Low }, { new NorGateDefinition(), LogicState.High, LogicState.High, LogicState.Low },
        { new XorGateDefinition(), LogicState.Low, LogicState.Low, LogicState.Low }, { new XorGateDefinition(), LogicState.Low, LogicState.High, LogicState.High }, { new XorGateDefinition(), LogicState.High, LogicState.Low, LogicState.High }, { new XorGateDefinition(), LogicState.High, LogicState.High, LogicState.Low },
        { new XnorGateDefinition(), LogicState.Low, LogicState.Low, LogicState.High }, { new XnorGateDefinition(), LogicState.Low, LogicState.High, LogicState.Low }, { new XnorGateDefinition(), LogicState.High, LogicState.Low, LogicState.Low }, { new XnorGateDefinition(), LogicState.High, LogicState.High, LogicState.High }
    };

    [Theory, MemberData(nameof(BinaryTruthTables))]
    public void Binary_gates_evaluate_their_complete_truth_tables(ILogicGateDefinition gate, LogicState left, LogicState right, LogicState expected) => Assert.Equal(expected, gate.Evaluate([left, right]));

    [Theory]
    [InlineData(LogicState.Low, LogicState.High)]
    [InlineData(LogicState.High, LogicState.Low)]
    public void Not_gate_evaluates_its_complete_truth_table(LogicState input, LogicState expected) => Assert.Equal(expected, new NotGateDefinition().Evaluate([input]));

    [Fact]
    public void Truth_table_is_generated_for_variable_input_gate() =>
        Assert.Equal(8, new AndGateDefinition().CreateTruthTable(3).Count);

    [Fact]
    public void Fixed_arity_gate_rejects_wrong_input_count() =>
        Assert.Throws<ArgumentException>(() => new NotGateDefinition().Evaluate([LogicState.Low, LogicState.High]));
}
