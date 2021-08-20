using System.Threading.Tasks;
using HoneydewExtractors.Core;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    public interface ISolutionLoadingStrategy
    {
        Task<SolutionModel> Load(Solution solution, IFactExtractorCreator extractorCreator);
    }
}
