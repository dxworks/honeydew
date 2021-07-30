using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
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

        public async Task<SolutionModel> Load(Solution solution, CSharpFactExtractor extractor)
        {
            SolutionModel solutionModel = new()
            {
                FilePath = solution.FilePath
            };

            var i = 1;
            var projectCount = solution.Projects.Count();
            foreach (var project in solution.Projects)
            {
                _progressLogger.LogLine();
                _progressLogger.LogLine($"Loading C# Project from {project.FilePath} ({i}/{projectCount})");
                var projectModel = await _projectLoadingStrategy.Load(project, extractor);

                solutionModel.Projects.Add(projectModel);
                i++;
            }

            return solutionModel;
        }
    }
}
