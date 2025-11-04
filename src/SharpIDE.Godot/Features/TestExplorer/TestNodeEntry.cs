using Godot;
using SharpIDE.Application.Features.Testing.Client.Dtos;

namespace SharpIDE.Godot.Features.TestExplorer;

public partial class TestNodeEntry : MarginContainer
{
    private Label _testNameLabel = null!;
    private Label _testNodeStatusLabel = null!;

    public TestNode TestNode { get; set; } = null!;

    public override void _Ready()
    {
        _testNameLabel = GetNode<Label>("%TestNameLabel");
        _testNodeStatusLabel = GetNode<Label>("%TestNodeStatusLabel");
        _testNameLabel.Text = string.Empty;
        _testNodeStatusLabel.Text = string.Empty;
        SetValues();
    }

    public void SetValues()
    {
        if (TestNode == null) return;
        _testNameLabel.Text = TestNode.DisplayName;
        _testNodeStatusLabel.Text = TestNode.ExecutionState;
    }
}