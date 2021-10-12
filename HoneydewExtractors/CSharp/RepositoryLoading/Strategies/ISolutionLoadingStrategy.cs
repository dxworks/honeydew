using System.Collections.Generic;
using System.Threading.Tasks;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    public interface ISolutionLoadingStrategy
    {
        Task<SolutionLoadingResult> Load(Solution solution, IFactExtractorCreator extractorCreator,
            ISet<string> processedProjectsPaths);
    }
}
