using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class FieldsRelationVisitor : IReferenceModelVisitor
{
    public const string FieldsDependencyMetricName = "FieldsDependency";

    private readonly IAddStrategy _addStrategy;

    public FieldsRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public void Visit(ReferenceEntity referenceEntity)
    {
        if (referenceEntity is not ClassModel classModel)
        {
            return;
        }

        var dependencies = new Dictionary<string, int>();

        foreach (var fieldModel in classModel.Fields)
        {
            if (fieldModel is PropertyModel)
            {
                continue;
            }

            _addStrategy.AddDependency(dependencies, fieldModel.Type);
        }

        referenceEntity[FieldsDependencyMetricName] = dependencies;
    }
}
