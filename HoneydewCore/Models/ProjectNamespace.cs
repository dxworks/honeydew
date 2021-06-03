using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public class ProjectNamespace : ProjectEntity
    {
        public IList<ProjectEntity> Entities { get; init; }
    }
}