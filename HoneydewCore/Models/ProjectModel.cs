using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public class ProjectModel : ProjectEntity
    {
        public IList<ProjectNamespace> Namespaces { get; init; }
    }
}