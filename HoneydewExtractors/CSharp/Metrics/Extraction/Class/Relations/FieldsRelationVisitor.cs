using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
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
                if (dependencies.ContainsKey(fieldType.Type.Name))
                {
                    dependencies[fieldType.Type.Name]++;
                }
                else
                {
                    dependencies.Add(fieldType.Type.Name, 1);
                }
            }

            return dependencies;
        }
    }
}
