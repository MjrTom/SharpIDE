namespace SharpIDE.Application.Features.FileSystem;

public class FileSystemService
{
	public static async Task<SharpIdeRootFolder> GetSharpIdeRootFolderForSolutionAsync(string solutionFilePath)
	{
		var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(solutionFilePath)!);
		if (!directoryInfo.Exists) throw new DirectoryNotFoundException();
		var rootFolder = new SharpIdeRootFolder(directoryInfo);
		return rootFolder;
	}
}
