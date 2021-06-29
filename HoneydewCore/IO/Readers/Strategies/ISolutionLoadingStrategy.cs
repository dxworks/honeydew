using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public interface ISolutionLoadingStrategy
    {
        SolutionModel Load(Solution solution, IList<IFactExtractor> extractors);
    }
}