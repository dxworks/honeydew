using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public record ProjectModel
    {
        public string Name { get; set; }

        public string FilePath { get; set; }

        public IList<string> ProjectReferences { get; set; } = new List<string>();

        public IDictionary<string, NamespaceModel> Namespaces { get; set; } =
            new Dictionary<string, NamespaceModel>();

        public ProjectModel()
        {
        }

        public ProjectModel(string name)
        {
            Name = name;
        }


        public void Add(ClassModel classModel)
        {
            if (Namespaces.TryGetValue(classModel.Namespace, out var projectNamespace))
            {
                projectNamespace.Add(classModel);
            }
            else
            {
                var p = new NamespaceModel();
                p.Add(classModel);
                Namespaces.Add(classModel.Namespace, p);
            }
        }
    }
}