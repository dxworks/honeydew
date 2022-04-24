using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors.Load;

public interface IProjectProvider
{
    Task<Project> GetProject(string path, CancellationToken cancellationToken);
}
