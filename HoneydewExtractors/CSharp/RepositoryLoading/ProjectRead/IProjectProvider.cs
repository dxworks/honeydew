using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.RepositoryLoading.CSharp.ProjectRead
{
    public interface IProjectProvider
    {
        Task<Project> GetProject(string path);
    }
}
