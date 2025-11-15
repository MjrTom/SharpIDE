using CliWrap.Buffered;
using ParallelPipelines.Application.Attributes;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Host.Helpers;

namespace Deploy.Steps;

[DependsOnStep<RestoreAndBuildStep>]
public class CreateMacosRelease : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		var godotPublishDirectory = await PipelineFileHelper.GitRootDirectory.GetDirectory("./artifacts/publish-godot");
		godotPublishDirectory.Create();
		var macosPublishDirectory = await godotPublishDirectory.GetDirectory("./osx");
		macosPublishDirectory.Create();

		var godotProjectFile = await PipelineFileHelper.GitRootDirectory.GetFile("./src/SharpIDE.Godot/project.godot");

		var godotExportResult = await PipelineCliHelper.RunCliCommandAsync(
			"godot",
			$"--headless --verbose --export-release macOS --project {godotProjectFile.GetFullNameUnix()}",
			cancellationToken
		);

		var macosDmgFile = await macosPublishDirectory.GetFile("SharpIDE.dmg");
		macosDmgFile.MoveTo($"{PipelineFileHelper.GitRootDirectory.FullName}/artifacts/publish-godot/sharpide-osx-universal.dmg");

		return [godotExportResult];
	}
}
