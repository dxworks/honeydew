using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class DataClassExtensions
{
    public static IEnumerable<FieldModel> PublicAttributes(this ClassModel type)
    {
        return type.Fields.Where(f => f.IsPubliclyVisible && !f.IsReadonly && !f.IsConst && !f.IsStatic);
    }

    public static IEnumerable<PropertyModel> Accessors(this ClassModel type)
    {
        return type.Properties.Where(m => m.IsPubliclyVisible && !m.IsAbstract && !m.IsStatic);
    }
}
