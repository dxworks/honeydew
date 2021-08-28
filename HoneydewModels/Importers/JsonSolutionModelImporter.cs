using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Importers
{
    public class JsonSolutionModelImporter : IModelImporter<SolutionModel>
    {
        private readonly IConverterList _converterList;

        public JsonSolutionModelImporter(IConverterList converterList)
        {
            _converterList = converterList;
        }
        
        public SolutionModel Import(string fileContent)
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            foreach (var converter in _converterList.GetConverters())
            {
                jsonSerializerOptions.Converters.Add(converter);
            }

            return JsonSerializer.Deserialize<SolutionModel>(fileContent, jsonSerializerOptions);
        }
    }
}
