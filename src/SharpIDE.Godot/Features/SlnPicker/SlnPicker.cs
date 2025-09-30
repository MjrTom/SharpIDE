using Godot;

namespace SharpIDE.Godot.Features.SlnPicker;

public partial class SlnPicker : Control
{
    private FileDialog _fileDialog = null!;

    public override void _Ready()
    {
        _fileDialog = GetNode<FileDialog>("%FileDialog");
    }
    public async Task<string?> GetSelectedSolutionPath()
    {
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        await this.InvokeAsync(() =>
        {
            _fileDialog.FileSelected += path => tcs.SetResult(path);
            _fileDialog.Canceled += () => tcs.SetResult(null);
            _fileDialog.PopupCentered();
        });
        
        var selectedPath = await tcs.Task;
        return selectedPath;
    }
}