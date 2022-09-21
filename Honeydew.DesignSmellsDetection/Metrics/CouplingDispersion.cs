using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class CouplingDispersion
{
    public static double Value(MethodModel method, int cint)
    {
        if (cint == 0)
        {
            return 0;
        }

        return (double)method.ProvidersForCoupledMethods().Count() / cint;
    }
}