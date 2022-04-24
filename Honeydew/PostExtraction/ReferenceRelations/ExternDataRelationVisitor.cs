using System.Collections.Generic;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class ExternDataRelationVisitor : IEntityModelVisitor
{
    public const string ExtDataMetricName = "extData";

    private readonly IAddStrategy _addStrategy;

    public ExternDataRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public string Name => ExtDataMetricName;

    public IDictionary<string, int> Visit(EntityModel entityModel)
    {
        var dependencies = new Dictionary<string, int>();

        if (entityModel is ClassModel classModel)
        {
            foreach (var methodModel in classModel.Methods)
            {
                foreach (var accessedField in methodModel.FieldAccesses)
                {
                    if (accessedField.AccessEntityType.Entity != entityModel)
                    {
                        _addStrategy.AddDependency(dependencies, accessedField.AccessEntityType);
                    }
                }
            }

            foreach (var methodModel in classModel.Constructors)
            {
                foreach (var accessedField in methodModel.FieldAccesses)
                {
                    if (accessedField.AccessEntityType.Entity != entityModel)
                    {
                        _addStrategy.AddDependency(dependencies, accessedField.AccessEntityType);
                    }
                }
            }

            if (classModel.Destructor != null)
            {
                foreach (var accessedField in classModel.Destructor.FieldAccesses)
                {
                    if (accessedField.AccessEntityType.Entity != entityModel)
                    {
                        _addStrategy.AddDependency(dependencies, accessedField.AccessEntityType);
                    }
                }
            }

            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var accessedField in accessor.FieldAccesses)
                    {
                        if (accessedField.AccessEntityType.Entity != entityModel)
                        {
                            _addStrategy.AddDependency(dependencies, accessedField.AccessEntityType);
                        }
                    }
                }
            }
        }
        else if (entityModel is InterfaceModel interfaceModel)
        {
            foreach (var methodModel in interfaceModel.Methods)
            {
                foreach (var accessedField in methodModel.FieldAccesses)
                {
                    if (accessedField.AccessEntityType.Entity != entityModel)
                    {
                        _addStrategy.AddDependency(dependencies, accessedField.AccessEntityType);
                    }
                }
            }

            foreach (var propertyType in interfaceModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var accessedField in accessor.FieldAccesses)
                    {
                        if (accessedField.AccessEntityType.Entity != entityModel)
                        {
                            _addStrategy.AddDependency(dependencies, accessedField.AccessEntityType);
                        }
                    }
                }
            }
        }

        return dependencies;
    }
}
