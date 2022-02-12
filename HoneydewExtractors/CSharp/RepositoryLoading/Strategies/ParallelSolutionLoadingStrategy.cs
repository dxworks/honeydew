using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies;

public class ParallelSolutionLoadingStrategy : ISolutionLoadingStrategy
{
    private readonly ILogger _logger;
    private readonly IProjectLoadingStrategy _projectLoadingStrategy;
    private readonly IProgressLogger _progressLogger;

    public ParallelSolutionLoadingStrategy(ILogger logger, IProjectLoadingStrategy projectLoadingStrategy,
        IProgressLogger progressLogger)
    {
        _logger = logger;
        _projectLoadingStrategy = projectLoadingStrategy;
        _progressLogger = progressLogger;
    }

    public async Task<SolutionLoadingResult> Load(Solution solution, IFactExtractorCreator extractorCreator,
        ISet<string> processedProjectsPaths)
    {
        SolutionModel solutionModel = new()
        {
            FilePath = ActualFilePathProvider.GetActualFilePath(solution.FilePath)
        };

        var projectCount = solution.Projects.Count();

        var progressLogger =
            _progressLogger.CreateProgressLogger(projectCount == 0 ? 1 : projectCount, solutionModel.FilePath);

        progressLogger.Start();

        var dependencyGraph = solution.GetProjectDependencyGraph();

        var projectModels = new List<ProjectModel>();


        var processedSolutionCount = 1;

        await Parallel.ForEachAsync(dependencyGraph.GetTopologicallySortedProjects(), async (projectId, _) =>
        {
            var project = solution.GetProject(projectId);
            if (project != null)
            {
                var projectFilePath = ActualFilePathProvider.GetActualFilePath(project.FilePath);
                solutionModel.ProjectsPaths.Add(projectFilePath);


                _logger.Log();
                _logger.Log($"Loading C# Project from {projectFilePath} ({processedSolutionCount}/{projectCount})");

                var projectModel = await _projectLoadingStrategy.Load(project, extractorCreator);

                progressLogger.Step($"{projectFilePath}");

                if (processedProjectsPaths.Contains(projectFilePath))
                {
                    _progressLogger.Log(
                        $"Skipping {projectFilePath}. Was already processed ({processedSolutionCount}/{projectCount})");
                    _logger.Log(
                        $"Skipping {projectFilePath}. Was already processed ({processedSolutionCount}/{projectCount})");
                }
                else
                {
                    if (projectModel != null)
                    {
                        projectModels.Add(projectModel);
                    }
                    else
                    {
                        _logger.Log($"Skipping {projectFilePath} ({processedSolutionCount}/{projectCount})",
                            LogLevels.Warning);
                    }
                }

                Interlocked.Increment(ref processedSolutionCount);
            }
        });
        progressLogger.Stop();

        return new SolutionLoadingResult(solutionModel, projectModels);
    }
}
