using SharpIDE.Application.Features.Events;
using SharpIDE.Application.Features.SolutionDiscovery;

namespace SharpIDE.Godot;

public static class GodotGlobalEvents
{
    public static event Func<BottomPanelType, Task> BottomPanelTabExternallySelected = _ => Task.CompletedTask;
    public static void InvokeBottomPanelTabExternallySelected(BottomPanelType type) => BottomPanelTabExternallySelected.InvokeParallelFireAndForget(type);
    
    public static event Func<BottomPanelType?, Task> BottomPanelTabSelected = _ => Task.CompletedTask;
    public static void InvokeBottomPanelTabSelected(BottomPanelType? type) => BottomPanelTabSelected.InvokeParallelFireAndForget(type);
    
    public static event Func<bool, Task> BottomPanelVisibilityChangeRequested = _ => Task.CompletedTask;
    public static void InvokeBottomPanelVisibilityChangeRequested(bool show) => BottomPanelVisibilityChangeRequested.InvokeParallelFireAndForget(show);
    
    public static event Func<SharpIdeFile, Task> FileSelected = _ => Task.CompletedTask;
    public static void InvokeFileSelected(SharpIdeFile file) => FileSelected.InvokeParallelFireAndForget(file);
    public static event Func<SharpIdeFile, Task> FileExternallySelected = _ => Task.CompletedTask;
    public static void InvokeFileExternallySelected(SharpIdeFile file) => FileExternallySelected.InvokeParallelFireAndForget(file);
    public static async Task InvokeFileExternallySelectedAndWait(SharpIdeFile file) => await FileExternallySelected.InvokeParallelAsync(file);
    
}

public enum BottomPanelType
{
    Run,
    Debug,
    Build,
    Problems
}