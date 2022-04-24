using Microsoft.CodeAnalysis;

namespace Honeydew.RepositoryLoading.ProjectRead;

public interface IProjectProvider
{
    Task<Project> GetProject(string path, CancellationToken cancellationToken);
}
