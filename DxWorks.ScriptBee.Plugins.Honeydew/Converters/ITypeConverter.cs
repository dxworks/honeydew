namespace DxWorks.ScriptBee.Plugins.Honeydew.Converters;

public interface ITypeConverter<out TInterfaceType>
{
    TInterfaceType Convert(string type);

    TInterfaceType Convert(object value);
}
