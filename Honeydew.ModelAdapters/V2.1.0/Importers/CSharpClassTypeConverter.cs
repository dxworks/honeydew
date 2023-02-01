using Honeydew.ModelAdapters.V2._1._0.CSharp;

namespace Honeydew.ModelAdapters.V2._1._0.Importers;

public static class CSharpClassTypeConverter
{
    public static IClassType_V210 ConvertType(string type)
    {
        return type == "delegate" ? new DelegateModel_V210() : new ClassModel_V210();
    }

    public static IClassType_V210 ConvertObject(object value)
    {
        if (value is string stringValue)
        {
            return ConvertType(stringValue);
        }

        if (value is DelegateModel_V210 delegateModel)
        {
            return delegateModel;
        }

        return (ClassModel_V210)value;
    }
}
