using System.Collections.Generic;
using System.Linq;

namespace HoneydewCore.Models
{
    public class ProjectNamespace
    {
        public string Name { get; set; }
        public IList<ProjectClassModel> ClassModels { get; set; } = new List<ProjectClassModel>();

        public void Add(ProjectClassModel classModel)
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