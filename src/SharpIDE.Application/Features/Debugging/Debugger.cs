using SharpIDE.Application.Features.Debugging.Experimental;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.Debugging;

public class Debugger
{
	public required SharpIdeProjectModel Project { get; init; }
	public required int ProcessId { get; init; }
	public async Task Attach(CancellationToken cancellationToken)
	{
		var debuggingService = new DebuggingService();
		await debuggingService.Attach(ProcessId, cancellationToken);
	}
}
