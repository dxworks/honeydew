using System.Threading.Tasks;
using HoneydewExtractors.Metrics.CSharp;
using HoneydewModels;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public interface IProjectLoadingStrategy
    {
        Task<ProjectModel> Load(Project project, CSharpFactExtractor extractor);
    }
}
