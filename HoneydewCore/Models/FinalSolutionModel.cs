using System.Collections.Generic;
using System.Linq;

namespace HoneydewCore.Models
{
    public class FinalSolutionModel
    {
        // Projects

        public IList<ProjectNamespace> Namespaces { get; set; } = new List<ProjectNamespace>();

        public void Add(ClassModel classModel)
        {
            var firstOrDefault = Namespaces.FirstOrDefault(ns => ns.Name == classModel.Namespace);
            if (firstOrDefault == default)
            {
                var projectNamespace = new ProjectNamespace();
                projectNamespace.Add(classModel);
                Namespaces.Add(projectNamespace);
            }
            else
            {
                firstOrDefault.Add(classModel);
            }
        }
    }
}