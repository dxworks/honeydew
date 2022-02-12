using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies;

public class ParallelProjectLoadingStrategy : IProjectLoadingStrategy
{
    private readonly ILogger _logger;
    private readonly ICompilationMaker _compilationMaker;

    public ParallelProjectLoadingStrategy(ILogger logger, ICompilationMaker compilationMaker)
    {
        _logger = logger;
        _compilationMaker = compilationMaker;
    }

    public async Task<ProjectModel> Load(Project project, IFactExtractorCreator extractorCreator)
    {
        var projectFilePath = ActualFilePathProvider.GetActualFilePath(project.FilePath);
        var projectModel = new ProjectModel(project.Name)
        {
            FilePath = projectFilePath,
            ProjectReferences = project.AllProjectReferences
                .Select(reference => ExtractPathFromProjectId(reference.ProjectId.ToString())).ToList()
        };

        var extractor = extractorCreator.Create(project.Language);

        // var compilation = await project.GetCompilationAsync();
        // if (compilation == null)
        // {
        //     _logger.Log();
        //     _logger.Log($"Could not get compilation from {projectFilePath} !", LogLevels.Warning);
        //
        //     return null;
        // }

        if (extractor == null)
        {
            _logger.Log();
            _logger.Log($"{project.Language} type projects are not currently supported!", LogLevels.Warning);

            return null;
        }

        // compilation = compilation.AddReferences(_compilationMaker.FindTrustedReferences());

        var processedProjectCount = 1;
        var documentCount = project.Documents.Count();
        var projectSemaphore = new Semaphore(1, 1);


        await Parallel.ForEachAsync(project.Documents, async (document, token) =>
        {
            var syntaxTreeFilePath = document.FilePath;

            try
            {
                var syntaxTree = await document.GetSyntaxTreeAsync(token);
                var semanticModel = await document.GetSemanticModelAsync(token);
                syntaxTreeFilePath = ActualFilePathProvider.GetActualFilePath(syntaxTree?.FilePath);

                _logger.Log($"Extracting facts from {syntaxTreeFilePath} ({processedProjectCount}/{documentCount})...");


                var compilationUnitType = extractor.Extract(syntaxTree, semanticModel);

                _logger.Log($"Done extracting from {syntaxTreeFilePath} ({processedProjectCount}/{documentCount})");

                compilationUnitType.FilePath = syntaxTreeFilePath;

                foreach (var classModel in compilationUnitType.ClassTypes)
                {
                    classModel.FilePath = syntaxTreeFilePath;
                }

                projectSemaphore.WaitOne();
                projectModel.Add(compilationUnitType);
                projectSemaphore.Release();
            }
            catch (Exception e)
            {
                _logger.Log(
                    $"Could not extract from {syntaxTreeFilePath} ({processedProjectCount}/{documentCount}) because {e}",
                    LogLevels.Warning);
            }

            Interlocked.Increment(ref processedProjectCount);
        });

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
