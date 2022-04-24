using Honeydew.Extractors;
using Honeydew.Logging;
using Honeydew.Models;
using Honeydew.Models.Types;
using Honeydew.Utils;
using Microsoft.CodeAnalysis;

namespace Honeydew.RepositoryLoading.Strategies;

public class ParallelProjectLoadingStrategy : IProjectLoadingStrategy
{
    private readonly ILogger _logger;
    private readonly ActualFilePathProvider _actualFilePathProvider;

    public ParallelProjectLoadingStrategy(ILogger logger, ActualFilePathProvider actualFilePathProvider)
    {
        _logger = logger;
        _actualFilePathProvider = actualFilePathProvider;
    }

    public async Task<ProjectModel> Load(Project project, IFactExtractor factExtractor,
        CancellationToken cancellationToken)
    {
        var projectFilePath = _actualFilePathProvider.GetActualFilePath(project.FilePath);
        var projectModel = new ProjectModel
        {
            Name = project.Name,
            FilePath = projectFilePath,
            ProjectReferences = project.AllProjectReferences
                .Select(reference => ExtractPathFromProjectId(reference.ProjectId.ToString())).ToList()
        };

        var documentCount = project.Documents.Count();
        using var projectSemaphore = new SemaphoreSlim(1, 1);


        var tasks = project.Documents.Select((document, index) =>
            ExtractCompilationUnit(factExtractor, document, index + 1, documentCount, cancellationToken));

        foreach (var bucket in TaskUtils.Interleaved(tasks))
        {
            var task = await bucket;
            var compilationUnitType = await task;

            if (compilationUnitType == null)
            {
                continue;
            }

            await projectSemaphore.WaitAsync(cancellationToken);
            try
            {
                projectModel.Add(compilationUnitType);
            }
            finally
            {
                projectSemaphore.Release();
            }
        }

        return projectModel;
    }

    private async Task<ICompilationUnitType?> ExtractCompilationUnit(IFactExtractor factExtractor, Document document,
        int currentProjectIndex, int documentCount, CancellationToken cancellationToken)
    {
        var syntaxTreeFilePath = document.FilePath;

        try
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            if (syntaxTree is null)
            {
                _logger.Log($"Syntax tree is null for document {syntaxTreeFilePath}", LogLevels.Warning);
                return null;
            }

            if (semanticModel is null)
            {
                _logger.Log($"Semantic Model is null for document {syntaxTreeFilePath}", LogLevels.Warning);
                return null;
            }

            syntaxTreeFilePath = _actualFilePathProvider.GetActualFilePath(syntaxTree.FilePath);

            _logger.Log($"Extracting facts from {syntaxTreeFilePath} ({currentProjectIndex}/{documentCount})...");

            var compilationUnitType = factExtractor.Extract(syntaxTree, semanticModel);

            _logger.Log($"Done extracting from {syntaxTreeFilePath} ({currentProjectIndex}/{documentCount})");

            compilationUnitType.FilePath = syntaxTreeFilePath;

            foreach (var classModel in compilationUnitType.ClassTypes)
            {
                classModel.FilePath = syntaxTreeFilePath;
            }

            return compilationUnitType;
        }
        catch (Exception e)
        {
            _logger.Log(
                $"Could not extract from {syntaxTreeFilePath} ({currentProjectIndex}/{documentCount}) because {e}",
                LogLevels.Warning);
        }

        return null;
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
