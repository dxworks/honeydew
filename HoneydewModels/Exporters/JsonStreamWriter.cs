using System.IO;
using Newtonsoft.Json;

namespace HoneydewModels.Exporters
{
    public class JsonStreamWriter
    {
        public void Write(string filePath, object obj)
        {
            var jsonSerializer = JsonSerializer.Create();
            using (var writer = new StreamWriter(filePath))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonSerializer.Serialize(jsonWriter, obj);
                }
            }
        }
    }
}
