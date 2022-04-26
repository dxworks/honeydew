using Honeydew.Models;

namespace Honeydew.Extractors.Load;

public interface IProjectLoadingStrategy<in TProject>
{
    Task<ProjectModel> Load(TProject project, IFactExtractor factExtractor, CancellationToken cancellationToken);
}
