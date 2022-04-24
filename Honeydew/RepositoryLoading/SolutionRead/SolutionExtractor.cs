using Honeydew.Extractors;
using Honeydew.Extractors.Load;
using Honeydew.Logging;
using Honeydew.Models;
using Honeydew.RepositoryLoading.ProjectRead;
using Honeydew.RepositoryLoading.Strategies;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Honeydew.RepositoryLoading.SolutionRead;

public class SolutionExtractor : ISolutionExtractor
{
    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly ProjectExtractorFactory _projectExtractorFactory;
    private readonly ActualFilePathProvider _actualFilePathProvider;

    public SolutionExtractor(ILogger logger, IProgressLogger progressLogger,
        ProjectExtractorFactory projectExtractorFactory, ActualFilePathProvider actualFilePathProvider)
    {
        _logger = logger;
        _progressLogger = progressLogger;
        _projectExtractorFactory = projectExtractorFactory;
        _actualFilePathProvider = actualFilePathProvider;
    }

    public async Task<SolutionLoadingResult?> Extract(string path, ISet<string> processedProjectPaths,
        CancellationToken cancellationToken)
    {
        _logger.Log();
        _logger.Log();
        _logger.Log($"Opening the solution from {path}");

        try
        {
            var solution = await GetSolution(path, cancellationToken);
            _logger.Log($"Found {solution.Projects.Count()} Projects in solution {path}");

            SolutionModel solutionModel = new()
            {
                FilePath = _actualFilePathProvider.GetActualFilePath(solution.FilePath)
            };

            var i = 1;
            var projectCount = solution.Projects.Count();

            var progressLogger =
                _progressLogger.CreateProgressLogger(projectCount == 0 ? 1 : projectCount, solutionModel.FilePath);

            progressLogger.Start();

            var dependencyGraph = solution.GetProjectDependencyGraph();

            var projectModels = new List<ProjectModel>();

            foreach (var projectId in dependencyGraph.GetTopologicallySortedProjects())
            {
                var project = solution.GetProject(projectId);
                if (project == null)
                {
                    _logger.Log($"Could not open open project with id {projectId} ({i}/{projectCount})",
                        LogLevels.Error);
                    _progressLogger.Log($"Could not open open project with id {projectId} ({i}/{projectCount})");
                    i++;

                    continue;
                }

                var projectFilePath = _actualFilePathProvider.GetActualFilePath(project.FilePath);
                solutionModel.ProjectsPaths.Add(projectFilePath);

                if (processedProjectPaths.Contains(projectFilePath))
                {
                    _progressLogger.Log($"Skipping {projectFilePath}. Was already processed ({i}/{projectCount})");
                    _logger.Log($"Skipping {projectFilePath}. Was already processed ({i}/{projectCount})");
                }
                else
                {
                    var projectExtractor = _projectExtractorFactory.GetProjectExtractor(project.Language);

                    if (projectExtractor == null)
                    {
                        _logger.Log(
                            $"Extractor for {project.Language} project not implemented. Skipping {projectFilePath} ({i}/{projectCount})",
                            LogLevels.Error);
                        _progressLogger.Log(
                            $"Extractor for {project.Language} project not implemented. Skipping {projectFilePath} ({i}/{projectCount})");
                        i++;

                        continue;
                    }

                    _logger.Log();
                    _logger.Log($"Loading {project.Language} Project from {projectFilePath} ({i}/{projectCount})");

                    var projectModel = await projectExtractor.Extract(project, cancellationToken);

                    progressLogger.Step($"{projectFilePath}");


                    if (projectModel != null)
                    {
                        projectModel.Name = project.Name;
                        projectModel.FilePath = projectFilePath;

                        projectModels.Add(projectModel);
                    }
                    else
                    {
                        _logger.Log($"Skipping {projectFilePath} ({i}/{projectCount})", LogLevels.Warning);
                    }
                }

                i++;
            }

            progressLogger.Stop();

            return new SolutionLoadingResult(solutionModel, projectModels);
        }
        catch (Exception e)
        {
            _logger.Log($"Could not open solution from {path} because {e}", LogLevels.Error);
            _progressLogger.Log($"Could not open solution from {path} because {e}");
        }

        return null;
    }

    private static Task<Solution> GetSolution(string path, CancellationToken cancellationToken)
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        var msBuildWorkspace = MSBuildWorkspace.Create();

        if (!msBuildWorkspace.Diagnostics.IsEmpty)
        {
            throw new ProjectWithErrorsException();
        }

        return msBuildWorkspace.OpenSolutionAsync(path, cancellationToken: cancellationToken);
    }
}
