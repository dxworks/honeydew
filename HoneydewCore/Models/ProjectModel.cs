using System.Collections.Generic;

namespace HoneydewCore.Models
{
     public class ProjectModel
     {
          public IList<ProjectNamespace> Namespaces { get; init; }
          public string Name { get; init; }
          public string Path { get; init; }
     }
}