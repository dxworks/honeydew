using System.Threading.Tasks;
using HoneydewExtractors;
using HoneydewModels;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public interface ISolutionLoadingStrategy
    {
        Task<SolutionModel> Load(Solution solution, IFactExtractor extractor);
    }
}
