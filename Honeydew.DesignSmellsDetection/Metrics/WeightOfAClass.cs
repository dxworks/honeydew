using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class WeightOfAClass
{
    public static double Value(ClassModel type)
    {
        return (double)AllPublicFunctionalMembersCount(type) / AllPublicMembersCount(type);
    }

    private static int AllPublicFunctionalMembersCount(ClassModel type)
    {
        return type.Methods.Count(m => m.IsPubliclyVisible && !m.IsAbstract);
    }

    private static int AllPublicMembersCount(ClassModel type)
    {
        return type.Methods.Count(m => m.IsPubliclyVisible) + type.Properties.Count(p => p.IsPubliclyVisible) +
               type.Fields.Count(f => f.IsPubliclyVisible);
    }
}