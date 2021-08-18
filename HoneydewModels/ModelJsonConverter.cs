using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoneydewModels
{
    internal class ModelJsonConverter<TInterfaceModel, TModel> : JsonConverter<TInterfaceModel>
        where TModel : TInterfaceModel
    {
        public override TInterfaceModel Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TModel>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TInterfaceModel value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (TModel)value, options);
        }
    }
}
