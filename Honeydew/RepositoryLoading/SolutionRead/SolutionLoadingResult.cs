using System.Collections.Generic;
using Honeydew.Models;

namespace Honeydew.RepositoryLoading.SolutionRead;

public record SolutionLoadingResult(SolutionModel Solution, List<ProjectModel> ProjectModels);
