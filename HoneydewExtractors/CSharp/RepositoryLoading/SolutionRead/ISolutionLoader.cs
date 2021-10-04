using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public interface ISolutionLoader
    {
        Task<SolutionLoadingResult> LoadSolution(string pathToFile, ISet<string> processedProjectPaths);
    }
}
