using System.Collections.Generic;
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

        entityModel[DeclarationsMetricName] = Visit(entityModel);
    }

    private Dictionary<string, int> Visit(EntityModel entityModel)
    {
        var dependencies = new Dictionary<string, int>();

        _parameterRelationVisitor?.Visit(entityModel);

        _fieldsRelationVisitor?.Visit(entityModel);

        _propertiesRelationVisitor?.Visit(entityModel);

        _localVariablesRelationVisitor?.Visit(entityModel);

        return dependencies;
    }
}
