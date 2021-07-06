using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public class BasicProjectLoadingStrategy : IProjectLoadingStrategy
    {
        public async Task<ProjectModel> Load(Project project, IList<IFactExtractor> extractors)
        {
            var projectModel = new ProjectModel(project.Name)
            {
                FilePath = project.FilePath
            };

            foreach (var document in project.Documents)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();

                try
                {
                    var classModels = extractors.SelectMany(extractor => extractor.Extract(syntaxTree)).ToList();

                    foreach (var classModel in classModels)
                    {
                        classModel.FilePath = document.FilePath;
                        projectModel.Add(classModel);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not extract from {document.FilePath} because {e}");
                }
            }

            return projectModel;
        }
    }
}