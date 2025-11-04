using SharpIDE.Application.Features.Evaluation;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;
using SharpIDE.Application.Features.Testing.Client;
using SharpIDE.Application.Features.Testing.Client.Dtos;

namespace SharpIDE.Application.Features.Testing;

public class TestRunnerService
{
	public async Task<List<TestNode>> DiscoverTests(SharpIdeSolutionModel solutionModel)
	{
		await Task.WhenAll(solutionModel.AllProjects.Select(s => s.MsBuildEvaluationProjectTask));
		var testProjects = solutionModel.AllProjects.Where(p => p.IsMtpTestProject).ToList();
		List<TestNode> allDiscoveredTestNodes = [];
		foreach (var testProject in testProjects)
		{
			using var client = await GetInitialisedClientAsync(testProject);
			List<TestNodeUpdate> testNodeUpdates = [];
			var discoveryResponse = await client.DiscoverTestsAsync(Guid.NewGuid(), node =>
			{
				testNodeUpdates.AddRange(node);
				return Task.CompletedTask;
			});
			await discoveryResponse.WaitCompletionAsync();

			await client.ExitAsync();
			allDiscoveredTestNodes.AddRange(testNodeUpdates.Select(tn => tn.Node));
		}

		return allDiscoveredTestNodes;
	}

	public async Task RunTestsAsync(SharpIdeSolutionModel solutionModel, Func<TestNodeUpdate[], Task> func)
	{
		await Task.WhenAll(solutionModel.AllProjects.Select(s => s.MsBuildEvaluationProjectTask));
		var testProjects = solutionModel.AllProjects.Where(p => p.IsMtpTestProject).ToList();
		foreach (var testProject in testProjects)
		{
			await RunTestsAsync(testProject, func);
		}
	}

	// Assumes it has already been built
	public async Task RunTestsAsync(SharpIdeProjectModel project, Func<TestNodeUpdate[], Task> func)
	{
		using var client = await GetInitialisedClientAsync(project);
		List<TestNodeUpdate> testNodeUpdates = [];
		var discoveryResponse = await client.DiscoverTestsAsync(Guid.NewGuid(), async nodeUpdates =>
		{
			testNodeUpdates.AddRange(nodeUpdates);
			await func(nodeUpdates);
		});
		await discoveryResponse.WaitCompletionAsync();

		ResponseListener runRequest = await client.RunTestsAsync(Guid.NewGuid(), testNodeUpdates.Select(x => x.Node).ToArray(), func);
		await runRequest.WaitCompletionAsync();
		await client.ExitAsync();
	}

	private async Task<TestingPlatformClient> GetInitialisedClientAsync(SharpIdeProjectModel project)
	{
		var outputDllPath = ProjectEvaluation.GetOutputDllFullPath(project);
		var outputExecutablePath = 0 switch
		{
			_ when OperatingSystem.IsWindows() => outputDllPath!.Replace(".dll", ".exe"),
			_ when OperatingSystem.IsLinux() => outputDllPath!.Replace(".dll", ""),
			_ when OperatingSystem.IsMacOS() => outputDllPath!.Replace(".dll", ""),
			_ => throw new PlatformNotSupportedException("Unsupported OS for running tests.")
		};

		var client = await TestingPlatformClientFactory.StartAsServerAndConnectToTheClientAsync(outputExecutablePath);
		await client.InitializeAsync();
		return client;
	}
}
