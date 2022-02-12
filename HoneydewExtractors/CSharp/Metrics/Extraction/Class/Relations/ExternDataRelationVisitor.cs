using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.Reference;


namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

public class ExternDataRelationVisitor : IModelVisitor<ClassModel>, IRelationVisitor
{
    public string PrettyPrint()
    {
        return "extData";
    }

    public void Visit(ClassModel classModel)
    {
        var dependencies = new Dictionary<string, int>();

        foreach (var methodType in classModel.Methods)
        {
            foreach (var accessedField in methodType.AccessedFields)
            {
                if (accessedField.Field.Class != classModel)
                {
                    AddDependency(accessedField.Field?.Class?.Name);
                }
            }
        }

        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                foreach (var accessedField in accessor.AccessedFields)
                {
                    if (accessedField.Field.Class != classModel)
                    {
                        AddDependency(accessedField.Field?.Class?.Name);
                    }
                }
            }
        }

        classModel.Metrics.Add(new MetricModel
        {
            ExtractorName = GetType().ToString(),
            Value = dependencies,
            ValueType = dependencies.GetType().ToString()
        });

        void AddDependency(string dependencyName)
        {
            if (string.IsNullOrEmpty(dependencyName))
            {
                return;
            }

            if (dependencies.ContainsKey(dependencyName))
            {
                dependencies[dependencyName]++;
            }
            else
            {
                dependencies.Add(dependencyName, 1);
            }
        }
    }
}
