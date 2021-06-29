using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public interface ISolutionLoader
    {
        SolutionModel LoadSolution(string pathToFile);
    }
}