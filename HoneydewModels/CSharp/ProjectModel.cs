using System.Collections.Generic;
using System.Linq;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record ProjectModel : IModelEntity
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

        public void Add(IClassType classModel)
        {
            var namespaceModel = Namespaces.FirstOrDefault(model => model.Name == classModel.ContainingTypeName);

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
