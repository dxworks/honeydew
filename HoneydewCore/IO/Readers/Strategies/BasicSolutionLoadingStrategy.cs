using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.Logging;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public class BasicSolutionLoadingStrategy : ISolutionLoadingStrategy
    {
        private readonly IProgressLogger _progressLogger;
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;

        public BasicSolutionLoadingStrategy(IProgressLogger progressLogger,
            IProjectLoadingStrategy projectLoadingStrategy)
        {
            _progressLogger = progressLogger;
            _projectLoadingStrategy = projectLoadingStrategy;
        }

        public async Task<SolutionModel> Load(Solution solution, IList<IFactExtractor> extractors)
        {
            SolutionModel solutionModel = new();
            
            var i = 1;
            var projectCount = solution.Projects.Count();
            foreach (var project in solution.Projects)
            {
                _progressLogger.LogLine();
                _progressLogger.LogLine($"Loading C# Project from {project.FilePath} ({i}/{projectCount})");
                var projectModel = await _projectLoadingStrategy.Load(project, extractors);

                solutionModel.Projects.Add(projectModel);
                i++;
            }

            return solutionModel;
        }
    }
}