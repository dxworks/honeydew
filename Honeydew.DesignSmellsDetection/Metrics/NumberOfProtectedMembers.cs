using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class NumberOfProtectedMembers
{
    public static int Value(ClassModel type)
    {
        return type.ProtectedMembers().Count();
    }
}
