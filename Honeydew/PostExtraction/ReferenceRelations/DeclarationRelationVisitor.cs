using System.Collections.Generic;
using HoneydewScriptBeePlugin;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class DeclarationRelationVisitor : IReferenceModelVisitor
{
    public const string DeclarationsMetricName = "declarations";

    private readonly LocalVariablesRelationVisitor _localVariablesRelationVisitor;
    private readonly ParameterRelationVisitor _parameterRelationVisitor;
    private readonly FieldsRelationVisitor _fieldsRelationVisitor;
    private readonly PropertiesRelationVisitor _propertiesRelationVisitor;

    public DeclarationRelationVisitor(LocalVariablesRelationVisitor localVariablesRelationVisitor,
        ParameterRelationVisitor parameterRelationVisitor, FieldsRelationVisitor fieldsRelationVisitor,
        PropertiesRelationVisitor propertiesRelationVisitor)
    {
        _localVariablesRelationVisitor = localVariablesRelationVisitor;
        _parameterRelationVisitor = parameterRelationVisitor;
        _fieldsRelationVisitor = fieldsRelationVisitor;
        _propertiesRelationVisitor = propertiesRelationVisitor;
    }

    public void Visit(ReferenceEntity referenceEntity)
    {
        if (referenceEntity is not EntityModel entityModel)
        {
            return;
        }

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

        var dependencies = new Dictionary<string, int>();

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

        entityModel[DeclarationsMetricName] = dependencies;
    }

    private static void AddDependencies(IDictionary<string, int> dependencies, Dictionary<string, int> dependencyNames)
    {
        foreach (var (dependencyName, count) in dependencyNames)
        {
            if (dependencies.ContainsKey(dependencyName))
            {
                dependencies[dependencyName] += count;
            }
            else
            {
                dependencies.Add(dependencyName, count);
            }
        }
    }
}
