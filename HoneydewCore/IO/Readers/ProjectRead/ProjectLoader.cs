using System.Collections.Generic;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers.ProjectRead
{
    public class ProjectLoader : IProjectLoader
    {
        private readonly IList<IFactExtractor> _extractors;

        private readonly IProjectProvider _projectProvider;
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;

        public ProjectLoader(IList<IFactExtractor> extractors, IProjectProvider projectProvider,
            IProjectLoadingStrategy projectLoadingStrategy)
        {
            _extractors = extractors;
            _projectProvider = projectProvider;
            _projectLoadingStrategy = projectLoadingStrategy;
        }

        public async Task<ProjectModel> Load(string path)
        {
            var solution = await _projectProvider.GetProject(path);

            var projectModel = await _projectLoadingStrategy.Load(solution, _extractors);

            return projectModel;
        }
    }
}