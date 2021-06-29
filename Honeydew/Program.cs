using System;
using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.CompilationUnitMetrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.IO.Writers;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations;
using HoneydewCore.Processors;

namespace Honeydew
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(
                    $"Incorrect Argument Count! Usage {AppDomain.CurrentDomain.FriendlyName} <path_to_solution>");
                return;
            }

            var pathToProject = args[0];
            Console.WriteLine("Reading project from {0}...", pathToProject);

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

            ISolutionLoader solutionLoader =
                new SolutionFileLoader(extractors, new MsBuildSolutionProvider(), new BasicSolutionLoadingStrategy());
            var projectModel = solutionLoader.LoadSolution(pathToProject);

            Console.WriteLine("Project read");
            Console.WriteLine();
            // raw export
            // var exportedSolutionModel = projectModel.Export(new RawModelExporter());
            // Console.WriteLine(exportedSolutionModel);

            var solutionModelProcessable = new ProcessorChain(IProcessable.Of(projectModel))
                .Process(new FullNameDependencyProcessor())
                .Finish<SolutionModel>();

            // Console.WriteLine(solutionModelProcessable.Value.Export(new RawModelExporter()));

            var fileRelationsProcessable = new ProcessorChain(solutionModelProcessable)
                .Process(new SolutionModelToFileRelationsProcessor())
                .Peek<FileRelationsRepresentation>(relationsRepresentation =>
                    relationsRepresentation.UsePrettyPrint = true)
                .Finish<FileRelationsRepresentation>();

            var csvExport = fileRelationsProcessable.Value.Export(new CsvModelExporter
            {
                ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                {
                    new("Total Count", ExportUtils.CsvSumPerLine)
                }
            });
            Console.WriteLine(csvExport);

            const string outputPath = "D:\\Downloads\\New Text Document.csv";
            var fileWriter = new FileWriter();
            fileWriter.WriteFile(outputPath, csvExport);
        }
    }
}