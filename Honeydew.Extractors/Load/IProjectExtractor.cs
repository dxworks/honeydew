using Honeydew.Models;

namespace Honeydew.Extractors.Load;

public interface IProjectExtractor
{
    Task<ProjectModel?> Extract(string filePath, CancellationToken cancellationToken);
}
