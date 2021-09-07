using System.IO;
using HoneydewModels.CSharp;
using Newtonsoft.Json;

namespace HoneydewModels.Exporters
{
    public class JsonRepositoryModelExporter : IModelExporter<RepositoryModel>
    {
        public void Export(string filePath, RepositoryModel model)
        {
            var jsonSerializer = JsonSerializer.Create();
            jsonSerializer.Serialize(new StreamWriter(filePath), model);
        }
    }
}
