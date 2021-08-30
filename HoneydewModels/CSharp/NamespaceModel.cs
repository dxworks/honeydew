using System.Collections.Generic;
using System.Linq;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record NamespaceModel : IModelEntity
    {
        public string Name { get; set; }
        public IList<IClassType> ClassModels { get; set; } = new List<IClassType>();

        public void Add(IClassType classType)
        {
            if (!string.IsNullOrEmpty(Name) && classType.ContainingTypeName != Name)
            {
                return;
            }

            if (ClassModels.Any(model => model.Name == classType.Name))
            {
                return;
            }

            Name = classType.ContainingTypeName;

            ClassModels.Add(classType);
        }
    }
}
