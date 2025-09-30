using SharpIDE.Application.Features.Analysis;
using SharpIDE.Application.Features.Events;
using SharpIDE.Application.Features.SolutionDiscovery;

namespace SharpIDE.Godot;

public class GodotGlobalEvents
{
    public static GodotGlobalEvents Instance { get; set; } = null!;
    public event Func<BottomPanelType, Task> BottomPanelTabExternallySelected = _ => Task.CompletedTask;
    public void InvokeBottomPanelTabExternallySelected(BottomPanelType type) => BottomPanelTabExternallySelected.InvokeParallelFireAndForget(type);
    
    public event Func<BottomPanelType?, Task> BottomPanelTabSelected = _ => Task.CompletedTask;
    public void InvokeBottomPanelTabSelected(BottomPanelType? type) => BottomPanelTabSelected.InvokeParallelFireAndForget(type);
    
    public event Func<bool, Task> BottomPanelVisibilityChangeRequested = _ => Task.CompletedTask;
    public void InvokeBottomPanelVisibilityChangeRequested(bool show) => BottomPanelVisibilityChangeRequested.InvokeParallelFireAndForget(show);
    
    public event Func<SharpIdeFile, SharpIdeFileLinePosition?, Task> FileSelected = (_, _) => Task.CompletedTask;
    public void InvokeFileSelected(SharpIdeFile file, SharpIdeFileLinePosition? fileLinePosition = null) => FileSelected.InvokeParallelFireAndForget(file, fileLinePosition);
    public async Task InvokeFileSelectedAndWait(SharpIdeFile file, SharpIdeFileLinePosition? fileLinePosition) => await FileSelected.InvokeParallelAsync(file, fileLinePosition);
    public event Func<SharpIdeFile, SharpIdeFileLinePosition?, Task> FileExternallySelected = (_, _) => Task.CompletedTask;
    public void InvokeFileExternallySelected(SharpIdeFile file, SharpIdeFileLinePosition? fileLinePosition = null) => FileExternallySelected.InvokeParallelFireAndForget(file, fileLinePosition);
    public async Task InvokeFileExternallySelectedAndWait(SharpIdeFile file, SharpIdeFileLinePosition? fileLinePosition = null) => await FileExternallySelected.InvokeParallelAsync(file, fileLinePosition);
    
}

public enum BottomPanelType
{
    Run,
    Debug,
    Build,
    Problems,
    IdeDiagnostics
}