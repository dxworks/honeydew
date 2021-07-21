using System.Collections.Generic;
using System.Linq;

namespace HoneydewModels
{
    public record ProjectModel
    {
        public string Name { get; set; }

        public string FilePath { get; set; }

        public IList<string> ProjectReferences { get; set; } = new List<string>();

        public IList<NamespaceModel> Namespaces { get; set; } = new List<NamespaceModel>();

        public ProjectModel()
        {
        }

        public ProjectModel(string name)
        {
            Name = name;
        }

        public void Add(ClassModel classModel)
        {
            var namespaceModel = Namespaces.FirstOrDefault(model => model.Name == classModel.Namespace);

            if (namespaceModel == null)
            {
                var model = new NamespaceModel();
                model.Add(classModel);
                Namespaces.Add(model);
            }
            else
            {
                namespaceModel.Add(classModel);
            }
        }
    }
}
