using System;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors;
using HoneydewModels;
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

        public async Task<ProjectModel> Load(Project project, IFactExtractor extractors)
        {
            var projectModel = new ProjectModel(project.Name)
            {
                FilePath = project.FilePath,
                ProjectReferences = project.AllProjectReferences
                    .Select(reference => reference.ProjectId.ToString()).ToList()
            };

            var i = 1;
            var documentCount = project.Documents.Count();
            foreach (var document in project.Documents)
            {
                // var syntaxTree = await document.GetSyntaxTreeAsync();

                try
                {
                    _progressLogger.Log($"Extracting facts from {document.FilePath} ({i}/{documentCount})...");

                    var fileContent = await document.GetTextAsync();
                    var classModels = extractors.Extract(fileContent.ToString());
                    

                    _progressLogger.LogLine("done");

                    foreach (var classModel in classModels)
                    {
                        // classModel.FilePath = document.FilePath;
                        projectModel.Add((ClassModel) classModel);
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
