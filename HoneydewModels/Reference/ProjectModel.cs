using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class ProjectModel : ReferenceEntity
    {
        public string Name { get; set; }

        public string FilePath { get; set; }

        public IList<ProjectModel> ProjectReferences { get; set; } = new List<ProjectModel>();

        public IList<NamespaceModel> Namespaces { get; set; } = new List<NamespaceModel>();

        public IList<FileModel> CompilationUnits { get; set; } = new List<FileModel>();

        public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>(); 
        
        public RepositoryModel Repository { get; init; }
    }
}
