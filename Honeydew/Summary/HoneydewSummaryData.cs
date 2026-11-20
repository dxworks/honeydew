using System.Globalization;

namespace Honeydew.Summary;

public record HoneydewSummaryData
{
    public string Status { get; init; } = HoneydewSummaryService.SuccessStatus;

    public string ProjectName { get; init; } = "unknown";

    public int SolutionsCount { get; init; }

    public int ProjectsCount { get; init; }

    public int ProjectsCSharpCount { get; init; }

    public int ProjectsVisualBasicCount { get; init; }

    public int FilesCount { get; init; }

    public int FilesCSharpCount { get; init; }

    public int FilesVisualBasicCount { get; init; }

    public int TopLevelClassesCount { get; init; }

    public int InterfacesCount { get; init; }

    public int AbstractClassesCount { get; init; }

    public int UnprocessedProjectsCount { get; init; }

    public int UnprocessedSourceFilesCount { get; init; }

    public long SourceCodeLines { get; init; }

    public string GeneratedAt { get; init; } =
        DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
}
