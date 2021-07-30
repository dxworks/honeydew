using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public interface ISolutionProvider
    {
        Task<Solution> GetSolution(string path);
    }
}
