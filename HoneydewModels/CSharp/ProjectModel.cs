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

        public IList<ICompilationUnitType> CompilationUnits { get; set; } = new List<ICompilationUnitType>();

        public ProjectModel()
        {
        }

        public ProjectModel(string name)
        {
            Name = name;
        }

        public void Add(ICompilationUnitType compilationUnitType)
        {
            CompilationUnits.Add(compilationUnitType);

            foreach (var classType in compilationUnitType.ClassTypes)
            {
                var namespaceModel = Namespaces.FirstOrDefault(model => model.Name == classType.ContainingTypeName);

                if (namespaceModel == null)
                {
                    var model = new NamespaceModel();
                    model.Add(classType);
                    Namespaces.Add(model);
                }
                else
                {
                    namespaceModel.Add(classType);
                }
            }
        }
    }
}
