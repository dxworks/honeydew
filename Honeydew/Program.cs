using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.CompilationUnitMetrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Writers;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations;
using HoneydewCore.Processors;

namespace Honeydew
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

            await result.MapResult(async options =>
            {
                var inputPath = options.InputFilePath;
                var outputPath = options.OutputFilePath;
                var representationType = options.RepresentationType;
                var exportType = options.ExportType;

                RepositoryModel repositoryModel;
                switch (options.Command)
                {
                    case "load":
                    {
                        repositoryModel = await LoadModel(inputPath);
                    }
                        break;

                    case "extract":
                    case "":
                    {
                        var extractors = LoadExtractors();
                        repositoryModel = await ExtractModel(inputPath, extractors);
                    }
                        break;

                    default:
                    {
                        await Console.Error.WriteLineAsync("Invalid Command!");
                        return;
                    }
                }

                // change representation
                IExportable exportable = repositoryModel;
                switch (representationType)
                {
                    case "class_relations":
                    {
                        var classRelationsProcessable = new ProcessorChain(IProcessable.Of(repositoryModel))
                            .Process(new FullNameDependencyProcessor())
                            .Process(new RepositoryModelToClassRelationsProcessor())
                            .Peek<ClassRelationsRepresentation>(relationsRepresentation =>
                                relationsRepresentation.UsePrettyPrint = true)
                            .Finish<ClassRelationsRepresentation>();
                        exportable = classRelationsProcessable.Value;
                    }
                        break;

                    default:
                    {
                    }
                        break;
                }
                
                IFileWriter writer;
                IExporter exporter = new JsonModelExporter();
                
                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    writer = new ConsoleWriter();
                }
                else
                {
                    writer = new FileWriter();

                    if (string.IsNullOrEmpty(exportType))
                    {
                        if (outputPath.EndsWith(".csv"))
                        {
                            exportType = "csv";
                        }
                        else
                        {
                            exportType = "json";
                        }
                    }
                }

                if (exportType == "csv")
                {
                    // add formulas for each row for the csv export
                    exporter = new CsvModelExporter
                    {
                        ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                        {
                            new("Total Count", ExportUtils.CsvSumPerLine)
                        }
                    };
                }

                var exportString = exportable.Export(exporter);

                if (string.IsNullOrEmpty(exportString))
                {
                    Console.WriteLine(
                        $"Export type '{exportType}' not supported for the '{representationType}' representation !");
                    return;
                }

                writer.WriteFile(outputPath, exportString);
                if (writer is not ConsoleWriter)
                {
                    Console.WriteLine("Extraction Complete!");
                    Console.WriteLine($"Output File will be found at {outputPath}");
                }
                else
                {
                    // write to result folder
                }
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

        private static async Task<RepositoryModel> LoadModel(string inputPath)
        {
            // Load repository model from path
            IRepositoryLoader repositoryLoader = new RawFileRepositoryLoader(new FileReader());
            var repositoryModel = await repositoryLoader.Load(inputPath);
            return repositoryModel;
        }

        private static async Task<RepositoryModel> ExtractModel(string inputPath, IList<IFactExtractor> extractors)
        {
            // Create repository model from path
            IRepositoryLoader repositoryLoader = new RepositoryLoader(extractors);
            var repositoryModel = await repositoryLoader.Load(inputPath);

            return repositoryModel;
        }
    }
}