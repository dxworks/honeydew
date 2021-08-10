using System;
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
        private readonly ISolutionProvider _solutionProvider;
        private readonly IProjectProvider _projectProvider;
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;
        private readonly ILogger _logger;
        private readonly IProgressLoggerFactory _progressLoggerFactory;
        private readonly CSharpFactExtractor _extractor;
        private const string CsprojExtension = ".csproj";
        private const string SlnExtension = ".sln";

        public CSharpRepositoryLoader(ISolutionProvider solutionProvider, IProjectProvider projectProvider,
            IProjectLoadingStrategy projectLoadingStrategy, ISolutionLoadingStrategy solutionLoadingStrategy,
            ILogger logger, IProgressLoggerFactory progressLoggerFactory, CSharpFactExtractor extractor)
        {
            _projectLoadingStrategy = projectLoadingStrategy;
            _solutionLoadingStrategy = solutionLoadingStrategy;
            _logger = logger;
            _progressLoggerFactory = progressLoggerFactory;
            _extractor = extractor;
            _projectProvider = projectProvider;
            _solutionProvider = solutionProvider;
        }

        public async Task<RepositoryModel> Load(string path)
        {
            var repositoryModel = new RepositoryModel();

            if (File.Exists(path))
            {
                if (path.EndsWith(SlnExtension))
                {
                    var progressLogger = _progressLoggerFactory.CreateProgressLogger(1, path, null, ConsoleColor.Yellow);
                    progressLogger.Start();
                    _logger.Log($"Solution file found at {path}");

                    var solutionLoader = new SolutionFileLoader(_logger, _extractor,
                        _solutionProvider,
                        _solutionLoadingStrategy);
                    var solutionModel = await solutionLoader.LoadSolution(path);

                    if (solutionModel != null)
                    {
                        progressLogger.Step(solutionModel.FilePath);
                        repositoryModel.Solutions.Add(solutionModel);
                    }

                    progressLogger.Stop();
                }
                else if (path.EndsWith(CsprojExtension))
                {
                    var progressLogger = _progressLoggerFactory.CreateProgressLogger(1, path, null, ConsoleColor.Yellow);
                    progressLogger.Start();

                    _logger.Log($"C# Project file found at {path}");

                    var projectLoader = new ProjectLoader(_extractor, _projectProvider,
                        _projectLoadingStrategy, _logger);
                    var projectModel = await projectLoader.Load(path);

                    if (projectModel != null)
                    {
                        progressLogger.Step(projectModel.FilePath);

                        repositoryModel.Solutions.Add(new SolutionModel
                        {
                            Projects = { projectModel }
                        });
                    }

                    progressLogger.Stop();
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

                var progressLogger = _progressLoggerFactory.CreateProgressLogger(solutionPaths.Length, path, null, ConsoleColor.Yellow);
                progressLogger.Start();

                foreach (var solutionPath in solutionPaths)
                {
                    var solutionLoader =
                        new SolutionFileLoader(_logger, _extractor, _solutionProvider,
                            _solutionLoadingStrategy);
                    var solutionModel = await solutionLoader.LoadSolution(solutionPath);
                    if (solutionModel != null)
                    {
                        progressLogger.Step(solutionModel.FilePath);
                        repositoryModel.Solutions.Add(solutionModel);
                    }
                }

                progressLogger.Stop();

                _logger.Log(
                    $"Searching for C# Project files that are not in any of the found solutions at {path}");

                var defaultSolutionModel = new SolutionModel();
                var projectPaths = Directory.GetFiles(path, $"*{CsprojExtension}", SearchOption.AllDirectories);

                // progressLogger = _progressLoggerFactory.CreateProgressLogger(projectPaths.Length, path, null, ConsoleColor.Yellow);

                progressLogger.Start();

                foreach (var relativeProjectPath in projectPaths)
                {
                    var projectPath = Path.GetFullPath(relativeProjectPath);

                    var isUsedInASolution = repositoryModel.Solutions.Any(solutionModel =>
                        solutionModel.Projects.Any(project => project.FilePath == projectPath));

                    if (isUsedInASolution) continue;

                    progressLogger.Step(projectPath);

                    _logger.Log($"C# Project file found at {projectPath}");

                    var projectLoader = new ProjectLoader(_extractor, _projectProvider,
                        _projectLoadingStrategy, _logger);
                    var projectModel = await projectLoader.Load(projectPath);
                    if (projectModel != null)
                    {
                        defaultSolutionModel.Projects.Add(projectModel);
                    }
                }

                progressLogger.Stop();

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
