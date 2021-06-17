using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public record ProjectModel
    {
        public string Name { get; set; }

        public ProjectModel()
        {
        }

        public ProjectModel(string name)
        {
            Name = name;
        }

        public IDictionary<string, NamespaceModel> Namespaces { get; set; } =
            new Dictionary<string, NamespaceModel>();

        public void Add(ClassModel classModel)
        {
            if (string.IsNullOrEmpty(classModel.Namespace))
            {
                return;
            }

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


        public string FindClassFullNameInUsings(IList<string> usings, string className)
        {
            foreach (var usingName in usings)
            {
                if (Namespaces.TryGetValue(usingName, out var projectNamespace))
                {
                    foreach (var classModel in projectNamespace.ClassModels)
                    {
                        if (classModel.FullName == $"{usingName}.{className}")
                        {
                            return classModel.FullName;
                        }
                    }
                }
            }

            return className;
        }
    }
}