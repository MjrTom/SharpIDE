using SharpIDE.Application.Features.SolutionDiscovery;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.Debugging;

// TODO: Why does this exist separate from DebuggingService?
public class Debugger
{
	public required SharpIdeProjectModel Project { get; init; }
	public required int ProcessId { get; init; }
	private DebuggingService _debuggingService = new DebuggingService();
	public async Task Attach(string? debuggerExecutablePath, Dictionary<SharpIdeFile, List<Breakpoint>> breakpointsByFile, CancellationToken cancellationToken)
	{
		await _debuggingService.Attach(ProcessId, debuggerExecutablePath, breakpointsByFile, cancellationToken);
	}

	public async Task StepOver(int threadId, CancellationToken cancellationToken = default) => await _debuggingService.StepOver(threadId, cancellationToken);
	public async Task<ThreadsStackTraceModel> GetInfoAtStopPoint() => await _debuggingService.GetInfoAtStopPoint();
}
