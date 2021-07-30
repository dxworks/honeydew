using System.Threading.Tasks;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead
{
    public interface IProjectLoader
    {
        Task<ProjectModel> Load(string path);
    }
}
