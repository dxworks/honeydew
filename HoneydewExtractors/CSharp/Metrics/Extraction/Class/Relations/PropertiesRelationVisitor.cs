using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
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

            var dependencies = GetDependencies(classTypeWithProperties);

            classTypeWithProperties.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }

        public Dictionary<string, int> GetDependencies(IPropertyMembersClassType classTypeWithProperties)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var propertyType in classTypeWithProperties.Properties)
            {
                var typeName = CSharpConstants.GetNonNullableName(propertyType.Type.Name);
                if (dependencies.ContainsKey(typeName))
                {
                    dependencies[typeName]++;
                }
                else
                {
                    dependencies.Add(typeName, 1);
                }
            }

            return dependencies;
        }
    }
}
