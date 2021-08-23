using System.Collections.Generic;
using System.Linq;

namespace HoneydewModels.CSharp
{
    public record NamespaceModel : IModelEntity
    {
        public string Name { get; set; }
        public IList<ClassModel> ClassModels { get; set; } = new List<ClassModel>();

        public void Add(ClassModel classModel)
        {
            if (!string.IsNullOrEmpty(Name) && classModel.Namespace != Name)
            {
                return;
            }

            if (ClassModels.Any(model => model.Name == classModel.Name))
            {
                return;
            }

            Name = classModel.Namespace;

            ClassModels.Add(classModel);
        }
    }
}
