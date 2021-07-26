using System.Threading.Tasks;
using HoneydewModels;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.RepositoryLoading.CSharp.ProjectRead
{
    public interface IProjectLoader
    {
        Task<ProjectModel> Load(string path);
    }
}
