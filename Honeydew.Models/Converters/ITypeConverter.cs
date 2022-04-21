namespace Honeydew.Models.Converters;

public interface ITypeConverter<out TInterfaceType>
{
    TInterfaceType Convert(string type);

    TInterfaceType Convert(object value);
}
