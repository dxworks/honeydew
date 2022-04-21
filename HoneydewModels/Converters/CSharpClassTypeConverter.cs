using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewModels.Converters;

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
