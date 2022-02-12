using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

public class FieldsRelationVisitor : IModelVisitor<ClassModel>, IRelationVisitor
{
    public string PrettyPrint()
    {
        return "Fields Dependency";
    }

    public void Visit(ClassModel classModel)
    {
        var dependencies = GetDependencies(classModel);

        classModel.Metrics.Add(new MetricModel
        {
            ExtractorName = GetType().ToString(),
            Value = dependencies,
            ValueType = dependencies.GetType().ToString()
        });
    }

    public Dictionary<string, int> GetDependencies(ClassModel classModel)
    {
        var dependencies = new Dictionary<string, int>();

        foreach (var fieldModel in classModel.Fields)
        {
            if (fieldModel is PropertyModel)
            {
                continue;
            }

            var typeName = CSharpConstants.GetNonNullableName(fieldModel.Type.Name);
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
