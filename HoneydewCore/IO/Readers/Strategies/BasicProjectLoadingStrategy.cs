using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.Logging;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public class BasicProjectLoadingStrategy : IProjectLoadingStrategy
    {
        private readonly IProgressLogger _progressLogger;

        public BasicProjectLoadingStrategy(IProgressLogger progressLogger)
        {
            _progressLogger = progressLogger;
        }

        public async Task<ProjectModel> Load(Project project, IList<IFactExtractor> extractors)
        {
            var projectModel = new ProjectModel(project.Name)
            {
                FilePath = project.FilePath
            };

            var i = 1;
            var documentCount = project.Documents.Count();
            foreach (var document in project.Documents)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();

                try
                {
                    _progressLogger.Log($"Extracting facts from {document.FilePath} ({i}/{documentCount})...");

                    var classModels = extractors.SelectMany(extractor => extractor.Extract(syntaxTree)).ToList();

                    _progressLogger.LogLine("done");

                    foreach (var classModel in classModels)
                    {
                        classModel.FilePath = document.FilePath;
                        projectModel.Add(classModel);
                    }
                }
                catch (Exception e)
                {
                    await Console.Error.WriteLineAsync($"Could not extract from {document.FilePath} because {e}");
                }

                i++;
            }

            return projectModel;
        }
    }
}