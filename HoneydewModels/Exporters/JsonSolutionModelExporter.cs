using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonSolutionModelExporter : IModelExporter<SolutionModel>
    {
        private readonly IConverterList _converterList;

        public JsonSolutionModelExporter(IConverterList converterList)
        {
            _converterList = converterList;
        }

        public string Export(SolutionModel model)
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
