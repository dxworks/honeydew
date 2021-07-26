using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Importers
{
    public class JsonRepositoryModelImporter : IModelImporter<RepositoryModel>
    {
        public RepositoryModel Import(string fileContent)
        {
            return JsonSerializer.Deserialize<RepositoryModel>(fileContent);
        }
    }
}
