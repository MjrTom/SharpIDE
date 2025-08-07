using Ardalis.GuardClauses;
using Microsoft.Build.Evaluation;

namespace SharpIDE.Application.Features.ProjectIntrospection;

public static class Test
{
	private static readonly ProjectCollection _projectCollection = ProjectCollection.GlobalProjectCollection;
	public static async Task<Project> GetProject(string projectFilePath)
	{
		Guard.Against.Null(projectFilePath, nameof(projectFilePath));

		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

		var project = _projectCollection.LoadProject(projectFilePath);
		Console.WriteLine($"Project loaded: {project.FullPath}");
		//var outputType = project.GetProperty("OutputType");
		return project;
	}
}
