using System.Collections.Generic;
using System.Linq;

namespace HoneydewCore.Models
{
    public class ProjectNamespace
    {
        public string Name { get; set; }
        public IList<FinalClassModel> ClassModels { get; set; } = new List<FinalClassModel>();

        public void Add(ClassModel classModel)
        {
            if (!string.IsNullOrEmpty(Name) && classModel.Namespace != Name)
            {
                return;
            }

            var fullName = $"{classModel.Namespace}.{classModel.Name}";

            if (ClassModels.Any(model => model.FullName == fullName))
            {
                return;
            }

            Name = classModel.Namespace;

            ClassModels.Add(new FinalClassModel
            {
                FullName = fullName,
                Metrics = classModel.Metrics.Metrics.Select(metric => new ClassMetric()
                {
                    ExtractorName = metric.Key.FullName,
                    Value = metric.Value.GetValue(),
                    ValueType = metric.Value.GetValueType()
                }).ToList()
            });
        }
    }
}