using Honeydew.ModelAdapters.V2._1._0.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Honeydew.ModelAdapters.V2._1._0.Importers;

internal class ClassTypeConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value != null)
        {
            serializer.Serialize(writer, CSharpClassTypeConverter.ConvertObject(value));
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

        var classType = CSharpClassTypeConverter.ConvertType(classTypeToken.ToString());

        serializer.Populate(obj.CreateReader(), classType);
        return classType;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IClassType_V210);
    }
}
