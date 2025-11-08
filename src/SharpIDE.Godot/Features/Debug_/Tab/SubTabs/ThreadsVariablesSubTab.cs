using Ardalis.GuardClauses;
using Godot;
using SharpIDE.Application.Features.Debugging;
using SharpIDE.Application.Features.Events;
using SharpIDE.Application.Features.Run;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Godot.Features.Debug_.Tab.SubTabs;

public partial class ThreadsVariablesSubTab : Control
{
	private PackedScene _threadListItemScene = GD.Load<PackedScene>("res://Features/Debug_/Tab/SubTabs/ThreadListItem.tscn");
	private VBoxContainer _threadsVboxContainer = null!;
	private VBoxContainer _stackFramesVboxContainer = null!;
	private VBoxContainer _variablesVboxContainer = null!;
	public SharpIdeProjectModel Project { get; set; } = null!;
	// private ThreadModel? _selectedThread = null!; // null when not at a stop point
	
    [Inject] private readonly RunService _runService = null!;

	public override void _Ready()
	{
		_threadsVboxContainer = GetNode<VBoxContainer>("%ThreadsVBoxContainer");
		_stackFramesVboxContainer = GetNode<VBoxContainer>("%StackFramesVBoxContainer");
		_variablesVboxContainer = GetNode<VBoxContainer>("%VariablesVBoxContainer");
		GlobalEvents.Instance.DebuggerExecutionStopped.Subscribe(OnDebuggerExecutionStopped);
		
	}

	private async Task OnDebuggerExecutionStopped(ExecutionStopInfo stopInfo)
	{
		var result = await _runService.GetInfoAtStopPoint();
		var threadScenes = result.Threads.Select(s =>
		{
			var threadListItem = _threadListItemScene.Instantiate<Control>();
			threadListItem.GetNode<Label>("Label").Text = $"{s.Id}: {s.Name}";
			return threadListItem;
		}).ToList(); 
		await this.InvokeAsync(() =>
		{
			_threadsVboxContainer.QueueFreeChildren();
			foreach (var scene in threadScenes)
			{
				_threadsVboxContainer.AddChild(scene);
			}
		});

		var stoppedThreadId = stopInfo.ThreadId;
		var stoppedThread = result.Threads.SingleOrDefault(t => t.Id == stoppedThreadId);
		Guard.Against.Null(stoppedThread, nameof(stoppedThread));
		var stackFrameScenes = stoppedThread!.StackFrames.Select(s =>
		{
			var stackFrameItem = _threadListItemScene.Instantiate<Control>();
			stackFrameItem.GetNode<Label>("Label").Text = $"{s.ClassName}.{s.MethodName}() in {s.Namespace}, {s.AssemblyName}";
			return stackFrameItem;
		}).ToList();
		await this.InvokeAsync(() =>
		{
			_stackFramesVboxContainer.QueueFreeChildren();
			foreach (var scene in stackFrameScenes)
			{
				_stackFramesVboxContainer.AddChild(scene);
			}
		});
		
		var currentFrame = stoppedThread.StackFrames.First();
		var variableScenes = currentFrame.Scopes.SelectMany(s => s.Variables).Select(v =>
		{
			var variableListItem = _threadListItemScene.Instantiate<Control>();
			variableListItem.GetNode<Label>("Label").Text = $$"""{{v.Name}} = {{{v.Type}}} {{v.Value}}""";
			return variableListItem;
		}).ToList();
		
		await this.InvokeAsync(() =>
		{
			_variablesVboxContainer.QueueFreeChildren();
			foreach (var scene in variableScenes)
			{
				_variablesVboxContainer.AddChild(scene);
			}
		});
	}
}