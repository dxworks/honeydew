using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public interface ISolutionLoader
    {
        SolutionModel LoadSolution(string pathToSolution, ISolutionLoadingStrategy solutionLoadingStrategy);

        SolutionModel LoadModelFromFile(IFileReader fileReader, string pathToModel);
    }
}