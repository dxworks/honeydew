using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class PropertiesRelationVisitor : IReferenceModelVisitor
{
    public const string PropertiesDependencyMetricName = "PropertiesDependency";

    private readonly IAddStrategy _addStrategy;

    public PropertiesRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public void Visit(ReferenceEntity referenceEntity)
    {
        if (referenceEntity is not EntityModel entityModel)
        {
            return;
        }
        
        var dependencies = new Dictionary<string, int>();

        var properties = entityModel switch
        {
            ClassModel classModel => classModel.Properties,
            InterfaceModel interfaceModel => interfaceModel.Properties,
            _ => new List<PropertyModel>(),
        };

        foreach (var propertyType in properties)
        {
            _addStrategy.AddDependency(dependencies, propertyType.Type);
        }

        entityModel[PropertiesDependencyMetricName] = dependencies;
    }
}
