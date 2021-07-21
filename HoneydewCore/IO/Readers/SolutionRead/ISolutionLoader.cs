using System.Threading.Tasks;
using HoneydewCore.Models;
using HoneydewModels;

namespace HoneydewCore.IO.Readers.SolutionRead
{
    public interface ISolutionLoader
    {
        Task<SolutionModel> LoadSolution(string pathToFile);
    }
}
