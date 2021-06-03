using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public class SolutionModel : ProjectEntity
    {
        public IList<ProjectModel> Projects { get; init; }
    }
}