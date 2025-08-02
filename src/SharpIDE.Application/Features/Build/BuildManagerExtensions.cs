using Microsoft.Build.Execution;

namespace SharpIDE.Application.Features.Build;

public static class BuildManagerExtensions
{
	public static async Task<BuildResult> BuildAsync(this BuildManager buildManager, BuildParameters buildParameters, BuildRequestData buildRequest)
	{
		var buildCompleteTcs = new TaskCompletionSource<BuildResult>();
		buildManager.BeginBuild(buildParameters);
		var buildSubmission = buildManager.PendBuildRequest(buildRequest);
		buildSubmission.ExecuteAsync(submission =>
		{
			buildCompleteTcs.SetResult(submission.BuildResult!);
		}, null);
		var buildResult = await buildCompleteTcs.Task.ConfigureAwait(false);
		buildManager.EndBuild();
		return buildResult;
	}
}
