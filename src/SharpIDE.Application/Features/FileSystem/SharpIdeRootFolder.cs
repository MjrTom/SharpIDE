using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using ObservableCollections;
using R3;
using SharpIDE.Application.Features.SolutionDiscovery;

namespace SharpIDE.Application.Features.FileSystem;

/// The root folder of the "solution" - contains the open solution file, and all child files and folders
public class SharpIdeRootFolder : IExpandableSharpIdeNode
{
	public bool Expanded { get; set; }
	public required string Path { get; set; }
	public ObservableList<SharpIdeFile> Files { get; init; }
	public ObservableList<SharpIdeFolder> Folders { get; init; }
	public required ConcurrentDictionary<string, SharpIdeFile> AllFiles { get; set; }
	public ObservableList<SharpIdeFolder> AllFolders { get; init; }

	/// key is path to .csproj file, folder is containing folder
	//public ObservableDictionary<string, SharpIdeFolder> AllProjects { get; init; }

	[SetsRequiredMembers]
	public SharpIdeRootFolder(DirectoryInfo folderInfo)
	{
		Path = folderInfo.FullName;
		var allFiles = new ConcurrentBag<SharpIdeFile>();
		var allFolders = new ConcurrentBag<SharpIdeFolder>();
		Files = new ObservableList<SharpIdeFile>(folderInfo.GetFiles(this, allFiles));
		Folders = new ObservableList<SharpIdeFolder>(this.GetSubFolders(this, allFiles, allFolders));
		AllFiles = new ConcurrentDictionary<string, SharpIdeFile>(allFiles.DistinctBy(s => s.Path).ToDictionary(s => s.Path));
		AllFolders = new ObservableList<SharpIdeFolder>(allFolders);
		//AllProjects = new ObservableDictionary<string, SharpIdeFolder>(AllFiles.Where(s => s.IsCsprojFile).ToDictionary(s => s.Path, s => s.Parent));
	}

	public (ObservableList<SharpIdeFile>, ObservableList<SharpIdeFolder>) GetFilesAndFoldersForProject(string csprojFullPath)
	{
		var csprojFile = AllFiles.GetValueOrDefault(csprojFullPath);
		Guard.Against.Null(csprojFile);
		var parent = csprojFile.Parent;
		if (parent is SharpIdeFolder folder)
		{
			return (folder.Files, folder.Folders);
		}
		if (parent is SharpIdeRootFolder rootFolder)
		{
			return (rootFolder.Files, rootFolder.Folders);
		}
		throw new UnreachableException();
	}
}
