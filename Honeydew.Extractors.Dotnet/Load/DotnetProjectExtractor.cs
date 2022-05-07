using Honeydew.Extractors.Load;
using Honeydew.Logging;
using Honeydew.Models;
using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors.Dotnet.Load;

public class DotnetProjectExtractor : IProjectExtractor
{
    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly IDotnetProjectLoadingStrategy _projectLoadingStrategy;
    private readonly IFactExtractor _factExtractor;
    private readonly IProjectProvider<Project> _projectProvider;

    public DotnetProjectExtractor(ILogger logger, IProgressLogger progressLogger,
        IDotnetProjectLoadingStrategy projectLoadingStrategy, IFactExtractor factExtractor,
        IProjectProvider<Project> projectProvider)
    {
        _logger = logger;
        _progressLogger = progressLogger;
        _projectLoadingStrategy = projectLoadingStrategy;
        _factExtractor = factExtractor;
        _projectProvider = projectProvider;
    }

    public async Task<ProjectModel?> Extract(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var project = await _projectProvider.GetProject(filePath, cancellationToken);

            return await Extract(project, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.Log($"Could not extract from project {filePath} because {e}", LogLevels.Error);
            _progressLogger.Log($"Could not extract from project {filePath} because {e}");
            return null;
        }
    }

    public async Task<ProjectModel?> Extract(Project project, CancellationToken cancellationToken)
    {
        try
        {
            var projectModel = await _projectLoadingStrategy.Load(project, _factExtractor, cancellationToken);
            projectModel.Language = project.Language;

            return projectModel;
        }
        catch (Exception e)
        {
            _logger.Log($"Could extract from project {project.FilePath} because {e}", LogLevels.Error);
            _progressLogger.Log($"Could extract from project {project.FilePath} because {e}");
        }

        return null;
    }
}
