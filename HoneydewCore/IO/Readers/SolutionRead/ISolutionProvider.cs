using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.SolutionRead
{
    public interface ISolutionProvider
    {
        Task<Solution> GetSolution(string path);
    }
}