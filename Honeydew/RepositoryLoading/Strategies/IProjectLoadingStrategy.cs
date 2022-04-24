using System.Threading;
using System.Threading.Tasks;
using Honeydew.Extractors;
using Honeydew.Models;
using Microsoft.CodeAnalysis;

namespace Honeydew.RepositoryLoading.Strategies;

public interface IProjectLoadingStrategy
{
    Task<ProjectModel> Load(Project project, IFactExtractor factExtractor, CancellationToken cancellationToken);
}
