using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.SolutionDiscovery;

public class SharpIdeFolder : ISharpIdeNode, IExpandableSharpIdeNode, IChildSharpIdeNode
{
	public required IExpandableSharpIdeNode Parent { get; set; }
	public required string Path { get; set; }
	public required string Name { get; set; }
	public required List<SharpIdeFile> Files { get; set; }
	public required List<SharpIdeFolder> Folders { get; set; }
	public bool Expanded { get; set; }

	[SetsRequiredMembers]
	public SharpIdeFolder(DirectoryInfo folderInfo, IExpandableSharpIdeNode parent, ConcurrentBag<SharpIdeFile> allFiles)
	{
		Parent = parent;
		Path = folderInfo.FullName;
		Name = folderInfo.Name;
		Files = folderInfo.GetFiles(this, allFiles);
		Folders = this.GetSubFolders(this, allFiles);
	}

	public SharpIdeFolder()
	{

	}
}
