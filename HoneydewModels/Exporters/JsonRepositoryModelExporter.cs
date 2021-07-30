using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonRepositoryModelExporter : IModelExporter<RepositoryModel>
    {
        public string Export(RepositoryModel model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}
