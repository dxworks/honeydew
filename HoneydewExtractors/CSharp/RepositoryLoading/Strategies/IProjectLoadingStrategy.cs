using System.Threading.Tasks;
using HoneydewExtractors.Core;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    public interface IProjectLoadingStrategy
    {
        Task<ProjectModel> Load(Project project, IFactExtractorCreator extractorCreator);
    }
}
