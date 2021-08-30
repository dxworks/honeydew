using HoneydewModels.CSharp;
using Newtonsoft.Json;

namespace HoneydewModels.Exporters
{
    public class JsonRepositoryModelExporter : IModelExporter<RepositoryModel>
    {
        public string Export(RepositoryModel model)
        {
            return JsonConvert.SerializeObject(model);
        }
    }
}
