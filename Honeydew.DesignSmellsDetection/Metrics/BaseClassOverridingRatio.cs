using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class BaseClassOverridingRatio
{
    public static double Value(ClassModel type)
    {
        var overridingMethods = type.OverridingMembers();
        var bovr = (double)overridingMethods.Count() / type.MethodsAndProperties.Count(m => !m.IsStatic && !m.IsAbstract);

        return bovr;
    }
}
