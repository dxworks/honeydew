using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Honeydew.RepositoryLoading.ProjectRead;

public interface IProjectProvider
{
    Task<Project> GetProject(string path);
}
