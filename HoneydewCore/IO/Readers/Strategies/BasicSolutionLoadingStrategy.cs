﻿using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public class BasicSolutionLoadingStrategy : ISolutionLoadingStrategy
    {
        public SolutionModel Load(Solution solution, IList<IFactExtractor> extractors)
        {
            SolutionModel solutionModel = new();

            foreach (var project in solution.Projects)
            {
                var projectModel = new ProjectModel(project.Name);

                foreach (var document in project.Documents)
                {
                    var syntaxTree = document.GetSyntaxTreeAsync().Result;

                    try
                    {
                        var classModels = extractors.SelectMany(extractor => extractor.Extract(syntaxTree)).ToList();

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
    }
}