using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class BaseClassUsageRatio
{
    public static double Value(ClassModel type)
    {
        var bur = (double)type.ProtectedMembersUsed().Count() / type.ProtectedMembers().Count();
        return bur;
    }
}