using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.IO.Readers;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
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
        private readonly IProgressLogger _progressLogger;
        private readonly IFactExtractorCreator _extractorCreator;
        private const string CsprojExtension = ".csproj";
        private const string SlnExtension = ".sln";

        public CSharpRepositoryLoader(ISolutionProvider solutionProvider, IProjectProvider projectProvider,
            IProjectLoadingStrategy projectLoadingStrategy, ISolutionLoadingStrategy solutionLoadingStrategy,
            ILogger logger, IProgressLogger progressLogger, IFactExtractorCreator extractorCreator)
        {
            _projectLoadingStrategy = projectLoadingStrategy;
            _solutionLoadingStrategy = solutionLoadingStrategy;
            _logger = logger;
            _progressLogger = progressLogger;
            _extractorCreator = extractorCreator;
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
                    _logger.Log($"Solution file found at {path}");
                    _progressLogger.Log($"Solution file found at {path}");
                    _progressLogger.Log();

                    _progressLogger.CreateProgressBars(new[] { path });

                    var solutionLoader = new SolutionFileLoader(_logger, _extractorCreator,
                        _solutionProvider,
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
                    _progressLogger.Log($"C# Project file found at {path}");
                    _progressLogger.Log("Started Extracting...");

                    var projectLoader = new ProjectLoader(_extractorCreator, _projectProvider,
                        _projectLoadingStrategy, _logger);
                    var projectModel = await projectLoader.Load(path);

                    if (projectModel != null)
                    {
                        repositoryModel.Solutions.Add(new SolutionModel
                        {
                            Projects = { projectModel }
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
                _progressLogger.Log($"Searching for solution files at {path}");
                _progressLogger.Log();

                var solutionPathsArray = Directory.GetFiles(path, $"*{SlnExtension}", SearchOption.AllDirectories);

                var solutionPaths = new HashSet<string>();
                foreach (var p in solutionPathsArray)
                {
                    solutionPaths.Add(p);
                }

                _logger.Log($"Found {solutionPaths.Count} Solutions");

                _progressLogger.Log($"Found {solutionPaths.Count} Solutions");
                _progressLogger.Log();

                // _progressLogger.CreateProgressBars(solutionPaths);

                foreach (var solutionPath in solutionPaths)
                {
                    var solutionLoader =
                        new SolutionFileLoader(_logger, _extractorCreator, _solutionProvider,
                            _solutionLoadingStrategy);
                    var solutionModel = await solutionLoader.LoadSolution(solutionPath);
                    if (solutionModel != null)
                    {
                        repositoryModel.Solutions.Add(solutionModel);
                    }
                    else
                    {
                        _progressLogger.StopProgressBar(solutionPath);
                    }
                }

                _logger.Log(
                    $"Searching for C# Project files that are not in any of the found solutions at {path}");
                _progressLogger.Log();
                _progressLogger.Log(
                    $"Searching for C# Project files that are not in any of the found solutions at {path}");

                var defaultSolutionModel = new SolutionModel();

                var notProcessedProjectPaths = new List<string>();

                foreach (var relativeProjectPath in Directory.GetFiles(path, $"*{CsprojExtension}",
                    SearchOption.AllDirectories))
                {
                    var projectPath = Path.GetFullPath(relativeProjectPath);

                    var isUsedInASolution = repositoryModel.Solutions.Any(solutionModel =>
                        solutionModel.Projects.Any(project => project.FilePath == projectPath));

                    if (isUsedInASolution) continue;
                    notProcessedProjectPaths.Add(projectPath);
                }

                if (notProcessedProjectPaths.Count > 0)
                {
                    _logger.Log(
                        $"{notProcessedProjectPaths.Count} C# Projects were found that didn't belong to any solution file");
                    _progressLogger.Log(
                        $"{notProcessedProjectPaths.Count} C# Projects were found that didn't belong to any solution file");
                    _progressLogger.Log();

                    var progressBar =
                        _progressLogger.CreateProgressLogger(notProcessedProjectPaths.Count, "Extracting C# Projects");
                    progressBar.Start();

                    foreach (var projectPath in notProcessedProjectPaths)
                    {
                        _logger.Log($"C# Project file found at {projectPath}");
                        progressBar.Step(projectPath);

                        var projectLoader = new ProjectLoader(_extractorCreator, _projectProvider,
                            _projectLoadingStrategy, _logger);
                        var projectModel = await projectLoader.Load(projectPath);
                        if (projectModel != null)
                        {
                            defaultSolutionModel.Projects.Add(projectModel);
                        }
                    }

                    progressBar.Stop();

                    repositoryModel.Solutions.Add(defaultSolutionModel);
                }
            }

            return repositoryModel;
        }
    }
}
