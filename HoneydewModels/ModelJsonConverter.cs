using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HoneydewModels
{
    internal class ModelJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JContainer lJContainer = default(JContainer);

            if (reader.TokenType == JsonToken.StartObject)
            {
                lJContainer = JObject.Load(reader);
                existingValue = Activator.CreateInstance(objectType);

                serializer.Populate(lJContainer.CreateReader(), existingValue);
            }

            return existingValue;
            
            // if (reader.TokenType == JsonToken.Null)
            // {
            //     return "";
            // }
            //
            // if (reader.TokenType == JsonToken.String)
            // {
            //     return serializer.Deserialize(reader, objectType);
            // }
            //
            // try
            // {
            //     var obj = JToken.Load(reader);
            //     if (obj["ClassType"] != null)
            //     {
            //         var classType = obj["ClassType"].ToString();
            //         if (classType == "delegate")
            //         {
            //             return serializer.Deserialize<DelegateModel>(reader);
            //         }
            //
            //         return serializer.Deserialize<ClassModel>(reader);
            //     }
            // }
            // catch (Exception)
            // {
            //     return serializer.Deserialize(reader, objectType);
            // }
            //
            // return serializer.Deserialize(reader, objectType);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
