using System;
using System.Threading;
using System.Threading.Tasks;
using Honeydew.Extractors;
using Honeydew.Logging;
using Honeydew.Models;
using Honeydew.RepositoryLoading.Strategies;
using Microsoft.CodeAnalysis;

namespace Honeydew.RepositoryLoading.ProjectRead;

public class ProjectExtractor
{
    public IFactExtractor FactExtractor { get; }

    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly IProjectLoadingStrategy _projectLoadingStrategy;

    public ProjectExtractor(ILogger logger, IProgressLogger progressLogger,
        IProjectLoadingStrategy projectLoadingStrategy, IFactExtractor factExtractor)
    {
        _logger = logger;
        _progressLogger = progressLogger;
        _projectLoadingStrategy = projectLoadingStrategy;
        FactExtractor = factExtractor;
    }

    public async Task<ProjectModel?> Extract(string filePath, IProjectProvider projectProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectProvider.GetProject(filePath, cancellationToken);

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
            var projectModel = await _projectLoadingStrategy.Load(project, FactExtractor, cancellationToken);
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
