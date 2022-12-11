using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class LocalityOfAttributeAccess
{
    public static double Value(MethodModel method)
    {
        return (double)method.OwnMembersUsed().Count() / method.AllMembersUsed().Count();
    }
}
