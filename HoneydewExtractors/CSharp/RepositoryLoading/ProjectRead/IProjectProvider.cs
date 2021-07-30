using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead
{
    public interface IProjectProvider
    {
        Task<Project> GetProject(string path);
    }
}
