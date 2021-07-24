using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonRepositoryLoader : IModelLoader<RepositoryModel>
    {
        public RepositoryModel Load(string fileContent)
        {
            return JsonSerializer.Deserialize<RepositoryModel>(fileContent);
        }
    }
}
