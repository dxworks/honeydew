using System.Threading.Tasks;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public interface ISolutionLoader
    {
        Task<SolutionModel> LoadSolution(string pathToFile);
    }
}