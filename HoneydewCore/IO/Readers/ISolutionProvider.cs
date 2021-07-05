using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers
{
    public interface ISolutionProvider
    {
        Task<Solution> GetSolution(string path);
    }
}