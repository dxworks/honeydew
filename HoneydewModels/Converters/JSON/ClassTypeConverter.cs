using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HoneydewModels.Types;

namespace HoneydewModels.Converters.JSON
{
    internal class ClassTypeConverter : JsonConverter<IClassType>
    {
        private readonly ITypeConverter<IClassType> _typeConverter;

        public ClassTypeConverter(ITypeConverter<IClassType> typeConverter)
        {
            _typeConverter = typeConverter;
        }

        public override IClassType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var readerCopy = reader;

            if (readerCopy.Read())
            {
                if (readerCopy.TokenType == JsonTokenType.PropertyName)
                {
                    if (readerCopy.Read())
                    {
                        var convert = _typeConverter.Convert(readerCopy.GetString());
                        return (IClassType)JsonSerializer.Deserialize(ref reader, convert, options);
                    }
                }
            }

            return (IClassType)JsonSerializer.Deserialize(ref reader, _typeConverter.DefaultType(), options);
        }


        public override void Write(Utf8JsonWriter writer, IClassType value, JsonSerializerOptions options)
        {
            var convertedValue = _typeConverter.Convert(value);
            JsonSerializer.Serialize(writer, convertedValue, options);
        }
    }
}
