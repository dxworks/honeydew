using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class ForeignDataProviders
{
    public static int Value(MethodModel method)
    {
        return method.ForeignDataProviders().Count();
    }
}
