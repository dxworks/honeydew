using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Importers
{
    public class JsonSolutionModelImporter : IModelImporter<SolutionModel>
    {
        public SolutionModel Import(string fileContent)
        {
            return JsonSerializer.Deserialize<SolutionModel>(fileContent);
        }
    }
}
