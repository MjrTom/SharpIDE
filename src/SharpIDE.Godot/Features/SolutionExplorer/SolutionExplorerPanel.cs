using Ardalis.GuardClauses;
using Godot;
using SharpIDE.Application.Features.Analysis;
using SharpIDE.Application.Features.SolutionDiscovery;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;
using SharpIDE.Godot.Features.Problems;

namespace SharpIDE.Godot.Features.SolutionExplorer;

public partial class SolutionExplorerPanel : MarginContainer
{
	[Export]
	public Texture2D CsharpFileIcon { get; set; } = null!;
	[Export]
	public Texture2D FolderIcon { get; set; } = null!;
	[Export]
	public Texture2D SlnFolderIcon { get; set; } = null!;
	[Export]
	public Texture2D CsprojIcon { get; set; } = null!;
	[Export]
	public Texture2D SlnIcon { get; set; } = null!;
	
	public SharpIdeSolutionModel SolutionModel { get; set; } = null!;
	private Tree _tree = null!;
	public override void _Ready()
	{
		_tree = GetNode<Tree>("Tree");
		_tree.ItemMouseSelected += TreeOnItemMouseSelected;
		GodotGlobalEvents.Instance.FileExternallySelected.Subscribe(OnFileExternallySelected);
	}

	private void TreeOnItemMouseSelected(Vector2 mousePosition, long mouseButtonIndex)
	{
		var selected = _tree.GetSelected();
		if (selected is null) return;
		
		var mouseButtonMask = (MouseButtonMask)mouseButtonIndex;

		var genericMetadata = selected.GetMetadata(0).As<RefCounted?>();
		switch (mouseButtonMask, genericMetadata)
		{
			case (MouseButtonMask.Left, RefCountedContainer<SharpIdeFile> fileContainer): GodotGlobalEvents.Instance.FileSelected.InvokeParallelFireAndForget(fileContainer.Item, null); break;
			case (MouseButtonMask.Right, RefCountedContainer<SharpIdeFile> fileContainer): OpenContextMenuFile(fileContainer.Item); break;
			case (MouseButtonMask.Left, RefCountedContainer<SharpIdeProjectModel>): break;
			case (MouseButtonMask.Right, RefCountedContainer<SharpIdeProjectModel> projectContainer): OpenContextMenuProject(projectContainer.Item); break;
			case (MouseButtonMask.Left, RefCountedContainer<SharpIdeFolder>): break;
			case (MouseButtonMask.Right, RefCountedContainer<SharpIdeFolder> folderContainer): OpenContextMenuFolder(folderContainer.Item); break;
			case (MouseButtonMask.Left, RefCountedContainer<SharpIdeSolutionFolder>): break;
			default: break;
		}
	}
	
	private void OpenContextMenuFolder(SharpIdeFolder folder)
	{
		var menu = new PopupMenu();
		AddChild(menu);
		menu.AddItem("Reveal in File Explorer", 0);
		menu.PopupHide += () => menu.QueueFree();
		menu.IdPressed += id =>
		{
			if (id is 0)
			{
				// Reveal in File Explorer
				OS.ShellOpen(folder.Path);
			}
		};
			
		var globalMousePosition = GetGlobalMousePosition();
		menu.Position = new Vector2I((int)globalMousePosition.X, (int)globalMousePosition.Y);
		menu.Popup();
	}
	
	private void OpenContextMenuProject(SharpIdeProjectModel project)
	{
		var menu = new PopupMenu();
		AddChild(menu);
		menu.AddItem("Run", 0);
		menu.AddSeparator();
		menu.AddItem("Build", 1);
		menu.AddItem("Rebuild", 2);
		menu.AddItem("Clean", 3);
		menu.PopupHide += () =>
		{
			GD.Print("QueueFree menu");
			menu.QueueFree();
		};
		menu.IdPressed += id =>
		{
			if (id is 0)
			{
				// Run project
			}
			if (id is 1)
			{
				// Build project
			}
			else if (id is 2)
			{
				// Rebuild project
			}
			else if (id is 3)
			{
				// Clean project
			}
		};
			
		var globalMousePosition = GetGlobalMousePosition();
		menu.Position = new Vector2I((int)globalMousePosition.X, (int)globalMousePosition.Y);
		menu.Popup();
	}
	
	private void OpenContextMenuFile(SharpIdeFile file)
	{
		var menu = new PopupMenu();
		AddChild(menu);
		menu.AddItem("Open", 0);
		menu.AddItem("Reveal in File Explorer", 1);
		menu.AddSeparator();
		menu.AddItem("Copy Full Path", 2);
		menu.PopupHide += () =>
		{
			GD.Print("QueueFree menu");
			menu.QueueFree();
		};
		menu.IdPressed += id =>
		{
			if (id is 0)
			{
				GodotGlobalEvents.Instance.FileSelected.InvokeParallelFireAndForget(file, null);
			}
			else if (id is 1)
			{
				OS.ShellOpen(Path.GetDirectoryName(file.Path)!);
			}
			else if (id is 2)
			{
				DisplayServer.ClipboardSet(file.Path);
			}
		};
			
		var globalMousePosition = GetGlobalMousePosition();
		menu.Position = new Vector2I((int)globalMousePosition.X, (int)globalMousePosition.Y);
		menu.Popup();
	}
	
	private async Task OnFileExternallySelected(SharpIdeFile file, SharpIdeFileLinePosition? fileLinePosition)
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		var task = GodotGlobalEvents.Instance.FileSelected.InvokeParallelAsync(file, fileLinePosition);
		var item = FindItemRecursive(_tree.GetRoot(), file);
		if (item is not null)
		{
			await this.InvokeAsync(() =>
			{
				item.UncollapseTree();
				_tree.SetSelected(item, 0);
				_tree.ScrollToItem(item, true);
				_tree.QueueRedraw();
			});
		}
		await task.ConfigureAwait(false);
	}
	
	private static TreeItem? FindItemRecursive(TreeItem item, SharpIdeFile file)
	{
		if (item.GetTypedMetadata<RefCountedContainer<SharpIdeFile>?>(0)?.Item == file)
			return item;

		var child = item.GetFirstChild();
		while (child != null)
		{
			var result = FindItemRecursive(child, file);
			if (result != null)
				return result;

			child = child.GetNext();
		}

		return null;
	}

	public void RepopulateTree()
	{
		_tree.Clear();

		var rootItem = _tree.CreateItem();
		rootItem.SetText(0, SolutionModel.Name);
		rootItem.SetIcon(0, SlnIcon);

		// Add projects directly under solution
		foreach (var project in SolutionModel.Projects)
		{
			AddProjectToTree(rootItem, project);
		}

		// Add folders under solution
		foreach (var folder in SolutionModel.Folders)
		{
			AddSlnFolderToTree(rootItem, folder);
		}
		rootItem.SetCollapsedRecursive(true);
		rootItem.Collapsed = false;
	}

	private void AddSlnFolderToTree(TreeItem parent, SharpIdeSolutionFolder folder)
	{
		var folderItem = _tree.CreateItem(parent);
		folderItem.SetText(0, folder.Name);
		folderItem.SetIcon(0, SlnFolderIcon);
		var container = new RefCountedContainer<SharpIdeSolutionFolder>(folder);
		folderItem.SetMetadata(0, container);

		foreach (var project in folder.Projects)
		{
			AddProjectToTree(folderItem, project);
		}

		foreach (var subFolder in folder.Folders)
		{
			AddSlnFolderToTree(folderItem, subFolder); // recursion
		}

		foreach (var sharpIdeFile in folder.Files)
		{
			AddFileToTree(folderItem, sharpIdeFile);
		}
	}

	private void AddProjectToTree(TreeItem parent, SharpIdeProjectModel project)
	{
		var projectItem = _tree.CreateItem(parent);
		projectItem.SetText(0, project.Name);
		projectItem.SetIcon(0, CsprojIcon);
		var container = new RefCountedContainer<SharpIdeProjectModel>(project);
		projectItem.SetMetadata(0, container);

		foreach (var sharpIdeFolder in project.Folders)
		{
			AddFolderToTree(projectItem, sharpIdeFolder);
		}

		foreach (var file in project.Files)
		{
			AddFileToTree(projectItem, file);
		}
	}

	private void AddFolderToTree(TreeItem projectItem, SharpIdeFolder sharpIdeFolder)
	{
		var folderItem = _tree.CreateItem(projectItem);
		folderItem.SetText(0, sharpIdeFolder.Name);
		folderItem.SetIcon(0, FolderIcon);
		var container = new RefCountedContainer<SharpIdeFolder>(sharpIdeFolder);
		folderItem.SetMetadata(0, container);

		foreach (var subFolder in sharpIdeFolder.Folders)
		{
			AddFolderToTree(folderItem, subFolder); // recursion
		}

		foreach (var file in sharpIdeFolder.Files)
		{
			AddFileToTree(folderItem, file);
		}
	}

	private void AddFileToTree(TreeItem parent, SharpIdeFile file)
	{
		var fileItem = _tree.CreateItem(parent);
		fileItem.SetText(0, file.Name);
		fileItem.SetIcon(0, CsharpFileIcon);
		var container = new RefCountedContainer<SharpIdeFile>(file);
		fileItem.SetMetadata(0, container);
	}
}
