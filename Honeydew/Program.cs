using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Honeydew.Scripts;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Logging;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.RepositoryLoading;
using HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead;
using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewExtractors.Processors;
using HoneydewModels;
using HoneydewModels.CSharp;
using HoneydewModels.Exporters;
using HoneydewModels.Importers;

namespace Honeydew;

class Program
{
    private const string DefaultPathForAllRepresentations = "results";

    public static async Task Main(string[] args)
    {
        var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

        await result.MapResult(async options =>
        {
            var missingFilesLogger =
                new SerilogLogger($"{DefaultPathForAllRepresentations}/missing_files_logs.txt");

            var logFilePath = $"{DefaultPathForAllRepresentations}/logs.txt";
            var logger = new SerilogLogger(logFilePath);
            var disableProgressBars = options.DisableProgressBars || options.UseVoyager;
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


            logger.Log($"Input Path {options.InputFilePath}");
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

            var inputPath = options.InputFilePath;

            var projectName = options.ProjectName;
            if (string.IsNullOrEmpty(projectName))
            {
                projectName = GetProjectName(inputPath);
            }

            var relationMetricHolder = new RelationMetricHolder();
            RepositoryModel repositoryModel;
            switch (options.Command)
            {
                case "load":
                {
                    progressLogger.Log($"Loading model from file {inputPath}");
                    progressLogger.Log();
                    repositoryModel = await LoadModel(logger, inputPath);
                }
                    break;

                case "extract":
                {
                    repositoryModel = await ExtractModel(logger, progressLogger, missingFilesLogger,
                        relationMetricHolder, inputPath, options.ParallelExtraction);
                }
                    break;

                default:
                {
                    await Console.Error.WriteLineAsync("Invalid Command! Please use extract or load");
                    return;
                }
            }

            repositoryModel.Version = honeydewVersion;

            if (options.UseVoyager)
            {
                logger.Log("Exporting Raw Model");
                progressLogger.Log("Exporting Raw Model");

                new ScriptRunner(progressLogger, new Dictionary<string, object>
                {
                    { "outputPath", DefaultPathForAllRepresentations },
                    { "repositoryModel", repositoryModel },
                    { "rawJsonOutputName", $"{projectName}-raw.json" },
                }).Run(false, new List<ScriptRuntime>
                {
                    new(new ExportRawModelScript(new JsonModelExporter())),
                });

                logger.Log();
                logger.Log("Extraction Complete!");
                logger.Log();
                logger.Log($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");

                progressLogger.Log();
                progressLogger.Log("Extraction Complete!");
                progressLogger.Log();
                progressLogger.Log($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");

                logger.CloseAndFlush();

                return;
            }

            if (!options.DisablePathTrimming)
            {
                logger.Log();
                logger.Log("Trimming File Paths");
                progressLogger.Log();
                progressLogger.Log("Trimming File Paths");

                repositoryModel = new FilePathShortenerProcessor(inputPath).Process(repositoryModel);
            }

            var referenceRepositoryModel =
                new RepositoryModelToReferenceRepositoryModelProcessor().Process(repositoryModel);

            var scriptRunner = new ScriptRunner(progressLogger, new Dictionary<string, object>
            {
                { "outputPath", DefaultPathForAllRepresentations },
                { "referenceRepositoryModel", referenceRepositoryModel },
                { "rawJsonOutputName", $"{projectName}-raw.json" },
                { "classRelationsOutputName", $"{projectName}-class_relations.csv" },
                { "cycloOutputName", $"{projectName}-cyclomatic.json" },
                { "statisticsFileOutputName", $"{projectName}-stats.json" },
                { "projectName", projectName },
            });

            var csvRelationsRepresentationExporter = new CsvRelationsRepresentationExporter
            {
                ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                {
                    new("all", ExportUtils.CsvSumPerLine)
                }
            };

            var jsonModelExporter = new JsonModelExporter();

            RunScripts(scriptRunner, jsonModelExporter, csvRelationsRepresentationExporter, logger, progressLogger,
                projectName, options.ParallelExtraction);

            logger.Log();
            logger.Log("Extraction Complete!");
            logger.Log();
            logger.Log($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");

            progressLogger.Log();
            progressLogger.Log("Extraction Complete!");
            progressLogger.Log();
            progressLogger.Log($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");

            logger.CloseAndFlush();
        }, _ => Task.FromResult("Some Error Occurred"));
    }

    private static string GetProjectName(string inputPath)
    {
        return Path.GetFileNameWithoutExtension(inputPath);
    }

    private static void RunScripts(ScriptRunner scriptRunner, JsonModelExporter jsonModelExporter,
        CsvRelationsRepresentationExporter csvRelationsRepresentationExporter, ILogger logger,
        IProgressLogger progressLogger, string projectName, bool runInParallel)
    {
        var exportFileRelationsScript = new ExportFileRelationsScript(csvRelationsRepresentationExporter);

        var newReferenceRepositoryModel =
            scriptRunner.RunForResult(
                new ScriptRuntime(new ApplyPostExtractionVisitorsScript(logger, progressLogger)));

        scriptRunner.UpdateArgument("referenceRepositoryModel", newReferenceRepositoryModel);

        scriptRunner.Run(runInParallel, new List<ScriptRuntime>
        {
            new(new ExportRawModelScript(jsonModelExporter)),
            new(new ExportCyclomaticComplexityPerFileScript(jsonModelExporter)),
            new(new ExportClassRelationsScript(csvRelationsRepresentationExporter)),
            new(new ExportStatisticsScript(jsonModelExporter)),
            new(exportFileRelationsScript, new Dictionary<string, object>
            {
                { "fileRelationsOutputName", $"{projectName}-structural_relations_all.csv" },
                { "fileRelationsStrategy", new HoneydewChooseStrategy() },
                { "fileRelationsHeaders", null },
            }),
            new(exportFileRelationsScript, new Dictionary<string, object>
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
            }),
            new(new ClassRelationScript(csvRelationsRepresentationExporter)),
            new(new ExportRelationsBetweenSolutionsAndProjectsScripts(new CsvRelationsRepresentationExporter(),
                new TextFileExporter())),
            new(new GenericDependenciesScript(csvRelationsRepresentationExporter), new Dictionary<string, object>
            {
                { "genericDependenciesOutputName", $"{projectName}-generic_relations.csv" },
                { "addStrategy", new AddGenericNamesStrategy() },
                { "ignorePrimitives", true }
            }),
            new(new TestingStuffExportScript(jsonModelExporter), new Dictionary<string, object>
            {
                { "testingStuffOutputName", $"{projectName}-testing_stuff.json" },
            }),
        });
    }

    private static async Task<RepositoryModel> LoadModel(ILogger logger, string inputPath)
    {
        // Load repository model from path
        IRepositoryLoader<RepositoryModel> repositoryLoader =
            new RawCSharpFileRepositoryLoader(logger, new JsonModelImporter<RepositoryModel>(new ConverterList()));
        var repositoryModel = await repositoryLoader.Load(inputPath);
        return repositoryModel;
    }

    private static async Task<RepositoryModel> ExtractModel(ILogger logger, IProgressLogger progressLogger,
        ILogger missingFilesLogger, IRelationMetricHolder relationMetricHolder, string inputPath,
        bool parallelExtraction)
    {
        var solutionProvider = new MsBuildSolutionProvider();
        var projectProvider = new MsBuildProjectProvider();

        var cSharpCompilationMaker = new CSharpCompilationMaker();
        IProjectLoadingStrategy projectLoadingStrategy = parallelExtraction
            ? new ParallelProjectLoadingStrategy(logger, cSharpCompilationMaker)
            : new BasicProjectLoadingStrategy(logger, cSharpCompilationMaker);

        ISolutionLoadingStrategy solutionLoadingStrategy = parallelExtraction
            ? new ParallelSolutionLoadingStrategy(logger, projectLoadingStrategy, progressLogger)
            : new BasicSolutionLoadingStrategy(logger, projectLoadingStrategy, progressLogger);

        var repositoryLoader = new CSharpRepositoryLoader(solutionProvider, projectProvider, projectLoadingStrategy,
            solutionLoadingStrategy, logger, progressLogger, missingFilesLogger,
            new FactExtractorCreator(VisitorLoaderHelper.LoadVisitors(relationMetricHolder, logger)),
            cSharpCompilationMaker, parallelExtraction);
        var repositoryModel = await repositoryLoader.Load(inputPath);

        return repositoryModel;
    }
}
