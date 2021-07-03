using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.CompilationUnitMetrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.IO.Writers;
using HoneydewCore.IO.Writers.Exporters;
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
                var useClassRelationsRepresentation = false;
                IFileWriter writer;
                IExporter exporter = new JsonModelExporter();

                var pathToSolution = options.InputFilePath;
                var outputPath = options.OutputFilePath;
                var representationType = options.RepresentationType;
                var exportType = options.ExportType;


                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    writer = new ConsoleWriter();
                }
                else
                {
                    if (exportType == "json")
                    {
                        if (outputPath.EndsWith(".csv"))
                        {
                            exportType = "csv";
                        }
                    }

                    writer = new FileWriter();
                }

                if (representationType == "class_relations")
                {
                    useClassRelationsRepresentation = true;
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

                // add the metrics that will be run after the solution is loaded

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

                // Load solution model from path
                ISolutionLoader solutionLoader =
                    new SolutionFileLoader(extractors, new MsBuildSolutionProvider(),
                        new BasicSolutionLoadingStrategy());
                var solutionModel = await solutionLoader.LoadSolution(pathToSolution);

                string exportString;

                if (useClassRelationsRepresentation)
                {
                    // transform solution model to Class Relations Representation
                    var classRelationsProcessable = new ProcessorChain(IProcessable.Of(solutionModel))
                        .Process(new SolutionModelToClassRelationsProcessor())
                        .Peek<ClassRelationsRepresentation>(relationsRepresentation =>
                            relationsRepresentation.UsePrettyPrint = true)
                        .Finish<ClassRelationsRepresentation>();
                    exportString = classRelationsProcessable.Value.Export(exporter);
                }
                else
                {
                    exportString = solutionModel.Export(exporter);
                }

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
            }, errors => Task.FromResult("Some Error Occurred"));
        }
    }
}