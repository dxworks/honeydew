using System;
using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Filters;

namespace Honeydew
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathToProject = "D:\\Work\\Visual Studio 2019\\CSharp\\Catan";

            Console.WriteLine("Reading project from {0}...", pathToProject);

            var filters = new List<PathFilter>();
            filters.Add(path => path.EndsWith(".cs"));

            var extractors = new List<Extractor<IMetricExtractor>>();

            ISolutionLoader solutionLoader = new SolutionLoader(new FileReader(filters), extractors);

            var projectModel = solutionLoader.LoadSolution(pathToProject);

            Console.WriteLine("Project read");
            Console.WriteLine(projectModel);

            Console.WriteLine("Enter any key to exit ...");
            Console.ReadKey();
        }
    }
}