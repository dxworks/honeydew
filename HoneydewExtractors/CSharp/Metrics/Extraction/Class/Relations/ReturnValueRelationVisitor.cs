using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

public class ReturnValueRelationVisitor : IModelVisitor<ClassModel>, IRelationVisitor
{
    public string PrettyPrint()
    {
        return "returns";
    }

    public void Visit(ClassModel classModel)
    {
        var dependencies = new Dictionary<string, int>();

        foreach (var methodModel in classModel.Methods)
        {
            if (methodModel.ReturnValue == null)
            {
                continue;
            }

            var typeName = CSharpConstants.GetNonNullableName(methodModel.ReturnValue.Type.Name);
            if (dependencies.ContainsKey(typeName))
            {
                dependencies[typeName]++;
            }
            else
            {
                dependencies.Add(typeName, 1);
            }
        }

        classModel.Metrics.Add(new MetricModel
        {
            ExtractorName = GetType().ToString(),
            Value = dependencies,
            ValueType = dependencies.GetType().ToString()
        });
    }
}
