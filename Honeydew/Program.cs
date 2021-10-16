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

namespace Honeydew
{
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
                IProgressLogger progressLogger =
                    options.DisableProgressBars ? new NoBarsProgressLogger() : new ProgressLogger();

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

                if (options.DisableProgressBars)
                {
                    logger.Log("Progress bars are disabled");
                    logger.Log();

                    progressLogger.Log("Progress bars are disabled");
                    progressLogger.Log();
                }

                var inputPath = options.InputFilePath;

                var projectName = GetProjectName(inputPath);

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
                            relationMetricHolder, inputPath);
                    }
                        break;

                    default:
                    {
                        await Console.Error.WriteLineAsync("Invalid Command! Please use extract or load");
                        return;
                    }
                }

                repositoryModel.Version = honeydewVersion;

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

                var scriptRunner = new ScriptRunner(logger, new Dictionary<string, object>
                {
                    { "outputPath", DefaultPathForAllRepresentations },
                    { "repositoryModel", repositoryModel },
                    { "referenceRepositoryModel", referenceRepositoryModel },
                    { "rawJsonOutputName", "honeydew.json" },
                    { "classRelationsOutputName", "honeydew.csv" },
                    { "cycloOutputName", "honeydew_cyclomatic.json" },
                    { "statisticsFileOutputName", "honeydew_stats.json" },
                });

                var csvRelationsRepresentationExporter = new CsvRelationsRepresentationExporter
                {
                    ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                    {
                        new("all", ExportUtils.CsvSumPerLine)
                    }
                };

                var jsonModelExporter = new JsonModelExporter();

                if (!options.DeactivateBindingProcessing)
                {
                    logger.Log();
                    logger.Log("Exporting Intermediate Results");
                    progressLogger.Log();
                    progressLogger.Log("Exporting Intermediate Results");

                    RunIntermediateScripts(scriptRunner, jsonModelExporter, csvRelationsRepresentationExporter);

                    logger.Log();
                    logger.Log("Resolving Full Name Dependencies");
                    progressLogger.Log();
                    progressLogger.Log("Resolving Full Name Dependencies");
                    progressLogger.Log();

                    scriptRunner.Run(new List<ScriptRuntime>
                    {
                        new(new FullNameModelProcessorScript(logger, progressLogger), new Dictionary<string, object>
                        {
                            { "disableLocalVariablesBinding", options.DisableLocalVariablesBinding }
                        })
                    }, true);
                }

                RunScripts(scriptRunner, jsonModelExporter, csvRelationsRepresentationExporter,
                    options.DisableExternTypeInLocalTypeSearch, logger, progressLogger, projectName);

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

        private static void RunIntermediateScripts(ScriptRunner scriptRunner, JsonModelExporter jsonModelExporter,
            CsvRelationsRepresentationExporter csvRelationsRepresentationExporter)
        {
            var exportFileRelationsScript = new ExportFileRelationsScript(csvRelationsRepresentationExporter);

            scriptRunner.Run(new List<ScriptRuntime>
            {
                new(new ExportRawModelScript(jsonModelExporter), new Dictionary<string, object>
                {
                    { "rawJsonOutputName", "honeydew_intermediate.json" }
                }),
                new(new ExportCyclomaticComplexityPerFileScript(jsonModelExporter), new Dictionary<string, object>
                {
                    { "cycloOutputName", "honeydew_cyclomatic_intermediate.json" }
                }),
                new(new ExportClassRelationsScript(csvRelationsRepresentationExporter), new Dictionary<string, object>
                {
                    { "classRelationsOutputName", "honeydew_intermediate.csv" }
                }),
                new(exportFileRelationsScript, new Dictionary<string, object>
                {
                    { "fileRelationsOutputName", "honeydew_file_relations_all_intermediate.csv" },
                    { "fileRelationsStrategy", new HoneydewChooseStrategy() },
                    { "fileRelationsHeaders", null },
                }),
                new(exportFileRelationsScript, new Dictionary<string, object>
                {
                    { "fileRelationsOutputName", "honeydew_file_relations_intermediate.csv" },
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
            });
        }

        private static void RunScripts(ScriptRunner scriptRunner, JsonModelExporter jsonModelExporter,
            CsvRelationsRepresentationExporter csvRelationsRepresentationExporter,
            bool disableExternTypeInLocalTypeSearch, ILogger logger, IProgressLogger progressLogger, string projectName)
        {
            var exportFileRelationsScript = new ExportFileRelationsScript(csvRelationsRepresentationExporter);

            scriptRunner.Run(new List<ScriptRuntime>
            {
                new(new ApplyPostExtractionVisitorsScript(logger, progressLogger), new Dictionary<string, object>
                {
                    { "disableSearchForExternTypes", disableExternTypeInLocalTypeSearch }
                })
            }, true);

            scriptRunner.Run(new List<ScriptRuntime>
            {
                new(new ExportRawModelScript(jsonModelExporter)),
                new(new ExportCyclomaticComplexityPerFileScript(jsonModelExporter)),
                new(new ExportClassRelationsScript(csvRelationsRepresentationExporter)),
                new(new ExportStatisticsScript(jsonModelExporter)),
                new(exportFileRelationsScript, new Dictionary<string, object>
                {
                    { "fileRelationsOutputName", "honeydew_file_relations_all.csv" },
                    { "fileRelationsStrategy", new HoneydewChooseStrategy() },
                    { "fileRelationsHeaders", null },
                }),
                new(exportFileRelationsScript, new Dictionary<string, object>
                {
                    { "fileRelationsOutputName", "honeydew_file_relations.csv" },
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
                new(new ClassRelationScript(csvRelationsRepresentationExporter), new Dictionary<string, object>
                {
                    { "classRelationsOutputName", $"{projectName}-class_relations.csv" }
                })
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
            ILogger missingFilesLogger, IRelationMetricHolder relationMetricHolder, string inputPath)
        {
            var solutionProvider = new MsBuildSolutionProvider();
            var projectProvider = new MsBuildProjectProvider();

            var cSharpCompilationMaker = new CSharpCompilationMaker();
            var projectLoadingStrategy = new BasicProjectLoadingStrategy(logger, cSharpCompilationMaker);

            var solutionLoadingStrategy =
                new BasicSolutionLoadingStrategy(logger, projectLoadingStrategy, progressLogger);

            var repositoryLoader = new CSharpRepositoryLoader(solutionProvider, projectProvider, projectLoadingStrategy,
                solutionLoadingStrategy, logger, progressLogger, missingFilesLogger,
                new FactExtractorCreator(VisitorLoaderHelper.LoadVisitors(relationMetricHolder, logger)),
                cSharpCompilationMaker);
            var repositoryModel = await repositoryLoader.Load(inputPath);

            return repositoryModel;
        }
    }
}
