using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    public class BasicSolutionLoadingStrategy : ISolutionLoadingStrategy
    {
        private readonly ILogger _logger;
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;
        private readonly IProgressLogger _progressLogger;

        public BasicSolutionLoadingStrategy(ILogger logger, IProjectLoadingStrategy projectLoadingStrategy,
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
                FilePath = solution.FilePath
            };

            var i = 1;
            var projectCount = solution.Projects.Count();

            var progressLogger =
                _progressLogger.CreateProgressLogger(projectCount == 0 ? 1 : projectCount, solution.FilePath);

            progressLogger.Start();

            var dependencyGraph = solution.GetProjectDependencyGraph();

            var projectModels = new List<ProjectModel>();

            foreach (var projectId in dependencyGraph.GetTopologicallySortedProjects())
            {
                var project = solution.GetProject(projectId);
                if (project == null)
                {
                    continue;
                }

                solutionModel.ProjectsPaths.Add(project.FilePath);


                _logger.Log();
                _logger.Log($"Loading C# Project from {project.FilePath} ({i}/{projectCount})");

                var projectModel = await _projectLoadingStrategy.Load(project, extractorCreator);

                progressLogger.Step($"{project.FilePath}");

                if (processedProjectsPaths.Contains(project.FilePath))
                {
                    _progressLogger.Log($"Skipping {project.FilePath}. Was already processed ({i}/{projectCount})");
                    _logger.Log($"Skipping {project.FilePath}. Was already processed ({i}/{projectCount})");
                }
                else
                {
                    if (projectModel != null)
                    {
                        projectModels.Add(projectModel);
                    }
                    else
                    {
                        _logger.Log($"Skipping {project.FilePath} ({i}/{projectCount})", LogLevels.Warning);
                    }
                }

                i++;
            }

            progressLogger.Stop();

            return new SolutionLoadingResult(solutionModel, projectModels);
        }
    }
}
