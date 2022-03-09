using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class SolutionModel : ReferenceEntity
{
    public string FilePath { get; set; }

    public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();

    public RepositoryModel Repository { get; init; }
}
