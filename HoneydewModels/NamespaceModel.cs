using System.Collections.Generic;
using System.Linq;
using HoneydewModels;

namespace HoneydewModels
{
    public record NamespaceModel
    {
        public string Name { get; set; }
        public IList<ClassModel> ClassModels { get; set; } = new List<ClassModel>();

        public void Add(ClassModel classModel)
        {
            if (!string.IsNullOrEmpty(Name) && classModel.Namespace != Name)
            {
                return;
            }

            if (ClassModels.Any(model => model.FullName == classModel.FullName))
            {
                return;
            }

            Name = classModel.Namespace;

            ClassModels.Add(classModel);
        }
    }
}
