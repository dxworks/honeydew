using System.Threading.Tasks;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    public interface ISolutionLoadingStrategy
    {
        Task<SolutionModel> Load(Solution solution, CSharpFactExtractor extractor);
    }
}
