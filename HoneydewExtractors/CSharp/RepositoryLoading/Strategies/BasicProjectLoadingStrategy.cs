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

            var extractor = extractorCreator.Create(project.Language);

            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
            {
                _logger.Log();
                _logger.Log($"Could not get compilation from {project.FilePath} !", LogLevels.Warning);

                return null;
            }

            if (extractor == null)
            {
                _logger.Log();
                _logger.Log($"{project.Language} type projects are not currently supported!", LogLevels.Warning);

                return null;
            }

            compilation = compilation.AddReferences(_compilationMaker.FindTrustedReferences());

            var i = 1;
            var documentCount = project.Documents.Count();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                try
                {
                    _logger.Log($"Extracting facts from {syntaxTree.FilePath} ({i}/{documentCount})...");


                    var semanticModel = compilation.GetSemanticModel(syntaxTree);

                    var compilationUnitType = extractor.Extract(syntaxTree, semanticModel);

                    _logger.Log($"Done extracting from {syntaxTree.FilePath} ({i}/{documentCount})");

                    compilationUnitType.FilePath = syntaxTree.FilePath;

                    foreach (var classModel in compilationUnitType.ClassTypes)
                    {
                        classModel.FilePath = syntaxTree.FilePath;
                    }

                    projectModel.Add(compilationUnitType);
                }
                catch (Exception e)
                {
                    _logger.Log($"Could not extract from {syntaxTree.FilePath} ({i}/{documentCount}) because {e}",
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
