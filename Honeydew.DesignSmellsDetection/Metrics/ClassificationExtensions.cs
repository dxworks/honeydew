using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class ClassificationExtensions
{
    public static IEnumerable<MemberModel> ProtectedMembers(this ClassModel type)
    {
        var classModel = type.BaseClass!.Entity as ClassModel;
        return classModel!.Members.Where(m => m.IsProtected).ToHashSet();
    }

    public static IEnumerable<MemberModel> ProtectedMembersUsed(this ClassModel type)
    {
        return type.ProtectedMembers().Where(type.Uses).ToList();
    }

    public static IEnumerable<MemberModel> OverridingMembers(this ClassModel type)
    {
        // TODO: currently we don't check that the method overrides a method from the base class
        return type.MethodsAndProperties.Where(
            m => !m.IsStatic && !m.IsAbstract && m.IsOverride);
    }
}
