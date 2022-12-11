using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class PropertiesRelationVisitor : IEntityModelVisitor
{
    public const string PropertiesDependencyMetricName = "PropertiesDependency";

    private readonly IAddStrategy _addStrategy;

    public PropertiesRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public string Name => PropertiesDependencyMetricName;

    public IDictionary<string, int> Visit(EntityModel entityModel)
    {
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

        return dependencies;
    }
}
