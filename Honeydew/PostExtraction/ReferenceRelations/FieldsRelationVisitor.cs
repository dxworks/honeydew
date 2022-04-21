using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class FieldsRelationVisitor : IEntityModelVisitor
{
    public const string FieldsDependencyMetricName = "FieldsDependency";

    private readonly IAddStrategy _addStrategy;

    public FieldsRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public string Name => FieldsDependencyMetricName;

    public IDictionary<string, int> Visit(EntityModel referenceEntity)
    {
        if (referenceEntity is not ClassModel classModel)
        {
            return new Dictionary<string, int>();
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

        return dependencies;
    }
}
