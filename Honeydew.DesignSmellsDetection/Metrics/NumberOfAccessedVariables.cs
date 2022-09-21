using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class NumberOfAccessedVariables
{
    public static int Value(MethodModel method)
    {
       return method.AllMembersUsed().Count() + method.LocalVariables.Count + method.Parameters.Count;
    }
}