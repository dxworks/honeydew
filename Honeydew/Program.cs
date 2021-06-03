using System;
using HoneydewCore;
using HoneydewCore.IO.Readers;

namespace Honeydew
{
     class Program
     {
          static void Main(string[] args)
          {
               string pathToProject = "D:\\Work\\Visual Studio 2019\\CSharp\\Catan";

               Console.WriteLine("Reading project from {0}...", pathToProject);

               ISolutionLoader solutionLoader = new SolutionLoader();

               var projectModel = solutionLoader.LoadSolution(pathToProject);

               Console.WriteLine("Project read");
               Console.WriteLine(projectModel);


               Console.ReadKey();
          }
     }
}