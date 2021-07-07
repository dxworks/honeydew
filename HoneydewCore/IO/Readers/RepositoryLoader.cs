using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.ProjectRead;
using HoneydewCore.IO.Readers.SolutionRead;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;
using HoneydewCore.Processors;

namespace HoneydewCore.IO.Readers
{
    public class RepositoryLoader : IRepositoryLoader
    {
        private readonly IList<IFactExtractor> _extractors;
        private const string CsprojExtension = ".csproj";
        private const string SlnExtension = ".sln";

        public RepositoryLoader(IList<IFactExtractor> extractors)
        {
            _extractors = extractors;
        }

        public async Task<RepositoryModel> Load(string path)
        {
            var repositoryModel = new RepositoryModel();

            if (File.Exists(path))
            {
                if (path.EndsWith(SlnExtension))
                {
                    var solutionLoader = new SolutionFileLoader(_extractors, new MsBuildSolutionProvider(),
                        new BasicSolutionLoadingStrategy(new BasicProjectLoadingStrategy()));
                    var solutionModel = await solutionLoader.LoadSolution(path);
                    repositoryModel.Solutions.Add(solutionModel);
                }
                else if (path.EndsWith(CsprojExtension))
                {
                    var projectLoader = new ProjectLoader(_extractors, new MsBuildProjectProvider(),
                        new BasicProjectLoadingStrategy());
                    var projectModel = await projectLoader.Load(path);

                    repositoryModel.Solutions.Add(new SolutionModel
                    {
                        Projects = {projectModel}
                    });
                }
                else
                {
                    throw new SolutionNotFoundException();
                }
            }
            else if (Directory.Exists(path))
            {
                var solutionPaths = Directory.GetFiles(path, $"*{SlnExtension}", SearchOption.AllDirectories);

                foreach (var solutionPath in solutionPaths)
                {
                    var solutionLoader =
                        new SolutionFileLoader(_extractors, new MsBuildSolutionProvider(),
                            new BasicSolutionLoadingStrategy(new BasicProjectLoadingStrategy()));
                    var solutionModel = await solutionLoader.LoadSolution(solutionPath);
                    repositoryModel.Solutions.Add(solutionModel);
                }

                var defaultSolutionModel = new SolutionModel();
                var projectPaths = Directory.GetFiles(path, $"*{CsprojExtension}", SearchOption.AllDirectories);

                foreach (var relativeProjectPath in projectPaths)
                {
                    var projectPath = Path.GetFullPath(relativeProjectPath);
                    
                    var isUsedInASolution = repositoryModel.Solutions.Any(solutionModel =>
                        solutionModel.Projects.Any(project => project.FilePath == projectPath));

                    if (isUsedInASolution) continue;

                    var projectLoader = new ProjectLoader(_extractors, new MsBuildProjectProvider(),
                        new BasicProjectLoadingStrategy());
                    var projectModel = await projectLoader.Load(projectPath);
                    defaultSolutionModel.Projects.Add(projectModel);
                }

                if (defaultSolutionModel.Projects.Count > 0)
                {
                    defaultSolutionModel = AddFullNameToDependencies(defaultSolutionModel);

                    repositoryModel.Solutions.Add(defaultSolutionModel);
                }
            }

            return repositoryModel;
        }

        private static SolutionModel AddFullNameToDependencies(SolutionModel solutionModel)
        {
            var solutionModelProcessable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(new FullNameModelProcessor())
                .Finish<SolutionModel>();

            return solutionModelProcessable.Value;
        }
    }
}