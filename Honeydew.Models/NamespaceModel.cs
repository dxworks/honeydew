using System.Collections.Generic;
using System.Linq;
using Honeydew.Models.Types;

namespace Honeydew.Models;

public record NamespaceModel
{
    public string Name { get; set; } = "";

    public IList<string> ClassNames { get; set; } = new List<string>();

    public void Add(IClassType classType)
    {
        if (!string.IsNullOrEmpty(Name) && classType.ContainingNamespaceName != Name)
        {
            return;
        }

        if (ClassNames.Any(name => name == classType.Name))
        {
            return;
        }

        Name = classType.ContainingNamespaceName;

        ClassNames.Add(classType.Name);
    }
}
