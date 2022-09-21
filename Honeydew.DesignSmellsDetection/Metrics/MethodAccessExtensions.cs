using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class MethodAccessExtensions
{
    public static IEnumerable<FieldModel> AllMembersUsed(this MethodModel method)
    {
        var allUsedMembers = method.FieldAccesses.Where(f =>
            f.Field.Entity is not EnumModel).Select(f => f.Field).ToHashSet();
        return allUsedMembers;
    }

    public static IEnumerable<FieldModel> OwnMembersUsed(this MethodModel method)
    {
        return method.AllMembersUsed().Where(memberUsed => memberUsed.Entity == method.Entity)
            .ToHashSet();
    }
}