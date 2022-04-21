using System.Threading;
using System.Threading.Tasks;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;
using Microsoft.CodeAnalysis;

namespace Honeydew.RepositoryLoading.Strategies;

public interface IProjectLoadingStrategy
{
    Task<ProjectModel> Load(Project project, IFactExtractor factExtractor, CancellationToken cancellationToken);
}
