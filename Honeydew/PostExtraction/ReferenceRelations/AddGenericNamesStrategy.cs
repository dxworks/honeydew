﻿using System.Collections.Generic;
using Honeydew.Models.CSharp;
using Honeydew.ScriptBeePlugin.Models;
using ClassModel = Honeydew.ScriptBeePlugin.Models.ClassModel;
using DelegateModel = Honeydew.ScriptBeePlugin.Models.DelegateModel;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class AddGenericNamesStrategy : AddNameStrategy
{
    private readonly bool _ignorePrimitives;

    public AddGenericNamesStrategy(bool ignorePrimitives)
    {
        _ignorePrimitives = ignorePrimitives;
    }

    public override void AddDependency(IDictionary<string, int> dependencies, EntityType type)
    {
        switch (type.Entity)
        {
            case ClassModel classModel:
                AddDependency(dependencies, classModel.Name, 1);
                break;
            case DelegateModel delegateModel:
                AddDependency(dependencies, delegateModel.Name, 1);
                break;
            case InterfaceModel interfaceModel:
                AddDependency(dependencies, interfaceModel.Name, 1);
                break;
        }

        foreach (var containedType in type.GenericTypes)
        {
            AddDependency(dependencies, containedType);
        }
    }

    public override void AddDependency(IDictionary<string, int> dependencies, string typeName, int count)
    {
        if (_ignorePrimitives)
        {
            if (CSharpConstants.IsPrimitive(typeName) || CSharpConstants.IsPrimitiveArray(typeName))
            {
                return;
            }
        }

        base.AddDependency(dependencies, typeName, count);
    }
}
