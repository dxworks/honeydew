using System.IO;
using Newtonsoft.Json;

namespace HoneydewModels.Importers
{
    public class JsonModelImporter<TModel>
    {
        private readonly IConverterList _converterList;

        public JsonModelImporter(IConverterList converterList)
        {
            _converterList = converterList;
        }

        public TModel Import(string filePath)
        {
            using (var streamReader = File.OpenText(filePath))
            {
                var serializer = new JsonSerializer();
                foreach (var converter in _converterList.GetConverters())
                {
                    serializer.Converters.Add(converter);
                }

                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    return serializer.Deserialize<TModel>(jsonTextReader);
                }
            }
        }
    }
}
