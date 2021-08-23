using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonRepositoryModelExporter : IModelExporter<RepositoryModel>
    {
        private readonly IConverterList _converterList;

        public JsonRepositoryModelExporter(IConverterList converterList)
        {
            _converterList = converterList;
        }

        public string Export(RepositoryModel model)
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            foreach (var converter in _converterList.GetConverters())
            {
                jsonSerializerOptions.Converters.Add(converter);
            }

            return JsonSerializer.Serialize(model, jsonSerializerOptions);
        }
    }
}
