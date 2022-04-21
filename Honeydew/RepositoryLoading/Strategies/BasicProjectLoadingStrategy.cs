using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;
using Microsoft.CodeAnalysis;

namespace Honeydew.RepositoryLoading.Strategies;

public class BasicProjectLoadingStrategy : IProjectLoadingStrategy
{
    private readonly ILogger _logger;

    public BasicProjectLoadingStrategy(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<ProjectModel> Load(Project project, IFactExtractor factExtractor,
        CancellationToken cancellationToken)
    {
        var projectFilePath = ActualFilePathProvider.GetActualFilePath(project.FilePath);
        var projectModel = new ProjectModel
        {
            Name = project.Name,
            FilePath = projectFilePath,
            ProjectReferences = project.AllProjectReferences
                .Select(reference => ExtractPathFromProjectId(reference.ProjectId.ToString())).ToList()
        };

        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation == null)
        {
            _logger.Log();
            _logger.Log($"Could not get compilation from {projectFilePath} !", LogLevels.Warning);

            return null;
        }

        var i = 1;
        var documentCount = project.Documents.Count();
        var semaphore = new Semaphore(0, 1);

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var syntaxTreeFilePath = ActualFilePathProvider.GetActualFilePath(syntaxTree.FilePath);
            try
            {
                _logger.Log($"Extracting facts from {syntaxTreeFilePath} ({i}/{documentCount})...");


                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var compilationUnitType = factExtractor.Extract(syntaxTree, semanticModel);

                _logger.Log($"Done extracting from {syntaxTreeFilePath} ({i}/{documentCount})");

                compilationUnitType.FilePath = syntaxTreeFilePath;

                foreach (var classModel in compilationUnitType.ClassTypes)
                {
                    classModel.FilePath = syntaxTreeFilePath;
                }

                semaphore.WaitOne();
                projectModel.Add(compilationUnitType);
                semaphore.Release();
            }
            catch (Exception e)
            {
                _logger.Log($"Could not extract from {syntaxTreeFilePath} ({i}/{documentCount}) because {e}",
                    LogLevels.Warning);
            }

            i++;
        }

        return projectModel;
    }

    private static string ExtractPathFromProjectId(string s)
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
