using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class AverageMethodWeight
{
    public static double Value(ClassModel type, double wmc)
    {
        return wmc / type.MethodsPropertiesAndConstructors.Count;
    }
}