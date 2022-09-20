using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class LocalityOfAttributeAccess
{
    public static double Value(MethodModel method)
    {
        return (double)OwnMembersUsedBy(method).Count() / AllMembersUsedBy(method).Count();
    }

    private static IEnumerable<FieldModel> AllMembersUsedBy(MethodModel method)
    {
        var allUsedMembers = method.FieldAccesses.Where(f =>
            f.Field.Entity is not EnumModel).Select(f => f.Field).ToHashSet();
        return allUsedMembers;
    }

    private static IEnumerable<FieldModel> OwnMembersUsedBy(MethodModel method)
    {
        return AllMembersUsedBy(method).Where(memberUsed => memberUsed.Entity == method.Entity)
            .ToHashSet();
    }
}