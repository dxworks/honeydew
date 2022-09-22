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
        var ownFieldsUSed = method.FieldAccesses.Where(f =>
                f.Field.Entity is not EnumModel && f.IsFrom(method.Entity)).Select(fa => fa.Field)
            .ToHashSet();

        return ownFieldsUSed;
    }

    public static bool IsFrom(this FieldAccess fa, EntityModel entity)
    {
        /*
         * TODO: this is probably a bug
         * if the method is on generic class, the method.Entity will be a generic class (e.g. BaseClass<T>), but the fa.AccessEntityType.Entity will not be generic (e.g. BaseClass).
         * This is why we compare the name of an EntityType to the name of an EntityModel
        */
        return fa.AccessEntityType.Name == entity.Name;
    }

    public static bool IsFrom(this MethodCall call, EntityModel entity)
    {
        /*
         * TODO: this is probably a bug
         * if the method is on generic class, the method.Entity will be a generic class (e.g. BaseClass<T>), but the fa.AccessEntityType.Entity will not be generic (e.g. BaseClass).
         * This is why we compare the name of an EntityType to the name of an EntityModel
        */
        return call.CalledEnitityType.Name == entity.Name;
    }
}
