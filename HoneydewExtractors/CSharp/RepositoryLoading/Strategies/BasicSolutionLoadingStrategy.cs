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
        private readonly ILogger _logger;
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;

        public BasicSolutionLoadingStrategy(ILogger logger,
            IProjectLoadingStrategy projectLoadingStrategy)
        {
            _logger = logger;
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
                _logger.Log();
                _logger.Log($"Loading C# Project from {project.FilePath} ({i}/{projectCount})");
                var projectModel = await _projectLoadingStrategy.Load(project, extractor);

                solutionModel.Projects.Add(projectModel);
                i++;
            }

            return solutionModel;
        }
    }
}
