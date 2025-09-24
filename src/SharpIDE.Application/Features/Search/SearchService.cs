using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.Search;

public static class SearchService
{
	public static async Task<List<SearchResult>> FindInFiles(SharpIdeSolutionModel solutionModel, string searchTerm, CancellationToken cancellationToken)
	{
		if (searchTerm.Length < 4)
		{
			return [];
		}

		var timer = Stopwatch.StartNew();
		var files = solutionModel.AllFiles;
		ConcurrentBag<SearchResult> results = [];
		await Parallel.ForEachAsync(files, cancellationToken, async (file, ct) =>
			{
				if (cancellationToken.IsCancellationRequested) return;
				await foreach (var (index, line) in File.ReadLinesAsync(file.Path, ct).Index().WithCancellation(ct))
				{
					if (cancellationToken.IsCancellationRequested) return;
					if (line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
					{
						results.Add(new SearchResult
						{
							File = file,
							Line = index + 1,
							StartColumn = line.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) + 1,
							LineText = line.Trim()
						});
					}
				}
			}
		).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
		timer.Stop();
		Console.WriteLine($"Search completed in {timer.ElapsedMilliseconds} ms. Found {results.Count} results. {(cancellationToken.IsCancellationRequested ? "(Cancelled)" : "")}");
		return results.ToList();
	}
}
