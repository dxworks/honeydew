using Honeydew.Extractors.CSharp;
using Honeydew.Logging;
using Honeydew.Models;
using Honeydew.Models.Types;
using Honeydew.RepositoryLoading.ProjectRead;
using Honeydew.RepositoryLoading.SolutionRead;
using Honeydew.RepositoryLoading.Strategies;
using Honeydew.Utils;

namespace Honeydew.RepositoryLoading;

public class RepositoryExtractor
{
    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly ILogger _missingFilesLogger;
    private readonly CSharpCompilationMaker _compilationMaker;
    private readonly ProjectExtractorFactory _projectExtractorFactory;
    private readonly bool _parallelExtraction;

    private const string CsprojExtension = ".csproj";
    private const string SlnExtension = ".sln";
    private const string CsFileExtension = ".cs";

    public RepositoryExtractor(ILogger logger, IProgressLogger progressLogger, ILogger missingFilesLogger,
        CSharpCompilationMaker compilationMaker, ProjectExtractorFactory projectExtractorFactory,
        bool parallelExtraction)
    {
        _logger = logger;
        _progressLogger = progressLogger;
        _missingFilesLogger = missingFilesLogger;
        _compilationMaker = compilationMaker;
        _parallelExtraction = parallelExtraction;
        _projectExtractorFactory = projectExtractorFactory;
    }

    public async Task<RepositoryModel> Load(string path, CancellationToken cancellationToken)
    {
        var repositoryModel = new RepositoryModel();

        if (File.Exists(path))
        {
            if (path.EndsWith(SlnExtension))
            {
                return await ExtractFromSolutionFile(path, repositoryModel, cancellationToken);
            }

            if (path.EndsWith(CsprojExtension))
            {
                return await ExtractFromCsProjectFile(path, repositoryModel, cancellationToken);
            }

            _logger.Log($"No Solution file or C# Project file found at {path}");
            _progressLogger.Log($"No Solution file or C# Project file found at {path}");

            return repositoryModel;
        }

        if (!Directory.Exists(path))
        {
            return repositoryModel;
        }

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

        foreach (var solutionPath in solutionPaths)
        {
            _progressLogger.Log();
            _progressLogger.Log($"Solution {currentSolutionIndex}/{totalSolutions} - {solutionPath}");

            var solutionExtractor = new SolutionExtractor(_logger, _progressLogger, _projectExtractorFactory,
                new ActualFilePathProvider(_logger));
            var solutionLoadingResult =
                await solutionExtractor.Extract(solutionPath, processedProjectFilePaths, cancellationToken);

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

        _logger.Log($"Searching for C# Project files that are not in any of the found solutions at {path}");
        _progressLogger.Log();
        _progressLogger.Log($"Searching for C# Project files that are not in any of the found solutions at {path}");

        var notProcessedProjectPaths = Directory
            .GetFiles(path, $"*{CsprojExtension}", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .Where(projectPath => !processedProjectFilePaths.Contains(projectPath)).ToList();

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

                var projectExtractor =
                    _projectExtractorFactory.GetProjectExtractor(ProjectExtractorFactory.CSharp)!;

                var projectModel =
                    await projectExtractor.Extract(projectPath, new MsBuildProjectProvider(), cancellationToken);
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

        var notProcessedFilePaths = Directory.GetFiles(path, $"*{CsFileExtension}", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .Where(filePath => !processedSourceCodeFilePaths.Contains(filePath)).ToList();

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

            var projectExtractor = _projectExtractorFactory.GetProjectExtractor(ProjectExtractorFactory.CSharp)!;

            var factExtractor = projectExtractor.FactExtractor;

            CSharpSyntacticModelCreator syntacticModelCreator = new();
            CSharpSemanticModelCreator semanticModelCreator = new(_compilationMaker);

            if (_parallelExtraction)
            {
                async Task<ICompilationUnitType?> ExtractCompilationUnit(string s)
                {
                    try
                    {
                        _logger.Log($"C# File found at {s}");
                        _missingFilesLogger.Log(s);
                        progressBar.Step(s);

                        var fileContent = await File.ReadAllTextAsync(s, cancellationToken);

                        var syntaxTree = syntacticModelCreator.Create(fileContent);
                        var semanticModel = semanticModelCreator.Create(syntaxTree);

                        var compilationUnitType = factExtractor.Extract(syntaxTree, semanticModel);
                        compilationUnitType.FilePath = s;

                        return compilationUnitType;
                    }
                    catch (Exception e)
                    {
                        _logger.Log($"The following exception occurred when parsing file {s}: ${e}");
                    }

                    return null;
                }

                var tasks = notProcessedFilePaths.Select(ExtractCompilationUnit);

                using var filesSemaphore = new SemaphoreSlim(1, 1);

                foreach (var bucket in TaskUtils.Interleaved(tasks))
                {
                    var task = await bucket;
                    var compilationUnitType = await task;

                    if (compilationUnitType == null)
                    {
                        continue;
                    }

                    await filesSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        projectModelWithUnprocessedFiles.Add(compilationUnitType);
                    }
                    finally
                    {
                        filesSemaphore.Release();
                    }
                }
            }
            else
            {
                foreach (var filePath in notProcessedFilePaths)
                {
                    try
                    {
                        _logger.Log($"C# File found at {filePath}");
                        _missingFilesLogger.Log(filePath);
                        progressBar.Step(filePath);

                        var fileContent = await File.ReadAllTextAsync(filePath, cancellationToken);

                        var syntaxTree = syntacticModelCreator.Create(fileContent);
                        var semanticModel = semanticModelCreator.Create(syntaxTree);

                        var compilationUnitType = factExtractor.Extract(syntaxTree, semanticModel);

                        projectModelWithUnprocessedFiles.Add(compilationUnitType);
                    }
                    catch (Exception e)
                    {
                        _logger.Log($"The following exception occurred when parsing file {filePath}: ${e}");
                    }
                }
            }

            repositoryModel.Projects.Add(projectModelWithUnprocessedFiles);

            progressBar.Stop();
        }

        return repositoryModel;
    }

    private async Task<RepositoryModel> ExtractFromCsProjectFile(string path, RepositoryModel repositoryModel,
        CancellationToken cancellationToken)
    {
        _logger.Log($"C# Project file found at {path}");
        _progressLogger.Log($"C# Project file found at {path}");
        _progressLogger.Log("Started Extracting...");

        var projectExtractor = _projectExtractorFactory.GetProjectExtractor(ProjectExtractorFactory.CSharp)!;

        var projectModel = await projectExtractor.Extract(path, new MsBuildProjectProvider(), cancellationToken);

        if (projectModel != null)
        {
            repositoryModel.Projects.Add(projectModel);
        }

        return repositoryModel;
    }

    private async Task<RepositoryModel> ExtractFromSolutionFile(string path, RepositoryModel repositoryModel,
        CancellationToken cancellationToken)
    {
        _logger.Log($"Solution file found at {path}");
        _progressLogger.Log($"Solution file found at {path}");
        _progressLogger.Log();

        _progressLogger.CreateProgressBars(new[] { path });

        var solutionExtractor = new SolutionExtractor(_logger, _progressLogger, _projectExtractorFactory,
            new ActualFilePathProvider(_logger));

        var solutionLoadingResult = await solutionExtractor.Extract(path, new HashSet<string>(), cancellationToken);

        if (solutionLoadingResult == null)
        {
            return repositoryModel;
        }

        repositoryModel.Solutions.Add(solutionLoadingResult.Solution);
        foreach (var projectModel in solutionLoadingResult.ProjectModels)
        {
            repositoryModel.Projects.Add(projectModel);
        }

        return repositoryModel;
    }
}
