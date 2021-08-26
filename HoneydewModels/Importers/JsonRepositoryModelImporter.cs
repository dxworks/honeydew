using HoneydewModels.CSharp;
using Newtonsoft.Json;

namespace HoneydewModels.Importers
{
    public class JsonRepositoryModelImporter : IModelImporter<RepositoryModel>
    {
        public RepositoryModel Import(string fileContent)
        {
            return JsonConvert.DeserializeObject<RepositoryModel>(fileContent);
        }
    }
}
