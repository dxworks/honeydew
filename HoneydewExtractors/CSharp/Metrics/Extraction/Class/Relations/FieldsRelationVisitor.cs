using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class FieldsRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        public string PrettyPrint()
        {
            return "Fields Dependency";
        }

        public void Visit(IClassType modelType)
        {
            if (modelType is not IMembersClassType membersClassType)
            {
                return;
            }

            var dependencies = GetDependencies(membersClassType);

            membersClassType.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }

        public Dictionary<string, int> GetDependencies(IMembersClassType membersClassType)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var fieldType in membersClassType.Fields)
            {
                var typeName = CSharpConstants.GetNonNullableName(fieldType.Type.Name);
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
