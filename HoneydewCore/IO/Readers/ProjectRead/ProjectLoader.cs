using System.Threading.Tasks;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewExtractors.Metrics.CSharp;
using HoneydewModels;

namespace HoneydewCore.IO.Readers.ProjectRead
{
    public class ProjectLoader : IProjectLoader
    {
        private readonly CSharpFactExtractor _extractor;

        private readonly IProjectProvider _projectProvider;
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;

        public ProjectLoader(CSharpFactExtractor extractor, IProjectProvider projectProvider,
            IProjectLoadingStrategy projectLoadingStrategy)
        {
            _extractor = extractor;
            _projectProvider = projectProvider;
            _projectLoadingStrategy = projectLoadingStrategy;
        }

        public async Task<ProjectModel> Load(string path)
        {
            var solution = await _projectProvider.GetProject(path);

            var projectModel = await _projectLoadingStrategy.Load(solution, _extractor);

            return projectModel;
        }
    }
}
