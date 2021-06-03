using System.Collections.Generic;
using HoneydewCore.IO.Readers.Filters;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public interface ISolutionLoader
    {
        void SetPathFilters(IList<PathFilter> filters);

        SolutionModel LoadSolution(string projectPath);
    }
}