using System.IO;
using HoneydewModels.CSharp;
using Newtonsoft.Json;

namespace HoneydewModels.Exporters
{
    public class JsonSolutionModelExporter : IModelExporter<SolutionModel>
    {
        public void Export(string filePath, SolutionModel model)
        {
            var jsonSerializer = JsonSerializer.Create();
            jsonSerializer.Serialize(new StreamWriter(filePath), model);
        }
    }
}
