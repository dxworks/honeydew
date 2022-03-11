﻿using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class HierarchyRelationVisitor : IReferenceModelVisitor
{
    public const string HierarchyMetricName = "hierarchy";

    private readonly IAddStrategy _addStrategy;

    public HierarchyRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public void Visit(ReferenceEntity referenceEntity)
    {
        if (referenceEntity is not EntityModel entityModel)
        {
            return;
        }

        entityModel[HierarchyMetricName] = Visit(entityModel);
    }
    
    private Dictionary<string, int> Visit(EntityModel entityModel)
    {
        var dependencies = new Dictionary<string, int>();

        if (entityModel is ClassModel classModel)
        {
            foreach (var baseType in classModel.BaseTypes)
            {
                _addStrategy.AddDependency(dependencies, baseType);
            }
        }
        else if (entityModel is InterfaceModel interfaceModel)
        {
            foreach (var baseType in interfaceModel.BaseTypes)
            {
                _addStrategy.AddDependency(dependencies, baseType);
            }
        }

        return dependencies;
    }
}