using System;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    public class BasicProjectLoadingStrategy : IProjectLoadingStrategy
    {
        private readonly ILogger _logger;
        private readonly ICompilationMaker _compilationMaker;

        public BasicProjectLoadingStrategy(ILogger logger, ICompilationMaker compilationMaker)
        {
            _logger = logger;
            _compilationMaker = compilationMaker;
        }

        public async Task<ProjectModel> Load(Project project, IFactExtractorCreator extractorCreator)
        {
            var projectModel = new ProjectModel(project.Name)
            {
                FilePath = project.FilePath,
                ProjectReferences = project.AllProjectReferences
                    .Select(reference => ExtractPathFromProjectId(reference.ProjectId.ToString())).ToList()
            };

            _compilationMaker.AddReference(project.CompilationOutputInfo.AssemblyPath);

            var extractor = extractorCreator.Create(project.Language);

            if (extractor == null)
            {
                _logger.Log();
                _logger.Log($"{project.Language} type projects are not currently supported!", LogLevels.Warning);

                return null;
            }

            var i = 1;
            var documentCount = project.Documents.Count();

            foreach (var document in project.Documents)
            {
                try
                {
                    _logger.Log($"Extracting facts from {document.FilePath} ({i}/{documentCount})...");

                    var fileContent = await document.GetTextAsync();


                    var compilationUnitType = extractor.Extract(fileContent.ToString());
                    compilationUnitType.FilePath = document.FilePath;
                    var classTypes = compilationUnitType.ClassTypes;

                    _logger.Log($"Done extracting from {document.FilePath} ({i}/{documentCount})");

                    foreach (var classModel in classTypes)
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
