using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class ClassModelExtensions
{
    public static IList<MethodModel> MethodsToConsiderForDesignSmells(this ClassModel classModel)
    {
        var allMethods = new List<MethodModel>();
        allMethods.AddRange(classModel.Methods);
        allMethods.AddRange(classModel.Constructors);
        allMethods.AddRange(classModel.Properties.SelectMany(p => p.Accessors));
        if (classModel.Destructor != null)
        {
            allMethods.Add(classModel.Destructor);
        }

        return allMethods;
    }
}
