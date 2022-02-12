using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HoneydewCore.IO.Readers;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
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
        private readonly IProgressLogger _progressLogger;
        private readonly ILogger _missingFilesLogger;
        private readonly IFactExtractorCreator _extractorCreator;
        private readonly CSharpCompilationMaker _compilationMaker;

        private const string CsprojExtension = ".csproj";
        private const string SlnExtension = ".sln";
        private const string CsFileExtension = ".scs";

        public CSharpRepositoryLoader(ISolutionProvider solutionProvider, IProjectProvider projectProvider,
            IProjectLoadingStrategy projectLoadingStrategy, ISolutionLoadingStrategy solutionLoadingStrategy,
            ILogger logger, IProgressLogger progressLogger, ILogger missingFilesLogger,
            IFactExtractorCreator extractorCreator, CSharpCompilationMaker compilationMaker)
        {
            _projectLoadingStrategy = projectLoadingStrategy;
            _solutionLoadingStrategy = solutionLoadingStrategy;
            _logger = logger;
            _progressLogger = progressLogger;
            _missingFilesLogger = missingFilesLogger;
            _extractorCreator = extractorCreator;
            _compilationMaker = compilationMaker;
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

                    var solutionLoader = new SolutionFileLoader(_logger, _progressLogger, _extractorCreator,
                        _solutionProvider,
                        _solutionLoadingStrategy);

                    var processedProjectFilePaths = new HashSet<string>();
                    var solutionLoadingResult = await solutionLoader.LoadSolution(path, processedProjectFilePaths);

                    if (solutionLoadingResult != null)
                    {
                        repositoryModel.Solutions.Add(solutionLoadingResult.Solution);
                        foreach (var projectModel in solutionLoadingResult.ProjectModels)
                        {
                            repositoryModel.Projects.Add(projectModel);
                        }
                    }
                }
                else if (path.EndsWith(CsprojExtension))
                {
                    _logger.Log($"C# Project file found at {path}");
                    _progressLogger.Log($"C# Project file found at {path}");
                    _progressLogger.Log("Started Extracting...");

                    var projectLoader = new ProjectLoader(_extractorCreator, _projectProvider,
                        _projectLoadingStrategy, _logger, _progressLogger);
                    var projectModel = await projectLoader.Load(path);

                    if (projectModel != null)
                    {
                        repositoryModel.Projects.Add(projectModel);
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

                var processedProjectFilePaths = new HashSet<string>();
                var processedSourceCodeFilePaths = new HashSet<string>();

                var solutionPathsArray = Directory.GetFiles(path, $"*{SlnExtension}", SearchOption.AllDirectories);

                var solutionPaths = new HashSet<string>();
                foreach (var p in solutionPathsArray)
                {
                    solutionPaths.Add(p);
                }

                _logger.Log($"Found {solutionPaths.Count} Solutions");

                _progressLogger.Log($"Found {solutionPaths.Count} Solutions");
                _progressLogger.Log();

                var totalSolutions = solutionPaths.Count;
                var currentSolutionIndex = 1;

                // _progressLogger.CreateProgressBars(solutionPaths);

                foreach (var solutionPath in solutionPaths)
                {
                    _progressLogger.Log();
                    _progressLogger.Log($"Solution {currentSolutionIndex}/{totalSolutions} - {solutionPath}");
                    var solutionLoader = new SolutionFileLoader(_logger, _progressLogger, _extractorCreator,
                        _solutionProvider,
                        _solutionLoadingStrategy);
                    var solutionLoadingResult =
                        await solutionLoader.LoadSolution(solutionPath, processedProjectFilePaths);

                    if (solutionLoadingResult != null)
                    {
                        repositoryModel.Solutions.Add(solutionLoadingResult.Solution);
                        foreach (var projectModel in solutionLoadingResult.ProjectModels)
                        {
                            repositoryModel.Projects.Add(projectModel);
                            processedProjectFilePaths.Add(projectModel.FilePath);

                            foreach (var compilationUnit in projectModel.CompilationUnits)
                            {
                                processedSourceCodeFilePaths.Add(compilationUnit.FilePath);
                            }
                        }
                    }
                    else
                    {
                        _progressLogger.StopProgressBar(solutionPath);
                    }

                    currentSolutionIndex++;
                }

                _logger.Log(
                    $"Searching for C# Project files that are not in any of the found solutions at {path}");
                _progressLogger.Log();
                _progressLogger.Log(
                    $"Searching for C# Project files that are not in any of the found solutions at {path}");

                var notProcessedProjectPaths = new List<string>();

                foreach (var relativeProjectPath in Directory.GetFiles(path, $"*{CsprojExtension}",
                    SearchOption.AllDirectories))
                {
                    var projectPath = Path.GetFullPath(relativeProjectPath);

                    if (processedProjectFilePaths.Contains(projectPath))
                    {
                        continue;
                    }

                    notProcessedProjectPaths.Add(projectPath);
                }

                if (notProcessedProjectPaths.Count > 0)
                {
                    _logger.Log();
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
                            _projectLoadingStrategy, _logger, _progressLogger);
                        var projectModel = await projectLoader.Load(projectPath);
                        if (projectModel != null)
                        {
                            repositoryModel.Projects.Add(projectModel);
                        }
                    }

                    progressBar.Stop();
                }

                _logger.Log();
                _logger.Log($"Searching for C# Files that were not processed yet at {path}");
                _progressLogger.Log();
                _progressLogger.Log($"Searching for C# Files that were not processed yet at {path}");

                var notProcessedFilePaths = new List<string>();

                foreach (var relativeFilePath in Directory.GetFiles(path, $"*{CsFileExtension}",
                    SearchOption.AllDirectories))
                {
                    var filePath = Path.GetFullPath(relativeFilePath);

                    if (processedSourceCodeFilePaths.Contains(filePath))
                    {
                        continue;
                    }

                    notProcessedFilePaths.Add(filePath);
                }

                if (notProcessedFilePaths.Count > 0)
                {
                    var projectModelWithUnprocessedFiles = new ProjectModel
                    {
                        Name = "Project_With_Unprocessed_Files"
                    };

                    _logger.Log($"{notProcessedFilePaths.Count} C# Files were found that were not processed!");
                    _progressLogger.Log($"{notProcessedFilePaths.Count} C# Files were found that were not processed!");
                    _progressLogger.Log();

                    var progressBar = _progressLogger.CreateProgressLogger(notProcessedFilePaths.Count,
                        "Extracting Facts from C# Files");
                    progressBar.Start();

                    var factExtractor = _extractorCreator.Create("C#");

                    CSharpSyntacticModelCreator syntacticModelCreator = new();
                    CSharpSemanticModelCreator semanticModelCreator = new(_compilationMaker);

                    foreach (var filePath in notProcessedFilePaths)
                    {
                        _logger.Log($"C# File found at {filePath}");
                        _missingFilesLogger.Log(filePath);
                        progressBar.Step(filePath);

                        var fileContent = await File.ReadAllTextAsync(filePath);

                        var syntaxTree = syntacticModelCreator.Create(fileContent);
                        var semanticModel = semanticModelCreator.Create(syntaxTree);

                        var compilationUnitType = factExtractor.Extract(syntaxTree, semanticModel);

                        projectModelWithUnprocessedFiles.Add(compilationUnitType);
                    }

                    repositoryModel.Projects.Add(projectModelWithUnprocessedFiles);

                    progressBar.Stop();
                }
            }

            return repositoryModel;
        }
    }
}
