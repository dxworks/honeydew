using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.CompilationUnitMetrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Writers;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Logging;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations;
using HoneydewCore.Processors;

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
                var inputPath = options.InputFilePath;
                var progressLogger = new ConsoleProgressLogger();

                RepositoryModel repositoryModel;
                switch (options.Command)
                {
                    case "load":
                    {
                        repositoryModel = await LoadModel(progressLogger, inputPath);
                    }
                        break;

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

                WriteAllRepresentations(repositoryModel, DefaultPathForAllRepresentations);

                Console.WriteLine("Extraction Complete!");
                Console.WriteLine($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");
            }, _ => Task.FromResult("Some Error Occurred"));
        }

        private static IList<IFactExtractor> LoadExtractors()
        {
            var cSharpClassFactExtractor = new CSharpClassFactExtractor();
            cSharpClassFactExtractor.AddMetric<BaseClassMetric>();
            cSharpClassFactExtractor.AddMetric<UsingsCountMetric>();
            cSharpClassFactExtractor.AddMetric<IsAbstractMetric>();
            cSharpClassFactExtractor.AddMetric<ParameterDependencyMetric>();
            cSharpClassFactExtractor.AddMetric<ReturnValueDependencyMetric>();
            cSharpClassFactExtractor.AddMetric<LocalVariablesDependencyMetric>();

            var extractors = new List<IFactExtractor>
            {
                cSharpClassFactExtractor
            };

            return extractors;
        }

        private static async Task<RepositoryModel> LoadModel(IProgressLogger progressLogger, string inputPath)
        {
            // Load repository model from path
            IRepositoryLoader repositoryLoader = new RawFileRepositoryLoader(progressLogger, new FileReader());
            var repositoryModel = await repositoryLoader.Load(inputPath);
            return repositoryModel;
        }

        private static async Task<RepositoryModel> ExtractModel(IProgressLogger progressLogger, string inputPath,
            IList<IFactExtractor> extractors)
        {
            // Create repository model from path
            IRepositoryLoader repositoryLoader = new RepositoryLoader(progressLogger, extractors);
            var repositoryModel = await repositoryLoader.Load(inputPath);

            return repositoryModel;
        }

        private static void WriteAllRepresentations(RepositoryModel repositoryModel, string outputPath)
        {
            var writer = new FileWriter();

            writer.WriteFile(Path.Combine(outputPath, "honeydew.json"),
                repositoryModel.Export(new JsonModelExporter()));

            var classRelationsRepresentation = GetClassRelationsRepresentation(repositoryModel);
            writer.WriteFile(Path.Combine(outputPath, "honeydew.csv"),
                classRelationsRepresentation.Export(new CsvModelExporter
                {
                    ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                    {
                        new("Total Count", ExportUtils.CsvSumPerLine)
                    }
                }));
        }

        private static IExportable GetClassRelationsRepresentation(RepositoryModel repositoryModel)
        {
            var classRelationsProcessable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(new FullNameDependencyProcessor())
                .Process(new RepositoryModelToClassRelationsProcessor())
                .Peek<ClassRelationsRepresentation>(relationsRepresentation =>
                    relationsRepresentation.UsePrettyPrint = true)
                .Finish<ClassRelationsRepresentation>();
            IExportable exportable = classRelationsProcessable.Value;
            return exportable;
        }
    }
}