using System.Collections.Generic;
using System.Linq;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record NamespaceModel
    {
        public string Name { get; set; } = "";
        public IList<string> ClassNames { get; set; } = new List<string>();

        public void Add(IClassType classType)
        {
            if (!string.IsNullOrEmpty(Name) && classType.ContainingTypeName != Name)
            {
                return;
            }

            if (ClassNames.Any(name => name == classType.Name))
            {
                return;
            }

            Name = classType.ContainingTypeName;

            ClassNames.Add(classType.Name);
        }
    }
}
