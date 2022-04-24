using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class ExternCallsRelationVisitor : IEntityModelVisitor
{
    public const string ExtCallsMetricName = "extCalls";

    private readonly IAddStrategy _addStrategy;

    public ExternCallsRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public string Name => ExtCallsMetricName;

    public IDictionary<string, int> Visit(EntityModel entityModel)
    {
        var dependencies = new Dictionary<string, int>();

        if (entityModel is ClassModel classModel)
        {
            foreach (var methodModel in classModel.Methods)
            {
                foreach (var calledMethod in methodModel.OutgoingCalls)
                {
                    if (calledMethod.Caller != methodModel && calledMethod.CalledEnitityType.Entity != classModel)
                    {
                        _addStrategy.AddDependency(dependencies, calledMethod.CalledEnitityType);
                    }
                }
            }

            foreach (var methodModel in classModel.Constructors)
            {
                foreach (var calledMethod in methodModel.OutgoingCalls)
                {
                    if (calledMethod.Caller != methodModel && calledMethod.CalledEnitityType.Entity != classModel)
                    {
                        _addStrategy.AddDependency(dependencies, calledMethod.CalledEnitityType);
                    }
                }
            }

            if (classModel.Destructor != null)
            {
                foreach (var calledMethod in classModel.Destructor.OutgoingCalls)
                {
                    if (calledMethod.Caller != classModel.Destructor &&
                        calledMethod.CalledEnitityType.Entity != classModel)
                    {
                        _addStrategy.AddDependency(dependencies, calledMethod.CalledEnitityType);
                    }
                }
            }

            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var calledMethod in accessor.OutgoingCalls)
                    {
                        if (calledMethod.Caller != accessor && calledMethod.CalledEnitityType.Entity != classModel)
                        {
                            _addStrategy.AddDependency(dependencies, calledMethod.CalledEnitityType);
                        }
                    }
                }
            }
        }
        else if (entityModel is InterfaceModel interfaceModel)
        {
            foreach (var methodModel in interfaceModel.Methods)
            {
                foreach (var calledMethod in methodModel.OutgoingCalls)
                {
                    if (calledMethod.Caller != methodModel && calledMethod.CalledEnitityType.Entity != interfaceModel)
                    {
                        _addStrategy.AddDependency(dependencies, calledMethod.CalledEnitityType);
                    }
                }
            }

            foreach (var propertyType in interfaceModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var calledMethod in accessor.OutgoingCalls)
                    {
                        if (calledMethod.Caller != accessor && calledMethod.CalledEnitityType.Entity != interfaceModel)
                        {
                            _addStrategy.AddDependency(dependencies, calledMethod.CalledEnitityType);
                        }
                    }
                }
            }
        }

        return dependencies;
    }
}
