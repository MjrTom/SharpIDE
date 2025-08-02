using System.Diagnostics;
using Ardalis.GuardClauses;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.Run;

public class RunService
{
	// TODO: optimise this Copilot junk
	public async Task RunProject(SharpIdeProjectModel project)
	{
		Guard.Against.Null(project, nameof(project));
		Guard.Against.NullOrWhiteSpace(project.FilePath, nameof(project.FilePath), "Project file path cannot be null or empty.");

		var processStartInfo = new ProcessStartInfo
		{
			FileName = "dotnet",
			Arguments = $"run --project \"{project.FilePath}\"",
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		using var process = new Process();
		process.StartInfo = processStartInfo;

		process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
		process.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);

		process.Start();

		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		await process.WaitForExitAsync();

		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException($"Project run failed with exit code {process.ExitCode}.");
		}

		Console.WriteLine("Project ran successfully.");
	}
}
