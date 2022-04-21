using System.Collections.Generic;
using HoneydewModels;

namespace Honeydew.RepositoryLoading.SolutionRead;

public record SolutionLoadingResult(SolutionModel Solution, List<ProjectModel> ProjectModels);
