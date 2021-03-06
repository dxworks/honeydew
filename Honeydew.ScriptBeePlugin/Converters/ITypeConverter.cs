namespace Honeydew.ScriptBeePlugin.Converters;

public interface ITypeConverter<out TInterfaceType>
{
    TInterfaceType Convert(string type);

    TInterfaceType Convert(object value);
}
