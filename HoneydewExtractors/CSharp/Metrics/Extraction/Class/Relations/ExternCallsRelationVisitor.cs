using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

public class ExternCallsRelationVisitor : IModelVisitor<ClassModel>, IRelationVisitor
{
    public string PrettyPrint()
    {
        return "extCalls";
    }

    public void Visit(ClassModel classModel)
    {
        var dependencies = new Dictionary<string, int>();

        foreach (var methodType in classModel.Methods)
        {
            foreach (var calledMethod in methodType.CalledMethods)
            {
                if (calledMethod.Class != classModel)
                {
                    AddDependency(calledMethod.Class?.Name);
                }
            }
        }

        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                foreach (var calledMethod in accessor.CalledMethods)
                {
                    if (calledMethod.Class != classModel)
                    {
                        AddDependency(calledMethod.Class?.Name);
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
