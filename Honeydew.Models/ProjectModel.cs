using System.Collections.Generic;
using System.Linq;
using Honeydew.Models.Types;

namespace Honeydew.Models;

public record ProjectModel
{
    public string Language { get; set; }

    public string Name { get; set; }

    public string FilePath { get; set; }

    public IList<string> ProjectReferences { get; set; } = new List<string>();

    public IList<NamespaceModel> Namespaces { get; set; } = new List<NamespaceModel>();

    public IList<ICompilationUnitType> CompilationUnits { get; set; } = new List<ICompilationUnitType>();

    public void Add(ICompilationUnitType compilationUnitType)
    {
        CompilationUnits.Add(compilationUnitType);

        foreach (var classType in compilationUnitType.ClassTypes)
        {
            var namespaceModel = Namespaces.FirstOrDefault(model => model.Name == classType.ContainingNamespaceName);

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
