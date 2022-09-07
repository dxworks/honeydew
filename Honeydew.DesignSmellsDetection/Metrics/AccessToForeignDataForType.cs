using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class AccessToForeignDataForType
{
    public static int Value(ClassModel type)
    {
        return type.ForeignData().Count();
    }
}