﻿using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class ReturnValueRelationVisitor : IReferenceModelVisitor
{
    public const string ReturnsMetricName = "returns";

    private readonly IAddStrategy _addStrategy;

    public ReturnValueRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public void Visit(ReferenceEntity referenceEntity)
    {
        if (referenceEntity is not EntityModel entityModel)
        {
            return;
        }

        entityModel[ReturnsMetricName] = Visit(entityModel);
    }

    private Dictionary<string, int> Visit(EntityModel entityModel)
    {
        var dependencies = new Dictionary<string, int>();

        switch (entityModel)
        {
            case ClassModel classModel:
                foreach (var methodType in classModel.Methods)
                {
                    if (methodType.Type is MethodType.Method or MethodType.LocalFunction
                            or MethodType.Extension && methodType.ReturnValue != null)
                    {
                        _addStrategy.AddDependency(dependencies, methodType.ReturnValue.Type);
                    }
                }

                break;

            case DelegateModel delegateModel:
                if (delegateModel.ReturnValue != null)
                {
                    _addStrategy.AddDependency(dependencies, delegateModel.ReturnValue.Type);
                }

                break;

            case InterfaceModel interfaceModel:
                foreach (var methodType in interfaceModel.Methods)
                {
                    if (methodType.Type is MethodType.Method or MethodType.LocalFunction
                            or MethodType.Extension && methodType.ReturnValue != null)
                    {
                        _addStrategy.AddDependency(dependencies, methodType.ReturnValue.Type);
                    }
                }

                break;
        }

        return dependencies;
    }
}
