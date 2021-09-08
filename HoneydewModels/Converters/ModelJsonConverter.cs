using System;
using Newtonsoft.Json;

namespace HoneydewModels.Converters
{
    internal class ModelJsonConverter<TInterfaceModel, TConcreteModel> : JsonConverter
        where TConcreteModel : TInterfaceModel
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(TConcreteModel));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TInterfaceModel);
        }
    }
}
