using System.Threading.Tasks;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public interface ISolutionLoader
    {
        Task<SolutionModel> LoadSolution(string pathToFile);
    }
}
