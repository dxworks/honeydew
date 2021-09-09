using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class ReturnValueRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        public string PrettyPrint()
        {
            return "Return Value Dependency";
        }

        public void Visit(IClassType modelType)
        {
            if (modelType is not IMembersClassType membersClassType)
            {
                return;
            }

            var dependencies = new Dictionary<string, int>();

            foreach (var methodType in membersClassType.Methods)
            {
                if (dependencies.ContainsKey(methodType.ReturnValue.Type.Name))
                {
                    dependencies[methodType.ReturnValue.Type.Name]++;
                }
                else
                {
                    dependencies.Add(methodType.ReturnValue.Type.Name, 1);
                }
            }

            membersClassType.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }
    }
}
