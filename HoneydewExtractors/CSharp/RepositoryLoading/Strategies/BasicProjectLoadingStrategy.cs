using System;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    public class BasicProjectLoadingStrategy : IProjectLoadingStrategy
    {
        private readonly IProgressLogger _progressLogger;

        public BasicProjectLoadingStrategy(IProgressLogger progressLogger)
        {
            _progressLogger = progressLogger;
        }

        public async Task<ProjectModel> Load(Project project, CSharpFactExtractor extractors)
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
                try
                {
                    _progressLogger.Log($"Extracting facts from {document.FilePath} ({i}/{documentCount})...");

                    var fileContent = await document.GetTextAsync();
                    var classModels = extractors.Extract(fileContent.ToString());

                    _progressLogger.Log($"Done extracting from {document.FilePath} ({i}/{documentCount})");

                    foreach (var classModel in classModels)
                    {
                        classModel.FilePath = document.FilePath;
                        projectModel.Add(classModel);
                    }
                }
                catch (Exception e)
                {
                    _progressLogger.Log($"Could not extract from {document.FilePath} ({i}/{documentCount}) because {e}", LogLevels.Warning);
                }

                i++;
            }

            return projectModel;
        }
    }
}
