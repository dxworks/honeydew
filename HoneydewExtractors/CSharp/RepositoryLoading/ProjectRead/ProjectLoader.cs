using System;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead
{
    public class ProjectLoader : IProjectLoader
    {
        private readonly IFactExtractorCreator _extractorCreator;
        private readonly ILogger _logger;
        private readonly IProgressLogger _progressLogger;
        private readonly IProjectProvider _projectProvider;
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;

        public ProjectLoader(IFactExtractorCreator extractorCreator, IProjectProvider projectProvider,
            IProjectLoadingStrategy projectLoadingStrategy, ILogger logger, IProgressLogger progressLogger)
        {
            _extractorCreator = extractorCreator;
            _projectProvider = projectProvider;
            _projectLoadingStrategy = projectLoadingStrategy;
            _logger = logger;
            _progressLogger = progressLogger;
        }

        public async Task<ProjectModel> Load(string path)
        {
            try
            {
                var solution = await _projectProvider.GetProject(path);

                var projectModel = await _projectLoadingStrategy.Load(solution, _extractorCreator);

                return projectModel;
            }
            catch (Exception e)
            {
                _logger.Log($"Could not open project from {path} because {e}", LogLevels.Error);
                _progressLogger.Log($"Could not open project from {path} because {e}");
            }

            return null;
        }
    }
}
