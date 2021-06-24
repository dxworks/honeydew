using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace HoneydewCore.IO.Readers
{
    public class MsBuildSolutionReader : ISolutionLoader
    {
        private readonly IList<IFactExtractor> _extractors;

        public MsBuildSolutionReader(IList<IFactExtractor> extractors)
        {
            _extractors = extractors;
        }

        public SolutionModel LoadSolution(string pathToSolution, ISolutionLoadingStrategy solutionLoadingStrategy)
        {
            MSBuildLocator.RegisterDefaults();

            var msBuildWorkspace = MSBuildWorkspace.Create();

            if (!msBuildWorkspace.Diagnostics.IsEmpty)
            {
                throw new ProjectWithErrorsException();
            }

            Solution solution;
            try
            {
                solution = msBuildWorkspace.OpenSolutionAsync(pathToSolution).Result;
            }
            catch (Exception e)
            {
                throw new ProjectNotFoundException();
            }

            SolutionModel solutionModel = new();

            foreach (var project in solution.Projects)
            {
                var projectModel = new ProjectModel(project.Name);

                foreach (var document in project.Documents)
                {
                    var syntaxTree = document.GetSyntaxTreeAsync().Result;

                    try
                    {
                        var classModels = _extractors.SelectMany(extractor => extractor.Extract(syntaxTree)).ToList();

                        foreach (var classModel in classModels)
                        {
                            classModel.FilePath = document.FilePath;
                            projectModel.Add(classModel);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Could not extract from {document.FilePath}");
                    }
                }

                solutionModel.Projects.Add(projectModel);
            }

            return solutionModel;
        }

        public SolutionModel LoadModelFromFile(IFileReader fileReader, string pathToModel)
        {
            var fileContent = fileReader.ReadFile(pathToModel);

            try
            {
                var solutionModel = JsonSerializer.Deserialize<SolutionModel>(fileContent);

                if (solutionModel == null)
                {
                    return null;
                }

                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var (_, projectNamespace) in projectModel.Namespaces)
                    {
                        foreach (var classModel in projectNamespace.ClassModels)
                        {
                            foreach (var metric in classModel.Metrics)
                            {
                                var returnType = Type.GetType(metric.ValueType);
                                if (returnType == null)
                                {
                                    continue;
                                }

                                metric.Value = JsonSerializer.Deserialize(((JsonElement) metric.Value).GetRawText(),
                                    returnType);
                            }
                        }
                    }
                }

                return solutionModel;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}