using System.Runtime.CompilerServices;
using Honeydew.Logging;
using Honeydew.Models;
using Honeydew.Models.Types;

namespace Honeydew.Extractors.Load;

public class RepositoryExtractor
{
    private readonly List<SolutionSchema> _solutionSchemas;
    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly ILogger _missingFilesLogger;
    private readonly bool _parallelExtraction;

    public RepositoryExtractor(List<SolutionSchema> solutionSchemas, ILogger logger, IProgressLogger progressLogger,
        ILogger missingFilesLogger, bool parallelExtraction)
    {
        _solutionSchemas = solutionSchemas;
        _logger = logger;
        _progressLogger = progressLogger;
        _missingFilesLogger = missingFilesLogger;
        _parallelExtraction = parallelExtraction;
    }

    public async Task<RepositoryModel> Load(string path, CancellationToken cancellationToken)
    {
        var repositoryModel = new RepositoryModel();

        if (File.Exists(path))
        {
            return await ExtractFromFile(path, cancellationToken, repositoryModel);
        }

        if (!Directory.Exists(path))
        {
            _logger.Log("Provided path is not a file or directory.", LogLevels.Error);
            _progressLogger.Log("Provided path is not a file or directory.");

            return repositoryModel;
        }

        return await ExtractFromDirectory(path, cancellationToken, repositoryModel);
    }

    private async Task<RepositoryModel> ExtractFromDirectory(string path, CancellationToken cancellationToken,
        RepositoryModel repositoryModel)
    {
        _logger.Log($"Searching for solution files at {path}");
        _progressLogger.Log($"Searching for solution files at {path}");
        _progressLogger.Log();


        var processedProjectFilePaths = new HashSet<string>();
        var processedSourceCodeFilePaths = new HashSet<string>();

        await foreach (var (solutionModel, projectModels) in ExtractSolutionFilesFromDirectory(path,
                           processedProjectFilePaths, cancellationToken))
        {
            repositoryModel.Solutions.Add(solutionModel);
            foreach (var projectModel in projectModels)
            {
                repositoryModel.Projects.Add(projectModel);
                processedProjectFilePaths.Add(projectModel.FilePath);

                foreach (var compilationUnit in projectModel.CompilationUnits)
                {
                    processedSourceCodeFilePaths.Add(compilationUnit.FilePath);
                }
            }
        }


        _logger.Log($"Searching for Project files that are not in any of the found solutions at {path}");
        _progressLogger.Log();
        _progressLogger.Log($"Searching for Project files that are not in any of the found solutions at {path}");

        await foreach (var projectModel in ExtractUnprocessedProjectFiles(path, processedProjectFilePaths,
                           cancellationToken))
        {
            repositoryModel.Projects.Add(projectModel);
        }

        _logger.Log();
        _logger.Log($"Searching for Source Files that were not processed yet at {path}");
        _progressLogger.Log();
        _progressLogger.Log($"Searching for Source Files that were not processed yet at {path}");


        await foreach (var projectWithUnprocessedFiles in ExtractUnprocessedSourceFiles(path,
                           processedSourceCodeFilePaths, cancellationToken))
        {
            repositoryModel.Projects.Add(projectWithUnprocessedFiles);
        }

        return repositoryModel;
    }


    private async Task<RepositoryModel> ExtractFromFile(string path, CancellationToken cancellationToken,
        RepositoryModel repositoryModel)
    {
        var solutionLoadingResult = await ExtractFromSolutionFile(path, cancellationToken);

        if (solutionLoadingResult is not null)
        {
            repositoryModel.Solutions.Add(solutionLoadingResult.Solution);
            foreach (var projectModel in solutionLoadingResult.ProjectModels)
            {
                repositoryModel.Projects.Add(projectModel);
            }

            return repositoryModel;
        }

        _logger.Log($"No Solution Schema found for file {path}. Trying to search Project Schemas.");
        _progressLogger.Log($"No Solution Schema found for file {path}. Trying to search Project Schemas.");

        var extractedProjectModel = await ExtractFromProjectFile(path, cancellationToken);

        if (extractedProjectModel is not null)
        {
            repositoryModel.Projects.Add(extractedProjectModel);

            return repositoryModel;
        }

        _logger.Log($"No Project Schema found for file {path}");
        _progressLogger.Log($"No Project Schema found for file {path}");

        // todo: continue with individual file extraction

        return repositoryModel;
    }

    private async Task<SolutionLoadingResult?> ExtractFromSolutionFile(string path, CancellationToken cancellationToken)
    {
        var solutionSchema = _solutionSchemas.FirstOrDefault(schema => path.EndsWith(schema.Extension));
        if (solutionSchema is null)
        {
            return null;
        }

        _logger.Log($"Solution file found at {path}");
        _progressLogger.Log($"Solution file found at {path}");
        _progressLogger.Log();

        _progressLogger.CreateProgressBars(new[] { path });

        var solutionLoadingResult =
            await solutionSchema.SolutionExtractor.Extract(path, new HashSet<string>(), cancellationToken);

        if (solutionLoadingResult is null)
        {
            _logger.Log($"Could not load solution file: {path}", LogLevels.Error);
            _progressLogger.Log($"Could not load solution file: {path}");
        }

        return solutionLoadingResult;
    }

    private async Task<ProjectModel?> ExtractFromProjectFile(string path, CancellationToken cancellationToken)
    {
        var projectSchema = _solutionSchemas.SelectMany(schema => schema.ProjectSchemas)
            .FirstOrDefault(schema => path.EndsWith(schema.Extension));
        if (projectSchema is null)
        {
            return null;
        }

        var projectModel = await projectSchema.ProjectExtractor.Extract(path, cancellationToken);

        if (projectModel is null)
        {
            _logger.Log($"Could not load project file: {path}", LogLevels.Error);
            _progressLogger.Log($"Could not load project file: {path}");
        }

        return projectModel;
    }


    private async IAsyncEnumerable<ProjectModel> ExtractUnprocessedSourceFiles(string path,
        ISet<string> processedSourceCodeFilePaths,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var (language, _, _, fileSchemata) in _solutionSchemas.SelectMany(schema => schema.ProjectSchemas))
        {
            foreach (var (extension, factExtractor) in fileSchemata)
            {
                var projectModelWithUnprocessedFiles = new ProjectModel
                {
                    Name = $"Project_With_Unprocessed_Files_Of_Type_{extension}",
                    Language = language,
                    CompilationUnits = new List<ICompilationUnitType>()
                };

                var notProcessedFilePaths =
                    Directory.GetFiles(path, $"*{extension}", SearchOption.AllDirectories)
                        .Select(Path.GetFullPath)
                        .Where(filePath => !processedSourceCodeFilePaths.Contains(filePath)).ToList();

                if (notProcessedFilePaths.Count > 0)
                {
                    _logger.Log($"{notProcessedFilePaths.Count} {extension} Files were found that were not processed!");
                    _progressLogger.Log(
                        $"{notProcessedFilePaths.Count} {extension} Files were found that were not processed!");
                    _progressLogger.Log();

                    var progressBar = _progressLogger.CreateProgressLogger(notProcessedFilePaths.Count,
                        $"Extracting Facts from {extension} Files");
                    progressBar.Start();

                    var tasks = notProcessedFilePaths.Select(ExtractCompilationUnit);

                    if (_parallelExtraction)
                    {
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
                        foreach (var task in tasks)
                        {
                            var compilationUnitType = await task;

                            if (compilationUnitType == null)
                            {
                                continue;
                            }

                            projectModelWithUnprocessedFiles.Add(compilationUnitType);
                        }
                    }

                    progressBar.Stop();

                    async Task<ICompilationUnitType?> ExtractCompilationUnit(string filePath)
                    {
                        try
                        {
                            _logger.Log($"{extension} File found at {filePath}");
                            _missingFilesLogger.Log(filePath);
                            progressBar.Step(filePath);

                            processedSourceCodeFilePaths.Add(filePath);

                            var compilationUnitType = await factExtractor.Extract(filePath, cancellationToken);
                            compilationUnitType.FilePath = filePath;

                            return compilationUnitType;
                        }
                        catch (Exception e)
                        {
                            _logger.Log($"The following exception occurred when parsing file {filePath}: ${e}");
                        }

                        return null;
                    }
                }

                if (projectModelWithUnprocessedFiles.CompilationUnits.Count > 0)
                {
                    yield return projectModelWithUnprocessedFiles;
                }
            }
        }
    }

    private async IAsyncEnumerable<ProjectModel> ExtractUnprocessedProjectFiles(string path,
        IReadOnlySet<string> processedProjectFilePaths, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var (_, extension, projectExtractor, _) in _solutionSchemas.SelectMany(schema =>
                     schema.ProjectSchemas))
        {
            var notProcessedProjectPaths =
                Directory.GetFiles(path, $"*{extension}", SearchOption.AllDirectories)
                    .Select(Path.GetFullPath)
                    .Where(projectPath => !processedProjectFilePaths.Contains(projectPath))
                    .ToList();

            if (notProcessedProjectPaths.Count <= 0)
            {
                continue;
            }

            _logger.Log();
            _logger.Log(
                $"{notProcessedProjectPaths.Count} {extension} Projects were found that didn't belong to any solution file");
            _progressLogger.Log(
                $"{notProcessedProjectPaths.Count} {extension} Projects were found that didn't belong to any solution file");
            _progressLogger.Log();

            var progressBar =
                _progressLogger.CreateProgressLogger(notProcessedProjectPaths.Count,
                    $"Extracting {extension} Projects");
            progressBar.Start();

            foreach (var projectPath in notProcessedProjectPaths)
            {
                _logger.Log($"{extension} Project file found at {projectPath}");
                progressBar.Step(projectPath);


                var projectModel = await projectExtractor.Extract(projectPath, cancellationToken);
                if (projectModel != null)
                {
                    yield return projectModel;
                }
            }

            progressBar.Stop();
        }
    }

    private async IAsyncEnumerable<SolutionLoadingResult> ExtractSolutionFilesFromDirectory(string path,
        ISet<string> processedProjectFilePaths, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var (solutionExtension, solutionExtractor, _) in _solutionSchemas)
        {
            var solutionPathsArray = Directory.GetFiles(path, $"*{solutionExtension}", SearchOption.AllDirectories);

            _logger.Log($"Found {solutionPathsArray.Length} Solutions");
            _logger.Log();

            _progressLogger.Log($"Found {solutionPathsArray.Length} Solutions");
            _progressLogger.Log();

            for (var solutionIndex = 0; solutionIndex < solutionPathsArray.Length; solutionIndex++)
            {
                var solutionPath = solutionPathsArray[solutionIndex];

                _progressLogger.Log();
                _progressLogger.Log($"Solution {solutionIndex + 1}/{solutionPathsArray.Length} - {solutionPath}");

                var solutionLoadingResult =
                    await solutionExtractor.Extract(solutionPath, processedProjectFilePaths, cancellationToken);

                if (solutionLoadingResult != null)
                {
                    yield return solutionLoadingResult;
                }
                else
                {
                    _progressLogger.StopProgressBar(solutionPath);
                }
            }
        }
    }
}
