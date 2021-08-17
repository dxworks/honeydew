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
        private readonly ILogger _logger;

        public BasicProjectLoadingStrategy(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ProjectModel> Load(Project project, CSharpFactExtractor extractor)
        {
            var projectModel = new ProjectModel(project.Name)
            {
                FilePath = project.FilePath,
                ProjectReferences = project.AllProjectReferences
                    .Select(reference => ExtractPathFromProjectId(reference.ProjectId.ToString())).ToList()
            };

            var i = 1;
            var documentCount = project.Documents.Count();

            foreach (var document in project.Documents)
            {
                try
                {
                    _logger.Log($"Extracting facts from {document.FilePath} ({i}/{documentCount})...");

                    var fileContent = await document.GetTextAsync();

                    var classModels = extractor.Extract(fileContent.ToString());

                    _logger.Log($"Done extracting from {document.FilePath} ({i}/{documentCount})");

                    foreach (var classModel in classModels)
                    {
                        classModel.FilePath = document.FilePath;
                        projectModel.Add(classModel);
                    }
                }
                catch (Exception e)
                {
                    _logger.Log($"Could not extract from {document.FilePath} ({i}/{documentCount}) because {e}",
                        LogLevels.Warning);
                }

                i++;
            }

            return projectModel;
        }

        private string ExtractPathFromProjectId(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var parts = s.Split(" - ");

            if (parts.Length != 2)
            {
                return s;
            }

            return parts[1][..^1];
        }
    }
}
