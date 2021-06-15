using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public interface ISolutionLoader
    {
        SolutionModel LoadSolution(string projectPath, ISolutionLoadingStrategy strategy);

        SolutionModel LoadModelFromFile(string pathToModel);
    }
}