using System.Collections.Generic;
using HoneydewModels.CSharp;
using Newtonsoft.Json;

namespace HoneydewModels.Importers
{
    public class JsonSolutionModelImporter : IModelImporter<SolutionModel>
    {
        public SolutionModel Import(string fileContent)
        {
            return JsonConvert.DeserializeObject<SolutionModel>(fileContent, new JsonSerializerSettings
            {
                // Converters = new List<JsonConverter>()
                // {
                //     new ModelJsonConverter(),
                // }
            });
        }
    }
}
