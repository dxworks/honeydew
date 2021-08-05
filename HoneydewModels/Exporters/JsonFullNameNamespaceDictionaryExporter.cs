using System.Collections.Generic;
using System.Text.Json;
using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonFullNameNamespaceDictionaryExporter : IModelExporter<IDictionary<string, NamespaceTree>>
    {
        public string Export(IDictionary<string, NamespaceTree> model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}
