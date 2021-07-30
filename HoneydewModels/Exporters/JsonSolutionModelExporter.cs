using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonSolutionModelExporter : IModelExporter<SolutionModel>
    {
        public string Export(SolutionModel model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}
