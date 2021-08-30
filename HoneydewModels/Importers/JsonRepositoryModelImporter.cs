using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Importers
{
    public class JsonRepositoryModelImporter : IModelImporter<RepositoryModel>
    {
        private readonly IConverterList _converterList;

        public JsonRepositoryModelImporter(IConverterList converterList)
        {
            _converterList = converterList;
        }

        public RepositoryModel Import(string fileContent)
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            foreach (var converter in _converterList.GetConverters())
            {
                jsonSerializerOptions.Converters.Add(converter);
            }

            return JsonSerializer.Deserialize<RepositoryModel>(fileContent, jsonSerializerOptions);
        }
    }
}
