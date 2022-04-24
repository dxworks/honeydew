using System.Collections.Generic;
using Honeydew.ScriptBeePlugin;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class DeclarationRelationVisitor : IEntityModelVisitor
{
    public const string DeclarationsMetricName = "declarations";

    private readonly IAddStrategy _addStrategy;
    private readonly LocalVariablesRelationVisitor _localVariablesRelationVisitor;
    private readonly ParameterRelationVisitor _parameterRelationVisitor;
    private readonly FieldsRelationVisitor _fieldsRelationVisitor;
    private readonly PropertiesRelationVisitor _propertiesRelationVisitor;

    public DeclarationRelationVisitor(IAddStrategy addStrategy,
        LocalVariablesRelationVisitor localVariablesRelationVisitor,
        ParameterRelationVisitor parameterRelationVisitor, FieldsRelationVisitor fieldsRelationVisitor,
        PropertiesRelationVisitor propertiesRelationVisitor)
    {
        _addStrategy = addStrategy;
        _localVariablesRelationVisitor = localVariablesRelationVisitor;
        _parameterRelationVisitor = parameterRelationVisitor;
        _fieldsRelationVisitor = fieldsRelationVisitor;
        _propertiesRelationVisitor = propertiesRelationVisitor;
    }

    public string Name => DeclarationsMetricName;

    public IDictionary<string, int> Visit(EntityModel entityModel)
    {
        var dependencies = new Dictionary<string, int>();

        if (!entityModel.HasProperty(ParameterRelationVisitor.ParameterDependencyMetricName))
        {
            _parameterRelationVisitor?.Visit(entityModel);
        }

        if (!entityModel.HasProperty(FieldsRelationVisitor.FieldsDependencyMetricName))
        {
            _fieldsRelationVisitor?.Visit(entityModel);
        }

        if (!entityModel.HasProperty(PropertiesRelationVisitor.PropertiesDependencyMetricName))
        {
            _propertiesRelationVisitor?.Visit(entityModel);
        }

        if (!entityModel.HasProperty(LocalVariablesRelationVisitor.LocalVariablesDependencyMetricName))
        {
            _localVariablesRelationVisitor?.Visit(entityModel);
        }

        if (entityModel.HasProperty(ParameterRelationVisitor.ParameterDependencyMetricName) &&
            entityModel[ParameterRelationVisitor.ParameterDependencyMetricName] is Dictionary<string, int>
                parameterDependencies)
        {
            AddDependencies(dependencies, parameterDependencies);
        }

        if (entityModel.HasProperty(FieldsRelationVisitor.FieldsDependencyMetricName) &&
            entityModel[FieldsRelationVisitor.FieldsDependencyMetricName] is Dictionary<string, int> fieldsDependencies)
        {
            AddDependencies(dependencies, fieldsDependencies);
        }

        if (entityModel.HasProperty(PropertiesRelationVisitor.PropertiesDependencyMetricName) &&
            entityModel[PropertiesRelationVisitor.PropertiesDependencyMetricName] is Dictionary<string, int>
                propertiesDependencies)
        {
            AddDependencies(dependencies, propertiesDependencies);
        }

        if (entityModel.HasProperty(LocalVariablesRelationVisitor.LocalVariablesDependencyMetricName) &&
            entityModel[LocalVariablesRelationVisitor.LocalVariablesDependencyMetricName] is Dictionary<string, int>
                localVariablesDependencies)
        {
            AddDependencies(dependencies, localVariablesDependencies);
        }

        return dependencies;
    }

    private void AddDependencies(IDictionary<string, int> dependencies, Dictionary<string, int> dependencyNames)
    {
        foreach (var (dependencyName, count) in dependencyNames)
        {
            _addStrategy.AddDependency(dependencies, dependencyName, count);
        }
    }
}
