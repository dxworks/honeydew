using System.Collections.Generic;
using HoneydewCore.IO.Readers.Filters;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
     public interface ISolutionLoader
     {
          SolutionModel LoadSolution(string projectPath, IList<PathFilter> filters);

          SolutionModel LoadSolution(string projectPath);
     }
}
