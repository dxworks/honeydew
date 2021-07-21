using System.Threading.Tasks;
using HoneydewExtractors;
using HoneydewModels;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public interface IProjectLoadingStrategy
    {
        Task<ProjectModel> Load(Project project, IFactExtractor extractor);
    }
}
