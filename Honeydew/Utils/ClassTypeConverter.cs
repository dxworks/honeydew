using Honeydew.Models.Converters;
using Honeydew.Models.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Honeydew.Utils;

internal class ClassTypeConverter : JsonConverter
{
    private readonly ITypeConverter<IClassType> _typeConverter;

    public ClassTypeConverter(ITypeConverter<IClassType> typeConverter)
    {
        _typeConverter = typeConverter;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not null)
        {
            serializer.Serialize(writer, _typeConverter.Convert(value));
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);

        var classTypeToken = obj.GetValue("ClassType");
        if (classTypeToken is null)
        {
            throw new JsonException("ClassType is missing");
        }

        var classType = _typeConverter.Convert(classTypeToken.ToString());

        serializer.Populate(obj.CreateReader(), classType);
        return classType;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IClassType);
    }
}
