using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Writers;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Logging;
using HoneydewCore.Models.Representations;
using HoneydewExtractors.Metrics.CSharp;
using HoneydewExtractors.Metrics.Extraction.ClassLevel;
using HoneydewExtractors.Metrics.Extraction.CompilationUnitLevel;
using HoneydewExtractors.Processors;
using HoneydewModels;

namespace HoneydewExtractorsDemo
{
    class Program
    {
        private const string DefaultPathForAllRepresentations = "results";

        public static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

            await result.MapResult(async options =>
            {
                var inputPath = options.InputFilePath;
                var progressLogger = new ConsoleProgressLogger();

                RepositoryModel repositoryModel;
                switch (options.Command)
                {
                    // case "load":
                    // {
                    //     // repositoryModel = await LoadModel(progressLogger, inputPath);
                    // }
                    // break;

                    case "extract":
                    {
                        var extractors = LoadExtractors();
                        repositoryModel = await ExtractModel(progressLogger, inputPath, extractors);
                    }
                        break;

                    default:
                    {
                        await Console.Error.WriteLineAsync("Invalid Command! Please use extract or load");
                        return;
                    }
                }

                progressLogger.LogLine("Resolving Full Name Dependencies");

                ConsoleLoggerWithHistory consoleLoggerWithHistory = new(new ConsoleProgressLogger());

                repositoryModel = new FullNameModelProcessor(consoleLoggerWithHistory).Process(repositoryModel);

                WriteAllRepresentations(repositoryModel, consoleLoggerWithHistory, DefaultPathForAllRepresentations);

                progressLogger.LogLine("Extraction Complete!");
                progressLogger.LogLine($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");
            }, _ => Task.FromResult("Some Error Occurred"));
        }

        private static CSharpFactExtractor LoadExtractors()
        {
            var cSharpFactExtractor = new CSharpFactExtractor();
            cSharpFactExtractor.AddMetric<IBaseClassMetric>();
            cSharpFactExtractor.AddMetric<IUsingsCountMetric>();
            cSharpFactExtractor.AddMetric<IIsAbstractMetric>();
            cSharpFactExtractor.AddMetric<IParameterDependencyMetric>();
            cSharpFactExtractor.AddMetric<IReturnValueDependencyMetric>();
            cSharpFactExtractor.AddMetric<ILocalVariablesDependencyMetric>();

            return cSharpFactExtractor;
        }

        // private static async Task<RepositoryModel> LoadModel(IProgressLogger progressLogger, string inputPath)
        // {
        //     // Load repository model from path
        //     IRepositoryLoader repositoryLoader = new RawFileRepositoryLoader(progressLogger, new FileReader());
        //     var repositoryModel = await repositoryLoader.Load(inputPath);
        //     return repositoryModel;
        // }

        private static async Task<RepositoryModel> ExtractModel(IProgressLogger progressLogger, string inputPath,
            CSharpFactExtractor extractor)
        {
            // Create repository model from path
            var repositoryLoader = new CSharpRepositoryLoader(progressLogger, extractor);
            var repositoryModel = await repositoryLoader.Load(inputPath);

            return repositoryModel;
        }

        private static void WriteAllRepresentations(RepositoryModel repositoryModel,
            ConsoleLoggerWithHistory consoleLoggerWithHistory, string outputPath)
        {
            var writer = new FileWriter();

            var fileContent = JsonSerializer.Serialize(repositoryModel);
            writer.WriteFile(Path.Combine(outputPath, "honeydew.json"), fileContent);

            var classRelationsRepresentation = GetClassRelationsRepresentation(repositoryModel);
            writer.WriteFile(Path.Combine(outputPath, "honeydew.csv"),
                classRelationsRepresentation.Export(new CsvModelExporter
                {
                    ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                    {
                        new("Total Count", ExportUtils.CsvSumPerLine)
                    }
                }));


            var ambiguousHistory = consoleLoggerWithHistory.GetHistory();
            if (!string.IsNullOrEmpty(ambiguousHistory))
            {
                writer.WriteFile(Path.Combine(outputPath, "honeydew_ambiguous.txt"), ambiguousHistory);
            }
        }

        private static IExportable GetClassRelationsRepresentation(RepositoryModel repositoryModel)
        {
            var classRelationsRepresentation = new RepositoryModelToClassRelationsProcessor().Process(repositoryModel);
            classRelationsRepresentation.UsePrettyPrint = true;
            
            return classRelationsRepresentation;
        }
    }
}
