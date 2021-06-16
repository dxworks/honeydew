using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Filters;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.IO.Writers;
using HoneydewCore.Models;
using HoneydewCore.Processors;

namespace Honeydew
{
    class Program
    {
        static void Main(string[] args)
        {
            const string pathToProject = "D:\\Work\\Visual Studio 2019\\CSharp\\Catan";

            Console.WriteLine("Reading project from {0}...", pathToProject);

            var cSharpClassFactExtractor = new CSharpClassFactExtractor();
            cSharpClassFactExtractor.AddMetric<BaseClassMetric>();
            cSharpClassFactExtractor.AddMetric<UsingsCountMetric>();
            cSharpClassFactExtractor.AddMetric<IsAbstractMetric>();
            cSharpClassFactExtractor.AddMetric<ParameterDependenciesMetric>();

            var extractors = new List<IFactExtractor>
            {
                cSharpClassFactExtractor
            };

            var filters = extractors
                .Select(extractor => (PathFilter) (path => path.EndsWith(extractor.FileType())))
                .ToList();

            ISolutionLoader solutionLoader = new SolutionLoader(new FileReader(filters), extractors);
            ISolutionLoadingStrategy loadingStrategy = new DirectSolutionLoading();

            var projectModel = solutionLoader.LoadSolution(pathToProject, loadingStrategy);

            Console.WriteLine("Project read");
            // raw export
            // var exportedSolutionModel = projectModel.Export(new RawModelExporter());
            // Console.WriteLine(exportedSolutionModel);
            
            var a = new ProcessorChain(IProcessable.Of(projectModel))
                .Process(new FullNameDependencyProcessor())
                .Finish<SolutionModel>();
            
            Console.WriteLine(a.Value.Export(new RawModelExporter()));

            // Console.WriteLine("Enter any key to exit ...");
            // Console.ReadKey();
        }
    }
}