using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Filters;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.IO.Writers;

namespace Honeydew
{
    class Program
    {
        static void Main(string[] args)
        {
            var pathToProject = "D:\\Work\\Visual Studio 2019\\CSharp\\Catan";

            Console.WriteLine("Reading project from {0}...", pathToProject);

            var extractors = new List<IFactExtractor>
            {
                new CSharpClassFactExtractor(new List<CSharpMetricExtractor>
                {
                    new BaseClassMetric(),
                    new UsingsCountMetric()
                })
            };

            var filters = extractors
                .Select(extractor => (PathFilter) (path => path.EndsWith(extractor.FileType())))
                .ToList();

            ISolutionLoader solutionLoader = new SolutionLoader(new FileReader(filters), extractors);
            ISolutionLoadingStrategy loadingStrategy = new DirectSolutionLoading();

            var projectModel = solutionLoader.LoadSolution(pathToProject, loadingStrategy);

            Console.WriteLine("Project read");
            var exportedSolutionModel = projectModel.Export(new RawModelExporter());

            Console.WriteLine(exportedSolutionModel);

            // Console.WriteLine("Enter any key to exit ...");
            // Console.ReadKey();
        }
    }
}