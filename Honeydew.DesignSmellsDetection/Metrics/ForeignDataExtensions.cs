using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class ForeignDataExtensions
{
    public static IEnumerable<FieldModel> ForeignData(this ClassModel type)
    {
        var foreignData = type.Methods.Where(m => !m.IsAbstract).SelectMany(ForeignData).ToHashSet();
        return foreignData;
    }

    public static IEnumerable<FieldModel> ForeignData(this MethodModel method)
    {
        var foreignData = method.InternalFieldAccesses.Where(f =>
            !method.Entity.Hierarchy.Contains(f.Field.Entity) &&
            f.Field.Entity is not EnumModel).Select(f => f.Field).ToHashSet();
        return foreignData;
    }

    public static IEnumerable<EntityModel> ForeignDataProviders(this MethodModel method)
    {
        var foreignDataProviders = method.ForeignData().Select(member => member.Entity).ToHashSet();

        return foreignDataProviders;
    }
}
