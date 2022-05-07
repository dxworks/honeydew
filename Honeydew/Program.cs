using System.Reflection;
using CommandLine;
using Honeydew;
using Honeydew.Extraction;
using Honeydew.Extractors.Converters;
using Honeydew.Extractors.CSharp;
using Honeydew.Extractors.CSharp.Converters;
using Honeydew.Extractors.Exporters;
using Honeydew.Extractors.Importers;
using Honeydew.Extractors.Load;
using Honeydew.Extractors.VisualBasic;
using Honeydew.Extractors.VisualBasic.Converters;
using Honeydew.IO.Writers.Exporters;
using Honeydew.Logging;
using Honeydew.Models;
using Honeydew.PostExtraction.ReferenceRelations;
using Honeydew.Processors;
using Honeydew.ScriptBeePlugin.Loaders;
using Honeydew.Scripts;
using Honeydew.Utils;

const string defaultPathForAllRepresentations = "results";

var result = Parser.Default.ParseArguments<ExtractOptions, LoadOptions>(args);

await result.MapResult(async options =>
{
    var missingFilesLogger = new SerilogLogger($"{defaultPathForAllRepresentations}/missing_files_logs.txt");

    const string logFilePath = $"{defaultPathForAllRepresentations}/logs.txt";
    var logger = new SerilogLogger(logFilePath);

    var commandLineOptions = (CommandLineOptions)options;
    var disableProgressBars = commandLineOptions.DisableProgressBars;

    IProgressLogger progressLogger =
        disableProgressBars ? new NoBarsProgressLogger() : new ProgressLogger();

    var honeydewVersion = "";
    try
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version != null)
        {
            honeydewVersion = version.ToString();
        }

        logger.Log($"Honeydew {honeydewVersion} starting");
        logger.Log();


        progressLogger.Log($"Honeydew {version} starting");
        progressLogger.Log();
    }
    catch (Exception)
    {
        logger.Log("Could not get Application version", LogLevels.Error);
        logger.Log();
    }


    logger.Log($"Input Path {commandLineOptions.InputFilePath}");
    logger.Log();

    logger.Log($"Log will be stored at {logFilePath}");
    logger.Log();

    progressLogger.Log($"Log will be stored at {logFilePath}");
    progressLogger.Log();

    if (disableProgressBars)
    {
        logger.Log("Progress bars are disabled");
        logger.Log();

        progressLogger.Log("Progress bars are disabled");
        progressLogger.Log();
    }

    var inputPath = commandLineOptions.InputFilePath;

    var projectName = commandLineOptions.ProjectName;
    if (string.IsNullOrEmpty(projectName))
    {
        projectName = GetProjectName(inputPath);
    }

    var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        cancellationTokenSource.Cancel();
        eventArgs.Cancel = true;
    };

    switch (options)
    {
        case ExtractOptions extractOptions:
        {
            if (extractOptions.ParallelExtraction)
            {
                logger.Log("Parallel Extracting Enabled");
                progressLogger.Log("Parallel Extracting Enabled");
            }

            var repositoryModel = await ExtractModel(logger, progressLogger, missingFilesLogger, inputPath,
                extractOptions.ParallelExtraction, cancellationTokenSource.Token);

            repositoryModel.Version = honeydewVersion;

            logger.Log();
            logger.Log("Extraction Complete!");
            logger.Log();
            logger.Log($"Output will be found at {Path.GetFullPath(defaultPathForAllRepresentations)}");

            progressLogger.Log();
            progressLogger.Log("Extraction Complete!");
            progressLogger.Log();
            progressLogger.Log($"Output will be found at {Path.GetFullPath(defaultPathForAllRepresentations)}");

            if (!extractOptions.DisablePathTrimming)
            {
                try
                {
                    logger.Log();
                    logger.Log("Trimming File Paths");
                    progressLogger.Log();
                    progressLogger.Log("Trimming File Paths");

                    repositoryModel =
                        new FilePathShortenerProcessor(new FolderPathValidator(), inputPath).Process(repositoryModel);
                }
                catch (Exception e)
                {
                    logger.Log();
                    logger.Log($"Could not trim file paths because {e}");
                    progressLogger.Log();
                    progressLogger.Log($"Could not trim file paths because {e}");
                }
            }

            logger.Log("Exporting Raw Model");
            progressLogger.Log("Exporting Raw Model");

            new ScriptRunner(progressLogger).Run(false, new List<ScriptRuntime>
            {
                new(new ExportRawModelScript(new JsonModelExporter()), new Dictionary<string, object?>
                {
                    { "outputPath", defaultPathForAllRepresentations },
                    { "repositoryModel", repositoryModel },
                    { "rawJsonOutputName", $"{projectName}-raw.json" },
                }),
            });

            logger.Log("Finished Exporting");
            progressLogger.Log("Finished Exporting!");
        }
            break;

        case LoadOptions loadOptions:
        {
            logger.Log($"Loading model from file {inputPath}");
            logger.Log();
            progressLogger.Log($"Loading model from file {inputPath}");
            progressLogger.Log();
            var repositoryModel = await LoadModel(logger, progressLogger, inputPath, cancellationTokenSource.Token);
            if (repositoryModel == null)
            {
                break;
            }

            logger.Log($"Found model of version {repositoryModel.Version}. Changing version to {honeydewVersion}");
            logger.Log();
            progressLogger.Log(
                $"Found model of version {repositoryModel.Version}. Changing version to {honeydewVersion}");
            progressLogger.Log();

            repositoryModel.Version = honeydewVersion;


            logger.Log();
            logger.Log("Converting to Reference Model");
            progressLogger.Log();
            progressLogger.Log("Converting to Reference Model");

            var referenceRepositoryModel =
                new RepositoryModelToReferenceRepositoryModelProcessor(logger, progressLogger).Process(repositoryModel);

            logger.Log("Done Converting Model");
            progressLogger.Log("Done Converting Model");

            var defaultArguments = new Dictionary<string, object?>
            {
                { "outputPath", defaultPathForAllRepresentations },
                { "referenceRepositoryModel", referenceRepositoryModel },
                { "rawJsonOutputName", $"{projectName}-raw.json" },
                { "classRelationsOutputName", $"{projectName}-class_relations.csv" },
                { "cycloOutputName", $"{projectName}-cyclomatic.json" },
                { "statisticsFileOutputName", $"{projectName}-stats.json" },
                { "projectName", projectName },
            };
            var scriptRunner = new ScriptRunner(progressLogger);

            var csvRelationsRepresentationExporter = new CsvRelationsRepresentationExporter(logger)
            {
                ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                {
                    new("all", ExportUtils.CsvSumPerLine)
                }
            };

            var jsonModelExporter = new JsonModelExporter();

            RunScripts(scriptRunner, defaultArguments, jsonModelExporter, csvRelationsRepresentationExporter, logger,
                progressLogger, projectName, loadOptions.ParallelRunning);

            logger.Log();
            logger.Log("Extraction Complete!");
            logger.Log();
            logger.Log($"Output will be found at {Path.GetFullPath(defaultPathForAllRepresentations)}");

            progressLogger.Log();
            progressLogger.Log("Extraction Complete!");
            progressLogger.Log();
            progressLogger.Log($"Output will be found at {Path.GetFullPath(defaultPathForAllRepresentations)}");
        }
            break;
    }

    logger.CloseAndFlush();
}, _ => Task.FromResult("Some Error Occurred"));

static string GetProjectName(string inputPath)
{
    return Path.GetFileNameWithoutExtension(inputPath);
}

static void RunScripts(ScriptRunner scriptRunner, Dictionary<string, object?> defaultArguments,
    JsonModelExporter jsonModelExporter, CsvRelationsRepresentationExporter csvRelationsRepresentationExporter,
    ILogger logger, IProgressLogger progressLogger, string projectName, bool runInParallel)
{
    var newReferenceRepositoryModel =
        scriptRunner.RunForResult(new ScriptRuntime(new ApplyPostExtractionVisitorsScript(logger, progressLogger),
            defaultArguments));

    if (newReferenceRepositoryModel != null)
    {
        defaultArguments["referenceRepositoryModel"] = newReferenceRepositoryModel;
    }

    scriptRunner.Run(false, new List<ScriptRuntime>
    {
        new(new ExportFileRelationsScript(csvRelationsRepresentationExporter), CreateArgumentsDictionary(
            defaultArguments, new Dictionary<string, object?>
            {
                { "fileRelationsOutputName", $"{projectName}-structural_relations_all.csv" },
                { "fileRelationsStrategy", new HoneydewChooseStrategy() },
                { "fileRelationsHeaders", null },
            })),
        new(new ExportFileRelationsScript(csvRelationsRepresentationExporter), CreateArgumentsDictionary(
            defaultArguments, new Dictionary<string, object?>
            {
                { "fileRelationsOutputName", $"{projectName}-structural_relations.csv" },
                { "fileRelationsStrategy", new JafaxChooseStrategy() },
                {
                    "fileRelationsHeaders", new List<string>
                    {
                        "extCalls",
                        "extData",
                        "hierarchy",
                        "returns",
                        "declarations",
                        "extDataStrict",
                    }
                },
            })),
    });

    scriptRunner.Run(runInParallel, new List<ScriptRuntime>
    {
        new(new ExportCyclomaticComplexityPerFileScript(jsonModelExporter), defaultArguments),
        new(new ExportClassRelationsScript(csvRelationsRepresentationExporter), defaultArguments),
        new(new ExportStatisticsScript(jsonModelExporter), defaultArguments),
        new(new ExportRelationsBetweenSolutionsAndProjectsScripts(new CsvRelationsRepresentationExporter(logger),
            new TextFileExporter(logger)), defaultArguments),
        new(new GenericDependenciesScript(csvRelationsRepresentationExporter), CreateArgumentsDictionary(
            defaultArguments, new Dictionary<string, object?>
            {
                { "genericDependenciesOutputName", $"{projectName}-generic_relations.csv" },
                { "addStrategy", new AddGenericNamesStrategy(true) },
            })),
        new(new SpektrumExportScript(jsonModelExporter), CreateArgumentsDictionary(defaultArguments,
            new Dictionary<string, object?>
            {
                { "spektrumOutputName", $"{projectName}-spektrum.json" },
            })),
    });
}

static async Task<RepositoryModel?> LoadModel(ILogger logger, IProgressLogger progressLogger, string inputPath,
    CancellationToken cancellationToken)
{
    var cSharpConverterList = new CSharpConverterList();
    var repositoryLoader = new RawFileRepositoryLoader(logger, progressLogger,
        new RepositoryModelImporter(new ProjectModelConverter(new Dictionary<string, IConverterList>
        {
            { ProjectExtractorFactory.CSharp, cSharpConverterList },
            { ProjectExtractorFactory.VisualBasic, new VisualBasicConverterList() },
        }, cSharpConverterList)));
    var repositoryModel = await repositoryLoader.Load(inputPath, cancellationToken);
    return repositoryModel;
}

static async Task<RepositoryModel> ExtractModel(ILogger logger, IProgressLogger progressLogger,
    ILogger missingFilesLogger, string inputPath, bool parallelExtraction, CancellationToken cancellationToken)
{
    var projectExtractorFactory = new ProjectExtractorFactory(logger, progressLogger, parallelExtraction);

    var csharpProjectExtractor = projectExtractorFactory.GetProjectExtractor(ProjectExtractorFactory.CSharp)!;
    var visualBasicProjectExtractor = projectExtractorFactory.GetProjectExtractor(ProjectExtractorFactory.VisualBasic)!;
    var solutionExtractor = new SolutionExtractor(logger, progressLogger, projectExtractorFactory,
        new ActualFilePathProvider(logger));

    var solutionSchemas = new List<SolutionSchema>
    {
        new(".sln", solutionExtractor, new List<ProjectSchema>
        {
            new(ProjectExtractorFactory.CSharp, ".csproj", csharpProjectExtractor, new List<FileSchema>
            {
                new(".cs", new CSharpFactExtractor(CSharpExtractionVisitors.GetVisitors(logger))),
            }),
            new(ProjectExtractorFactory.VisualBasic, ".vbproj", visualBasicProjectExtractor, new List<FileSchema>
            {
                new(".vb", new VisualBasicFactExtractor(VisualBasicExtractionVisitors.GetVisitors(logger))),
            }),
        })
    };

    var repositoryLoader =
        new RepositoryExtractor(solutionSchemas, logger, progressLogger, missingFilesLogger, parallelExtraction);

    var repositoryModel = await repositoryLoader.Load(inputPath, cancellationToken);

    return repositoryModel;
}

static Dictionary<string, object?> CreateArgumentsDictionary(IDictionary<string, object?> source,
    Dictionary<string, object?> additionalArguments)
{
    var result = new Dictionary<string, object?>(source);
    foreach (var (key, value) in additionalArguments)
    {
        result[key] = value;
    }

    return result;
}
