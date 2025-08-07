using System.Diagnostics;
using SharpIDE.Application.Features.ProjectIntrospection;

namespace SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

public static class VsPersistenceMapper
{
	public static async Task<SharpIdeSolutionModel> GetSolutionModel(string solutionFilePath, CancellationToken cancellationToken = default)
	{
		var timer = Stopwatch.StartNew();
		// This intermediate model is pretty much useless, but I have left it around as we grab the project nodes with it, which we might use later.
		var intermediateModel = await IntermediateMapper.GetIntermediateModel(solutionFilePath, cancellationToken);

		var solutionName = Path.GetFileName(solutionFilePath);
		var solutionModel = new SharpIdeSolutionModel
		{
			Name = solutionName,
			FilePath = solutionFilePath,
			Projects = intermediateModel.Projects.Select(GetSharpIdeProjectModel).ToList(),
			Folders = intermediateModel.SolutionFolders.Select(s => new SharpIdeSolutionFolder
			{
				Name = s.Model.Name,
				Files = s.Files.Select(GetSharpIdeFile).ToList(),
				Folders = s.Folders.Select(GetSharpIdeSolutionFolder).ToList(),
				Projects = s.Projects.Select(GetSharpIdeProjectModel).ToList()
			}).ToList(),
		};
		timer.Stop();
		Console.WriteLine($"Solution model fully created in {timer.ElapsedMilliseconds} ms");

		return solutionModel;
	}
	private static SharpIdeProjectModel GetSharpIdeProjectModel(IntermediateProjectModel projectModel) => new SharpIdeProjectModel
	{
		Name = projectModel.Model.ActualDisplayName,
		FilePath = projectModel.FullFilePath,
		Files = TreeMapperV2.GetFiles(projectModel.FullFilePath),
		Folders = TreeMapperV2.GetSubFolders(projectModel.FullFilePath),
		MsBuildEvaluationProject = Test.GetProject(projectModel.FullFilePath)
	};

	private static SharpIdeSolutionFolder GetSharpIdeSolutionFolder(IntermediateSlnFolderModel folderModel) => new SharpIdeSolutionFolder()
	{
		Name = folderModel.Model.Name,
		Files = folderModel.Files.Select(GetSharpIdeFile).ToList(),
		Folders = folderModel.Folders.Select(GetSharpIdeSolutionFolder).ToList(),
		Projects = folderModel.Projects.Select(GetSharpIdeProjectModel).ToList()
	};

	private static SharpIdeFile GetSharpIdeFile(IntermediateSlnFolderFileModel fileModel) => new SharpIdeFile
	{
		Path = fileModel.FullPath,
		Name = fileModel.Name
	};
}
