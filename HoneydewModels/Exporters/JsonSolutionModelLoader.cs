using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonSolutionModelLoader : IModelLoader<SolutionModel>
    {
        public SolutionModel Load(string fileContent)
        {
            return JsonSerializer.Deserialize<SolutionModel>(fileContent);
        }
    }
}
