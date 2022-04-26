using Honeydew.Extractors.Load;
using Honeydew.Logging;
using Honeydew.Models;
using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors.Dotnet.Load.Strategies;

public class BasicDotnetProjectLoadingStrategy : IDotnetProjectLoadingStrategy
{
    private readonly ILogger _logger;
    private readonly ActualFilePathProvider _actualFilePathProvider;

    public BasicDotnetProjectLoadingStrategy(ILogger logger, ActualFilePathProvider actualFilePathProvider)
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

        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation == null)
        {
            _logger.Log();
            _logger.Log($"Could not get compilation from {projectFilePath} !", LogLevels.Warning);

            return projectModel;
        }

        var i = 1;
        var documentCount = project.Documents.Count();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var syntaxTreeFilePath = _actualFilePathProvider.GetActualFilePath(syntaxTree.FilePath);
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

                projectModel.Add(compilationUnitType);
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
