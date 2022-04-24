using Honeydew.Models;

namespace Honeydew.Extractors.Load;

public record SolutionLoadingResult(SolutionModel Solution, List<ProjectModel> ProjectModels);
