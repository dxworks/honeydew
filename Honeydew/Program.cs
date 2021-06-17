using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            const string pathToProject = "D:\\Work\\Visual Studio 2019\\CSharp\\Catan\\Catan.sln";

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

            ISolutionLoader solutionLoader = new MsBuildSolutionReader(extractors);
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