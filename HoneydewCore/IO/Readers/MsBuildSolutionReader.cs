﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;
using Microsoft.Build.Locator;
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
                throw new ProjectNotFoundException("Project has Errors!");
            }

            var solution = msBuildWorkspace.OpenSolutionAsync(pathToSolution).Result;

            SolutionModel solutionModel = new();

            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var syntaxTree = document.GetSyntaxTreeAsync().Result;

                    var classModels = _extractors.SelectMany(extractor => extractor.Extract(syntaxTree)).ToList();

                    foreach (var classModel in classModels)
                    {
                        classModel.FilePath = document.FilePath;
                        solutionModel.Add(classModel);
                    }
                }
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

                foreach (var (_, projectNamespace) in solutionModel.Namespaces)
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

                return solutionModel;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}