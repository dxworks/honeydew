using System.Collections.Generic;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public record SolutionLoadingResult(SolutionModel Solution, List<ProjectModel> ProjectModels);
}
