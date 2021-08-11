using System;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead
{
    public class ProjectLoader : IProjectLoader
    {
        private readonly CSharpFactExtractor _extractor;
        private readonly ILogger _logger;
        private readonly IProjectProvider _projectProvider;
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;

        public ProjectLoader(CSharpFactExtractor extractor, IProjectProvider projectProvider,
            IProjectLoadingStrategy projectLoadingStrategy, ILogger logger)
        {
            _extractor = extractor;
            _projectProvider = projectProvider;
            _projectLoadingStrategy = projectLoadingStrategy;
            _logger = logger;
        }

        public async Task<ProjectModel> Load(string path)
        {
            try
            {
                var solution = await _projectProvider.GetProject(path);

                var projectModel = await _projectLoadingStrategy.Load(solution, _extractor);

                return projectModel;
            }
            catch (Exception e)
            {
                _logger.Log($"Could not open project from {path} because {e}", LogLevels.Error);
            }

            return null;
        }
    }
}
