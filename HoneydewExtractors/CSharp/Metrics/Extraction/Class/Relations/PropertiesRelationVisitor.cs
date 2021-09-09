using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class PropertiesRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        public string PrettyPrint()
        {
            return "Properties Dependency";
        }

        public void Visit(IClassType modelType)
        {
            if (modelType is not IPropertyMembersClassType classTypeWithProperties)
            {
                return;
            }

            var dependencies = new Dictionary<string, int>();

            foreach (var propertyType in classTypeWithProperties.Properties)
            {
                if (dependencies.ContainsKey(propertyType.Type.Name))
                {
                    dependencies[propertyType.Type.Name]++;
                }
                else
                {
                    dependencies.Add(propertyType.Type.Name, 1);
                }
            }

            classTypeWithProperties.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }
    }
}
