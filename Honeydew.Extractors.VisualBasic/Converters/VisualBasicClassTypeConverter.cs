﻿using Honeydew.Extractors.Converters;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;

namespace Honeydew.Extractors.VisualBasic.Converters;

public class VisualBasicClassTypeConverter : ITypeConverter<IClassType>
{
    IClassType ITypeConverter<IClassType>.Convert(string type)
    {
        return type switch
        {
            "delegate" => new VisualBasicDelegateModel(),
            "enum" => new VisualBasicEnumModel(),
            _ => new VisualBasicClassModel()
        };
    }

    public IClassType Convert(object value)
    {
        return value switch
        {
            VisualBasicDelegateModel delegateModel => delegateModel,
            VisualBasicEnumModel enumModel => enumModel,
            _ => (VisualBasicClassModel)value
        };
    }
}
