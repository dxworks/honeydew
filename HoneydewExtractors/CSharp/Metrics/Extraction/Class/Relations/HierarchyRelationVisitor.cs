using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

public class HierarchyRelationVisitor : IModelVisitor<ClassModel>, IRelationVisitor
{
    public string PrettyPrint()
    {
        return "hierarchy";
    }

    public void Visit(ClassModel classModel)
    {
        var dict = new Dictionary<string, int>();

        foreach (var modelTypeBaseType in classModel.BaseTypes)
        {
            if (dict.ContainsKey(modelTypeBaseType.Type.Name))
            {
                dict[modelTypeBaseType.Type.Name]++;
            }
            else
            {
                dict.Add(modelTypeBaseType.Type.Name, 1);
            }
        }

        classModel.Metrics.Add(new MetricModel
        {
            ExtractorName = GetType().ToString(),
            Value = dict,
            ValueType = dict.GetType().ToString()
        });
    }
}
