using SharpIDE.Application.Features.Events;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.FileWatching;

public class IdeFileChangeHandler
{
	public SharpIdeSolutionModel SolutionModel { get; set; } = null!;
	public IdeFileChangeHandler()
	{
		GlobalEvents.Instance.FileSystemWatcherInternal.FileChanged.Subscribe(OnFileChanged);
	}

	private async Task OnFileChanged(string arg)
	{
		var sharpIdeFile = SolutionModel.AllFiles.SingleOrDefault(f => f.Path == arg);
		if (sharpIdeFile is null) return;
		// TODO: Suppress if SharpIDE changed the file
		await sharpIdeFile.FileContentsChangedExternallyFromDisk.InvokeParallelAsync();
	}
}
