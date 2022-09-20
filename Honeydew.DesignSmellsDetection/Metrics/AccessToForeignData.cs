using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class AccessToForeignData
{
    public static int Value(ClassModel type)
    {
        return type.ForeignData().Count();
    }

    public static int Value(MethodModel method)
    {
        return method.ForeignData().Count();
    }
}
