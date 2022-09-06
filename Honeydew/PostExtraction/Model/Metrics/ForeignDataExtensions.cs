using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.Model.Metrics;

public static class ForeignDataExtensions
{
    public static IEnumerable<FieldModel> ForeignData(this ClassModel type)
    {
        return type.Methods.Where(m => !m.IsAbstract).SelectMany(ForeignData).ToHashSet();
    }

    public static IEnumerable<FieldModel> ForeignData(this MethodModel method)
    {
        return method.FieldAccesses.Where(f =>
            !method.Entity.Hierarchy.Contains(f.Field.Entity) &&
            f.Field.Entity is not EnumModel).Select(f => f.Field).ToHashSet();
    }

    //public static IEnumerable<IType> ForeignDataProviders(this IMethod method)
    //{
    //    return method.ForeignData().Select(member => member.ParentType).ExceptThirdParty().ToHashSetEx();
    //}
}