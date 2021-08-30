using System;

namespace HoneydewModels.Converters
{
    public interface ITypeConverter<in TInterfaceType>
    {
        object Convert(TInterfaceType type);

        Type Convert(string type);
        Type DefaultType();
    }
}
