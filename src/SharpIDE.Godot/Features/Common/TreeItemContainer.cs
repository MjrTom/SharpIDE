using System.Collections.Specialized;
using Godot;
using ObservableCollections;

namespace SharpIDE.Godot.Features.Common;

public class TreeItemContainer
{
    public TreeItem? Value { get; set; }
}

public static class ObservableTreeExtensions
{
    public static ObservableHashSet<T> WithInitialPopulation<T>(this ObservableHashSet<T> hashSet, Action<ViewChangedEvent<T, TreeItemContainer>> func) where T : class
    {
        foreach (var existing in hashSet)
        {
            var viewChangedEvent = new ViewChangedEvent<T, TreeItemContainer>(NotifyCollectionChangedAction.Add, (existing, new TreeItemContainer()), (null!, null!), -1, -1, new SortOperation<T>());
            func(viewChangedEvent);
        }
        return hashSet;
    }
}