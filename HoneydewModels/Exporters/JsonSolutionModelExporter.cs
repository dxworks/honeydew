using HoneydewModels.CSharp;
using Newtonsoft.Json;

namespace HoneydewModels.Exporters
{
    public class JsonSolutionModelExporter : IModelExporter<SolutionModel>
    {
        public string Export(SolutionModel model)
        {
            return JsonConvert.SerializeObject(model);
        }
    }
}
