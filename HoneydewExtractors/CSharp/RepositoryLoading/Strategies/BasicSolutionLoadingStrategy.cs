﻿using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics;
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

        public async Task<SolutionModel> Load(Solution solution, CSharpFactExtractor extractor)
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

            foreach (var projectId in dependencyGraph.GetTopologicallySortedProjects())
            {
                var project = solution.GetProject(projectId);
                if (project == null)
                {
                    continue;
                }

                if (project.Language != "C#") // currently only c# projects are supported
                {
                    _logger.Log();
                    _logger.Log($"{project.Language} type projects are not currently supported!", LogLevels.Warning);
                    _logger.Log($"Skipping {project.FilePath} ({i}/{projectCount})", LogLevels.Warning);
                    progressLogger.Step($"{project.FilePath}");
                    i++;
                    continue;
                }

                _logger.Log();
                _logger.Log($"Loading C# Project from {project.FilePath} ({i}/{projectCount})");
                var projectModel = await _projectLoadingStrategy.Load(project, extractor);

                progressLogger.Step($"{project.FilePath}");

                solutionModel.Projects.Add(projectModel);
                i++;
            }

            progressLogger.Stop();

            return solutionModel;
        }
    }
}
