using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class ParameterRelationVisitor : IEntityModelVisitor
{
    public const string ParameterDependencyMetricName = "ParameterDependency";

    private readonly IAddStrategy _addStrategy;

    public ParameterRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public string Name => ParameterDependencyMetricName;

    public IDictionary<string, int> Visit(EntityModel entityModel)
    {
        var dependencies = new Dictionary<string, int>();

        switch (entityModel)
        {
            case ClassModel classModel:
                foreach (var methodModel in classModel.Methods)
                {
                    foreach (var parameterType in methodModel.Parameters)
                    {
                        _addStrategy.AddDependency(dependencies, parameterType.Type);
                    }
                }

                foreach (var constructorModel in classModel.Constructors)
                {
                    foreach (var parameterType in constructorModel.Parameters)
                    {
                        _addStrategy.AddDependency(dependencies, parameterType.Type);
                    }
                }

                break;

            case DelegateModel delegateModel:
                foreach (var parameterType in delegateModel.Parameters)
                {
                    _addStrategy.AddDependency(dependencies, parameterType.Type);
                }

                break;

            case InterfaceModel interfaceModel:
                foreach (var methodModel in interfaceModel.Methods)
                {
                    foreach (var parameterType in methodModel.Parameters)
                    {
                        _addStrategy.AddDependency(dependencies, parameterType.Type);
                    }
                }

                break;
        }

        return dependencies;
    }
}
