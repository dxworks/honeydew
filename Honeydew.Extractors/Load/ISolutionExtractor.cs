namespace Honeydew.Extractors.Load;

public interface ISolutionExtractor
{
    Task<SolutionLoadingResult?> Extract(string path, ISet<string> processedProjectPaths,
        CancellationToken cancellationToken);
}
