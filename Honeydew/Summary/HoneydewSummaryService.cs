using System.Text.Json;
using Honeydew.Models;
using Honeydew.Models.Types;

namespace Honeydew.Summary;

public static class HoneydewSummaryService
{
    public const string SuccessStatus = "success";
    public const string PartialStatus = "partial";
    public const string FailedStatus = "failed";

    private const string SummaryDataFileName = "honeydew-summary-data.json";

    public static HoneydewSummaryData BuildSummaryData(string projectName, RepositoryModel repositoryModel)
    {
        var projects = repositoryModel.Projects;
        var compilationUnits = projects.SelectMany(project => project.CompilationUnits).ToList();
        var classTypes = compilationUnits.SelectMany(compilationUnit => compilationUnit.ClassTypes).ToList();

        var filesCount = compilationUnits.Count;
        var filesCSharpCount = compilationUnits.Count(compilationUnit =>
            HasExtension(compilationUnit.FilePath, ".cs") || HasExtension(compilationUnit.FilePath, ".csx"));
        var filesVisualBasicCount = compilationUnits.Count(compilationUnit => HasExtension(compilationUnit.FilePath, ".vb"));
        var sourceCodeLines = compilationUnits.Sum(compilationUnit => (long)compilationUnit.Loc.SourceLines);

        var topLevelClassesCount = classTypes.Count(classType => string.IsNullOrWhiteSpace(classType.ContainingClassName));
        var interfacesCount = classTypes.Count(classType =>
            string.Equals(classType.ClassType, "interface", StringComparison.OrdinalIgnoreCase));
        var abstractClassesCount = classTypes.Count(classType =>
            string.Equals(classType.ClassType, "class", StringComparison.OrdinalIgnoreCase)
            && (HasModifier(classType.Modifier, "abstract") || HasModifier(classType.Modifier, "mustinherit")));

        var unprocessedProjectsCount = repositoryModel.UnprocessedProjects.Count;
        var unprocessedSourceFilesCount = repositoryModel.UnprocessedSourceFiles.Count;

        return new HoneydewSummaryData
        {
            Status = ResolveStatus(filesCount, unprocessedProjectsCount, unprocessedSourceFilesCount),
            ProjectName = projectName,
            SolutionsCount = repositoryModel.Solutions.Count,
            ProjectsCount = projects.Count,
            ProjectsCSharpCount = projects.Count(project =>
                string.Equals(project.Language, ProjectExtractorFactory.CSharp, StringComparison.OrdinalIgnoreCase)),
            ProjectsVisualBasicCount = projects.Count(project =>
                string.Equals(project.Language, ProjectExtractorFactory.VisualBasic, StringComparison.OrdinalIgnoreCase)),
            FilesCount = filesCount,
            FilesCSharpCount = filesCSharpCount,
            FilesVisualBasicCount = filesVisualBasicCount,
            TopLevelClassesCount = topLevelClassesCount,
            InterfacesCount = interfacesCount,
            AbstractClassesCount = abstractClassesCount,
            UnprocessedProjectsCount = unprocessedProjectsCount,
            UnprocessedSourceFilesCount = unprocessedSourceFilesCount,
            SourceCodeLines = sourceCodeLines,
        };
    }

    public static string WriteSummaryData(string resultsDirectory, HoneydewSummaryData summaryData)
    {
        Directory.CreateDirectory(resultsDirectory);
        var summaryDataPath = Path.Combine(resultsDirectory, SummaryDataFileName);
        File.WriteAllText(summaryDataPath, JsonSerializer.Serialize(summaryData, new JsonSerializerOptions
        {
            WriteIndented = true,
        }));

        return summaryDataPath;
    }

    private static string ResolveStatus(int filesCount, int unprocessedProjectsCount, int unprocessedSourceFilesCount)
    {
        if (filesCount <= 0)
        {
            return FailedStatus;
        }

        if (unprocessedProjectsCount > 0 || unprocessedSourceFilesCount > 0)
        {
            return PartialStatus;
        }

        return SuccessStatus;
    }

    private static bool HasExtension(string filePath, string extension)
    {
        return string.Equals(Path.GetExtension(filePath), extension, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasModifier(string modifiers, string targetModifier)
    {
        return modifiers.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Any(modifier => string.Equals(modifier, targetModifier, StringComparison.OrdinalIgnoreCase));
    }
}
