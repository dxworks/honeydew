using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class HierarchyRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        public string PrettyPrint()
        {
            return "hierarchy";
        }

        public void Visit(IClassType modelType)
        {
            var dict = new Dictionary<string, int>();

            foreach (var modelTypeBaseType in modelType.BaseTypes)
            {
                dict.Add(modelTypeBaseType.Type.Name, 1);
            }

            modelType.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dict,
                ValueType = dict.GetType().ToString()
            });
        }
    }
}
