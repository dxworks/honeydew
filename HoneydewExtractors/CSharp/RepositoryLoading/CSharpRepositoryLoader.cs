using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.IO.Readers;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead;
using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.RepositoryLoading
{
    public class CSharpRepositoryLoader : IRepositoryLoader<RepositoryModel>
    {
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;
        private readonly ILogger _logger;
        private readonly CSharpFactExtractor _extractor;
        private const string CsprojExtension = ".csproj";
        private const string SlnExtension = ".sln";

        public CSharpRepositoryLoader(IProjectLoadingStrategy projectLoadingStrategy,
            ISolutionLoadingStrategy solutionLoadingStrategy, ILogger logger,
            CSharpFactExtractor extractor)
        {
            _projectLoadingStrategy = projectLoadingStrategy;
            _solutionLoadingStrategy = solutionLoadingStrategy;
            _logger = logger;
            _extractor = extractor;
        }

        public async Task<RepositoryModel> Load(string path)
        {
            var repositoryModel = new RepositoryModel();

            if (File.Exists(path))
            {
                if (path.EndsWith(SlnExtension))
                {
                    _logger.Log($"Solution file found at {path}");

                    var solutionLoader = new SolutionFileLoader(_logger, _extractor,
                        new MsBuildSolutionProvider(),
                        _solutionLoadingStrategy);
                    var solutionModel = await solutionLoader.LoadSolution(path);
                    if (solutionModel != null)
                    {
                        repositoryModel.Solutions.Add(solutionModel);
                    }
                }
                else if (path.EndsWith(CsprojExtension))
                {
                    _logger.Log($"C# Project file found at {path}");

                    var projectLoader = new ProjectLoader(_extractor, new MsBuildProjectProvider(),
                        _projectLoadingStrategy, _logger);
                    var projectModel = await projectLoader.Load(path);

                    if (projectModel != null)
                    {
                        repositoryModel.Solutions.Add(new SolutionModel
                        {
                            Projects = {projectModel}
                        });
                    }
                }
                else
                {
                    _logger.Log($"No Solution file or C# Project file found at {path}");

                    throw new SolutionNotFoundException();
                }
            }
            else if (Directory.Exists(path))
            {
                _logger.Log($"Searching for solution files at {path}");

                var solutionPaths = Directory.GetFiles(path, $"*{SlnExtension}", SearchOption.AllDirectories);

                _logger.Log($"Found {solutionPaths.Length} Solutions");

                foreach (var solutionPath in solutionPaths)
                {
                    var solutionLoader =
                        new SolutionFileLoader(_logger, _extractor, new MsBuildSolutionProvider(),
                            _solutionLoadingStrategy);
                    var solutionModel = await solutionLoader.LoadSolution(solutionPath);
                    if (solutionModel != null)
                    {
                        repositoryModel.Solutions.Add(solutionModel);
                    }
                }

                _logger.Log(
                    $"Searching for C# Project files that are not in any of the found solutions at {path}");

                var defaultSolutionModel = new SolutionModel();
                var projectPaths = Directory.GetFiles(path, $"*{CsprojExtension}", SearchOption.AllDirectories);

                foreach (var relativeProjectPath in projectPaths)
                {
                    var projectPath = Path.GetFullPath(relativeProjectPath);

                    var isUsedInASolution = repositoryModel.Solutions.Any(solutionModel =>
                        solutionModel.Projects.Any(project => project.FilePath == projectPath));

                    if (isUsedInASolution) continue;

                    _logger.Log($"C# Project file found at {projectPath}");

                    var projectLoader = new ProjectLoader(_extractor, new MsBuildProjectProvider(),
                        _projectLoadingStrategy, _logger);
                    var projectModel = await projectLoader.Load(projectPath);
                    if (projectModel != null)
                    {
                        defaultSolutionModel.Projects.Add(projectModel);
                    }
                }

                if (defaultSolutionModel.Projects.Count > 0)
                {
                    _logger.Log(
                        $"{defaultSolutionModel.Projects.Count} C# Projects were found that didn't belong to any solution file");

                    repositoryModel.Solutions.Add(defaultSolutionModel);
                }
            }

            return repositoryModel;
        }
    }
}
