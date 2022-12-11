using Honeydew.Models.CSharp;
using Honeydew.Models.Types;

namespace DxWorks.ScriptBee.Plugins.Honeydew.Converters;

public class CSharpClassTypeConverter : ITypeConverter<IClassType>
{
    IClassType ITypeConverter<IClassType>.Convert(string type)
    {
        return type switch
        {
            "delegate" => new CSharpDelegateModel(),
            "enum" => new CSharpEnumModel(),
            _ => new CSharpClassModel()
        };
    }

    public IClassType Convert(object value)
    {
        return value switch
        {
            CSharpDelegateModel delegateModel => delegateModel,
            CSharpEnumModel enumModel => enumModel,
            _ => (CSharpClassModel)value
        };
    }
}
