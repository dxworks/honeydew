using Honeydew.Models.Converters;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;

namespace Honeydew.Utils;

public class CSharpClassTypeConverter : ITypeConverter<IClassType>
{
    IClassType ITypeConverter<IClassType>.Convert(string type)
    {
        return type switch
        {
            "delegate" => new DelegateModel(),
            "enum" => new EnumModel(),
            _ => new ClassModel()
        };
    }

    public IClassType Convert(object value)
    {
        return value switch
        {
            DelegateModel delegateModel => delegateModel,
            EnumModel enumModel => enumModel,
            _ => (ClassModel)value
        };
    }
}
