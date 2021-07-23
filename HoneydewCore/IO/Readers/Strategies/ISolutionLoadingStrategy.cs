using System.Threading.Tasks;
using HoneydewExtractors;
using HoneydewExtractors.Metrics.CSharp;
using HoneydewModels;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public interface ISolutionLoadingStrategy
    {
        Task<SolutionModel> Load(Solution solution, CSharpFactExtractor extractor);
    }
}
